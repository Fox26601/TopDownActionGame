using Microsoft.Xna.Framework;

namespace IsometricActionGame.Dialogue
{
    /// <summary>
    /// Interface for dialogue positioning strategies
    /// Allows different positioning algorithms based on UI layout
    /// </summary>
    public interface IDialoguePositioningStrategy
    {
        /// <summary>
        /// Calculate optimal dialogue position considering other UI elements
        /// </summary>
        /// <param name="screenWidth">Current screen width</param>
        /// <param name="screenHeight">Current screen height</param>
        /// <param name="dialogueWidth">Dialogue panel width</param>
        /// <param name="dialogueHeight">Dialogue panel height</param>
        /// <returns>Optimal position for dialogue panel</returns>
        Vector2 CalculateDialoguePosition(int screenWidth, int screenHeight, int dialogueWidth, int dialogueHeight);
        
        /// <summary>
        /// Get the dialogue background rectangle
        /// </summary>
        /// <param name="position">Dialogue position</param>
        /// <param name="width">Dialogue width</param>
        /// <param name="height">Dialogue height</param>
        /// <returns>Background rectangle</returns>
        Rectangle GetDialogueBackground(Vector2 position, int width, int height);
    }
}
