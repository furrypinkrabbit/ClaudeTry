using System;
using UnityEngine;
using GuJian.Pooling;
using GuJian.Structures;

namespace GuJian.LevelFlow {
    /// <summary>
    /// 小桥:把 StructureEnemyPawn.OnRepaired 桥接到 LevelFlow 的“回池”回调。
    /// 同时作为 IPoolable,一个 pool 重复使用时如果前次没有解绑也会在 OnSpawn 重置。
    /// </summary>
    public class PooledEnemyTracker : MonoBehaviour, IPoolable {
        StructureEnemyPawn _pawn;
        Action _onDead;

        public void Bind(StructureEnemyPawn p, Action onDead) {
            Unbind();
            _pawn = p;
            _onDead = onDead;
            if (_pawn != null) _pawn.OnRepaired += HandleRepaired;
        }

        void Unbind() {
            if (_pawn != null) _pawn.OnRepaired -= HandleRepaired;
            _pawn = null; _onDead = null;
        }

        void HandleRepaired(IRepairable _) {
            var cb = _onDead;
            Unbind();
            cb?.Invoke();
        }

        public void OnSpawn()   { }
        public void OnDespawn() { Unbind(); }
    }
}
