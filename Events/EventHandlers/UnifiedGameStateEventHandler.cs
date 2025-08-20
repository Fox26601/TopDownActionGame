using System;
using Microsoft.Xna.Framework;
using IsometricActionGame.Events;
using IsometricActionGame.Events.EventData;
using IsometricActionGame.UI;
using IsometricActionGame.Quests;

namespace IsometricActionGame.Events.EventHandlers
{
    /// <summary>
    /// Unified handler for all game state-related events
    /// Consolidates game state, pause, restart, and level event handling
    /// </summary>
    public class UnifiedGameStateEventHandler : IEventHandler
    {
        private readonly ConsoleDisplay _console;
        private readonly Dialogue.DialogueManager _dialogueManager;
        private readonly GameRestartManager _restartManager;
        private readonly GameEventSystem _eventSystem;
        
        public UnifiedGameStateEventHandler(ConsoleDisplay console, Dialogue.DialogueManager dialogueManager, GameRestartManager restartManager)
        {
            _console = console;
            _dialogueManager = dialogueManager;
            _restartManager = restartManager;
            _eventSystem = GameEventSystem.Instance;
        }
        
        public void Initialize()
        {
            SubscribeToEvents();
        }
        
        public void SubscribeToEvents()
        {
            // Game state events
            _eventSystem.Subscribe<GameStateChangedEventData>(GameEvents.GAME_STATE_CHANGED, OnGameStateChanged);
            _eventSystem.Subscribe<GamePausedEventData>(GameEvents.GAME_PAUSED, OnGamePaused);
            _eventSystem.Subscribe<GameRestartEventData>(GameEvents.GAME_RESTART, OnGameRestart);
            _eventSystem.Subscribe<LevelEventData>(GameEvents.LEVEL_STARTED, OnLevelStarted);
            _eventSystem.Subscribe<LevelCompletedEventData>(GameEvents.LEVEL_COMPLETED, OnLevelCompleted);
            
            // System events
            _eventSystem.Subscribe<object>(GameEvents.GAME_STARTED, OnGameStarted);
            _eventSystem.Subscribe<object>(GameEvents.GAME_RESUMED, OnGameResumed);
            _eventSystem.Subscribe<object>(GameEvents.GAME_STOPPED, OnGameStopped);
            _eventSystem.Subscribe<object>(GameEvents.GAME_EXIT, OnGameExit);
            _eventSystem.Subscribe<object>(GameEvents.RETURN_TO_MENU, OnReturnToMenu);
            _eventSystem.Subscribe<object>(GameEvents.GO_TO_MENU, OnGoToMenu);
            _eventSystem.Subscribe<object>(GameEvents.GAME_COMPLETED, OnGameCompleted);
            _eventSystem.Subscribe<object>(GameEvents.SCENE_CLEAR_REQUESTED, OnSceneClearRequested);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventSystem.Unsubscribe<GameStateChangedEventData>(GameEvents.GAME_STATE_CHANGED, OnGameStateChanged);
            _eventSystem.Unsubscribe<GamePausedEventData>(GameEvents.GAME_PAUSED, OnGamePaused);
            _eventSystem.Unsubscribe<GameRestartEventData>(GameEvents.GAME_RESTART, OnGameRestart);
            _eventSystem.Unsubscribe<LevelEventData>(GameEvents.LEVEL_STARTED, OnLevelStarted);
            _eventSystem.Unsubscribe<LevelCompletedEventData>(GameEvents.LEVEL_COMPLETED, OnLevelCompleted);
            
            _eventSystem.Unsubscribe<object>(GameEvents.GAME_STARTED, OnGameStarted);
            _eventSystem.Unsubscribe<object>(GameEvents.GAME_RESUMED, OnGameResumed);
            _eventSystem.Unsubscribe<object>(GameEvents.GAME_STOPPED, OnGameStopped);
            _eventSystem.Unsubscribe<object>(GameEvents.GAME_EXIT, OnGameExit);
            _eventSystem.Unsubscribe<object>(GameEvents.RETURN_TO_MENU, OnReturnToMenu);
            _eventSystem.Unsubscribe<object>(GameEvents.GO_TO_MENU, OnGoToMenu);
            _eventSystem.Unsubscribe<object>(GameEvents.GAME_COMPLETED, OnGameCompleted);
            _eventSystem.Unsubscribe<object>(GameEvents.SCENE_CLEAR_REQUESTED, OnSceneClearRequested);
        }
        
        #region Game State Event Handlers
        
        private void OnGameStateChanged(GameStateChangedEventData data)
        {
            var reason = !string.IsNullOrEmpty(data.Reason) ? $" ({data.Reason})" : "";
            _console?.AddMessage($"Game state changed: {data.OldState} → {data.NewState}{reason}!", Color.LightBlue);
        }
        
        private void OnGamePaused(GamePausedEventData data)
        {
            var status = data.IsPaused ? "PAUSED" : "RESUMED";
            var reason = !string.IsNullOrEmpty(data.PauseReason) ? $" ({data.PauseReason})" : "";
            _console?.AddMessage($"Game {status}{reason}!", Color.Yellow);
        }
        
        private void OnGameRestart(GameRestartEventData data)
        {
            var reason = !string.IsNullOrEmpty(data.RestartReason) ? $" ({data.RestartReason})" : "";
            var source = data.IsFromSave ? " from save" : "";
            _console?.AddMessage($"Game restarting{source}{reason}!", Color.Orange);
            
            // Trigger restart process
            if (_restartManager != null)
            {
                _restartManager.InitiateRestart();
            }
        }
        
        private void OnLevelStarted(LevelEventData data)
        {
            _console?.AddMessage($"Level {data.LevelNumber} started: {data.LevelName}!", Color.LightGreen);
        }
        
        private void OnLevelCompleted(LevelCompletedEventData data)
        {
            _console?.AddMessage($"Level {data.LevelNumber} completed!", Color.Gold);
            _console?.AddMessage($"Completion time: {data.CompletionTime:F1}s", Color.Gold);
            _console?.AddMessage($"Enemies defeated: {data.EnemiesDefeated}", Color.Gold);
            _console?.AddMessage($"Items collected: {data.ItemsCollected}", Color.Gold);
        }
        
        private void OnGameStarted(object data = null)
        {
            _console?.AddMessage("Game started!", Color.LightGreen);
        }
        
        private void OnGameResumed(object data = null)
        {
            _console?.AddMessage("Game resumed!", Color.LightGreen);
        }
        
        private void OnGameStopped(object data = null)
        {
            var reason = GetPropertyValue<string>(data, "Reason") ?? "Unknown";
            _console?.AddMessage($"Game stopped! Reason: {reason}", Color.Orange);
        }
        
        private void OnSceneClearRequested(object data = null)
        {
            var clearType = GetPropertyValue<string>(data, "ClearType") ?? "Partial";
            _console?.AddMessage($"Scene clear requested: {clearType}", Color.Yellow);
        }
        
        private void OnGameExit(object data = null)
        {
            _console?.AddMessage("Game exit requested!", Color.Red);
        }
        
        private void OnReturnToMenu(object data = null)
        {
            _console?.AddMessage("Returning to menu!", Color.LightBlue);
            
            // Trigger restart to return to main menu
            if (_restartManager != null)
            {
                _restartManager.InitiateMenuReturn();
            }
        }
        
        private void OnGoToMenu(object data = null)
        {
            _console?.AddMessage("Going to menu!", Color.LightBlue);
        }
        
        private void OnGameCompleted(object data = null)
        {
            _console?.AddMessage("Game completed! Congratulations!", Color.Gold);
        }
        
        #endregion
        
        #region Helper Methods
        
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null) return default(T);
            
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.PropertyType == typeof(T))
                {
                    return (T)property.GetValue(obj);
                }
            }
            catch
            {
                // Ignore reflection errors
            }
            
            return default(T);
        }
        
        #endregion
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}
