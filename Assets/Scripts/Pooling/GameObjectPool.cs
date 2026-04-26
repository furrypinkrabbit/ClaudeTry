using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Pooling {
    /// <summary>
    /// 针对 GameObject/Prefab 的对象池。自动处理 SetActive + IPoolable 回调。
    /// 用法:
    ///   var pool = new GameObjectPool(prefab, parent, prewarm:8);
    ///   var go   = pool.Spawn(pos, rot);
    ///   pool.Despawn(go);                 // 或 PoolRegistry.DespawnAuto(go);
    /// </summary>
    public class GameObjectPool {
        readonly GameObject _prefab;
        readonly Transform  _parent;
        readonly Stack<GameObject> _stack = new();
        // 记录每个实例属于哪个池,Despawn 时能自动回收
        static readonly Dictionary<GameObject, GameObjectPool> s_instanceToPool = new();

        public int CountInactive => _stack.Count;

        public GameObjectPool(GameObject prefab, Transform parent = null, int prewarm = 0) {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++) {
                var go = CreateNew();
                go.SetActive(false);
                _stack.Push(go);
            }
        }

        GameObject CreateNew() {
            var go = Object.Instantiate(_prefab, _parent);
            s_instanceToPool[go] = this;
            return go;
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation) {
            GameObject go = _stack.Count > 0 ? _stack.Pop() : CreateNew();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            InvokePoolable(go, spawning: true);
            return go;
        }

        public void Despawn(GameObject go) {
            if (go == null) return;
            InvokePoolable(go, spawning: false);
            go.SetActive(false);
            if (_parent != null) go.transform.SetParent(_parent, worldPositionStays: false);
            _stack.Push(go);
        }

        /// <summary>全局查找该实例所属的池子并归还。</summary>
        public static bool DespawnAuto(GameObject go) {
            if (go == null) return false;
            if (s_instanceToPool.TryGetValue(go, out var pool)) { pool.Despawn(go); return true; }
            return false;
        }

        static readonly List<IPoolable> s_buf = new();
        static void InvokePoolable(GameObject go, bool spawning) {
            go.GetComponentsInChildren(true, s_buf);
            for (int i = 0; i < s_buf.Count; i++) {
                if (spawning) s_buf[i].OnSpawn();
                else          s_buf[i].OnDespawn();
            }
            s_buf.Clear();
        }
    }
}
