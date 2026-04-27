using System;
using UnityEngine;

namespace GuJian.Pawns.Parts {
    /// <summary>
    /// 工具槽位。玩家持有 N 把工具（第一个版本通常1：锤子）。
    /// 接受 SwitchTool / UseTool 意图。
    /// 与具体工具解耦：仅持有 Tools.ITool 接口。
    /// </summary>
    public class PawnToolSlot : PawnPartBase {
        [Header("已绑定到骨骼的工具（直接场景拖入，不会被 reparent）")]
        [SerializeField] Tools.ToolBase[] skeletonBoundTools;
        [SerializeField] Transform toolAnchor;
        [SerializeField] int maxSlots = 2;

        private readonly Tools.ITool[] _slots = new Tools.ITool[4];
        private int _activeIdx = 0;

        public Tools.ITool Current => _slots[_activeIdx];
        public Transform Anchor => toolAnchor ? toolAnchor : transform;
        public event Action<Tools.ITool> OnToolChanged;

        protected override void OnBind() {
            // 把骨骼绑定的工具自动注册到槽位，不做任何 transform 操作
            for (int i = 0; i < skeletonBoundTools.Length && i < maxSlots; i++) {
                if (skeletonBoundTools[i] == null) continue;
                _slots[i] = skeletonBoundTools[i];
                skeletonBoundTools[i].OnEquip(Pawn);
            }
        }
        
        public void EquipToSlot(int idx, Tools.ITool tool) {
            if (idx < 0 || idx >= maxSlots) return;
            _slots[idx]?.OnUnequip();
            _slots[idx] = tool;
            tool?.OnEquip(Pawn);
            if (idx == _activeIdx) OnToolChanged?.Invoke(tool);
        }

        public override bool HandlesIntent(PawnIntentKind k) =>
            k == PawnIntentKind.SwitchTool || k == PawnIntentKind.UseTool;

        public override void HandleIntent(in PawnIntent intent) {
            switch (intent.Kind) {
                case PawnIntentKind.SwitchTool:
                    var n = Mathf.Clamp(intent.IntPayload, 0, maxSlots - 1);
                    if (n != _activeIdx && _slots[n] != null) {
                        _activeIdx = n;
                        OnToolChanged?.Invoke(_slots[_activeIdx]);
                    }
                    break;
                case PawnIntentKind.UseTool:
                    Current?.Trigger(Tools.ToolActionType.Special, default);
                    break;
            }
        }

        void Update() {
            for (int i = 0; i < _slots.Length; i++) _slots[i]?.Tick(Time.deltaTime);
        }
    }
}
