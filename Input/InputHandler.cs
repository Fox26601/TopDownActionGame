using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.Events;
using IsometricActionGame.Dialogue;
using IsometricActionGame.UI;
using IsometricActionGame.SaveSystem;
using IsometricActionGame.Settings;

namespace IsometricActionGame
{
    // Handles all input processing and delegates to appropriate systems
    public class InputHandler
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private GameEventSystem _eventSystem;
        private InputSettings _inputSettings;
        
        // Game components
        private Player _player;
        private DialogueManager _dialogueManager;
        private PauseMenu _pauseMenu;
        private DeathPanel _deathPanel;
        private UIManager _uiManager;
        private LevelTransitionPanel _levelTransitionPanel;
        private SaveSlotManager _saveSlotManager;
        private GameSaveLoadManager _saveLoadManager;
        
        // Game state
        private bool _gameStarted;
        private bool _isGamePaused;
        
        public InputHandler()
        {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
            _eventSystem = GameEventSystem.Instance;
            _inputSettings = InputSettings.Instance;
        }
        
        // Initialize input handler with game components
        public void Initialize(
            Player player,
            DialogueManager dialogueManager,
            PauseMenu pauseMenu,
            DeathPanel deathPanel,
            UIManager uiManager,
            LevelTransitionPanel levelTransitionPanel,
            SaveSlotManager saveSlotManager,
            GameSaveLoadManager saveLoadManager)
        {
            _player = player;
            _dialogueManager = dialogueManager;
            _pauseMenu = pauseMenu;
            _deathPanel = deathPanel;
            _uiManager = uiManager;
            _levelTransitionPanel = levelTransitionPanel;
            _saveSlotManager = saveSlotManager;
            _saveLoadManager = saveLoadManager;
        }
        
        // Update LevelTransitionPanel reference when it's created
        public void UpdateLevelTransitionPanel(LevelTransitionPanel levelTransitionPanel)
        {
            _levelTransitionPanel = levelTransitionPanel;
        }
        
        // Update input handler state
        public void Update(GameTime gameTime, bool gameStarted, bool isGamePaused)
        {
            _gameStarted = gameStarted;
            _isGamePaused = isGamePaused;
            
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            
            // Handle global input (always processed)
            HandleGlobalInput();
            
            // Handle state-specific input
            if (_deathPanel.IsVisible)
            {
                HandleDeathPanelInput();
            }
            else if (_dialogueManager.IsDialogueActive)
            {
                HandleDialogueInput();
            }
            else if (_pauseMenu.IsVisible)
            {
                HandlePauseMenuInput();
            }
            else if (_levelTransitionPanel?.IsVisible == true)
            {
                HandleLevelTransitionInput();
            }
            else if (_saveSlotManager?.IsVisible == true)
            {
                HandleSaveMenuInput();
            }
            else if (!_gameStarted)
            {
                HandleMainMenuInput();
            }
            else if (_gameStarted && !_isGamePaused)
            {
                HandleGameplayInput();
            }
            
            // Handle inventory toggle globally (but with proper state checks)
            HandleInventoryToggle();
        }
        
        // Handle ESC key - publish event for unified event system to handle
        private void HandleEscapeKey()
        {
            var escJustPressed = IsKeyJustPressed(_inputSettings.PauseMenu);
            
            if (escJustPressed)
            {
                // Publish ESC event for unified event system to handle
                // This ensures consistent ESC handling across all UI states
                _eventSystem?.Publish<object>(GameEvents.ESC_PRESSED, new 
                { 
                    DialogueActive = _dialogueManager.IsDialogueActive,
                    InventoryOpen = _player.Inventory.IsOpen,
                    PauseVisible = _pauseMenu.IsVisible,
                    PauseConfirmationVisible = _pauseMenu.HasConfirmationDialogVisible,
                    LevelTransitionVisible = _levelTransitionPanel?.IsVisible,
                    SaveSlotVisible = _saveSlotManager?.IsVisible,
                    ExitConfirmationVisible = _uiManager?.ExitConfirmationVisible,
                    GameStarted = _gameStarted,
                    GamePaused = _isGamePaused
                });
            }
        }
        
        // Handle input when death panel is visible
        private void HandleDeathPanelInput()
        {
            // Death panel handles its own input internally
            // No additional input processing needed here
        }
        
        // Handle input when dialogue is active
        private void HandleDialogueInput()
        {
            // Dialogue system handles its own input through DialogueManager
            // Handle quick access hotkeys during dialogue
            HandleQuickAccessHotkeys();
        }
        
        // Handle input when pause menu is visible
        private void HandlePauseMenuInput()
        {
            // ESC is handled globally now
            // Pause menu handles its own button input internally
        }
        
        // Handle input when level transition panel is visible
        private void HandleLevelTransitionInput()
        {
            // ESC is handled globally now
            
            // Enter key to continue to next level
            var enterJustPressed = IsKeyJustPressed(_inputSettings.Confirm);
            
            if (enterJustPressed)
            {
                // Trigger next level through event system instead of direct event call
                _eventSystem?.Publish<object>(GameEvents.NEXT_LEVEL_TRIGGERED, null);
            }
        }
        
        // Handle input when save menu is visible
        private void HandleSaveMenuInput()
        {
            // ESC is handled globally now
            // Save menu handles its own input internally
        }
        
        // Handle input in main menu
        private void HandleMainMenuInput()
        {
            // ESC is handled globally now
            // Main menu handles its own input internally
        }
        
        // Handle input during gameplay
        private void HandleGameplayInput()
        {
            HandlePlayerMovement();
            HandlePlayerAttack();
            HandlePlayerInteraction();
            HandleQuickAccessHotkeys();
        }
        
        // Handle global input that applies to all states
        private void HandleGlobalInput()
        {
            HandleEscapeKey();
        }
        
        // Handle inventory toggle input
        private void HandleInventoryToggle()
        {
            if (_player == null || !_gameStarted) return;
            
            if (IsKeyJustPressed(_inputSettings.InventoryToggle))
            {
                _eventSystem?.Publish<object>(GameEvents.INVENTORY_TOGGLE_REQUESTED, null);
            }
        }
        
        // Handle player interaction input
        private void HandlePlayerInteraction()
        {
            if (_player == null || !_gameStarted || _isGamePaused || _pauseMenu.IsVisible) return;
            
            if (IsKeyJustPressed(_inputSettings.Interaction))
            {
                _eventSystem?.Publish<object>(GameEvents.PLAYER_INTERACTION_REQUESTED, null);
            }
        }
        
        // Handle quick access hotkeys
        private void HandleQuickAccessHotkeys()
        {
            if (_player == null || !_gameStarted) return;
            
            // Quick access hotkeys (1-6 keys, matching Inventory.QUICK_ACCESS_SLOTS)
            for (int i = 0; i < Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
            {
                if (IsKeyJustPressed(Keys.D1 + i))
                {
                    _eventSystem?.Publish(GameEvents.QUICK_ACCESS_USED, new { SlotIndex = i });
                }
            }
        }
        
        // Handle player movement input
        private void HandlePlayerMovement()
        {
            if (_player == null || !_gameStarted || _isGamePaused || _pauseMenu.IsVisible) return;
            
            Vector2 moveDirection = Vector2.Zero;
            bool isMovingLeft = false, isMovingRight = false, isMovingUp = false, isMovingDown = false;
            
            // Standard grid movement (no isometric transformation)
            if (_currentKeyboardState.IsKeyDown(_inputSettings.MoveLeft))
            {
                // A key: move left = (-1, 0) in world coordinates
                moveDirection.X -= 1;
                isMovingLeft = true;
            }
            
            if (_currentKeyboardState.IsKeyDown(_inputSettings.MoveRight))
            {
                // D key: move right = (1, 0) in world coordinates
                moveDirection.X += 1;
                isMovingRight = true;
            }
            
            if (_currentKeyboardState.IsKeyDown(_inputSettings.MoveUp))
            {
                // W key: move up = (0, -1) in world coordinates
                moveDirection.Y -= 1;
                isMovingUp = true;
            }
            
            if (_currentKeyboardState.IsKeyDown(_inputSettings.MoveDown))
            {
                // S key: move down = (0, 1) in world coordinates
                moveDirection.Y += 1;
                isMovingDown = true;
            }
            
            // Always publish movement event, even when not moving
            // This ensures HandleMovementEvent is called to switch to idle animation
            if (moveDirection != Vector2.Zero)
            {
                moveDirection.Normalize();
            }
            
            _eventSystem?.Publish(GameEvents.PLAYER_MOVEMENT_DIRECTION, new 
            { 
                Direction = moveDirection,
                IsMovingLeft = isMovingLeft,
                IsMovingRight = isMovingRight,
                IsMovingUp = isMovingUp,
                IsMovingDown = isMovingDown
            });
        }
        
        // Handle player attack input
        private void HandlePlayerAttack()
        {
            if (_player == null || !_gameStarted || _isGamePaused || _pauseMenu.IsVisible) return;
            
            // Attack on configurable key
            if (IsKeyJustPressed(_inputSettings.Attack))
            {
                _eventSystem?.Publish<object>(GameEvents.PLAYER_ATTACK_REQUESTED, null);
            }
        }
        
        // Check if a key was just pressed (down in current frame, up in previous)
        private bool IsKeyJustPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
        
        // Get current keyboard state for components that need it
        public KeyboardState GetCurrentKeyboardState()
        {
            return _currentKeyboardState;
        }
        
        // Get previous keyboard state for components that need it
        public KeyboardState GetPreviousKeyboardState()
        {
            return _previousKeyboardState;
        }
        
        // Reset input states (useful after game state changes)
        public void ResetInputStates()
        {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
        }
        
        // Get whether cursor should be visible based on current game state
        public bool ShouldShowCursor()
        {
            return CursorStateManager.ShouldShowCursor(
                _gameStarted,
                _isGamePaused,
                _deathPanel.IsVisible,
                _player.Inventory.IsOpen,
                _uiManager.MenuActive,
                _uiManager.SettingsActive,
                _dialogueManager.IsDialogueActive,
                _uiManager.ExitConfirmationVisible,
                _pauseMenu.IsVisible,
                _levelTransitionPanel?.IsVisible == true,
                _saveSlotManager?.IsVisible == true);
        }
    }
}
