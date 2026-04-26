using GuJian.Pawns.Parts;
using UnityEngine;

namespace GuJian.Pawns {
    /// <summary>
    /// 玩家角色。约定：必须挂 Movement + Combat + ToolSlot + Health。
    /// </summary>
    [RequireComponent(typeof(PawnMovement))]
    [RequireComponent(typeof(PawnToolSlot))]
    [RequireComponent(typeof(PawnCombat))]
    [RequireComponent(typeof(PawnHealth))]
    public class PlayerPawn : PawnBase {
        protected override void OnKilled() {
            // 死亡：广播事件、关闭操控（RunContext 负责 Run 结算）
            GetPart<PawnMovement>()?.enabled.Equals(false);
            GuJian.Core.EventBus.Publish(new GuJian.Core.RunFinishedEvent(false, 0, 0));
        }
    }
}
