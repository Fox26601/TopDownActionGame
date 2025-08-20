using System;
using Microsoft.Xna.Framework;
using IsometricActionGame.Events;
using IsometricActionGame.UI;

namespace IsometricActionGame.Events.EventHandlers
{
    /// <summary>
    /// Unified handler for all reset-related events
    /// Consolidates restart and reset event handling
    /// </summary>
    public class UnifiedResetEventHandler : IEventHandler
    {
        private readonly ConsoleDisplay _console;
        private readonly object _restartManager;
        private readonly GameEventSystem _eventSystem;
        private readonly PauseMenu _pauseMenu;
        private readonly DeathPanel _deathPanel;
        private readonly LevelTransitionPanel _levelTransitionPanel;
        private readonly UIManager _uiManager;
        
        public UnifiedResetEventHandler(ConsoleDisplay console, object restartManager, PauseMenu pauseMenu, DeathPanel deathPanel, LevelTransitionPanel levelTransitionPanel, UIManager uiManager)
        {
            _console = console;
            _restartManager = restartManager;
            _eventSystem = GameEventSystem.Instance;
            _pauseMenu = pauseMenu;
            _deathPanel = deathPanel;
            _levelTransitionPanel = levelTransitionPanel;
            _uiManager = uiManager;
        }
        
        public void Initialize()
        {
            SubscribeToEvents();
        }
        
        public void SubscribeToEvents()
        {
            // Reset events
            _eventSystem.Subscribe<object>(GameEvents.RESET_REQUESTED, OnResetRequested);
            _eventSystem.Subscribe<object>(GameEvents.RESET_COMPLETED, OnResetCompleted);
            _eventSystem.Subscribe<object>(GameEvents.RESET_FAILED, OnResetFailed);
            
            // Restart events
            _eventSystem.Subscribe<object>(GameEvents.RESTART_INITIATED, OnRestartInitiated);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_ENTITIES_CLEARED, OnRestartEntitiesCleared);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_PLAYER_RESET, OnRestartPlayerReset);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_ENEMIES_RESET, OnRestartEnemiesReset);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_ITEMS_RESET, OnRestartItemsReset);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_UI_RESET, OnRestartUIReset);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_SYSTEMS_RESET, OnRestartSystemsReset);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_COMPLETED, OnRestartCompleted);
            _eventSystem.Subscribe<object>(GameEvents.RESTART_FAILED, OnRestartFailed);
            _eventSystem.Subscribe<object>(GameEvents.PLAYER_RECREATED, OnPlayerRecreated);
            _eventSystem.Subscribe<object>(GameEvents.PEBBLE_RECREATED, OnPebbleRecreated);
            _eventSystem.Subscribe<object>(GameEvents.BAT_RECREATED, OnBatRecreated);
        }
        
        public void UnsubscribeFromEvents()
        {
            // Note: Unsubscribe methods are not implemented in the current GameEventSystem
            // This is a placeholder for future implementation
        }
        
        #region Reset Event Handlers
        
        private void OnResetRequested(object data)
        {
            var resetType = GetPropertyValue<string>(data, "ResetType");
            var isFromSave = GetPropertyValue<bool>(data, "IsFromSave");
            
            var source = isFromSave ? " from save" : "";
            _console?.AddMessage($"Reset requested: {resetType}{source}!", Color.Orange);
            
            // Handle UI reset for menu return
            if (resetType == "UI" && !isFromSave)
            {
                // This is a menu return, not a save load
                HandleMenuReturn();
            }
        }
        
        private void HandleMenuReturn()
        {
            try
            {
                _console?.AddMessage("Handling menu return...", Color.LightBlue);
                
                // Hide all game UI
                if (_pauseMenu != null)
                {
                    _pauseMenu.Hide();
                }
                
                if (_deathPanel != null)
                {
                    _deathPanel.Hide();
                }
                
                if (_levelTransitionPanel != null)
                {
                    _levelTransitionPanel.Hide();
                }
                
                // Publish events to stop game and clear scene
                _eventSystem.Publish<object>(GameEvents.GAME_STOPPED, null);
                _eventSystem.Publish<object>(GameEvents.SCENE_CLEAR_REQUESTED, null);
                
                // Activate the main menu
                if (_uiManager != null)
                {
                    _uiManager.ActivateMenu();
                }
                
                _console?.AddMessage("Menu return completed!", Color.Green);
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error in menu return: {ex.Message}", Color.Red);
            }
        }
        
        private void OnResetCompleted(object data)
        {
            var resetType = GetPropertyValue<string>(data, "ResetType");
            var handler = GetPropertyValue<string>(data, "Handler");
            
            _console?.AddMessage($"Reset completed: {resetType} by {handler}!", Color.Green);
        }
        
        private void OnResetFailed(object data)
        {
            var resetType = GetPropertyValue<string>(data, "ResetType");
            var error = GetPropertyValue<string>(data, "Error");
            
            var errorText = !string.IsNullOrEmpty(error) ? $" ({error})" : "";
            _console?.AddMessage($"Reset failed: {resetType}{errorText}!", Color.Red);
        }
        
        #endregion
        
        #region Restart Event Handlers
        
        private void OnRestartInitiated(object data)
        {
            _console?.AddMessage("Restart initiated!", Color.Orange);
        }
        
        private void OnRestartEntitiesCleared(object data)
        {
            _console?.AddMessage("Entities cleared!", Color.Cyan);
        }
        
        private void OnRestartPlayerReset(object data)
        {
            _console?.AddMessage("Player reset!", Color.Cyan);
        }
        
        private void OnRestartEnemiesReset(object data)
        {
            _console?.AddMessage("Enemies reset!", Color.Cyan);
        }
        
        private void OnRestartItemsReset(object data)
        {
            _console?.AddMessage("Items reset!", Color.Cyan);
        }
        
        private void OnRestartUIReset(object data)
        {
            _console?.AddMessage("UI reset!", Color.Cyan);
        }
        
        private void OnRestartSystemsReset(object data)
        {
            _console?.AddMessage("Systems reset!", Color.Cyan);
        }
        
        private void OnRestartCompleted(object data)
        {
            _console?.AddMessage("Restart completed!", Color.Green);
        }
        
        private void OnRestartFailed(object data)
        {
            var error = GetPropertyValue<string>(data, "Error");
            var errorText = !string.IsNullOrEmpty(error) ? $" ({error})" : "";
            _console?.AddMessage($"Restart failed{errorText}!", Color.Red);
        }
        
        private void OnPlayerRecreated(object data)
        {
            _console?.AddMessage("Player recreated!", Color.LightGreen);
        }
        
        private void OnPebbleRecreated(object data)
        {
            _console?.AddMessage("Pebble recreated!", Color.LightGreen);
        }
        
        private void OnBatRecreated(object data)
        {
            _console?.AddMessage("Bat recreated!", Color.LightGreen);
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
