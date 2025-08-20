using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.SaveSystem;
using System;
using System.Collections.Generic;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// Manages save slots for the game
    /// </summary>
    public class SaveSlotManager : IUIElement
    {
        private readonly ISaveManager _saveManager;
        private readonly SpriteFont _font;
        private readonly Texture2D _backgroundTexture;
        private readonly Texture2D _buttonTexture;
        
        private Rectangle _bounds;
        private bool _isVisible;
        private bool _isSaveMode; // true for save, false for load
        private int _selectedSlot = -1;
        private List<SaveFileInfo> _saveFiles;
        private const int SAVE_SLOTS = 3;
        
        // UI Elements
        private Button _slot1Button;
        private Button _slot2Button;
        private Button _slot3Button;
        private Button _quickSaveButton;
        private Button _quickLoadButton;
        private Button _backButton;
        
        // Events
        public event Action OnPanelClosed;
        public event Action<int> OnSaveRequested; // slot number (1-3)
        public event Action<int> OnLoadRequested; // slot number (1-3)
        public event Action OnQuickSaveRequested;
        public event Action OnQuickLoadRequested;
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        
        public SaveSlotManager(ISaveManager saveManager, SpriteFont font, Texture2D backgroundTexture, Texture2D buttonTexture)
        {
            _saveManager = saveManager;
            _font = font;
            _backgroundTexture = backgroundTexture;
            _buttonTexture = buttonTexture;
            
            InitializeButtons();
            RefreshSaveFiles();
        }
        
        private void InitializeButtons()
        {
            var buttonSize = new Point(200, 60);
            // var spacing = 20; // Removed unused variable
            
            // Create slot buttons
            _slot1Button = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Slot 1");
            _slot1Button.OnClick += () => HandleSlotClick(1);
            
            _slot2Button = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Slot 2");
            _slot2Button.OnClick += () => HandleSlotClick(2);
            
            _slot3Button = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Slot 3");
            _slot3Button.OnClick += () => HandleSlotClick(3);
            
            // Create quick save/load buttons
            _quickSaveButton = new Button(new Rectangle(0, 0, 150, 40), "Quick Save");
            _quickSaveButton.OnClick += () => OnQuickSaveRequested?.Invoke();
            
            _quickLoadButton = new Button(new Rectangle(0, 0, 150, 40), "Quick Load");
            _quickLoadButton.OnClick += () => OnQuickLoadRequested?.Invoke();
            
            // Create back button
            _backButton = new Button(new Rectangle(0, 0, 100, 30), "Back");
            _backButton.OnClick += ClosePanel;
        }
        
        public void Show(Rectangle bounds, bool isSaveMode)
        {
            _bounds = bounds;
            _isVisible = true;
            _isSaveMode = isSaveMode;
            _selectedSlot = -1;
            RefreshSaveFiles();
            UpdateButtonPositions();
        }
        
        public void Hide()
        {
            _isVisible = false;
        }
        
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            _slot1Button?.LoadContent(content);
            _slot2Button?.LoadContent(content);
            _slot3Button?.LoadContent(content);
            _quickSaveButton?.LoadContent(content);
            _quickLoadButton?.LoadContent(content);
            _backButton?.LoadContent(content);
        }
        
        public void Update(GameTime gameTime)
        {
            if (!_isVisible) return;
            
            HandleInput();
            UpdateButtons();
        }
        
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            
            // Handle slot selection with arrow keys
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                if (_selectedSlot > 1)
                    _selectedSlot--;
                else if (_selectedSlot == -1)
                    _selectedSlot = 3;
            }
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                if (_selectedSlot < 3)
                    _selectedSlot++;
                else if (_selectedSlot == -1)
                    _selectedSlot = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.Left))
            {
                if (_selectedSlot > 1)
                    _selectedSlot--;
                else if (_selectedSlot == -1)
                    _selectedSlot = 3;
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                if (_selectedSlot < 3)
                    _selectedSlot++;
                else if (_selectedSlot == -1)
                    _selectedSlot = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.Enter))
            {
                if (_selectedSlot >= 1 && _selectedSlot <= 3)
                {
                    HandleSlotClick(_selectedSlot);
                }
            }
            // ESC is now handled by InputHandler through event system
            // Remove direct ESC handling to avoid conflicts
        }
        
        private void UpdateButtons()
        {
            _slot1Button.Update(new GameTime());
            _slot2Button.Update(new GameTime());
            _slot3Button.Update(new GameTime());
            _quickSaveButton.Update(new GameTime());
            _quickLoadButton.Update(new GameTime());
            _backButton.Update(new GameTime());
        }
        
        private void UpdateButtonPositions()
        {
            var buttonY = _bounds.Y + 80;
            var buttonX = _bounds.X + 50;
            
            // Position slot buttons
            _slot1Button.Bounds = new Rectangle(buttonX, buttonY, 200, 60);
            _slot2Button.Bounds = new Rectangle(buttonX, buttonY + 70, 200, 60);
            _slot3Button.Bounds = new Rectangle(buttonX, buttonY + 140, 200, 60);
            
            // Position quick save/load buttons based on mode
            if (_isSaveMode)
            {
                // In save mode, show quick save button
                _quickSaveButton.Bounds = new Rectangle(buttonX + 220, buttonY, 150, 40);
                _quickLoadButton.Bounds = new Rectangle(buttonX + 220, buttonY, 0, 0); // Hide quick load
            }
            else
            {
                // In load mode, show quick load button
                _quickSaveButton.Bounds = new Rectangle(buttonX + 220, buttonY, 0, 0); // Hide quick save
                _quickLoadButton.Bounds = new Rectangle(buttonX + 220, buttonY, 150, 40);
            }
            
            // Position back button
            _backButton.Bounds = new Rectangle(_bounds.Right - 120, _bounds.Bottom - 40, 100, 30);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;
            
            // Draw background
            spriteBatch.Draw(_backgroundTexture, _bounds, Color.White);
            
            // Draw title
            var title = _isSaveMode ? "Save Game" : "Load Game";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(_bounds.Center.X - titleSize.X / 2, _bounds.Y + 20);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);
            
            // Draw slot buttons with save info
            DrawSlotButton(spriteBatch, _slot1Button, 1);
            DrawSlotButton(spriteBatch, _slot2Button, 2);
            DrawSlotButton(spriteBatch, _slot3Button, 3);
            
            // Draw quick save/load buttons based on mode
            if (_isSaveMode)
            {
                _quickSaveButton.Draw(spriteBatch);
            }
            else
            {
                _quickLoadButton.Draw(spriteBatch);
            }
            
            // Draw back button
            _backButton.Draw(spriteBatch);
            
            // Draw instructions
            var instructions = "Use Arrow Keys to select, Enter to confirm, Escape to cancel";
            var instructionsSize = _font.MeasureString(instructions);
            var instructionsPos = new Vector2(_bounds.Center.X - instructionsSize.X / 2, _bounds.Bottom - 20);
            spriteBatch.DrawString(_font, instructions, instructionsPos, Color.Gray);
        }
        
        private void DrawSlotButton(SpriteBatch spriteBatch, Button button, int slotNumber)
        {
            // Draw the button
            button.Draw(spriteBatch);
            
            // Highlight selected slot
            if (_selectedSlot == slotNumber)
            {
                var highlightRect = new Rectangle(button.Bounds.X - 2, button.Bounds.Y - 2, button.Bounds.Width + 4, button.Bounds.Height + 4);
                spriteBatch.Draw(_buttonTexture, highlightRect, Color.Yellow);
                button.Draw(spriteBatch); // Redraw button on top
            }
            
            // Draw save file info
            var saveName = $"save{slotNumber}";
            var saveInfo = GetSaveFileInfo(saveName);
            
            var infoText = saveInfo != null 
                ? $"Level {saveInfo.PlayerLevel} - {saveInfo.SaveTime:MM/dd HH:mm}"
                : "Empty Slot";
            
            var infoColor = saveInfo != null ? Color.White : Color.Gray;
            var infoPos = new Vector2(button.Bounds.X + 10, button.Bounds.Y + button.Bounds.Height + 5);
            spriteBatch.DrawString(_font, infoText, infoPos, infoColor);
        }
        
        private void HandleSlotClick(int slotNumber)
        {
            if (_isSaveMode)
            {
                OnSaveRequested?.Invoke(slotNumber);
            }
            else
            {
                OnLoadRequested?.Invoke(slotNumber);
            }
        }
        
        private void ClosePanel()
        {
            Hide();
            OnPanelClosed?.Invoke();
        }
        
        private void RefreshSaveFiles()
        {
            _saveFiles = _saveManager.GetSaveFiles();
        }
        
        private SaveFileInfo GetSaveFileInfo(string saveName)
        {
            return _saveFiles?.Find(f => f.SaveName == saveName);
        }
        
        public Rectangle Bounds => _bounds;
    }
}
