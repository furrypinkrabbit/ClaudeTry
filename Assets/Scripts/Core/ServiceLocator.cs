using System;
using System.Collections.Generic;

namespace GuJian.Core {
    /// <summary>
    /// 极简服务定位器。用于 Pawn/Controller/GameMode 等跨系统引用，
    /// 避免到处拉 singleton。仅在 GameBootstrap 启动时注册/清理。
    /// </summary>
    public static class ServiceLocator {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class {
            _services[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class {
            _services.Remove(typeof(T));
        }

        public static T Get<T>() where T : class {
            return _services.TryGetValue(typeof(T), out var s) ? (T)s : null;
        }

        public static bool TryGet<T>(out T service) where T : class {
            if (_services.TryGetValue(typeof(T), out var s)) { service = (T)s; return true; }
            service = null; return false;
        }

        public static void Clear() => _services.Clear();
    }
}
