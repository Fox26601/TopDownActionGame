using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Settings;
using IsometricActionGame.Core.Data;
using System;
using System.Collections.Generic;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// Main settings menu with resolution and fullscreen controls
    /// </summary>
    public class SettingsMenu : IUIElement
    {
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        
        public event Action OnBackToMenu;
        public event Action OnApplySettings;
        public event Action OnResetSettings;
        
        private GameSettings _gameSettings;
        private ResolutionSelector _resolutionSelector;
        private Checkbox _fullscreenCheckbox;
        private Button _applyButton;
        private Button _backButton;
        private Button _resetButton;
        
        private SpriteFont _titleFont;
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        
        private Rectangle _panelBounds;
        private const int PANEL_WIDTH = GameConstants.UILayout.SETTINGS_PANEL_WIDTH;
        private const int PANEL_HEIGHT = 400;
        private const int ELEMENT_SPACING = 30;
        private const int LABEL_HEIGHT = 25;
        private const int RESOLUTION_SELECTOR_WIDTH = 300;
        private const int RESOLUTION_SELECTOR_HEIGHT = 40;
        
        public SettingsMenu(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            
            // Panel will be positioned by Initialize method
            _panelBounds = new Rectangle(0, 0, PANEL_WIDTH, PANEL_HEIGHT);
        }
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            // Center the panel on screen
            int panelX = (screenWidth - PANEL_WIDTH) / 2;
            int panelY = (screenHeight - PANEL_HEIGHT) / 2;
            _panelBounds = new Rectangle(panelX, panelY, PANEL_WIDTH, PANEL_HEIGHT);
            
            // Calculate element positions
            int startY = panelY + 80; // Leave space for title
            int elementX = panelX + GameConstants.UILayout.SETTINGS_PANEL_ELEMENT_OFFSET;
            
            // Create resolution selector
            var resolutionBounds = new Rectangle(elementX, startY, RESOLUTION_SELECTOR_WIDTH, RESOLUTION_SELECTOR_HEIGHT);
            _resolutionSelector = new ResolutionSelector(resolutionBounds, "Resolution:", new List<Resolution>(_gameSettings.AvailableResolutions));
            System.Diagnostics.Debug.WriteLine($"SettingsMenu.Initialize: Created ResolutionSelector at {resolutionBounds}");
            
            if (_resolutionSelector != null)
            {
                _resolutionSelector.SetResolution(_gameSettings.CurrentResolution);
                System.Diagnostics.Debug.WriteLine($"SettingsMenu.Initialize: Set ResolutionSelector to {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}");
            }
            
            // Create fullscreen checkbox
            var checkboxPos = new Vector2(elementX, startY + ELEMENT_SPACING + LABEL_HEIGHT);
            _fullscreenCheckbox = new Checkbox(checkboxPos, "Fullscreen Mode");
            System.Diagnostics.Debug.WriteLine($"SettingsMenu.Initialize: Created FullscreenCheckbox at {checkboxPos}");
            
            if (_fullscreenCheckbox != null)
            {
                _fullscreenCheckbox.SetChecked(_gameSettings.IsFullscreen);
                System.Diagnostics.Debug.WriteLine($"SettingsMenu.Initialize: Set FullscreenCheckbox to {_gameSettings.IsFullscreen}");
            }
            
            // Update UI to reflect current settings
            UpdateUIFromSettings();
            
            // Create buttons
            int buttonY = startY + GameConstants.UILayout.SETTINGS_BUTTON_Y_OFFSET;
            int buttonWidth = 120;
            int buttonHeight = 40;
            int buttonSpacing = 20;
            
            var applyButtonBounds = new Rectangle(elementX, buttonY, buttonWidth, buttonHeight);
            _applyButton = new Button(applyButtonBounds, "Apply");
            _applyButton.OnClick += OnApplyClicked;
            _applyButton.IsEnabled = false; // Initially disabled until changes are made
            
            var resetButtonBounds = new Rectangle(elementX + buttonWidth + buttonSpacing, buttonY, buttonWidth, buttonHeight);
            _resetButton = new Button(resetButtonBounds, "Reset");
            _resetButton.OnClick += OnResetClicked;
            
            var backButtonBounds = new Rectangle(elementX + (buttonWidth + buttonSpacing) * 2, buttonY, buttonWidth, buttonHeight);
            _backButton = new Button(backButtonBounds, "Back");
            _backButton.OnClick += OnBackClicked;
        }
        
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _titleFont = content.Load<SpriteFont>("buttonFont");
            _font = content.Load<SpriteFont>("buttonFont");
            
            // Create a 1x1 white pixel texture for drawing
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Load UI element content
            _resolutionSelector.LoadContent(content, graphicsDevice);
            _fullscreenCheckbox.LoadContent(content, graphicsDevice);
            _applyButton.LoadContent(content);
            _resetButton.LoadContent(content);
            _backButton.LoadContent(content);
            
            // Set button colors
            _applyButton.FillColor = Color.LightGreen;
            _applyButton.HoverColor = Color.Green;
            _applyButton.TextColor = Color.Black;
            
            _resetButton.FillColor = Color.Orange;
            _resetButton.HoverColor = Color.DarkOrange;
            _resetButton.TextColor = Color.Black;
            
            _backButton.FillColor = Color.LightBlue;
            _backButton.HoverColor = Color.Blue;
            _backButton.TextColor = Color.Black;
        }
        
        public void Update(GameTime gameTime)
        {
            if (!IsVisible || !IsEnabled) return;
            
            _resolutionSelector.Update(gameTime);
            _fullscreenCheckbox.Update(gameTime);
            _applyButton.Update(gameTime);
            _resetButton.Update(gameTime);
            _backButton.Update(gameTime);
            
            // Check for changes and update apply button state
            CheckForChanges();
        }
        
        private void CheckForChanges()
        {
            // Check if resolution or fullscreen has changed
            bool resolutionChanged = _resolutionSelector.CurrentResolution != _gameSettings.CurrentResolution;
            bool fullscreenChanged = _fullscreenCheckbox.IsChecked != _gameSettings.IsFullscreen;
            bool hasChanges = resolutionChanged || fullscreenChanged;
            
            // Update pending settings only if there are actual changes
            if (resolutionChanged)
            {
                _gameSettings.SetPendingResolution(_resolutionSelector.CurrentResolution);
            }
            
            if (fullscreenChanged)
            {
                _gameSettings.SetPendingFullscreen(_fullscreenCheckbox.IsChecked);
            }
            
            UpdateApplyButtonState();
        }
        
        /// <summary>
        /// Update UI elements to reflect current settings
        /// </summary>
        private void UpdateUIFromSettings()
        {
            System.Diagnostics.Debug.WriteLine($"SettingsMenu.UpdateUIFromSettings: Called - Resolution: {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}, Fullscreen: {_gameSettings.IsFullscreen}");
            
            if (_resolutionSelector != null)
            {
                _resolutionSelector.SetResolution(_gameSettings.CurrentResolution);
                System.Diagnostics.Debug.WriteLine("SettingsMenu.UpdateUIFromSettings: ResolutionSelector updated");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SettingsMenu.UpdateUIFromSettings: _resolutionSelector is null!");
            }
            
            if (_fullscreenCheckbox != null)
            {
                _fullscreenCheckbox.SetChecked(_gameSettings.IsFullscreen);
                System.Diagnostics.Debug.WriteLine($"SettingsMenu.UpdateUIFromSettings: FullscreenCheckbox updated to {_gameSettings.IsFullscreen}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SettingsMenu.UpdateUIFromSettings: _fullscreenCheckbox is null!");
            }
            
            UpdateApplyButtonState();
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            
            // Draw background overlay
            var overlayColor = Color.Black * GameConstants.UI.PAUSE_OVERLAY_OPACITY;
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), overlayColor);
            
            // Draw panel background
            spriteBatch.Draw(_pixelTexture, _panelBounds, Color.DarkGray * GameConstants.UI.PANEL_OPACITY);
            
            // Draw panel border
            DrawRectangle(spriteBatch, _panelBounds, Color.Gray, 3);
            
            // Draw title
            if (_titleFont != null)
            {
                var titleText = "Game Settings";
                var titleSize = _titleFont.MeasureString(titleText);
                var titlePos = new Vector2(
                    _panelBounds.Center.X - titleSize.X / 2,
                    _panelBounds.Y + 20
                );
                spriteBatch.DrawString(_titleFont, titleText, titlePos, Color.White);
            }
            
            // Draw UI elements
            _resolutionSelector.Draw(spriteBatch);
            _fullscreenCheckbox.Draw(spriteBatch);
            _applyButton.Draw(spriteBatch);
            _resetButton.Draw(spriteBatch);
            _backButton.Draw(spriteBatch);
            
            // Draw instructions
            if (_font != null)
            {
                var instructionText = _gameSettings.HasPendingChanges 
                    ? "Click 'Apply' to save changes" 
                    : "No changes to apply";
                var instructionSize = _font.MeasureString(instructionText);
                var instructionPos = new Vector2(
                    _panelBounds.Center.X - instructionSize.X / 2,
                    _panelBounds.Bottom - 40
                );
                var instructionColor = _gameSettings.HasPendingChanges ? Color.LightGreen : Color.LightGray;
                spriteBatch.DrawString(_font, instructionText, instructionPos, instructionColor);
            }
        }
        
        public void RefreshSettings()
        {
            System.Diagnostics.Debug.WriteLine($"SettingsMenu.RefreshSettings: Called - Current GameSettings Resolution: {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}, Fullscreen: {_gameSettings.IsFullscreen}");
            
            // Cancel any pending changes and reset to current values
            _gameSettings.CancelPendingChanges();
            
            // Update UI to reflect current settings
            if (_resolutionSelector != null)
            {
                _resolutionSelector.SetResolution(_gameSettings.CurrentResolution);
                System.Diagnostics.Debug.WriteLine($"SettingsMenu.RefreshSettings: Updated ResolutionSelector to: {_resolutionSelector.CurrentResolution.Width}x{_resolutionSelector.CurrentResolution.Height}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SettingsMenu.RefreshSettings: _resolutionSelector is null!");
            }
            
            if (_fullscreenCheckbox != null)
            {
                _fullscreenCheckbox.SetChecked(_gameSettings.IsFullscreen);
                System.Diagnostics.Debug.WriteLine($"SettingsMenu.RefreshSettings: Updated FullscreenCheckbox to: {_gameSettings.IsFullscreen}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SettingsMenu.RefreshSettings: _fullscreenCheckbox is null!");
            }
            
            // Update apply button state
            UpdateApplyButtonState();
        }
        
        private void UpdateApplyButtonState()
        {
            if (_applyButton == null)
            {
                System.Diagnostics.Debug.WriteLine("SettingsMenu.UpdateApplyButtonState: _applyButton is null!");
                return;
            }
            
            _applyButton.IsEnabled = _gameSettings.HasPendingChanges;
            
            // Update button appearance based on state
            if (_gameSettings.HasPendingChanges)
            {
                _applyButton.FillColor = Color.LightGreen;
                _applyButton.HoverColor = Color.Green;
            }
            else
            {
                _applyButton.FillColor = Color.Gray;
                _applyButton.HoverColor = Color.DarkGray;
            }
        }
        
        private void OnApplyClicked()
        {
            if (!_gameSettings.HasPendingChanges) return;
            
            // Apply the pending settings
            _gameSettings.ApplyPendingSettings();
            
            // Notify that settings were applied
            OnApplySettings?.Invoke();
            
            // Update UI state
            UpdateApplyButtonState();
        }
        
        private void OnResetClicked()
        {
            // Reset to default settings immediately
            _gameSettings.ResetToDefaults();
            
            // Refresh UI to show the applied defaults
            RefreshSettings();
            
            // Notify that settings were reset
            OnResetSettings?.Invoke();
        }
        
        private void OnBackClicked()
        {
            // Cancel any pending changes before going back
            if (_gameSettings.HasPendingChanges)
            {
                _gameSettings.CancelPendingChanges();
                RefreshSettings();
            }
            
            OnBackToMenu?.Invoke();
        }
        
        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            // Draw rectangle outline using lines
            var topLeft = new Vector2(rect.Left, rect.Top);
            var topRight = new Vector2(rect.Right, rect.Top);
            var bottomLeft = new Vector2(rect.Left, rect.Bottom);
            var bottomRight = new Vector2(rect.Right, rect.Bottom);
            
            DrawLine(spriteBatch, topLeft, topRight, color, thickness);
            DrawLine(spriteBatch, topRight, bottomRight, color, thickness);
            DrawLine(spriteBatch, bottomRight, bottomLeft, color, thickness);
            DrawLine(spriteBatch, bottomLeft, topLeft, color, thickness);
        }
        
        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            var distance = Vector2.Distance(start, end);
            var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
            
            var rect = new Rectangle(
                (int)start.X,
                (int)start.Y,
                (int)distance,
                thickness
            );
            
            spriteBatch.Draw(_pixelTexture, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
