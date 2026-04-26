using UnityEngine;

namespace GuJian.Pawns {
    /// <summary>
    /// 可被控制器驱动的目标。玩家角色、敌人、NPC 都实现这个接口。
    /// </summary>
    public interface IPawn {
        Transform Transform { get; }
        bool IsAlive { get; }

        /// <summary>接收语义意图。</summary>
        void ReceiveIntent(in PawnIntent intent);

        /// <summary>查询挂在 Pawn 上的部件。</summary>
        T GetPart<T>() where T : class;
    }
}
