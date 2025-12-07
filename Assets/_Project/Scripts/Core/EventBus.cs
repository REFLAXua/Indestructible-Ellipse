using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class EventBus
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, List<Delegate>> _events = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> listener)
        {
            if (listener == null)
            {
                Debug.LogWarning("[EventBus] Attempting to subscribe null listener!");
                return;
            }

            var type = typeof(T);
            lock (_lock)
            {
                if (!_events.ContainsKey(type))
                {
                    _events[type] = new List<Delegate>();
                }
                _events[type].Add(listener);
            }
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (listener == null) return;

            var type = typeof(T);
            lock (_lock)
            {
                if (_events.ContainsKey(type))
                {
                    _events[type].Remove(listener);
                }
            }
        }

        public static void Publish<T>(T eventMessage)
        {
            var type = typeof(T);
            List<Delegate> listenersCopy;

            lock (_lock)
            {
                if (!_events.TryGetValue(type, out var listeners) || listeners.Count == 0)
                {
                    return;
                }
                listenersCopy = new List<Delegate>(listeners);
            }

            for (int i = listenersCopy.Count - 1; i >= 0; i--)
            {
                try
                {
                    (listenersCopy[i] as Action<T>)?.Invoke(eventMessage);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Exception in event handler for {type.Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public static void ClearAll()
        {
            lock (_lock)
            {
                _events.Clear();
            }
            Debug.Log("[EventBus] All subscriptions cleared.");
        }

        public static void ClearSubscriptions<T>()
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_events.ContainsKey(type))
                {
                    _events[type].Clear();
                }
            }
        }

        public static int GetSubscriberCount<T>()
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_events.TryGetValue(type, out var listeners))
                {
                    return listeners.Count;
                }
            }
            return 0;
        }
    }
}
