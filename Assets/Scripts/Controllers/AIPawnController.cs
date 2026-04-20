using UnityEngine;
using GuJian.Pawns;

namespace GuJian.Controllers {
    /// <summary>
    /// AI 控制器。它与玩家控制器共享完全相同的 PawnIntent 接口。
    /// 默认使用简单 FSM（Idle / Chase / Attack）；需要复杂 AI 时可接 IAIBrain。
    /// </summary>
    public class AIPawnController : PawnControllerBase {
        public interface IAIBrain {
            void Think(IPawn self, float dt, System.Action<PawnIntent> emit);
        }

        [SerializeField] MonoBehaviour brainBehaviour; // 任何实现 IAIBrain 的 MB
        [SerializeField] PawnBase autoPossess;

        IAIBrain _brain;

        void Awake() {
            _brain = brainBehaviour as IAIBrain;
        }

        void Start() {
            if (autoPossess != null) Possess(autoPossess);
        }

        public override void Tick(float dt) {
            if (Pawn == null || !Pawn.IsAlive) return;
            _brain?.Think(Pawn, dt, Emit);
        }

        void Emit(PawnIntent intent) => Pawn.ReceiveIntent(intent);
    }
}
