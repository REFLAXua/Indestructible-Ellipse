using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class ServiceLocator
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Attempting to register null service for type {typeof(T).Name}!");
                return;
            }

            var type = typeof(T);
            lock (_lock)
            {
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Overwriting.");
                    _services[type] = service;
                }
                else
                {
                    _services.Add(type, service);
                }
            }
        }

        public static void Unregister<T>()
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_services.ContainsKey(type))
                {
                    _services.Remove(type);
                }
            }
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_services.TryGetValue(type, out var service))
                {
                    return (T)service;
                }
            }

            throw new InvalidOperationException($"[ServiceLocator] Service {type.Name} not registered!");
        }

        public static bool TryGet<T>(out T service)
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_services.TryGetValue(type, out var obj))
                {
                    service = (T)obj;
                    return true;
                }
            }

            service = default;
            return false;
        }

        public static void ClearAll()
        {
            lock (_lock)
            {
                _services.Clear();
            }
            Debug.Log("[ServiceLocator] All services cleared.");
        }

        public static bool HasService<T>()
        {
            var type = typeof(T);
            lock (_lock)
            {
                return _services.ContainsKey(type);
            }
        }
    }
}
