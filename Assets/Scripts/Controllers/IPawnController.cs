using GuJian.Pawns;

namespace GuJian.Controllers {
    public interface IPawnController {
        IPawn Pawn { get; }
        void Possess(IPawn pawn);
        void Unpossess();
        void Tick(float dt);
    }
}
