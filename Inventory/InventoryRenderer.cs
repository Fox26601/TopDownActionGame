using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using System;
using System.Collections.Generic;

namespace IsometricActionGame.Inventory
{
    /// <summary>
    /// Handles rendering of inventory UI components
    /// Separates rendering logic from inventory logic
    /// </summary>
    public class InventoryRenderer
    {
        private Texture2D _backgroundTexture;
        private Texture2D _slotTexture;
        private bool _texturesInitialized = false;
        
        // UI Constants
        private const int BASE_SLOT_SIZE = 48;
        private const int BASE_SLOT_PADDING = 4;
        private const float BASE_MARGIN = 20f;
        
        // Calculated dimensions
        private int _slotSize;
        private int _slotPadding;
        private float _margin;
        private int _inventoryPanelWidth;
        private int _inventoryPanelHeight;
        private int _quickAccessPanelWidth;
        private int _quickAccessPanelHeight;
        
        // Screen dimensions
        private int _screenWidth;
        private int _screenHeight;
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Calculate UI scaling
            float scaleX = screenWidth / 1920f;
            float scaleY = screenHeight / 1080f;
            float scale = Math.Min(scaleX, scaleY);
            
            // Apply scaling
            _slotSize = (int)(BASE_SLOT_SIZE * scale);
            _slotPadding = (int)(BASE_SLOT_PADDING * scale);
            _margin = BASE_MARGIN * scale;
            
            // Calculate panel dimensions
            _inventoryPanelWidth = Inventory.INVENTORY_WIDTH * (_slotSize + _slotPadding) + _slotPadding;
            _inventoryPanelHeight = Inventory.INVENTORY_HEIGHT * (_slotSize + _slotPadding) + _slotPadding;
            _quickAccessPanelWidth = Inventory.QUICK_ACCESS_SLOTS * (_slotSize + _slotPadding) + _slotPadding;
            _quickAccessPanelHeight = _slotSize + _slotPadding * 2;
            
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
            
            // Draw background
            var backgroundRect = new Rectangle((int)inventoryPos.X, (int)inventoryPos.Y, 
                _inventoryPanelWidth, _inventoryPanelHeight);
            spriteBatch.Draw(_backgroundTexture, backgroundRect, Color.DarkGray * GameConstants.UI.OVERLAY_OPACITY);
            
            // Draw slots
            for (int x = 0; x < Inventory.INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < Inventory.INVENTORY_HEIGHT; y++)
                {
                    var slotPos = new Vector2(
                        inventoryPos.X + _slotPadding + x * (_slotSize + _slotPadding),
                        inventoryPos.Y + _slotPadding + y * (_slotSize + _slotPadding)
                    );
                    
                    var slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, _slotSize, _slotSize);
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
                            var textPos = slotPos + new Vector2(_slotSize - textSize.X - 2, _slotSize - textSize.Y - 2);
                            spriteBatch.DrawString(font, quantityText, textPos, Color.White);
                        }
                    }
                }
            }
            
            // Draw gold
            var goldText = $"Gold: {inventory.Gold}";
            var goldPos = inventoryPos + new Vector2(0, _inventoryPanelHeight + 10);
            spriteBatch.DrawString(font, goldText, goldPos, Color.Yellow);
            
            // Draw instructions
            var instructionText = "Press Q to close inventory";
            var instructionSize = font.MeasureString(instructionText);
            var instructionPos = inventoryPos + new Vector2(_inventoryPanelWidth - instructionSize.X, _inventoryPanelHeight + 10);
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
            
            // Draw background
            var backgroundRect = new Rectangle((int)quickAccessPos.X, (int)quickAccessPos.Y, 
                _quickAccessPanelWidth, _quickAccessPanelHeight);
            spriteBatch.Draw(_backgroundTexture, backgroundRect, Color.DarkBlue * GameConstants.UI.OVERLAY_OPACITY);
            
            // Draw slots
            for (int i = 0; i < Inventory.QUICK_ACCESS_SLOTS; i++)
            {
                var slotPos = new Vector2(
                    quickAccessPos.X + _slotPadding + i * (_slotSize + _slotPadding),
                    quickAccessPos.Y + _slotPadding
                );
                
                var slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, _slotSize, _slotSize);
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
                        var textPos = slotPos + new Vector2(_slotSize - textSize.X - 2, _slotSize - textSize.Y - 2);
                        spriteBatch.DrawString(font, quantityText, textPos, Color.White);
                    }
                }
                
                // Draw slot number
                var slotNumber = (i + 1).ToString();
                var numberSize = font.MeasureString(slotNumber);
                var numberPos = slotPos + new Vector2(_slotSize - numberSize.X - 2, 2);
                spriteBatch.DrawString(font, slotNumber, numberPos, Color.White);
            }
        }
        
        private Vector2 GetInventoryPosition()
        {
            float x = (_screenWidth - _inventoryPanelWidth) / 2f;
            float y = _margin;
            return new Vector2(x, y);
        }
        
        private Vector2 GetQuickAccessPosition()
        {
            float x = (_screenWidth - _quickAccessPanelWidth) / 2f;
            float y = _screenHeight - _quickAccessPanelHeight - _margin;
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// Get inventory slot at mouse position
        /// </summary>
        public Point? GetInventorySlotAtPosition(Vector2 mousePos)
        {
            Vector2 inventoryPos = GetInventoryPosition();
            
            // Check if mouse is within inventory panel bounds
            if (mousePos.X < inventoryPos.X || mousePos.X > inventoryPos.X + _inventoryPanelWidth ||
                mousePos.Y < inventoryPos.Y || mousePos.Y > inventoryPos.Y + _inventoryPanelHeight)
            {
                return null;
            }
            
            // Calculate slot coordinates
            float relativeX = mousePos.X - inventoryPos.X - _slotPadding;
            float relativeY = mousePos.Y - inventoryPos.Y - _slotPadding;
            
            int slotX = (int)(relativeX / (_slotSize + _slotPadding));
            int slotY = (int)(relativeY / (_slotSize + _slotPadding));
            
            // Check if slot coordinates are valid
            if (slotX >= 0 && slotX < Inventory.INVENTORY_WIDTH && 
                slotY >= 0 && slotY < Inventory.INVENTORY_HEIGHT)
            {
                return new Point(slotX, slotY);
            }
            
            return null;
        }
        
        /// <summary>
        /// Get quick access slot at mouse position
        /// </summary>
        public int? GetQuickAccessSlotAtPosition(Vector2 mousePos)
        {
            Vector2 quickAccessPos = GetQuickAccessPosition();
            
            // Check if mouse is within quick access panel bounds
            if (mousePos.X < quickAccessPos.X || mousePos.X > quickAccessPos.X + _quickAccessPanelWidth ||
                mousePos.Y < quickAccessPos.Y || mousePos.Y > quickAccessPos.Y + _quickAccessPanelHeight)
            {
                return null;
            }
            
            // Calculate slot index
            float relativeX = mousePos.X - quickAccessPos.X - _slotPadding;
            float relativeY = mousePos.Y - quickAccessPos.Y - _slotPadding;
            
            // Check if mouse is within slot height
            if (relativeY < 0 || relativeY > _slotSize)
            {
                return null;
            }
            
            int slotIndex = (int)(relativeX / (_slotSize + _slotPadding));
            
            // Check if slot index is valid
            if (slotIndex >= 0 && slotIndex < Inventory.QUICK_ACCESS_SLOTS)
            {
                return slotIndex;
            }
            
            return null;
        }
    }
}
