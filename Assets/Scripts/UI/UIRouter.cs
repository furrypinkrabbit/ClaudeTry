using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GuJian.Audio;

namespace GuJian.UI {
    /// <summary>
    /// UI 路由器。基于栈:Push/Pop/Replace;所有 Screen 共享一个 UIDocument。
    /// 高效切换:所有 Screen 预先 CloneTree 挂载在根,仅通过 display=None/Flex
    /// 和 class 切换可见性——不做 VisualElement 重建,不做 GC。
    /// </summary>
    public class UIRouter : MonoBehaviour {
        [Serializable]
        public class ScreenEntry {
            public string id;
            public VisualTreeAsset uxml;
            public StyleSheet[] stylesheets;
            [Tooltip("实现 UIScreen 的 类名(带命名空间)。用于运行时 Activator.CreateInstance。")]
            public string screenTypeFullName;
        }

        public static UIRouter Instance { get; private set; }

        [SerializeField] UIDocument document;
        [SerializeField] ScreenEntry[] screens;
        [Header("音频(可选)")]
        [SerializeField] string clickSfxId   = "ui_click";
        [SerializeField] string pushSfxId    = "switch_push";
        [SerializeField] string popSfxId     = "switch_pop";
        [SerializeField] string replaceSfxId = "switch_replace";

        readonly Dictionary<string, UIScreen> _byId = new();
        readonly Stack<string> _stack = new();

        public int Depth => _stack.Count;
        public string Top => _stack.Count > 0 ? _stack.Peek() : null;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (document == null) document = GetComponent<UIDocument>();
            EnsureHost();
            LoadAll();
        }

        VisualElement Host {
            get {
                EnsureHost();
                return document.rootVisualElement;
            }
        }

        void EnsureHost() {
            if (document == null) Debug.LogError("[UIRouter] 缺 UIDocument");
        }

        void LoadAll() {
            if (screens == null) return;
            for (int i = 0; i < screens.Length; i++) {
                var e = screens[i];
                if (e == null || e.uxml == null || string.IsNullOrEmpty(e.screenTypeFullName)) continue;
                var t = Type.GetType(e.screenTypeFullName);
                if (t == null) { Debug.LogError($"[UIRouter] 未找到类型: {e.screenTypeFullName}"); continue; }
                var inst = Activator.CreateInstance(t) as UIScreen;
                if (inst == null) { Debug.LogError($"[UIRouter] {e.screenTypeFullName} 不是 UIScreen"); continue; }
                inst.Initialize(e.id, e.uxml, e.stylesheets);
                Host.Add(inst.Root);
                _byId[e.id] = inst;
            }
        }

        // ========== 路由控制 ==========
        public void Push(string id) {
            if (!_byId.TryGetValue(id, out var next)) {
                Debug.LogWarning($"[UIRouter] 无此屏: {id}"); return;
            }
            if (_stack.Count > 0) _byId[_stack.Peek()].Pause();
            _stack.Push(id);
            next.Enter();
            PlaySfx(pushSfxId);
        }

        public void Pop() {
            if (_stack.Count == 0) return;
            var topId = _stack.Pop();
            _byId[topId].Exit();
            if (_stack.Count > 0) _byId[_stack.Peek()].Resume();
            PlaySfx(popSfxId);
        }

        /// <summary>清栈,跳到 id——常用于大层级切换(大厅 ↔ InGame)。</summary>
        public void Replace(string id) {
            while (_stack.Count > 0) {
                var x = _stack.Pop();
                _byId[x].Exit();
            }
            Push(id);
            PlaySfx(replaceSfxId);
        }

        public void PopToRoot() {
            while (_stack.Count > 1) {
                var x = _stack.Pop();
                _byId[x].Exit();
            }
            PlaySfx(popSfxId);
        }

        // ========== 通用声音 ==========
        public void PlayClickSfx() => PlaySfx(clickSfxId);
        void PlaySfx(string id) {
            if (string.IsNullOrEmpty(id)) return;
            AudioManager.Instance?.Play(id);
        }

        // ========== 对外 getter ==========
        public T Get<T>(string id) where T : UIScreen => _byId.TryGetValue(id, out var s) ? s as T : null;
    }
}
