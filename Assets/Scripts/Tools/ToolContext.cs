using UnityEngine;

namespace GuJian.Tools {
    /// <summary>
    /// 工具被 Trigger 时的上下文。PawnCombat 填充后传给工具。
    /// </summary>
    public struct ToolContext {
        public float DamageMul;
        public float CritChanceAdd;
        public float CritDamageAdd;
        public float ChargeSeconds;
        public Vector3 Origin;
        public Vector3 Facing;
    }
}
