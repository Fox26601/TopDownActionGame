using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.Events;
using IsometricActionGame.Settings;
using IsometricActionGame.Input;

namespace IsometricActionGame.Input.Handlers
{
    /// <summary>
    /// Handles input for dialogue system
    /// Processes dialogue progression, choice selection, and dialogue closing
    /// </summary>
    public class DialogueInputHandler
    {
        private InputSettings _inputSettings;
        private GameEventSystem _eventSystem;
        
        // State tracking
        private bool _isEnabled = true;
        private KeyboardState _previousKeyboardState;
        private KeyboardState _currentKeyboardState;
        
        public DialogueInputHandler(object inputManager = null) // UnifiedInputManager parameter kept for compatibility but not used
        {
            _inputSettings = InputSettings.Instance;
            _eventSystem = GameEventSystem.Instance;
            _previousKeyboardState = Keyboard.GetState();
            _currentKeyboardState = _previousKeyboardState;
        }
        
        /// <summary>
        /// Update dialogue input handler
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_isEnabled) return;
            
            // Update keyboard states for "just pressed" detection
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }
        
        /// <summary>
        /// Check if dialogue should be closed (ESC key)
        /// </summary>
        public bool ShouldCloseDialogue(KeyboardState keyboard)
        {
            return IsKeyJustPressed(_inputSettings.Cancel);
        }
        
        /// <summary>
        /// Handle dialogue progression (Space/Enter keys)
        /// </summary>
        public bool HandleDialogueProgression(KeyboardState keyboard)
        {
            return IsKeyJustPressed(_inputSettings.Confirm) || 
                   IsKeyJustPressed(Keys.Space);
        }
        
        /// <summary>
        /// Handle choice selection (number keys 1-4 and arrow keys for navigation)
        /// </summary>
        public int HandleChoiceSelection(KeyboardState keyboard, int maxChoices)
        {
            // Handle number keys (1, 2, 3, 4) - direct choice selection
            for (int i = 0; i < maxChoices && i < 4; i++)
            {
                if (IsKeyJustPressed(Keys.D1 + i))
                {
                    System.Diagnostics.Debug.WriteLine($"DialogueInputHandler: Number key {i + 1} pressed for direct choice selection");
                    return i;
                }
            }
            
            // Handle arrow keys for navigation (not direct selection)
            if (IsKeyJustPressed(Keys.Up))
            {
                System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Up arrow pressed for choice navigation");
                return -2; // Previous choice
            }
            if (IsKeyJustPressed(Keys.Down))
            {
                System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Down arrow pressed for choice navigation");
                return -3; // Next choice
            }
            if (IsKeyJustPressed(Keys.Left))
            {
                System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Left arrow pressed for choice navigation");
                return -2; // Previous choice (same as Up)
            }
            if (IsKeyJustPressed(Keys.Right))
            {
                System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Right arrow pressed for choice navigation");
                return -3; // Next choice (same as Down)
            }
            
            // Handle Enter/Space for confirming current selection
            if (IsKeyJustPressed(_inputSettings.Confirm) || IsKeyJustPressed(Keys.Space))
            {
                System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Enter/Space pressed for confirming current choice");
                return -1; // Confirm current selection
            }
            
            return -10; // No input detected
        }
        
        /// <summary>
        /// Check if a key was just pressed (down in current frame, up in previous)
        /// </summary>
        private bool IsKeyJustPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
        
        /// <summary>
        /// Update previous keyboard state for "just pressed" detection
        /// </summary>
        public void UpdatePreviousKeyboardState(KeyboardState currentKeyboardState)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = currentKeyboardState;
        }
        
        /// <summary>
        /// Reset input handler state
        /// </summary>
        public void Reset()
        {
            // Reset to a clean state - get current keyboard state and set it as both current and previous
            // This ensures that no "just pressed" events are triggered immediately after reset
            var currentState = Keyboard.GetState();
            _previousKeyboardState = currentState;
            _currentKeyboardState = currentState;
            System.Diagnostics.Debug.WriteLine("DialogueInputHandler: Reset called - cleared input state");
        }
        
        /// <summary>
        /// Enable or disable input handler
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }
    }
}
