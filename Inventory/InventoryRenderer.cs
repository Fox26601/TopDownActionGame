using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using IsometricActionGame.UI;
using System;
using System.Collections.Generic;

namespace IsometricActionGame.Inventory
{
    /// <summary>
    /// Handles rendering of inventory UI components
    /// Uses unified UIScalingManager for consistent scaling across all resolutions
    /// </summary>
    public class InventoryRenderer
    {
        private Texture2D _backgroundTexture;
        private Texture2D _slotTexture;
        private bool _texturesInitialized = false;
        
        // UI Constants (base values for reference resolution 1280x720)
        private const int BASE_SLOT_SIZE = GameConstants.UI.INVENTORY_SLOT_SIZE;
        private const int BASE_SLOT_PADDING = GameConstants.UI.INVENTORY_SLOT_PADDING;
        private const float BASE_MARGIN = GameConstants.UI.INVENTORY_MARGIN;
        private const float BASE_GOLD_SPACING = GameConstants.UI.INVENTORY_GOLD_SPACING;
        private const float BASE_INSTRUCTION_SPACING = GameConstants.UI.INVENTORY_INSTRUCTION_SPACING;
        
        // Screen dimensions
        private int _screenWidth;
        private int _screenHeight;
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Initialize UIScalingManager with current screen dimensions
            UIScalingManager.Initialize(screenWidth, screenHeight);
            
            // Mark textures for reinitialization
            _texturesInitialized = false;
        }
        
        public void InitializeTextures(GraphicsDevice graphicsDevice)
        {
            if (_texturesInitialized) return;
            
            // Create background texture (white pixel)
            _backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] { Color.White });
            
            // Create slot texture (white pixel)
            _slotTexture = new Texture2D(graphicsDevice, 1, 1);
            _slotTexture.SetData(new[] { Color.White });
            
            _texturesInitialized = true;
        }
        
        public void Dispose()
        {
            _backgroundTexture?.Dispose();
            _slotTexture?.Dispose();
            _texturesInitialized = false;
        }
        
        public void DrawInventory(SpriteBatch spriteBatch, SpriteFont font, Inventory inventory)
        {
            if (!inventory.IsOpen) return;
            
            InitializeTextures(spriteBatch.GraphicsDevice);
            
            Vector2 inventoryPos = GetInventoryPosition();
            
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            int inventoryPanelWidth = Inventory.INVENTORY_WIDTH * (slotSize + slotPadding) + slotPadding;
            int inventoryPanelHeight = Inventory.INVENTORY_HEIGHT * (slotSize + slotPadding) + slotPadding;
            
            // Draw background
            var backgroundRect = new Rectangle((int)inventoryPos.X, (int)inventoryPos.Y, 
                inventoryPanelWidth, inventoryPanelHeight);
            spriteBatch.Draw(_backgroundTexture, backgroundRect, Color.DarkGray * GameConstants.UI.OVERLAY_OPACITY);
            
            // Draw slots
            for (int x = 0; x < Inventory.INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < Inventory.INVENTORY_HEIGHT; y++)
                {
                    var slotPos = new Vector2(
                        inventoryPos.X + slotPadding + x * (slotSize + slotPadding),
                        inventoryPos.Y + slotPadding + y * (slotSize + slotPadding)
                    );
                    
                    var slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, slotSize, slotSize);
                    spriteBatch.Draw(_slotTexture, slotRect, Color.Gray);
                    
                    // Draw item if present
                    var item = inventory.GetItem(x, y);
                    if (item != null && item.Icon != null)
                    {
                        item.Draw(spriteBatch, slotPos, GameConstants.SpriteScale.ITEM_SCALE);
                        
                        // Draw quantity
                        if (item.IsStackable && item.Quantity > 1)
                        {
                            var quantityText = item.Quantity.ToString();
                            var textSize = font.MeasureString(quantityText);
                            var textPos = slotPos + new Vector2(slotSize - textSize.X - 2, slotSize - textSize.Y - 2);
                            spriteBatch.DrawString(font, quantityText, textPos, Color.White);
                        }
                    }
                }
            }
            
            // Draw gold with proper spacing
            var goldText = $"Gold: {inventory.Gold}";
            var goldSpacing = UIScalingManager.ScaleValue(BASE_GOLD_SPACING);
            var goldPos = inventoryPos + new Vector2(0, inventoryPanelHeight + goldSpacing);
            spriteBatch.DrawString(font, goldText, goldPos, Color.Yellow);
            
            // Draw instructions with proper spacing
            var instructionText = "Press Q to close inventory";
            var instructionSize = font.MeasureString(instructionText);
            var instructionSpacing = UIScalingManager.ScaleValue(BASE_INSTRUCTION_SPACING);
            var instructionPos = inventoryPos + new Vector2(inventoryPanelWidth - instructionSize.X, inventoryPanelHeight + instructionSpacing);
            spriteBatch.DrawString(font, instructionText, instructionPos, Color.White);
            
            // Draw dragged item on top of everything (highest Z-order)
            var dragState = inventory.GetDragState();
            if (dragState.IsActive && dragState.DraggedItem != null && dragState.DraggedItem.Icon != null)
            {
                dragState.DraggedItem.Draw(spriteBatch, dragState.DragOffset, GameConstants.SpriteScale.ITEM_SCALE);
            }
        }
        
        public void DrawQuickAccess(SpriteBatch spriteBatch, SpriteFont font, Inventory inventory)
        {
            InitializeTextures(spriteBatch.GraphicsDevice);
            
            Vector2 quickAccessPos = GetQuickAccessPosition();
            
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            int quickAccessPanelWidth = Inventory.QUICK_ACCESS_SLOTS * (slotSize + slotPadding) + slotPadding;
            int quickAccessPanelHeight = slotSize + slotPadding * 2;
            
            // Draw background
            var backgroundRect = new Rectangle((int)quickAccessPos.X, (int)quickAccessPos.Y, 
                quickAccessPanelWidth, quickAccessPanelHeight);
            spriteBatch.Draw(_backgroundTexture, backgroundRect, Color.DarkBlue * GameConstants.UI.OVERLAY_OPACITY);
            
            // Draw slots
            for (int i = 0; i < Inventory.QUICK_ACCESS_SLOTS; i++)
            {
                var slotPos = new Vector2(
                    quickAccessPos.X + slotPadding + i * (slotSize + slotPadding),
                    quickAccessPos.Y + slotPadding
                );
                
                var slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, slotSize, slotSize);
                spriteBatch.Draw(_slotTexture, slotRect, Color.Blue);
                
                // Draw item if present
                var item = inventory.GetQuickAccessItem(i);
                if (item != null && item.Icon != null)
                {
                    item.Draw(spriteBatch, slotPos, GameConstants.SpriteScale.ITEM_SCALE);
                    
                    // Draw quantity
                    if (item.IsStackable && item.Quantity > 1)
                    {
                        var quantityText = item.Quantity.ToString();
                        var textSize = font.MeasureString(quantityText);
                        var textPos = slotPos + new Vector2(slotSize - textSize.X - 2, slotSize - textSize.Y - 2);
                        spriteBatch.DrawString(font, quantityText, textPos, Color.White);
                    }
                }
                
                // Draw slot number
                var slotNumber = (i + 1).ToString();
                var numberSize = font.MeasureString(slotNumber);
                var numberPos = slotPos + new Vector2(slotSize - numberSize.X - 2, 2);
                spriteBatch.DrawString(font, slotNumber, numberPos, Color.White);
            }
        }
        
        private Vector2 GetInventoryPosition()
        {
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            float margin = UIScalingManager.ScaleValue(BASE_MARGIN);
            
            int inventoryPanelWidth = Inventory.INVENTORY_WIDTH * (slotSize + slotPadding) + slotPadding;
            int inventoryPanelHeight = Inventory.INVENTORY_HEIGHT * (slotSize + slotPadding) + slotPadding;
            
            float x = (_screenWidth - inventoryPanelWidth) / 2f;
            float y = margin;
            return new Vector2(x, y);
        }
        
        private Vector2 GetQuickAccessPosition()
        {
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            float margin = UIScalingManager.ScaleValue(BASE_MARGIN);
            
            int quickAccessPanelWidth = Inventory.QUICK_ACCESS_SLOTS * (slotSize + slotPadding) + slotPadding;
            int quickAccessPanelHeight = slotSize + slotPadding * 2;
            
            float x = (_screenWidth - quickAccessPanelWidth) / 2f;
            float y = _screenHeight - quickAccessPanelHeight - margin;
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// Get inventory slot at mouse position
        /// </summary>
        public Point? GetInventorySlotAtPosition(Vector2 mousePos)
        {
            Vector2 inventoryPos = GetInventoryPosition();
            
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            int inventoryPanelWidth = Inventory.INVENTORY_WIDTH * (slotSize + slotPadding) + slotPadding;
            int inventoryPanelHeight = Inventory.INVENTORY_HEIGHT * (slotSize + slotPadding) + slotPadding;
            
            System.Diagnostics.Debug.WriteLine($"GetInventorySlotAtPosition: Mouse pos: {mousePos}, Inventory pos: {inventoryPos}, Panel size: {inventoryPanelWidth}x{inventoryPanelHeight}");
            
            // Check if mouse is within inventory panel bounds
            if (mousePos.X < inventoryPos.X || mousePos.X > inventoryPos.X + inventoryPanelWidth ||
                mousePos.Y < inventoryPos.Y || mousePos.Y > inventoryPos.Y + inventoryPanelHeight)
            {
                System.Diagnostics.Debug.WriteLine($"GetInventorySlotAtPosition: Mouse outside inventory bounds");
                return null;
            }
            
            // Calculate slot coordinates
            float relativeX = mousePos.X - inventoryPos.X - slotPadding;
            float relativeY = mousePos.Y - inventoryPos.Y - slotPadding;
            
            int slotX = (int)(relativeX / (slotSize + slotPadding));
            int slotY = (int)(relativeY / (slotSize + slotPadding));
            
            System.Diagnostics.Debug.WriteLine($"GetInventorySlotAtPosition: Calculated slot: ({slotX}, {slotY})");
            
            // Check if slot coordinates are valid
            if (slotX >= 0 && slotX < Inventory.INVENTORY_WIDTH && 
                slotY >= 0 && slotY < Inventory.INVENTORY_HEIGHT)
            {
                System.Diagnostics.Debug.WriteLine($"GetInventorySlotAtPosition: Valid slot found: ({slotX}, {slotY})");
                return new Point(slotX, slotY);
            }
            
            System.Diagnostics.Debug.WriteLine($"GetInventorySlotAtPosition: Invalid slot coordinates: ({slotX}, {slotY})");
            return null;
        }
        
        /// <summary>
        /// Get quick access slot at mouse position
        /// </summary>
        public int? GetQuickAccessSlotAtPosition(Vector2 mousePos)
        {
            Vector2 quickAccessPos = GetQuickAccessPosition();
            
            // Get scaled dimensions
            int slotSize = UIScalingManager.ScaleValue(BASE_SLOT_SIZE);
            int slotPadding = UIScalingManager.ScaleValue(BASE_SLOT_PADDING);
            int quickAccessPanelWidth = Inventory.QUICK_ACCESS_SLOTS * (slotSize + slotPadding) + slotPadding;
            int quickAccessPanelHeight = slotSize + slotPadding * 2;
            
            System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Mouse pos: {mousePos}, Quick access pos: {quickAccessPos}, Panel size: {quickAccessPanelWidth}x{quickAccessPanelHeight}");
            
            // Check if mouse is within quick access panel bounds
            if (mousePos.X < quickAccessPos.X || mousePos.X > quickAccessPos.X + quickAccessPanelWidth ||
                mousePos.Y < quickAccessPos.Y || mousePos.Y > quickAccessPos.Y + quickAccessPanelHeight)
            {
                System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Mouse outside quick access bounds");
                return null;
            }
            
            // Calculate slot index
            float relativeX = mousePos.X - quickAccessPos.X - slotPadding;
            float relativeY = mousePos.Y - quickAccessPos.Y - slotPadding;
            
            // Check if mouse is within slot height
            if (relativeY < 0 || relativeY > slotSize)
            {
                System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Mouse outside slot height");
                return null;
            }
            
            int slotIndex = (int)(relativeX / (slotSize + slotPadding));
            
            System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Calculated slot index: {slotIndex}");
            
            // Check if slot index is valid
            if (slotIndex >= 0 && slotIndex < Inventory.QUICK_ACCESS_SLOTS)
            {
                System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Valid slot found: {slotIndex}");
                return slotIndex;
            }
            
            System.Diagnostics.Debug.WriteLine($"GetQuickAccessSlotAtPosition: Invalid slot index: {slotIndex}");
            return null;
        }
    }
}
