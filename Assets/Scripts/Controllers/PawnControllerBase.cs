using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Controllers {
    public abstract class PawnControllerBase : MonoBehaviour, IPawnController {
        public IPawn Pawn { get; protected set; }

        public virtual void Possess(IPawn pawn) {
            Unpossess();
            Pawn = pawn;
            OnPossessed();
        }

        public virtual void Unpossess() {
            if (Pawn == null) return;
            OnUnpossessed();
            Pawn = null;
        }

        protected virtual void OnPossessed() { }
        protected virtual void OnUnpossessed() { }

        public virtual void Tick(float dt) { }

        protected virtual void Update() { Tick(Time.deltaTime); }
    }
}
