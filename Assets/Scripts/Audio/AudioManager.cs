using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GuJian.Audio {
    /// <summary>
    /// 统一音频管理器。支持:
    ///   - 6 个 AudioCategory(特效/场景/动作/转场/切换/界面)
    ///   - 每类下独立 AudioSource 池(可配大小),减少分配开销
    ///   - Scene 类独占——交叉淡入/淡出
    ///   - 按 id 播放;同 id “回跳冷却”防止同帧多次触发栈音量
    ///   - 预加载/卸载 Bank 自动构索引
    ///
    /// 使用:
    ///   AudioManager.Instance.RegisterBank(bankSO);
    ///   AudioManager.Instance.Play("sfx_hammer_hit");
    ///   AudioManager.Instance.PlayScene("bgm_outer_yard");
    ///   AudioManager.Instance.SetCategoryVolume(AudioCategory.Ui, 0.6f);
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [SerializeField] AudioBank[] initialBanks;


        [Header("Mixer(可选)")]
        [SerializeField] AudioMixer mixer;
        [Tooltip("按 AudioCategory 顺序对应的 Mixer Group。")]
        [SerializeField] AudioMixerGroup[] groupsByCategory;

        [Header("池大小(按分类)")]
        [SerializeField] int sfxVoices        = 16;
        [SerializeField] int actionVoices     = 12;
        [SerializeField] int uiVoices         = 6;
        [SerializeField] int switchVoices     = 4;
        [SerializeField] int transitionVoices = 2;

        [Header("场景 BGM")]
        [SerializeField] float sceneCrossfadeSeconds = 1.25f;

        // ----- 内部 -----
        readonly Dictionary<AudioCategory, SourcePool> _pools = new();
        readonly List<AudioBank> _banks = new();
        readonly Dictionary<string, float> _lastPlayedAt = new();
        readonly float[] _categoryVolume = new float[6] { 1,1,1,1,1,1 };

        SceneChannel _sceneA;
        SceneChannel _sceneB;
        bool _aIsActive;
        Coroutine _sceneRoutine;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildPools();
            BuildSceneChannels();
            if (initialBanks != null) foreach (var b in initialBanks) RegisterBank(b);

        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        // ========= 公开 API =========
        public void RegisterBank(AudioBank bank) {
            if (bank == null || _banks.Contains(bank)) return;
            _banks.Add(bank);
        }

        public void UnregisterBank(AudioBank bank) { _banks.Remove(bank); }

        public void SetCategoryVolume(AudioCategory c, float v) {
            _categoryVolume[(int)c] = Mathf.Clamp01(v);
            // 推给 Mixer
            if (mixer != null) {
                var key = "vol_" + c.ToString();
                float db = v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;
                mixer.SetFloat(key, db);
            }
        }

        public float GetCategoryVolume(AudioCategory c) => _categoryVolume[(int)c];

        /// <summary>按 id 播放。自动取其 AudioCategory 所属的池。</summary>
        public AudioSource Play(string id, Vector3? at = null) {
            var e = FindEntry(id);
            if (e == null) { Debug.LogWarning($"[Audio] 找不到 id={id}"); return null; }
            if (!CheckCooldown(id, e.retriggerCooldown)) return null;
            return PlayEntry(e, at);
        }

        /// <summary>仅 Scene 类専用:切换场景 BGM,带交叉淡入/淡出。</summary>
        public void PlayScene(string id, float fadeSeconds = -1f) {
            var e = FindEntry(id);
            if (e == null || e.clip == null) { Debug.LogWarning($"[Audio] Scene id={id} 未找到"); return; }
            if (fadeSeconds < 0f) fadeSeconds = sceneCrossfadeSeconds;
            if (_sceneRoutine != null) StopCoroutine(_sceneRoutine);
            _sceneRoutine = StartCoroutine(CrossfadeScene(e, fadeSeconds));
        }

        /// <summary>停掉当前场景音。</summary>
        public void StopScene(float fadeSeconds = -1f) {
            if (fadeSeconds < 0f) fadeSeconds = sceneCrossfadeSeconds;
            if (_sceneRoutine != null) StopCoroutine(_sceneRoutine);
            _sceneRoutine = StartCoroutine(FadeOutScene(fadeSeconds));
        }

        /// <summary>停掉某个 id 的所有实例(Scene 类请走 StopScene)。</summary>
        public void StopAll(AudioCategory category) {
            if (_pools.TryGetValue(category, out var pool)) pool.StopAll();
        }

        // ========= 内部实现 =========
        AudioBank.Entry FindEntry(string id) {
            for (int i = 0; i < _banks.Count; i++) {
                var hit = _banks[i].Find(id);
                if (hit != null) return hit;
            }
            return null;
        }

        bool CheckCooldown(string id, float cd) {
            if (cd <= 0f) return true;
            float t = Time.unscaledTime;
            if (_lastPlayedAt.TryGetValue(id, out var last) && t - last < cd) return false;
            _lastPlayedAt[id] = t;
            return true;
        }

        AudioSource PlayEntry(AudioBank.Entry e, Vector3? at) {
            if (e.category == AudioCategory.Scene) {
                PlayScene(e.id); return null;
            }
            if (!_pools.TryGetValue(e.category, out var pool)) return null;
            var src = pool.Get();
            if (src == null) return null;
            src.clip       = e.clip;
            src.volume     = e.volume * _categoryVolume[(int)e.category];
            src.pitch      = e.pitchMin < e.pitchMax - 0.0001f ? Random.Range(e.pitchMin, e.pitchMax) : e.pitchMin;
            src.loop       = e.loop;
            src.spatialBlend = at.HasValue ? 1f : 0f;
            if (at.HasValue) src.transform.position = at.Value;
            src.Play();
            return src;
        }

        void BuildPools() {
            _pools[AudioCategory.Sfx]        = new SourcePool(this, AudioCategory.Sfx,        sfxVoices,        GroupOf(AudioCategory.Sfx));
            _pools[AudioCategory.Action]     = new SourcePool(this, AudioCategory.Action,     actionVoices,     GroupOf(AudioCategory.Action));
            _pools[AudioCategory.Ui]         = new SourcePool(this, AudioCategory.Ui,         uiVoices,         GroupOf(AudioCategory.Ui));
            _pools[AudioCategory.Switch]     = new SourcePool(this, AudioCategory.Switch,     switchVoices,     GroupOf(AudioCategory.Switch));
            _pools[AudioCategory.Transition] = new SourcePool(this, AudioCategory.Transition, transitionVoices, GroupOf(AudioCategory.Transition));
        }

        AudioMixerGroup GroupOf(AudioCategory c) {
            if (groupsByCategory == null || groupsByCategory.Length <= (int)c) return null;
            return groupsByCategory[(int)c];
        }

        void BuildSceneChannels() {
            _sceneA = new SceneChannel(this, "SceneA", GroupOf(AudioCategory.Scene));
            _sceneB = new SceneChannel(this, "SceneB", GroupOf(AudioCategory.Scene));
        }

        IEnumerator CrossfadeScene(AudioBank.Entry e, float fade) {
            var nxt = _aIsActive ? _sceneB : _sceneA;
            var prv = _aIsActive ? _sceneA : _sceneB;
            _aIsActive = !_aIsActive;

            float baseVol = e.volume * _categoryVolume[(int)AudioCategory.Scene];
            nxt.Play(e.clip, baseVol);

            float t = 0f;
            while (t < fade) {
                t += Time.unscaledDeltaTime;
                float u = fade > 0f ? Mathf.Clamp01(t / fade) : 1f;
                nxt.SetVolume(baseVol * u);
                prv.SetVolume(prv.InitialVolume * (1f - u));
                yield return null;
            }
            prv.Stop();
        }

        IEnumerator FadeOutScene(float fade) {
            var cur = _aIsActive ? _sceneA : _sceneB;
            float start = cur.CurrentVolume;
            float t = 0f;
            while (t < fade) {
                t += Time.unscaledDeltaTime;
                float u = fade > 0f ? Mathf.Clamp01(t / fade) : 1f;
                cur.SetVolume(start * (1f - u));
                yield return null;
            }
            cur.Stop();
        }

        // ========= 内嵌类 =========
        class SourcePool {
            readonly AudioManager _mgr;
            readonly AudioCategory _cat;
            readonly AudioSource[] _sources;
            int _cursor;

            public SourcePool(AudioManager mgr, AudioCategory cat, int count, AudioMixerGroup grp) {
                _mgr = mgr; _cat = cat;
                _sources = new AudioSource[count];
                for (int i = 0; i < count; i++) {
                    var go = new GameObject($"[{cat}]_{i}");
                    go.transform.SetParent(mgr.transform, false);
                    var s = go.AddComponent<AudioSource>();
                    s.playOnAwake = false;
                    s.outputAudioMixerGroup = grp;
                    _sources[i] = s;
                }
            }

            public AudioSource Get() {
                // 优先用空闲源;都在发时用轮转指针扣掉一个
                for (int i = 0; i < _sources.Length; i++) {
                    if (!_sources[i].isPlaying) return _sources[i];
                }
                var x = _sources[_cursor];
                _cursor = (_cursor + 1) % _sources.Length;
                x.Stop();
                return x;
            }

            public void StopAll() {
                for (int i = 0; i < _sources.Length; i++) _sources[i].Stop();
            }
        }

        class SceneChannel {
            public float InitialVolume { get; private set; }
            public float CurrentVolume => _src != null ? _src.volume : 0f;
            readonly AudioSource _src;
            public SceneChannel(AudioManager mgr, string name, AudioMixerGroup grp) {
                var go = new GameObject(name);
                go.transform.SetParent(mgr.transform, false);
                _src = go.AddComponent<AudioSource>();
                _src.playOnAwake = false;
                _src.loop = true;
                _src.outputAudioMixerGroup = grp;
            }
            public void Play(AudioClip clip, float vol) {
                _src.clip = clip;
                InitialVolume = vol;
                _src.volume = 0f;
                _src.Play();
            }
            public void SetVolume(float v) { if (_src != null) _src.volume = Mathf.Max(0f, v); }
            public void Stop() { if (_src != null) _src.Stop(); }
        }
    }
}
