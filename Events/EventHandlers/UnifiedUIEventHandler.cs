using System;
using Microsoft.Xna.Framework;
using IsometricActionGame.Events;
using IsometricActionGame.UI;
using IsometricActionGame.Settings;
using IsometricActionGame.Graphics;
using IsometricActionGame;

namespace IsometricActionGame.Events.EventHandlers
{
    /// <summary>
    /// Unified handler for all UI-related events
    /// Consolidates resolution, settings, and UI state event handling
    /// </summary>
    public class UnifiedUIEventHandler : IEventHandler
    {
        private readonly ConsoleDisplay _console;
        private readonly UIManager _uiManager;
        private readonly LevelSystem _levelSystem;
        private readonly GameEventSystem _eventSystem;
        private LevelTransitionPanel _levelTransitionPanel; // Direct reference
        
        public UnifiedUIEventHandler(ConsoleDisplay console, UIManager uiManager, LevelSystem levelSystem)
        {
            _console = console;
            _uiManager = uiManager;
            _levelSystem = levelSystem;
            _eventSystem = GameEventSystem.Instance;
            
            _console?.AddMessage($"UnifiedUIEventHandler created - LevelSystem: {_levelSystem != null}, LevelTransitionPanel: {_levelSystem?.LevelTransitionPanel != null}", Color.Cyan);
        }
        
        public void Initialize()
        {
            SubscribeToEvents();
        }
        
        public void SubscribeToEvents()
        {
            // UI events
            _eventSystem.Subscribe<Resolution>(GameEvents.RESOLUTION_CHANGED, OnResolutionChanged);
            _eventSystem.Subscribe<bool>(GameEvents.FULLSCREEN_CHANGED, OnFullscreenChanged);
            _eventSystem.Subscribe<object>(GameEvents.SETTINGS_RESET, OnSettingsReset);
            _eventSystem.Subscribe<object>(GameEvents.SETTINGS_OPENED, OnSettingsOpened);
            _eventSystem.Subscribe<object>(GameEvents.MENU_OPENED, OnMenuOpened);
            _eventSystem.Subscribe<object>(GameEvents.MENU_CLOSED, OnMenuClosed);
            _eventSystem.Subscribe<object>(GameEvents.BUTTON_CLICKED, OnButtonClicked);
            _eventSystem.Subscribe<object>(GameEvents.ESC_PRESSED, OnEscPressed);
            _eventSystem.Subscribe<object>(GameEvents.LEVEL_TRANSITION_PANEL_CREATED, OnLevelTransitionPanelCreated);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventSystem.Unsubscribe<Resolution>(GameEvents.RESOLUTION_CHANGED, OnResolutionChanged);
            _eventSystem.Unsubscribe<bool>(GameEvents.FULLSCREEN_CHANGED, OnFullscreenChanged);
            _eventSystem.Unsubscribe<object>(GameEvents.SETTINGS_RESET, OnSettingsReset);
            _eventSystem.Unsubscribe<object>(GameEvents.SETTINGS_OPENED, OnSettingsOpened);
            _eventSystem.Unsubscribe<object>(GameEvents.MENU_OPENED, OnMenuOpened);
            _eventSystem.Unsubscribe<object>(GameEvents.MENU_CLOSED, OnMenuClosed);
            _eventSystem.Unsubscribe<object>(GameEvents.BUTTON_CLICKED, OnButtonClicked);
            _eventSystem.Unsubscribe<object>(GameEvents.ESC_PRESSED, OnEscPressed);
            _eventSystem.Unsubscribe<object>(GameEvents.LEVEL_TRANSITION_PANEL_CREATED, OnLevelTransitionPanelCreated);
        }
        
        /// <summary>
        /// Update LevelTransitionPanel reference when it's created
        /// </summary>
        public void UpdateLevelTransitionPanel()
        {
            // Update direct reference from LevelSystem
            _levelTransitionPanel = _levelSystem?.LevelTransitionPanel;
        }
        
        /// <summary>
        /// Force refresh of LevelTransitionPanel reference
        /// </summary>
        public void RefreshLevelTransitionPanelReference()
        {
            // Update direct reference from LevelSystem
            _levelTransitionPanel = _levelSystem?.LevelTransitionPanel;
        }
        
        #region UI Event Handlers
        
        private void OnResolutionChanged(Resolution resolution)
        {
            _console?.AddMessage($"Resolution changed to {resolution.Width}x{resolution.Height}!", Color.Cyan);
            
            // Update UI scaling and positioning
            UIScalingManager.Initialize(resolution.Width, resolution.Height);
        }
        
        private void OnFullscreenChanged(bool fullscreen)
        {
            var mode = fullscreen ? "Fullscreen" : "Windowed";
            _console?.AddMessage($"Display mode changed to {mode}!", Color.Cyan);
        }
        
        private void OnSettingsReset(object data = null)
        {
            _console?.AddMessage("Settings reset to defaults!", Color.Green);
        }
        
        private void OnSettingsOpened(object data = null)
        {
            _console?.AddMessage("Settings menu opened!", Color.LightBlue);
            
            // Hide level transition panel when settings are opened
            if (_levelSystem?.LevelTransitionPanel != null && _levelSystem.LevelTransitionPanel.IsVisible)
            {
                _levelSystem.LevelTransitionPanel.Hide();
                _console?.AddMessage("Level transition panel hidden due to settings", Color.Gray);
            }
        }
        
        private void OnMenuOpened(object data = null)
        {
            _console?.AddMessage("Menu opened!", Color.LightBlue);
        }
        
        private void OnMenuClosed(object data = null)
        {
            _console?.AddMessage("Menu closed!", Color.Gray);
        }
        
        private void OnButtonClicked(object data = null)
        {
            _console?.AddMessage("Button clicked!", Color.Yellow);
        }
        
        private void OnLevelTransitionPanelCreated(object data = null)
        {
            _console?.AddMessage("Level transition panel created - updating reference!", Color.Cyan);
            System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnLevelTransitionPanelCreated: Event received");
            UpdateLevelTransitionPanel();
        }
        
        private void OnEscPressed(object data = null)
        {
            System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnEscPressed: Method called");
            _console?.AddMessage("ESC key pressed - handling unified ESC logic!", Color.Orange);
            
            try
            {
                // Extract UI state from event data
                var dialogueActive = GetPropertyValue<bool>(data, "DialogueActive");
                var inventoryOpen = GetPropertyValue<bool>(data, "InventoryOpen");
                var pauseVisible = GetPropertyValue<bool>(data, "PauseVisible");
                var pauseConfirmationVisible = GetPropertyValue<bool>(data, "PauseConfirmationVisible");
                var levelTransitionVisible = GetPropertyValue<bool>(data, "LevelTransitionVisible");
                var saveSlotVisible = GetPropertyValue<bool>(data, "SaveSlotVisible");
                var exitConfirmationVisible = GetPropertyValue<bool>(data, "ExitConfirmationVisible");
                var gameStarted = GetPropertyValue<bool>(data, "GameStarted");
                var gamePaused = GetPropertyValue<bool>(data, "GamePaused");
                
                _console?.AddMessage($"ESC state - Dialogue: {dialogueActive}, Inventory: {inventoryOpen}, Pause: {pauseVisible}, PauseConfirm: {pauseConfirmationVisible}, LevelTransition: {levelTransitionVisible}, SaveSlot: {saveSlotVisible}, ExitConfirm: {exitConfirmationVisible}, GameStarted: {gameStarted}, GamePaused: {gamePaused}", Color.Gray);
                
                // Priority order for ESC handling - close windows first
                if (dialogueActive)
                {
                    // ESC closes dialogue (handled by dialogue system)
                    _console?.AddMessage("ESC: Closing dialogue", Color.Cyan);
                    // Dialogue system will handle this internally
                }
                else if (inventoryOpen)
                {
                    // ESC closes inventory
                    _console?.AddMessage("ESC: Closing inventory", Color.Cyan);
                    // This should be handled by the player's inventory system
                    // We'll need to publish an event for this
                    _eventSystem.Publish<object>(GameEvents.INVENTORY_TOGGLE_REQUESTED, null);
                }
                else if (pauseConfirmationVisible)
                {
                    // ESC closes pause menu confirmation dialog
                    _console?.AddMessage("ESC: Closing pause menu confirmation dialog", Color.Cyan);
                    _eventSystem.Publish<object>(GameEvents.PAUSE_MENU_CONFIRMATION_CLOSE_REQUESTED, null);
                }
                else if (pauseVisible)
                {
                    // ESC closes pause menu
                    _console?.AddMessage("ESC: Closing pause menu", Color.Cyan);
                    _eventSystem.Publish<object>(GameEvents.PAUSE_MENU_CLOSE_REQUESTED, null);
                }
                else if (levelTransitionVisible)
                {
                    // ESC closes level transition panel and continues game
                    _console?.AddMessage("ESC: Closing level transition panel", Color.Cyan);
                    System.Diagnostics.Debug.WriteLine($"UnifiedUIEventHandler.OnEscPressed: LevelTransitionVisible = {levelTransitionVisible}");
                    System.Diagnostics.Debug.WriteLine($"UnifiedUIEventHandler.OnEscPressed: _levelSystem = {_levelSystem != null}");
                    System.Diagnostics.Debug.WriteLine($"UnifiedUIEventHandler.OnEscPressed: _levelSystem?.LevelTransitionPanel = {_levelSystem?.LevelTransitionPanel != null}");
                    System.Diagnostics.Debug.WriteLine($"UnifiedUIEventHandler.OnEscPressed: Direct _levelTransitionPanel = {_levelTransitionPanel != null}");
                    
                    // Try direct reference first, then fallback to LevelSystem reference
                    var panelToHide = _levelTransitionPanel ?? _levelSystem?.LevelTransitionPanel;
                    
                    if (panelToHide != null)
                    {
                        _console?.AddMessage("ESC: LevelTransitionPanel found, hiding it", Color.Green);
                        System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnEscPressed: About to call LevelTransitionPanel.Hide()");
                        panelToHide.Hide();
                        System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnEscPressed: LevelTransitionPanel.Hide() called");
                        
                        // Publish both events to ensure proper state management
                        _eventSystem.Publish<object>(GameEvents.GAME_RESUMED, null);
                        _eventSystem.Publish<object>(GameEvents.LEVEL_TRANSITION_PANEL_HIDDEN, null);
                        System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnEscPressed: Events published");
                    }
                    else
                    {
                        _console?.AddMessage("ESC: LevelTransitionPanel is null!", Color.Red);
                        System.Diagnostics.Debug.WriteLine("UnifiedUIEventHandler.OnEscPressed: LevelTransitionPanel is null!");
                    }
                }
                else if (saveSlotVisible)
                {
                    // ESC closes save menu
                    _console?.AddMessage("ESC: Closing save slot menu", Color.Cyan);
                    _eventSystem.Publish<object>(GameEvents.SAVE_SLOT_MENU_CLOSE_REQUESTED, null);
                }
                else if (exitConfirmationVisible)
                {
                    // ESC closes exit confirmation dialog
                    _console?.AddMessage("ESC: Closing exit confirmation", Color.Cyan);
                    _uiManager?.HideExitConfirmation();
                }
                else if (gameStarted && !gamePaused)
                {
                    // ESC opens pause menu during gameplay (only when not paused)
                    _console?.AddMessage("ESC: Opening pause menu", Color.Cyan);
                    _eventSystem.Publish<object>(GameEvents.PAUSE_MENU_OPEN_REQUESTED, null);
                }
                else if (!gameStarted)
                {
                    // ESC shows exit confirmation in main menu
                    _console?.AddMessage("ESC: Showing exit confirmation in main menu", Color.Cyan);
                    _uiManager?.ShowExitConfirmation();
                }
                else
                {
                    _console?.AddMessage("ESC: No action taken (game paused with no visible UI)", Color.Gray);
                }
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error handling ESC: {ex.Message}", Color.Red);
                System.Diagnostics.Debug.WriteLine($"Error in OnEscPressed: {ex}");
            }

        }
        
        /// <summary>
        /// Helper method to safely extract property values from anonymous objects
        /// </summary>
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return default(T);
                
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(obj);
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }
        
        #endregion
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}

