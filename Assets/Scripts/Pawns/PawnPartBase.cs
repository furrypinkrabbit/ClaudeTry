using UnityEngine;

namespace GuJian.Pawns {
    /// <summary>
    /// 任何挂在 Pawn GameObject 下的行为部件都继承此类。
    /// 部件声明自己关心的意图种类，PawnBase 分发时过滤。
    /// </summary>
    public abstract class PawnPartBase : MonoBehaviour {
        protected PawnBase Pawn { get; private set; }

        internal void Bind(PawnBase pawn) {
            Pawn = pawn;
            OnBind();
        }

        protected virtual void OnBind() { }

        public abstract bool HandlesIntent(PawnIntentKind kind);
        public abstract void HandleIntent(in PawnIntent intent);
    }
}
