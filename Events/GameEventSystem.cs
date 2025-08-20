using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using IsometricActionGame.UI;
using IsometricActionGame.Quests;
using IsometricActionGame.Graphics;

namespace IsometricActionGame.Events
{
    /// <summary>
    /// Universal Event System that integrates with existing C# events
    /// Provides centralized event management while maintaining backward compatibility
    /// </summary>
    public class GameEventSystem : IDisposable
    {
        private static GameEventSystem _instance;
        public static GameEventSystem Instance => _instance ??= new GameEventSystem();
        
        private readonly Dictionary<string, List<Delegate>> _eventHandlers;
        private readonly Queue<EventData> _eventQueue;
        private readonly Dictionary<string, int> _eventStatistics;
        private bool _disposed = false;
        
        private GameEventSystem()
        {
            _eventHandlers = new Dictionary<string, List<Delegate>>();
            _eventQueue = new Queue<EventData>();
            _eventStatistics = new Dictionary<string, int>();
        }
        
        #region Universal Event Management
        
        /// <summary>
        /// Subscribe to any event by name
        /// </summary>
        public void Subscribe<T>(string eventName, Action<T> handler)
        {
            if (_disposed || string.IsNullOrEmpty(eventName) || handler == null) return;
            
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<Delegate>();
            }
            
            _eventHandlers[eventName].Add(handler);
        }
        
        /// <summary>
        /// Subscribe with no parameters
        /// </summary>
        public void Subscribe(string eventName, Action handler)
        {
            Subscribe<object>(eventName, _ => handler?.Invoke());
        }
        
        /// <summary>
        /// Unsubscribe from event
        /// </summary>
        public void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            if (_disposed || !_eventHandlers.ContainsKey(eventName)) return;
            
            _eventHandlers[eventName].Remove(handler);
            if (_eventHandlers[eventName].Count == 0)
            {
                _eventHandlers.Remove(eventName);
            }
        }
        
        /// <summary>
        /// Publish event immediately
        /// </summary>
        public void Publish<T>(string eventName, T data = default)
        {
            if (_disposed || string.IsNullOrEmpty(eventName)) return;
            
            // Debug logging for GAME_RESTART events
            if (eventName == GameEvents.GAME_RESTART)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Publishing GAME_RESTART event");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {System.Environment.StackTrace}");
            }
            
            UpdateStatistics(eventName);
            
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                foreach (var handler in handlers.ToList()) // Copy to avoid modification during iteration
                {
                    try
                    {
                        if (handler is Action<T> typedHandler)
                        {
                            typedHandler.Invoke(data);
                        }
                        else if (handler is Action simpleHandler && data == null)
                        {
                            simpleHandler.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Event handler error for '{eventName}': {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Queue event for next frame processing
        /// </summary>
        public void QueueEvent<T>(string eventName, T data = default)
        {
            if (_disposed) return;
            
            _eventQueue.Enqueue(new EventData
            {
                EventName = eventName,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// Process all queued events
        /// </summary>
        public void ProcessQueuedEvents()
        {
            if (_disposed) return;
            
            while (_eventQueue.Count > 0)
            {
                var eventData = _eventQueue.Dequeue();
                PublishDynamic(eventData.EventName, eventData.Data);
            }
        }
        
        private void PublishDynamic(string eventName, object data)
        {
            if (data == null)
            {
                Publish<object>(eventName, null);
            }
            else
            {
                var publishMethod = typeof(GameEventSystem).GetMethod(nameof(Publish))
                    ?.MakeGenericMethod(data.GetType());
                publishMethod?.Invoke(this, new[] { eventName, data });
            }
        }
        
        private void UpdateStatistics(string eventName)
        {
            if (!_eventStatistics.ContainsKey(eventName))
            {
                _eventStatistics[eventName] = 0;
            }
            _eventStatistics[eventName]++;
        }
        
        #endregion
        

        
        #region Convenience Methods for Common Events
        
        public void PublishPlayerDamaged(int damage, Vector2 position)
        {
            Publish(GameEvents.PLAYER_DAMAGED, new { Damage = damage, Position = position });
        }
        
        public void PublishEnemyDefeated(string enemyType, Vector2 position)
        {
            Publish(GameEvents.ENEMY_DEFEATED, new { EnemyType = enemyType, Position = position });
        }
        
        public void PublishItemPickedUp(string itemName, int quantity)
        {
            Publish(GameEvents.ITEM_PICKED_UP, new { ItemName = itemName, Quantity = quantity });
        }
        
        public void PublishDialogueStarted(string npcName)
        {
            Publish(GameEvents.DIALOGUE_STARTED, npcName);
        }
        
        public void PublishGamePaused(string reason)
        {
            Publish(GameEvents.GAME_PAUSED, reason);
        }
        
        #endregion
        
        #region Statistics and Debugging
        
        public Dictionary<string, int> GetEventStatistics()
        {
            return new Dictionary<string, int>(_eventStatistics);
        }
        
        public void LogEventStatistics()
        {
            System.Diagnostics.Debug.WriteLine("=== EVENT SYSTEM STATISTICS ===");
            foreach (var kvp in _eventStatistics.OrderByDescending(x => x.Value))
            {
                System.Diagnostics.Debug.WriteLine($"{kvp.Key}: {kvp.Value} times");
            }
            System.Diagnostics.Debug.WriteLine($"Total registered handlers: {_eventHandlers.Sum(x => x.Value.Count)}");
            System.Diagnostics.Debug.WriteLine($"Queued events: {_eventQueue.Count}");
        }
        
        #endregion
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _eventHandlers.Clear();
            _eventQueue.Clear();
            _eventStatistics.Clear();
            _disposed = true;
            _instance = null;
        }
        
        private class EventData
        {
            public string EventName { get; set; }
            public object Data { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
