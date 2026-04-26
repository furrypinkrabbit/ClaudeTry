using System;
using System.Collections.Generic;

namespace GuJian.Pooling {
    /// <summary>
    /// 通用对象池。不挂 Unity——是纯 C#,适用于 struct / class。
    /// MonoBehaviour 实例请使用 <see cref="GameObjectPool"/>。
    /// </summary>
    public class ObjectPool<T> where T : class {
        readonly Stack<T>  _stack;
        readonly Func<T>   _factory;
        readonly Action<T> _onGet;
        readonly Action<T> _onRelease;
        readonly int       _hardCap;

        public int CountInactive => _stack.Count;

        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null,
                          int prewarm = 0, int hardCap = 256) {
            _factory   = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet     = onGet;
            _onRelease = onRelease;
            _hardCap   = hardCap;
            _stack     = new Stack<T>(Math.Max(prewarm, 4));
            for (int i = 0; i < prewarm; i++) _stack.Push(_factory());
        }

        public T Get() {
            var item = _stack.Count > 0 ? _stack.Pop() : _factory();
            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item) {
            if (item == null) return;
            _onRelease?.Invoke(item);
            if (_stack.Count < _hardCap) _stack.Push(item);
            // 超出硬顶的就让 GC 回收
        }

        public void Clear() => _stack.Clear();
    }
}
