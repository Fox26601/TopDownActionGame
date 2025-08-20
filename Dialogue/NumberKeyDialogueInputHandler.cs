using Microsoft.Xna.Framework.Input;

namespace IsometricActionGame.Dialogue
{
    /// <summary>
    /// Dialogue input handler that supports number keys 1-4 for choice selection
    /// Implements clean separation of input handling logic
    /// </summary>
    public class NumberKeyDialogueInputHandler : IDialogueInputHandler
    {
        private KeyboardState _previousKeyboardState;

        public NumberKeyDialogueInputHandler()
        {
            _previousKeyboardState = Keyboard.GetState(); // Initialize with current state
        }
        
        public bool HandleDialogueProgression(KeyboardState keyboard)
        {
            bool shouldAdvance = IsKeyJustPressed(keyboard, Keys.Space) ||
                                 IsKeyJustPressed(keyboard, Keys.Enter);
            
            System.Diagnostics.Debug.WriteLine($"InputHandler: HandleDialogueProgression returning {shouldAdvance}");
            return shouldAdvance;
        }
        
        public int HandleChoiceSelection(KeyboardState keyboard, int maxChoices)
        {
            // Handle number keys (1, 2, 3, 4)
            for (int i = 0; i < maxChoices; i++)
            {
                Keys numberKey = (Keys)((int)Keys.D1 + i);
                if (IsKeyJustPressed(keyboard, numberKey))
                {
                    System.Diagnostics.Debug.WriteLine($"InputHandler: Number key {i + 1} pressed for choice.");
                    return i; // Return 0-based index
                }
            }
            
            // Handle arrow keys for navigation
            if (IsKeyJustPressed(keyboard, Keys.Up))
            {
                System.Diagnostics.Debug.WriteLine("InputHandler: Up arrow pressed.");
                return -2; // Indicator for previous choice
            }
            
            if (IsKeyJustPressed(keyboard, Keys.Down))
            {
                System.Diagnostics.Debug.WriteLine("InputHandler: Down arrow pressed.");
                return -3; // Indicator for next choice
            }
            
            // Handle Enter or Space to confirm selection
            if (IsKeyJustPressed(keyboard, Keys.Enter) || IsKeyJustPressed(keyboard, Keys.Space))
            {
                System.Diagnostics.Debug.WriteLine("InputHandler: Enter/Space pressed to confirm choice.");
                return -1; // Indicator for confirmation
            }
            System.Diagnostics.Debug.WriteLine("InputHandler: HandleChoiceSelection returning -10 (no input).");
            return -10; // No input detected
        }
        
        public bool ShouldCloseDialogue(KeyboardState keyboard)
        {
            bool shouldClose = IsKeyJustPressed(keyboard, Keys.Escape);
            System.Diagnostics.Debug.WriteLine($"InputHandler: ShouldCloseDialogue returning {shouldClose}");
            return shouldClose;
        }
        
        public void ResetAllInputStates()
        {
            // With previousKeyboardState, we don't need to explicitly reset flags
            // The next update cycle will naturally compare against the current (reset) state
            _previousKeyboardState = Keyboard.GetState(); 
            System.Diagnostics.Debug.WriteLine("InputHandler: All input states reset (previous keyboard state updated).");
        }

        /// <summary>
        /// Helper to check if a key was just pressed (down in current frame, up in previous).
        /// </summary>
        private bool IsKeyJustPressed(KeyboardState currentKeyboardState, Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Call this at the end of each Update cycle to store the current keyboard state for the next frame.
        /// </summary>
        public void UpdatePreviousKeyboardState(KeyboardState currentKeyboardState)
        {
            _previousKeyboardState = currentKeyboardState;
        }
    }
}
