using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Audio {
    /// <summary>
    /// 一份音频库。以“包”为单位提供给 AudioManager。
    /// 生成流程:编辑器里新建 → 把 MP3 拖进 entries。 
    /// 运行时调用 AudioManager.PlayById("sfx_hammer_hit") 即可。
    /// </summary>
    [CreateAssetMenu(menuName = "GuJian/Audio/AudioBank", fileName = "AudioBank")]
    public class AudioBank : ScriptableObject {
        [Serializable] public class Entry {
            public string        id;             // 全局唯一,如 "sfx_hammer_hit"
            public AudioCategory category;
            public AudioClip     clip;           // mp3 导入后 = AudioClip
            [Range(0,2)] public float volume = 1f;
            [Range(-3,3)] public float pitchMin = 1f;
            [Range(-3,3)] public float pitchMax = 1f;
            public bool loop;                    // 一般仅 Scene/Transition 用
            [Tooltip("单位:秒。同 id 在此期间内不重复触发,防止音源堆积尖刺。")]
            public float retriggerCooldown = 0.02f;
        }

        public Entry[] entries;

        // 运行时索引(在 AudioManager 注册时 构造)
        [NonSerialized] Dictionary<string, Entry> _byId;
        public Entry Find(string id) {
            if (_byId == null) BuildIndex();
            return _byId.TryGetValue(id, out var e) ? e : null;
        }
        void BuildIndex() {
            _byId = new Dictionary<string, Entry>(entries?.Length ?? 0);
            if (entries == null) return;
            foreach (var e in entries) if (e != null && !string.IsNullOrEmpty(e.id)) _byId[e.id] = e;
        }
    }
}
