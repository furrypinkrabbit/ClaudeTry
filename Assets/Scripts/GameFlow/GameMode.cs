using UnityEngine;

namespace GuJian.GameFlow {
    public abstract class GameModeBase : MonoBehaviour {
        public abstract void Enter();
        public abstract void Exit();
    }
}
