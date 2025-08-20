using Microsoft.Xna.Framework;
using IsometricActionGame.UI;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Dialogue
{
    /// <summary>
    /// Dialogue positioning strategy that accounts for quick access panel
    /// Positions dialogue above the quick access panel to avoid overlap
    /// </summary>
    public class QuickAccessAwareDialoguePositioning : IDialoguePositioningStrategy
    {
        private readonly int _quickAccessPanelHeight;
        private readonly int _margin;
        
        public QuickAccessAwareDialoguePositioning()
        {
            // Initialize with default values - will be recalculated in CalculateDialoguePosition if UIScalingManager is available
            try
            {
                // Get scaled dimensions for quick access panel (safe access to UIScalingManager)
                _quickAccessPanelHeight = UIScalingManager.ScaleValue(Inventory.Inventory.QUICK_ACCESS_SLOTS * 48 + 8); // Slot size + padding
                _margin = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_MEDIUM);
            }
            catch
            {
                // Fallback to default values if UIScalingManager is not initialized
                _quickAccessPanelHeight = Inventory.Inventory.QUICK_ACCESS_SLOTS * 48 + 8; // Default unscaled values
                _margin = GameConstants.UI.MARGIN_MEDIUM;
                System.Diagnostics.Debug.WriteLine("QuickAccessAwareDialoguePositioning: UIScalingManager not ready, using default values");
            }
        }
        
        public Vector2 CalculateDialoguePosition(int screenWidth, int screenHeight, int dialogueWidth, int dialogueHeight)
        {
            // Use current scaled values if UIScalingManager is available, otherwise use fallback
            int currentQuickAccessHeight = _quickAccessPanelHeight;
            int currentMargin = _margin;
            
            try
            {
                // Recalculate with current scale if available
                currentQuickAccessHeight = UIScalingManager.ScaleValue(Inventory.Inventory.QUICK_ACCESS_SLOTS * 48 + 8);
                currentMargin = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_MEDIUM);
            }
            catch
            {
                // Use the values from constructor if UIScalingManager is still not ready
                System.Diagnostics.Debug.WriteLine("CalculateDialoguePosition: Using fallback values for positioning");
            }
            
            // Calculate quick access panel position (bottom of screen)
            int quickAccessY = screenHeight - currentQuickAccessHeight - currentMargin;
            
            // Position dialogue above quick access panel with margin
            int dialogueY = quickAccessY - dialogueHeight - currentMargin;
            
            // Center dialogue horizontally
            int dialogueX = (screenWidth - dialogueWidth) / 2;
            
            // Ensure dialogue doesn't go off-screen at the top
            if (dialogueY < currentMargin)
            {
                dialogueY = currentMargin;
            }
            
            return new Vector2(dialogueX, dialogueY);
        }
        
        public Rectangle GetDialogueBackground(Vector2 position, int width, int height)
        {
            return new Rectangle((int)position.X, (int)position.Y, width, height);
        }
    }
}
