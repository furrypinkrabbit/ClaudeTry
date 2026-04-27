using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace GuJian.GameFlow {
    /// <summary>
    /// 开场介绍幻灯片。
    /// 挂在 Boot 场景的一个 GameObject 上，同物体挂 UIDocument（Source Asset = Intro.uxml）。
    /// 所有幻灯片数据在 Inspector 的 slides[] 里填写，点击屏幕翻页，播完后调 GameBootstrap.GoLobby()。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class IntroPresenter : MonoBehaviour {

        [System.Serializable]
        public struct SlideData {
            [Tooltip("章节编号文字，如\"第一章 · 宫墙的稳\" ")]
            public string chapter;
            public string title;
            [TextArea(3, 8)]
            public string body;
        }

        [SerializeField] SlideData[] slides = new SlideData[] {
            new() {
                chapter = "序  ·  古建守护",
                title   = "混沌浊气",
                body    = "故宫的每一块砖、每一根梁、每一片瓦，\n都承载着古代工匠数百年的智慧。\n\n而今，一股混沌浊气悄然侵入，\n让砖块脱位、立柱松动、屋瓦四散……\n\n你是最后一位守护者。",
            },
            new() {
                chapter = "第一章  ·  宫墙的「稳」",
                title   = "磨砖对缝",
                body    = "故宫宫墙以磨砖对缝技术砌成，\n每一块砖严丝合缝，靠精准咬合稳稳矗立百年。\n\n浊气让砖块脱离墙体，四处乱撞。\n\n拿起木槌，敲回叛逆的砖块，\n重现工匠的精密技艺。",
            },
            new() {
                chapter = "第二章  ·  立柱的「力」",
                title   = "榫卯结构",
                body    = "宫殿的木柱与梁架靠榫卯连接，\n不用一颗钉子，凹凸咬合分散重量。\n\n浊气侵蚀了木构，立柱脱离榫卯，\n大殿随时可能倒塌。\n\n敲正这些立柱，把它们重新卡回榫卯里。",
            },
            new() {
                chapter = "第三章  ·  屋顶的「坡」",
                title   = "庑殿排水",
                body    = "故宫屋顶带优美的坡度与瓦当、滴水，\n靠坡度让雨水快速排走，保护木结构不受潮。\n\n浊气让瓦当筒瓦脱离屋顶，四处弹跳，\n屋顶漏雨、结构松动。\n\n击落这些瓦件，修复排水系统。",
            },
            new() {
                chapter = "BOSS关  ·  太和殿的「魂」",
                title   = "斗拱巨影",
                body    = "三关的浊气汇聚，让太和殿的核心斗拱完全错乱。\n\n斗拱是古代建筑的承重枢纽，\n靠层层叠加的木构件将屋顶重量传递到立柱，还能抗震。\n\n用木槌敲正每一层构件，让斗拱重新咬合，\n驱散浊气，让太和殿重归稳固。",
            },
        };

        // UI 元素引用
        VisualElement _root;
        Label _chapter, _title, _body, _hint;
        VisualElement _dotContainer;

        int  _current = 0;
        bool _transitioning = false;
        bool _finished = false;

        void OnEnable() {
            var doc = GetComponent<UIDocument>();
            _root        = doc.rootVisualElement.Q("intro-root");
            _chapter     = _root.Q<Label>("label-chapter");
            _title       = _root.Q<Label>("label-title");
            _body        = _root.Q<Label>("label-body");
            _hint        = _root.Q<Label>("label-hint");
            _dotContainer= _root.Q("dot-container");

            BuildDots();
            ShowSlide(_current, instant: true);

            // 注册点击/按键
            _root.RegisterCallback<ClickEvent>(_ => Advance());
            _root.RegisterCallback<KeyDownEvent>(e => {
                if (e.keyCode is KeyCode.Space or KeyCode.Return or KeyCode.Mouse0)
                    Advance();
            });
            _root.focusable = true;
            _root.Focus();

            // 淡入根容器
            _root.schedule.Execute(() => _root.AddToClassList("intro-root--visible"));
        }

        void BuildDots() {
            _dotContainer.Clear();
            for (int i = 0; i < slides.Length; i++) {
                var dot = new VisualElement();
                dot.AddToClassList("intro-dot");
                if (i == 0) dot.AddToClassList("intro-dot--active");
                _dotContainer.Add(dot);
            }
        }

        void UpdateDots(int index) {
            var dots = _dotContainer.Children();
            int i = 0;
            foreach (var dot in dots) {
                if (i == index) dot.AddToClassList("intro-dot--active");
                else            dot.RemoveFromClassList("intro-dot--active");
                i++;
            }
        }

        void ShowSlide(int index, bool instant = false) {
            var s = slides[index];
            _chapter.text = s.chapter.ToUpper();
            _title.text   = s.title;
            _body.text    = s.body;

            bool isLast = index == slides.Length - 1;
            _hint.text = isLast ? "点击开始游戏" : "点击任意处继续";

            UpdateDots(index);

            if (instant) {
                _title.AddToClassList("intro-title--in");
                _body.AddToClassList("intro-body--in");
                return;
            }

            // 入场动画：先清除，下一帧重新加
            _title.RemoveFromClassList("intro-title--in");
            _body.RemoveFromClassList("intro-body--in");
            _title.schedule.Execute(() => _title.AddToClassList("intro-title--in")).ExecuteLater(30);
            _body.schedule.Execute(()  => _body.AddToClassList("intro-body--in")).ExecuteLater(30);
        }

        void Advance() {
            if (_transitioning || _finished) return;

            if (_current < slides.Length - 1) {
                _current++;
                _transitioning = true;
                // 淡出再淡入
                _root.RemoveFromClassList("intro-root--visible");
                _root.schedule.Execute(() => {
                    ShowSlide(_current);
                    _root.AddToClassList("intro-root--visible");
                    _transitioning = false;
                }).ExecuteLater(400);
            } else {
                // 最后一页 → 淡出 → 进大厅
                _finished = true;
                _root.RemoveFromClassList("intro-root--visible");
                _root.schedule.Execute(() => {
                    gameObject.SetActive(false);
                    GameBootstrap.Instance?.GoLobby();
                }).ExecuteLater(650);
            }
        }
    }
}