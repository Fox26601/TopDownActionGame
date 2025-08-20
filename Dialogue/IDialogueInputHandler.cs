using Microsoft.Xna.Framework.Input;

namespace IsometricActionGame.Dialogue
{
    /// <summary>
    /// Interface for dialogue input handling
    /// Supports different input methods and key configurations
    /// </summary>
    public interface IDialogueInputHandler
    {
        /// <summary>
        /// Handle input for dialogue progression
        /// </summary>
        /// <param name="keyboard">Current keyboard state</param>
        /// <returns>True if dialogue should advance</returns>
        bool HandleDialogueProgression(KeyboardState keyboard);
        
        /// <summary>
        /// Handle input for dialogue choices
        /// </summary>
        /// <param name="keyboard">Current keyboard state</param>
        /// <param name="choiceCount">Number of available choices</param>
        /// <returns>Selected choice index (-1 if no choice selected)</returns>
        int HandleChoiceSelection(KeyboardState keyboard, int choiceCount);
        
        /// <summary>
        /// Check if dialogue should be closed
        /// </summary>
        /// <param name="keyboard">Current keyboard state</param>
        /// <returns>True if dialogue should close</returns>
        bool ShouldCloseDialogue(KeyboardState keyboard);

        /// <summary>
        /// Resets all internal input state flags. This should be called when dialogue state changes
        /// significantly (e.g., dialogue starts, ends, or choices are presented) to prevent
        /// a held key from triggering actions immediately in the new state.
        /// </summary>
        void ResetAllInputStates();

        /// <summary>
        /// Updates the previous keyboard state for "just pressed" detection.
        /// This should be called once per frame at the end of the input processing cycle.
        /// </summary>
        /// <param name="currentKeyboardState">The keyboard state of the current frame.</param>
        void UpdatePreviousKeyboardState(KeyboardState currentKeyboardState);
    }
}
