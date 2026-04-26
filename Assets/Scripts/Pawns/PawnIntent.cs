using UnityEngine;

namespace GuJian.Pawns {
    /// <summary>
    /// 语义化意图。控制器把按键/AI 决策翻译为意图，Pawn 只认这个。
    /// </summary>
    public enum PawnIntentKind {
        None = 0,
        MoveAxis,       // Vector2 = 目标移动方向（[-1,1]^2）
        LookAxis,       // Vector2 = 目标朝向（世界坐标 XZ）
        AttackPrimary,  //
        AttackHeavy,
        ChargeStart,
        ChargeRelease,  // Scalar = 累积秒数
        Dodge,          // Vector2 = 闪避方向
        Interact,
        UseTool,
        SwitchTool,     // IntPayload = slot
    }

    /// <summary>
    /// 以 readonly struct 传递，避免 GC。
    /// </summary>
    public readonly struct PawnIntent {
        public readonly PawnIntentKind Kind;
        public readonly Vector2 Vector;
        public readonly float   Scalar;
        public readonly int     IntPayload;

        public PawnIntent(PawnIntentKind k, Vector2 v = default, float s = 0f, int i = 0) {
            Kind = k; Vector = v; Scalar = s; IntPayload = i;
        }

        public static PawnIntent Move(Vector2 axis)    => new(PawnIntentKind.MoveAxis,      axis);
        public static PawnIntent Look(Vector2 axis)    => new(PawnIntentKind.LookAxis,      axis);
        public static PawnIntent Primary()              => new(PawnIntentKind.AttackPrimary);
        public static PawnIntent Heavy()                => new(PawnIntentKind.AttackHeavy);
        public static PawnIntent ChargeStart()          => new(PawnIntentKind.ChargeStart);
        public static PawnIntent ChargeRelease(float t) => new(PawnIntentKind.ChargeRelease, default, t);
        public static PawnIntent Dodge(Vector2 dir)    => new(PawnIntentKind.Dodge,         dir);
        public static PawnIntent Interact()              => new(PawnIntentKind.Interact);
        public static PawnIntent UseTool()               => new(PawnIntentKind.UseTool);
        public static PawnIntent SwitchTool(int slot)    => new(PawnIntentKind.SwitchTool,   default, 0f, slot);
    }
}
