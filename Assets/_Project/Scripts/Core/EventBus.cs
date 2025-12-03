using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// A generic Event Bus for decoupled communication between systems.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _events = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribes a listener to an event type.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_events.ContainsKey(type))
            {
                _events[type] = new List<Delegate>();
            }
            _events[type].Add(listener);
        }

        /// <summary>
        /// Unsubscribes a listener from an event type.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_events.ContainsKey(type))
            {
                _events[type].Remove(listener);
            }
        }

        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        public static void Publish<T>(T eventMessage)
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var listeners))
            {
                // Iterate backwards to allow unsubscription during event handling
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    (listeners[i] as Action<T>)?.Invoke(eventMessage);
                }
            }
        }
    }
}
