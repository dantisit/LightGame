using System;
using System.Collections.Generic;

namespace Light_and_controller.Scripts.Systems
{
    /// <summary>
    /// Global event bus system for decoupled communication between components
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _eventSubscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe to an event of type T
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler callback</param>
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);

            if (!_eventSubscribers.ContainsKey(eventType))
            {
                _eventSubscribers[eventType] = new List<Delegate>();
            }

            _eventSubscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event of type T
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler callback to remove</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);

            if (_eventSubscribers.ContainsKey(eventType))
            {
                _eventSubscribers[eventType].Remove(handler);

                // Clean up empty lists
                if (_eventSubscribers[eventType].Count == 0)
                {
                    _eventSubscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="eventData">Event data to publish</param>
        public static void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);

            if (_eventSubscribers.ContainsKey(eventType))
            {
                // Create a copy to avoid issues if subscribers modify the list
                List<Delegate> subscribers = new List<Delegate>(_eventSubscribers[eventType]);

                foreach (Delegate subscriber in subscribers)
                {
                    try
                    {
                        (subscriber as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other subscribers
                        UnityEngine.Debug.LogError($"Error invoking event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public static void ClearAll()
        {
            _eventSubscribers.Clear();
        }

        /// <summary>
        /// Clear all subscriptions for a specific event type
        /// </summary>
        /// <typeparam name="T">Event type to clear</typeparam>
        public static void Clear<T>() where T : class
        {
            Type eventType = typeof(T);
            if (_eventSubscribers.ContainsKey(eventType))
            {
                _eventSubscribers.Remove(eventType);
            }
        }

        /// <summary>
        /// Get the number of subscribers for a specific event type
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <returns>Number of subscribers</returns>
        public static int GetSubscriberCount<T>() where T : class
        {
            Type eventType = typeof(T);
            return _eventSubscribers.ContainsKey(eventType) ? _eventSubscribers[eventType].Count : 0;
        }
    }
}