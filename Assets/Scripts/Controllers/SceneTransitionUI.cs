using System;
 using System.Collections;
 using UnityEngine;
 using UnityEngine.UIElements;

 namespace GuJian.GameFlow {
     /// <summary>
     /// 全屏转场遮罩。DontDestroyOnLoad，挂在 Boot 场景的 [Transition] GO 上。
     /// UIDocument Sort Order 设最高（999）保证盖在所有 UI 上面。
     /// 用法：SceneTransitionUI.Instance.Transition(loadAction, label)
     /// </summary>
     [RequireComponent(typeof(UIDocument))]
     public class SceneTransitionUI : MonoBehaviour {
         public static SceneTransitionUI Instance { get; private set; }

         [SerializeField] float fadeOut = 0.4f;   // 遮黑时间
         [SerializeField] float holdMin = 0.5f;   // 最短遮黑时间（给场景加载兜底）
         [SerializeField] float fadeIn  = 0.4f;   // 揭开时间

         VisualElement _bg;
         Label         _label;
         bool          _busy;

         void Awake() {
             if (Instance != null && Instance != this) { Destroy(gameObject); return; }
             Instance = this;
             DontDestroyOnLoad(gameObject);

             var root = GetComponent<UIDocument>().rootVisualElement;
             _bg    = root.Q("transition-bg");
             _label = root.Q<Label>("transition-label");
         }

         /// <summary>
         /// fade out → 执行 loadAction（异步）→ 至少等 holdMin → fade in
         /// </summary>
         public void Transition(Action loadAction, string label = "") {
             if (_busy) return;
             StartCoroutine(Run(loadAction, label));
         }

         IEnumerator Run(Action loadAction, string label) {
             _busy = true;

             // 1. Fade out
             SetLabel(label);
             _bg.AddToClassList("transition-bg--visible");
             yield return new WaitForSeconds(fadeOut);
             if (!string.IsNullOrEmpty(label))
                 _label.AddToClassList("transition-label--visible");

             // 2. 触发加载 + 计时
             float t0 = Time.realtimeSinceStartup;
             loadAction?.Invoke();

             // 等场景切换完成（AsyncOperation 由外部 loadAction 发起，这里靠 holdMin 兜底）
             yield return new WaitUntil(() =>
                 Time.realtimeSinceStartup - t0 >= holdMin);

             // 3. Fade in
             _label.RemoveFromClassList("transition-label--visible");
             _bg.RemoveFromClassList("transition-bg--visible");
             yield return new WaitForSeconds(fadeIn);

             _busy = false;
         }

         void SetLabel(string text) {
             _label.text = text;
             _label.RemoveFromClassList("transition-label--visible");
         }
     }
 }