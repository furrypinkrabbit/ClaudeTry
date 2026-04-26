using UnityEngine;

namespace GuJian.Pawns.Parts {
    public interface IInteractable {
        void Interact(PawnBase user);
    }

    public class PawnInteractor : PawnPartBase {
        public Animator animator;
        [SerializeField] float radius = 1.5f;
        [SerializeField] LayerMask layer = ~0;
        readonly Collider[] _buf = new Collider[8];

        public override bool HandlesIntent(PawnIntentKind k) => k == PawnIntentKind.Interact;
        public override void HandleIntent(in PawnIntent intent) {
            int n = Physics.OverlapSphereNonAlloc(transform.position, radius, _buf, layer);
            for (int i = 0; i < n; i++) {
                if (_buf[i].TryGetComponent<IInteractable>(out var it))
                {
                    it.Interact(Pawn); 
                    animator.SetTrigger("Gather");
                    return;
                }
            }
        }
    }
}
