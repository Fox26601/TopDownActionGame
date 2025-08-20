using System;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Base class for all event data
    /// Provides common functionality and timestamp tracking
    /// </summary>
    public abstract class BaseEventData
    {
        public DateTime Timestamp { get; }
        public string Source { get; }
        
        protected BaseEventData(string source = null)
        {
            Timestamp = DateTime.UtcNow;
            Source = source ?? GetType().Name;
        }
    }
}
