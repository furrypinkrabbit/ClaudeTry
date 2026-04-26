using UnityEngine;
using UnityEngine.UIElements;
using GuJian.Core;

namespace GuJian.UI.Screens {
    /// <summary>
    /// InGame HUD:HP、体力、手艺经验、关卡提示、伤害打字。
    /// 订阅 EventBus 事件,更新单元格样式和 label text、不重建节点。
    /// </summary>
    public class IngameScreen : UIScreen {
        Label         _stageLabel;
        Label         _craftLabel;
        VisualElement _hpFill;
        VisualElement _stFill;
        VisualElement _xpFill;
        Label         _bannerLabel;
        IVisualElementScheduledItem _bannerJob;

        protected override void OnBind(VisualElement root) {
            _stageLabel  = root.Q<Label>("label-stage");
            _craftLabel  = root.Q<Label>("label-craft");
            _hpFill      = root.Q<VisualElement>("hp-fill");
            _stFill      = root.Q<VisualElement>("stamina-fill");
            _xpFill      = root.Q<VisualElement>("xp-fill");
            _bannerLabel = root.Q<Label>("banner");
        }

        protected override void OnEnter() {
            EventBus.Subscribe<CraftsmanshipGainedEvent>(OnCraftGained);
            EventBus.Subscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Subscribe<RoomEnteredEvent>(OnRoomEntered);
            EventBus.Subscribe<StructureHitEvent>(OnHit);
            EventBus.Subscribe<StructureRepairedEvent>(OnRepaired);
        }

        protected override void OnExit() {
            EventBus.Unsubscribe<CraftsmanshipGainedEvent>(OnCraftGained);
            EventBus.Unsubscribe<LevelUpRequestedEvent>(OnLevelUp);
            EventBus.Unsubscribe<RoomEnteredEvent>(OnRoomEntered);
            EventBus.Unsubscribe<StructureHitEvent>(OnHit);
            EventBus.Unsubscribe<StructureRepairedEvent>(OnRepaired);
        }

        // ---- 对外设值 API(用于 PawnHealth/Combat 每帧写值,不走 EventBus 以减少 GC) ----
        public void SetHp(float cur, float max)       { SetFillRatio(_hpFill, Mathf.Clamp01(cur/Mathf.Max(1f,max))); }
        public void SetStamina(float cur, float max)  { SetFillRatio(_stFill, Mathf.Clamp01(cur/Mathf.Max(1f,max))); }
        public void SetXp(int cur, int req, int lv) {
            SetFillRatio(_xpFill, Mathf.Clamp01((float)cur/Mathf.Max(1,req)));
            if (_craftLabel != null) _craftLabel.text = $"手艺 Lv.{lv}   {cur}/{req}";
        }

        void SetFillRatio(VisualElement e, float t) {
            if (e == null) return;
            e.style.width = Length.Percent(t * 100f);
        }

        // ---- 事件 ----
        void OnCraftGained(CraftsmanshipGainedEvent _) { }
        void OnLevelUp(LevelUpRequestedEvent e) => Banner($"户哌心光 Lv.{e.newLevel}", 1.8f);
        void OnRoomEntered(RoomEnteredEvent e)  { if (_stageLabel != null) _stageLabel.text = $"第 {e.index+1} 关 · {e.roomId}"; }
        void OnHit(StructureHitEvent e) {
            if (e.isCritical) Banner("！巧妙 - 重撞", 0.6f);
        }
        void OnRepaired(StructureRepairedEvent e) { Banner("＃ 修复完成", 0.9f); }

        void Banner(string msg, float seconds) {
            if (_bannerLabel == null) return;
            _bannerLabel.text = msg;
            _bannerLabel.RemoveFromClassList("banner--hidden");
            _bannerLabel.AddToClassList("banner--show");
            _bannerJob?.Pause();
            _bannerJob = _bannerLabel.schedule.Execute(() => {
                _bannerLabel.RemoveFromClassList("banner--show");
                _bannerLabel.AddToClassList("banner--hidden");
            }).StartingIn((long)(seconds * 1000f));
        }
    }
}
