using UnityEngine;
using UnityEngine.UIElements;

namespace GuJian.UI {
    /// <summary>
    /// 一个 UI 屏的基类。
    /// 生命周期:
    ///   Initialize(uxml, uss)→ OnBind()        (仅一次,缓存 Root,绑定控件)
    ///   Enter()                → OnEnter()      (订阅事件/拉取数据/在树里显示)
    ///   Pause()                → OnPause()      (被覆盖层压住)
    ///   Resume()               → OnResume()
    ///   Exit()                 → OnExit()       (从栈里弹出,隐藏,解除订阅)
    /// </summary>
    public abstract class UIScreen {
        public string Id { get; private set; }
        public VisualElement Root { get; private set; }
        public bool IsActive { get; private set; }

        internal void Initialize(string id, VisualTreeAsset uxml, StyleSheet[] sheets) {
            Id = id;
            Root = uxml.CloneTree();
            Root.name = id;
            Root.AddToClassList("gj-screen");
            if (sheets != null) {
                for (int i = 0; i < sheets.Length; i++)
                    if (sheets[i] != null) Root.styleSheets.Add(sheets[i]);
            }
            // 默认隐藏
            Root.style.display = DisplayStyle.None;
            Root.pickingMode = PickingMode.Position;
            OnBind(Root);
        }

        internal void Enter() {
            if (IsActive) return;
            IsActive = true;
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList("gj-screen--paused");
            // 高效动画:仅切换 class,由 USS transition 接管
            Root.AddToClassList("gj-screen--active");
            OnEnter();
        }

        internal void Pause() {
            if (!IsActive) return;
            Root.AddToClassList("gj-screen--paused");
            OnPause();
        }

        internal void Resume() {
            if (!IsActive) return;
            Root.RemoveFromClassList("gj-screen--paused");
            OnResume();
        }

        internal void Exit() {
            if (!IsActive) return;
            IsActive = false;
            Root.RemoveFromClassList("gj-screen--active");
            Root.RemoveFromClassList("gj-screen--paused");
            Root.style.display = DisplayStyle.None;
            OnExit();
        }

        protected abstract void OnBind(VisualElement root);
        protected virtual void OnEnter()  { }
        protected virtual void OnPause()  { }
        protected virtual void OnResume() { }
        protected virtual void OnExit()   { }
    }
}
