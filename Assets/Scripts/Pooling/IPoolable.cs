using UnityEngine;

namespace GuJian.Pooling {
    /// <summary>
    /// 任何可被对象池复用的 MonoBehaviour 实现此接口。
    /// 不要在 Awake 里做资源释放——放到 OnDespawn;初始化逻辑放到 OnSpawn。
    /// </summary>
    public interface IPoolable {
        /// <summary>从池子里取出、即将启用前调用。</summary>
        void OnSpawn();
        /// <summary>归还到池子、即将禁用前调用。</summary>
        void OnDespawn();
    }
}
