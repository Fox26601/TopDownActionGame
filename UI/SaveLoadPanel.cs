using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// UI panel for save and load operations
    /// </summary>
    public class SaveLoadPanel : IUIElement
    {
        private readonly ISaveManager _saveManager;
        private readonly SpriteFont _font;
        private readonly Texture2D _backgroundTexture;
        private readonly Texture2D _buttonTexture;
        
        private Rectangle _bounds;
        private bool _isVisible;
        private bool _isSaveMode; // true for save, false for load
        // private string _selectedSaveName; // Removed unused field
        private string _newSaveName = "";
        private List<SaveFileInfo> _saveFiles;
        private int _selectedIndex = -1;
        private float _scrollOffset = 0;
        private const int MAX_VISIBLE_SAVES = 8;
        
        // UI Elements
        private Button _saveButton;
        private Button _loadButton;
        private Button _deleteButton;
        private Button _backButton;
        private Button _quickSaveButton;
        private Button _quickLoadButton;
        
        // Events
        public event Action OnPanelClosed;
        public event Action<string> OnSaveRequested;
        public event Action<string> OnLoadRequested;
        public event Action<string> OnDeleteRequested;
        
        public SaveLoadPanel(ISaveManager saveManager, SpriteFont font, Texture2D backgroundTexture, Texture2D buttonTexture)
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
            var buttonSize = new Point(120, 30);
            
            _saveButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Save Game");
            _saveButton.OnClick += () => _isSaveMode = true;
            
            _loadButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Load Game");
            _loadButton.OnClick += () => _isSaveMode = false;
            
            _deleteButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Delete Save");
            _deleteButton.OnClick += DeleteSelectedSave;
            
            _backButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Back");
            _backButton.OnClick += ClosePanel;
            
            _quickSaveButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Quick Save");
            _quickSaveButton.OnClick += QuickSave;
            
            _quickLoadButton = new Button(new Rectangle(0, 0, buttonSize.X, buttonSize.Y), "Quick Load");
            _quickLoadButton.OnClick += QuickLoad;
        }
        
        public void Show(Rectangle bounds)
        {
            _bounds = bounds;
            _isVisible = true;
            _isSaveMode = true;
            // _selectedSaveName = ""; // Removed unused field
            _newSaveName = "";
            _selectedIndex = -1;
            _scrollOffset = 0;
            RefreshSaveFiles();
            UpdateButtonPositions();
        }
        
        public void Hide()
        {
            _isVisible = false;
        }
        
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Load content for buttons if needed
            _saveButton?.LoadContent(content);
            _loadButton?.LoadContent(content);
            _deleteButton?.LoadContent(content);
            _backButton?.LoadContent(content);
            _quickSaveButton?.LoadContent(content);
            _quickLoadButton?.LoadContent(content);
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
            
            // Handle save name input in save mode
            if (_isSaveMode)
            {
                foreach (var key in keyboardState.GetPressedKeys())
                {
                    if (key == Keys.Back && _newSaveName.Length > 0)
                    {
                        _newSaveName = _newSaveName.Substring(0, _newSaveName.Length - 1);
                    }
                    else if (key == Keys.Enter)
                    {
                        if (!string.IsNullOrWhiteSpace(_newSaveName))
                        {
                            OnSaveRequested?.Invoke(_newSaveName);
                        }
                    }
                    // ESC is now handled by InputHandler through event system
                    // Remove direct ESC handling to avoid conflicts
                    else if (key >= Keys.A && key <= Keys.Z || key >= Keys.D0 && key <= Keys.D9 || key == Keys.Space)
                    {
                        if (_newSaveName.Length < 20) // Limit save name length
                        {
                            _newSaveName += key.ToString();
                        }
                    }
                }
            }
            else
            {
                // Handle save file selection in load mode
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    if (_selectedIndex > 0)
                    {
                        _selectedIndex--;
                        if (_selectedIndex < _scrollOffset)
                        {
                            _scrollOffset = Math.Max(0, _scrollOffset - 1);
                        }
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.Down))
                {
                    if (_selectedIndex < _saveFiles.Count - 1)
                    {
                        _selectedIndex++;
                        if (_selectedIndex >= _scrollOffset + MAX_VISIBLE_SAVES)
                        {
                            _scrollOffset = Math.Min(_saveFiles.Count - MAX_VISIBLE_SAVES, _scrollOffset + 1);
                        }
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    if (_selectedIndex >= 0 && _selectedIndex < _saveFiles.Count)
                    {
                        OnLoadRequested?.Invoke(_saveFiles[_selectedIndex].SaveName);
                    }
                }
                // ESC is now handled by InputHandler through event system
                // Remove direct ESC handling to avoid conflicts
            }
        }
        
        private void UpdateButtons()
        {
            _saveButton.Update(new GameTime());
            _loadButton.Update(new GameTime());
            _deleteButton.Update(new GameTime());
            _backButton.Update(new GameTime());
            _quickSaveButton.Update(new GameTime());
            _quickLoadButton.Update(new GameTime());
        }
        
        private void UpdateButtonPositions()
        {
            var buttonY = _bounds.Y + 20;
            var buttonX = _bounds.X + 20;
            
            _saveButton.Bounds = new Rectangle(buttonX, buttonY, 120, 30);
            _loadButton.Bounds = new Rectangle(buttonX + 130, buttonY, 120, 30);
            _quickSaveButton.Bounds = new Rectangle(buttonX + 260, buttonY, 120, 30);
            _quickLoadButton.Bounds = new Rectangle(buttonX + 390, buttonY, 120, 30);
            
            _deleteButton.Bounds = new Rectangle(buttonX, buttonY + 40, 120, 30);
            _backButton.Bounds = new Rectangle(_bounds.Right - 140, _bounds.Bottom - 40, 120, 30);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;
            
            // Draw background
            spriteBatch.Draw(_backgroundTexture, _bounds, Color.White);
            
            // Draw title
            var title = _isSaveMode ? "Save Game" : "Load Game";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(_bounds.Center.X - titleSize.X / 2, _bounds.Y + 10);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);
            
            // Draw mode buttons
            _saveButton.Draw(spriteBatch);
            _loadButton.Draw(spriteBatch);
            _quickSaveButton.Draw(spriteBatch);
            _quickLoadButton.Draw(spriteBatch);
            _deleteButton.Draw(spriteBatch);
            _backButton.Draw(spriteBatch);
            
            if (_isSaveMode)
            {
                DrawSaveMode(spriteBatch);
            }
            else
            {
                DrawLoadMode(spriteBatch);
            }
        }
        
        private void DrawSaveMode(SpriteBatch spriteBatch)
        {
            var startY = _bounds.Y + 100;
            var startX = _bounds.X + 20;
            
            // Draw save name input
            spriteBatch.DrawString(_font, "Save Name:", new Vector2(startX, startY), Color.White);
            spriteBatch.DrawString(_font, _newSaveName + "|", new Vector2(startX, startY + 25), Color.Yellow);
            
            // Draw save file list
            spriteBatch.DrawString(_font, "Existing Saves:", new Vector2(startX, startY + 60), Color.White);
            
            var listStartY = startY + 85;
            for (int i = 0; i < Math.Min(MAX_VISIBLE_SAVES, _saveFiles.Count); i++)
            {
                var index = i + (int)_scrollOffset;
                if (index >= _saveFiles.Count) break;
                
                var saveFile = _saveFiles[index];
                var y = listStartY + i * 25;
                var color = index == _selectedIndex ? Color.Yellow : Color.White;
                
                var saveText = $"{saveFile.SaveName} - Level {saveFile.PlayerLevel} - {saveFile.SaveTime:MM/dd HH:mm}";
                spriteBatch.DrawString(_font, saveText, new Vector2(startX, y), color);
            }
        }
        
        private void DrawLoadMode(SpriteBatch spriteBatch)
        {
            var startY = _bounds.Y + 100;
            var startX = _bounds.X + 20;
            
            spriteBatch.DrawString(_font, "Select Save File:", new Vector2(startX, startY), Color.White);
            
            var listStartY = startY + 25;
            for (int i = 0; i < Math.Min(MAX_VISIBLE_SAVES, _saveFiles.Count); i++)
            {
                var index = i + (int)_scrollOffset;
                if (index >= _saveFiles.Count) break;
                
                var saveFile = _saveFiles[index];
                var y = listStartY + i * 30;
                var color = index == _selectedIndex ? Color.Yellow : Color.White;
                
                // Draw save file info
                var saveText = $"{saveFile.SaveName}";
                spriteBatch.DrawString(_font, saveText, new Vector2(startX, y), color);
                
                var detailsText = $"Level {saveFile.PlayerLevel} - {saveFile.SaveTime:MM/dd/yyyy HH:mm} - Play Time: {saveFile.PlayTime:hh\\:mm}";
                spriteBatch.DrawString(_font, detailsText, new Vector2(startX + 10, y + 15), Color.Gray);
            }
            
            // Draw instructions
            var instructionsY = _bounds.Bottom - 80;
            spriteBatch.DrawString(_font, "Use Up/Down arrows to select, Enter to load, Escape to cancel", 
                new Vector2(startX, instructionsY), Color.Gray);
        }
        
        private void RefreshSaveFiles()
        {
            _saveFiles = _saveManager.GetSaveFiles();
        }
        
        private void DeleteSelectedSave()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _saveFiles.Count)
            {
                var saveName = _saveFiles[_selectedIndex].SaveName;
                OnDeleteRequested?.Invoke(saveName);
                RefreshSaveFiles();
                _selectedIndex = -1;
            }
        }
        
        private void QuickSave()
        {
            OnSaveRequested?.Invoke("quicksave");
        }
        
        private void QuickLoad()
        {
            OnLoadRequested?.Invoke("quicksave");
        }
        
        private void ClosePanel()
        {
            Hide();
            OnPanelClosed?.Invoke();
        }
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        public Rectangle Bounds => _bounds;
    }
}
