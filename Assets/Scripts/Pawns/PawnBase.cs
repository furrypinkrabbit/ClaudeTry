using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Pawns {
    /// <summary>
    /// Pawn 基类。管理部件注册与意图分发。
    /// 具体行为全部由 PawnPartBase 子类实现，本类不应直接读写速度/血量。
    /// </summary>
    public abstract class PawnBase : MonoBehaviour, IPawn {
        private readonly List<PawnPartBase> _parts = new();
        // 类型缓存，加速 GetPart
        private readonly Dictionary<System.Type, PawnPartBase> _cache = new();

        public Transform Transform => transform;
        public virtual bool IsAlive { get; protected set; } = true;

        protected virtual void Awake() {
            GetComponentsInChildren(true, _parts);
            foreach (var p in _parts) {
                p.Bind(this);
                _cache[p.GetType()] = p;
            }
        }

        public virtual void ReceiveIntent(in PawnIntent intent) {
            if (!IsAlive) return;
            // 分发给所有声明感兴趣该意图的部件
            for (int i = 0; i < _parts.Count; i++) {
                var p = _parts[i];
                if (p.HandlesIntent(intent.Kind)) p.HandleIntent(intent);
            }
        }

        public T GetPart<T>() where T : class {
            if (_cache.TryGetValue(typeof(T), out var p) && p is T hit) return hit;
            // 遍历兼容父类/接口
            for (int i = 0; i < _parts.Count; i++) if (_parts[i] is T t) return t;
            return null;
        }

        public void Kill() {
            if (!IsAlive) return;
            IsAlive = false;
            OnKilled();
        }

        protected virtual void OnKilled() { }
    }
}
