using UnityEngine;

namespace GuJian.Input {
    /// <summary>
    /// 半原始输入事件。不暴露 Unity InputAction.CallbackContext。
    /// 控制器订阅这个事件后自己翻译为语义 PawnIntent。
    /// </summary>
    public enum PawnInputKind {
        MoveAxis,
        LookAxis,
        AttackPrimary_Down, AttackPrimary_Up,
        AttackHeavy_Down,   AttackHeavy_Up,
        Dodge_Down,
        Interact_Down,
        UseTool_Down,
        SwitchTool_Next, SwitchTool_Prev, SwitchTool_Slot,
        Charge_Down, Charge_Up,
    }

    public readonly struct PawnInputEvent {
        public readonly PawnInputKind Kind;
        public readonly Vector2 Vector;
        public readonly float   Scalar;
        public readonly int     IntPayload;

        public PawnInputEvent(PawnInputKind k, Vector2 v = default, float s = 0f, int i = 0) {
            Kind = k; Vector = v; Scalar = s; IntPayload = i;
        }
    }
}
