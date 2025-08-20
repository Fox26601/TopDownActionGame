using Microsoft.Xna.Framework;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// Centralized cursor state management system
    /// Determines cursor visibility based on all game states
    /// </summary>
    public static class CursorStateManager
    {
        public static bool ShouldShowCursor(
            bool gameStarted,
            bool isGamePaused,
            bool deathPanelVisible,
            bool inventoryOpen,
            bool menuActive,
            bool settingsActive,
            bool dialogueActive,
            bool confirmationDialogVisible = false,
            bool pauseMenuVisible = false,
            bool levelTransitionPanelVisible = false,
            bool saveSlotManagerVisible = false)
        {
            // Priority order for cursor visibility:
            
            // 1. Confirmation dialog - always show cursor
            if (confirmationDialogVisible)
                return true;
                
            // 2. Death panel - always show cursor
            if (deathPanelVisible)
                return true;
                
            // 3. Settings menu - always show cursor
            if (settingsActive)
                return true;
                
            // 4. Main menu - always show cursor
            if (menuActive)
                return true;
                
            // 5. Pause menu - always show cursor
            if (pauseMenuVisible)
                return true;
                
            // 6. Level transition panel - always show cursor
            if (levelTransitionPanelVisible)
                return true;
                
            // 7. Save slot manager - always show cursor
            if (saveSlotManagerVisible)
                return true;
                
            // 8. Game paused (pause menu) - always show cursor
            if (isGamePaused)
                return true;
                
            // 9. Dialogue active - show cursor for interaction
            if (dialogueActive)
                return true;
                
            // 10. Inventory open - show cursor for drag and drop
            if (inventoryOpen)
                return true;
                
            // 11. Game not started (main menu) - show cursor
            if (!gameStarted)
                return true;
                
            // 12. Game running normally - hide cursor
            return false;
        }
    }
}
