using System;
using System.Collections.Generic;

namespace GuJian.Core {
    /// <summary>
    /// 轻量事件总线。事件结构体通过 Publish&lt;T&gt; 广播。
    /// 比如：EventBus.Publish(new StructureRepairedEvent(...))。
    /// </summary>
    public static class EventBus {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct {
            if (_handlers.TryGetValue(typeof(T), out var d))
                _handlers[typeof(T)] = Delegate.Combine(d, handler);
            else
                _handlers[typeof(T)] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct {
            if (_handlers.TryGetValue(typeof(T), out var d)) {
                var nd = Delegate.Remove(d, handler);
                if (nd == null) _handlers.Remove(typeof(T));
                else _handlers[typeof(T)] = nd;
            }
        }

        public static void Publish<T>(in T evt) where T : struct {
            if (_handlers.TryGetValue(typeof(T), out var d))
                ((Action<T>)d).Invoke(evt);
        }

        public static void Clear() => _handlers.Clear();
    }
}
