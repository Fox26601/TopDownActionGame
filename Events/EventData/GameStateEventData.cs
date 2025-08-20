using Microsoft.Xna.Framework;
using IsometricActionGame.Events.EventData;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Event data for game state change events
    /// </summary>
    public class GameStateChangedEventData : BaseEventData
    {
        public string OldState { get; }
        public string NewState { get; }
        public string Reason { get; }
        
        public GameStateChangedEventData(string oldState, string newState, string reason = null) : base("GameState")
        {
            OldState = oldState;
            NewState = newState;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event data for game pause events
    /// </summary>
    public class GamePausedEventData : BaseEventData
    {
        public string PauseReason { get; }
        public bool IsPaused { get; }
        
        public GamePausedEventData(string pauseReason, bool isPaused = true) : base("GameState")
        {
            PauseReason = pauseReason;
            IsPaused = isPaused;
        }
    }
    
    /// <summary>
    /// Event data for game restart events
    /// </summary>
    public class GameRestartEventData : BaseEventData
    {
        public string RestartReason { get; }
        public bool IsFromSave { get; }
        
        public GameRestartEventData(string restartReason, bool isFromSave = false) : base("GameState")
        {
            RestartReason = restartReason;
            IsFromSave = isFromSave;
        }
    }
    
    /// <summary>
    /// Event data for level events
    /// </summary>
    public class LevelEventData : BaseEventData
    {
        public int LevelNumber { get; }
        public string LevelName { get; }
        public Vector2 PlayerStartPosition { get; }
        
        public LevelEventData(int levelNumber, string levelName, Vector2 playerStartPosition) : base("GameState")
        {
            LevelNumber = levelNumber;
            LevelName = levelName;
            PlayerStartPosition = playerStartPosition;
        }
    }
    
    /// <summary>
    /// Event data for level completion events
    /// </summary>
    public class LevelCompletedEventData : LevelEventData
    {
        public float CompletionTime { get; }
        public int EnemiesDefeated { get; }
        public int ItemsCollected { get; }
        
        public LevelCompletedEventData(int levelNumber, string levelName, Vector2 playerStartPosition, 
            float completionTime, int enemiesDefeated, int itemsCollected) 
            : base(levelNumber, levelName, playerStartPosition)
        {
            CompletionTime = completionTime;
            EnemiesDefeated = enemiesDefeated;
            ItemsCollected = itemsCollected;
        }
    }
}


