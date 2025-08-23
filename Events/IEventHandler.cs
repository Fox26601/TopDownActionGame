using System;

namespace IsometricActionGame.Events.EventHandlers
{
    /// <summary>
    /// Base interface for all event handlers
    /// Provides common functionality for event processing
    /// </summary>
    public interface IEventHandler : IDisposable
    {
        /// <summary>
        /// Initialize the event handler
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Subscribe to relevant events
        /// </summary>
        void SubscribeToEvents();
        
        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        void UnsubscribeFromEvents();
    }
}





