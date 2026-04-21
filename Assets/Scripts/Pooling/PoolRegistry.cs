using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Pooling {
    /// <summary>
    /// 按 key(一般是 prefab 自身或字符串 id)托管多个 GameObjectPool。
    /// 场景级单例。InGameGameMode 进入时 Prime(...)、退出时 Clear()。
    /// </summary>
    public class PoolRegistry : MonoBehaviour {
        public static PoolRegistry Instance { get; private set; }

        readonly Dictionary<Object, GameObjectPool> _byObj = new();
        readonly Dictionary<string, GameObjectPool> _byId  = new();
        Transform _root;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            var rt = new GameObject("[Pooled]");
            rt.transform.SetParent(transform, false);
            _root = rt.transform;
        }

        void OnDestroy() { if (Instance == this) { Clear(); Instance = null; } }

        public GameObjectPool Register(GameObject prefab, int prewarm = 0) {
            if (prefab == null) return null;
            if (_byObj.TryGetValue(prefab, out var p)) return p;
            p = new GameObjectPool(prefab, _root, prewarm);
            _byObj[prefab] = p;
            return p;
        }

        public GameObjectPool Register(string id, GameObject prefab, int prewarm = 0) {
            if (prefab == null) return null;
            if (_byId.TryGetValue(id, out var p)) return p;
            p = new GameObjectPool(prefab, _root, prewarm);
            _byId[id] = p;
            return p;
        }

        public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
            => Register(prefab).Spawn(pos, rot);

        public GameObject Spawn(string id, Vector3 pos, Quaternion rot)
            => _byId.TryGetValue(id, out var p) ? p.Spawn(pos, rot) : null;

        public static void Despawn(GameObject go) => GameObjectPool.DespawnAuto(go);

        public void Clear() {
            _byObj.Clear();
            _byId.Clear();
            if (_root != null) {
                for (int i = _root.childCount - 1; i >= 0; i--)
                    Destroy(_root.GetChild(i).gameObject);
            }
        }
    }
}
