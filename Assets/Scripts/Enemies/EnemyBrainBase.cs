using System;
using UnityEngine;
using GuJian.Controllers;
using GuJian.Pawns;
using GuJian.Pooling;

namespace GuJian.Enemies {
    /// <summary>
    /// 所有关卡 Brain 的公用基类:
    /// - 缓存 player 引用,避开每帧 Find
    /// - 提供常用工具(距离/方向/更新 look)
    /// - 实现 IPoolable,池化时重置状态
    /// 具体行为由子类 override <see cref="OnThink"/>。
    /// </summary>
    public abstract class EnemyBrainBase : MonoBehaviour, AIPawnController.IAIBrain, IPoolable {
        [Header("通用")]
        [SerializeField] protected float aggroRange = 14f;
        [SerializeField] protected string playerTag = "Player";

        Transform _playerCached;

        public Transform Player {
            get {
                if (_playerCached != null) return _playerCached;
                var go = GameObject.FindWithTag(playerTag);
                return _playerCached = (go != null ? go.transform : null);
            }
        }

        public void Think(IPawn self, float dt, Action<PawnIntent> emit) {
            if (self == null || !self.IsAlive) return;
            var p = Player;
            if (p == null) { emit(PawnIntent.Move(Vector2.zero)); return; }
            Vector3 to = p.position - self.Transform.position;
            float dist = new Vector2(to.x, to.z).magnitude;
            if (dist > aggroRange) { emit(PawnIntent.Move(Vector2.zero)); return; }
            OnThink(self, p, to, dist, dt, emit);
        }

        protected abstract void OnThink(IPawn self, Transform player, Vector3 toPlayer,
                                        float dist, float dt, Action<PawnIntent> emit);

        public virtual void OnSpawn() { _playerCached = null; }
        public virtual void OnDespawn() { }
    }
}
