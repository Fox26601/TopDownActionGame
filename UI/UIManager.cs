using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Settings;
using IsometricActionGame.Graphics;
using System;
using System.Collections.Generic;
using IsometricActionGame.Events;

namespace IsometricActionGame.UI
{
    public class UIManager
    {
        private List<Button> _buttons = new List<Button>();
        public IReadOnlyList<Button> Buttons => _buttons;
        public bool MenuActive { get; private set; } = true;
        public bool SettingsActive { get; private set; } = false;
        public bool ExitConfirmationVisible => _exitConfirmationDialog?.IsVisible ?? false;
        
        private GameSettings _gameSettings;
        private SettingsMenu _settingsMenu;
        private ConfirmationDialog _exitConfirmationDialog;
        private SpriteFont _font;
        private int _screenWidth;
        private int _screenHeight;
        
        public event Action OnStartGame;
        public event Action OnLoadGame;
        public event Action OnSettings;
        public event Action OnCloseGame;
        public event Action OnResetSettings;
        
        public void ActivateMenu()
        {
            System.Diagnostics.Debug.WriteLine("UIManager.ActivateMenu: Called");
            MenuActive = true;
            SettingsActive = false;
            System.Diagnostics.Debug.WriteLine($"UIManager.ActivateMenu: MenuActive = {MenuActive}, SettingsActive = {SettingsActive}");
        }
        
        public void DeactivateMenu()
        {
            MenuActive = false;
        }
        
        public void ActivateSettings()
        {
            System.Diagnostics.Debug.WriteLine("UIManager.ActivateSettings: Called");
            MenuActive = false;
            SettingsActive = true;
            System.Diagnostics.Debug.WriteLine($"UIManager.ActivateSettings: MenuActive = {MenuActive}, SettingsActive = {SettingsActive}");
            
            // Sync GameSettings with ResolutionManager to ensure current resolution is displayed
            if (_gameSettings != null && ResolutionManager.Instance != null)
            {
                var currentRes = ResolutionManager.Instance.CurrentResolution;
                var currentFullscreen = ResolutionManager.Instance.IsFullscreen;
                
                System.Diagnostics.Debug.WriteLine($"UIManager.ActivateSettings: Syncing settings - ResolutionManager: {currentRes.Width}x{currentRes.Height}, Fullscreen: {currentFullscreen}");
                System.Diagnostics.Debug.WriteLine($"UIManager.ActivateSettings: GameSettings before sync - Resolution: {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}, Fullscreen: {_gameSettings.IsFullscreen}");
                
                // Update GameSettings to match ResolutionManager without triggering events
                _gameSettings.UpdateResolution(currentRes);
                _gameSettings.UpdateFullscreen(currentFullscreen);
                
                System.Diagnostics.Debug.WriteLine($"UIManager.ActivateSettings: GameSettings after sync - Resolution: {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}, Fullscreen: {_gameSettings.IsFullscreen}");
            }
            
            // Ensure settings menu reflects current state
            _settingsMenu?.RefreshSettings();
        }
        
        public void DeactivateSettings()
        {
            System.Diagnostics.Debug.WriteLine("UIManager.DeactivateSettings: Called");
            SettingsActive = false;
            MenuActive = true;
            System.Diagnostics.Debug.WriteLine($"UIManager.DeactivateSettings: MenuActive = {MenuActive}, SettingsActive = {SettingsActive}");
        }
        
        public void ShowExitConfirmation()
        {
            _exitConfirmationDialog.Show("Exit Game", "Are you sure you want to exit the game?");
        }
        
        public void HideExitConfirmation()
        {
            _exitConfirmationDialog?.Hide();
        }

        public void Initialize(ContentManager content, GraphicsDevice graphicsDevice, int screenWidth, int screenHeight, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Load font for confirmation dialog
            _font = content.Load<SpriteFont>("buttonFont");
            
            // Initialize UI scaling manager
            UIScalingManager.Initialize(screenWidth, screenHeight);
            
            _buttons.Clear();
            
            // Use scaled dimensions from constants
            int btnWidth = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_WIDTH);
            int btnHeight = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_HEIGHT);
            int spacing = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_SPACING);
            int startY = UIScalingManager.ScaleValue(200);
            
            // Get centered position for buttons
            var centerPos = UIScalingManager.GetCenteredPosition(btnWidth, btnHeight);
            int centerX = (int)centerPos.X;

            var btnStart = new Button(new Rectangle(centerX, startY, btnWidth, btnHeight), "Start Game");
            btnStart.FillColor = Color.LightGreen;
            btnStart.HoverColor = Color.Green;
            btnStart.TextColor = Color.Black;
            btnStart.LoadContent(content);
            btnStart.OnClick += () => { DeactivateMenu(); OnStartGame?.Invoke(); };
            _buttons.Add(btnStart);

            var btnLoad = new Button(new Rectangle(centerX, startY + btnHeight + spacing, btnWidth, btnHeight), "Load Game");
            btnLoad.FillColor = Color.LightYellow;
            btnLoad.HoverColor = Color.Yellow;
            btnLoad.TextColor = Color.Black;
            btnLoad.LoadContent(content);
            btnLoad.OnClick += () => { 
                System.Diagnostics.Debug.WriteLine("UIManager: Load Game button clicked");
                OnLoadGame?.Invoke(); 
            };
            _buttons.Add(btnLoad);

            var btnSettings = new Button(new Rectangle(centerX, startY + 2 * (btnHeight + spacing), btnWidth, btnHeight), "Settings");
            btnSettings.FillColor = Color.LightBlue;
            btnSettings.HoverColor = Color.Blue;
            btnSettings.TextColor = Color.Black;
            btnSettings.LoadContent(content);
            btnSettings.OnClick += () => { 
                System.Diagnostics.Debug.WriteLine("UIManager: Settings button clicked");
                OnSettings?.Invoke(); 
                ActivateSettings(); 
            };
            _buttons.Add(btnSettings);

            var btnClose = new Button(new Rectangle(centerX, startY + 3 * (btnHeight + spacing), btnWidth, btnHeight), "Close Game");
            btnClose.FillColor = Color.LightCoral;
            btnClose.HoverColor = Color.Red;
            btnClose.TextColor = Color.Black;
            btnClose.LoadContent(content);
            btnClose.OnClick += () => { OnCloseGame?.Invoke(); };
            _buttons.Add(btnClose);
            
            // Initialize settings menu
            _settingsMenu = new SettingsMenu(_gameSettings);
            _settingsMenu.Initialize(screenWidth, screenHeight);
            _settingsMenu.LoadContent(content, graphicsDevice);
            _settingsMenu.OnBackToMenu += DeactivateSettings;
            _settingsMenu.OnApplySettings += OnSettingsApplied;
            _settingsMenu.OnResetSettings += OnSettingsReset;
            
            // Initialize exit confirmation dialog
            _exitConfirmationDialog = new ConfirmationDialog(_font, screenWidth, screenHeight);
            _exitConfirmationDialog.LoadContent(content, graphicsDevice);
            _exitConfirmationDialog.OnConfirm += () => OnCloseGame?.Invoke();
            _exitConfirmationDialog.OnCancel += () => { /* Dialog will hide itself */ };
        }

        public void Update(GameTime gameTime)
        {
            if (SettingsActive)
            {
                // When settings are active, only update settings menu
                _settingsMenu?.Update(gameTime);
            }
            else if (MenuActive)
            {
                // Update confirmation dialog first (highest priority) only when menu is active
                _exitConfirmationDialog?.Update(gameTime);
                
                // Don't update buttons if confirmation dialog is visible
                if (!(_exitConfirmationDialog?.IsVisible ?? false))
                {
                    foreach (var btn in _buttons)
                        btn.Update(gameTime);
                }
            }
            // No debug logging in Update to avoid performance issues
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (MenuActive)
            {
                // Draw main menu panel with background and borders
                DrawMainMenuPanel(spriteBatch);
                
                foreach (var btn in _buttons)
                    btn.Draw(spriteBatch);
                
                // Draw confirmation dialog on top of main menu
                _exitConfirmationDialog?.Draw(spriteBatch);
            }
            else if (SettingsActive)
            {
                _settingsMenu?.Draw(spriteBatch);
            }
        }
        
        private void DrawMainMenuPanel(SpriteBatch spriteBatch)
        {
            // Calculate panel dimensions to accommodate all buttons dynamically
            int buttonHeight = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_HEIGHT);
            int spacing = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE);
            int totalButtonHeight = _buttons.Count * buttonHeight + (_buttons.Count - 1) * spacing;
            
            // Panel dimensions using constants with dynamic height calculation
            int panelWidth = UIScalingManager.ScaleValue(GameConstants.UI.PANEL_WIDTH);
            int panelHeight = totalButtonHeight + UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE * 3); // Extra space for title and padding
            
            var panelCenterPos = UIScalingManager.GetCenteredPosition(panelWidth, panelHeight);
            var panelRect = new Rectangle(
                (int)panelCenterPos.X, 
                (int)panelCenterPos.Y, 
                panelWidth, 
                panelHeight
            );
            
            // Panel background using UI constants
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), panelRect, Color.DarkGray * GameConstants.UI.PANEL_OPACITY);
            
            // Panel border using UI constants
            var borderThickness = 3;
            var borderColor = Color.LightGray;
            
            // Top border
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, borderThickness), borderColor);
            // Bottom border
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                new Rectangle(panelRect.X, panelRect.Bottom - borderThickness, panelRect.Width, borderThickness), borderColor);
            // Left border
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                new Rectangle(panelRect.X, panelRect.Y, borderThickness, panelRect.Height), borderColor);
            // Right border
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                new Rectangle(panelRect.Right - borderThickness, panelRect.Y, borderThickness, panelRect.Height), borderColor);
            
            // Title using UI constants
            if (_font != null)
            {
                string title = "MAIN MENU";
                var titleSize = _font.MeasureString(title);
                var titlePos = new Vector2(
                    (_screenWidth - titleSize.X) / 2,
                    panelRect.Y + UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_MEDIUM)
                );
                spriteBatch.DrawString(_font, title, titlePos, Color.White);
            }
        }
        
        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
        
        private void OnSettingsApplied()
        {
            System.Diagnostics.Debug.WriteLine($"UIManager: Applying settings - Resolution: {_gameSettings.CurrentResolution.Width}x{_gameSettings.CurrentResolution.Height}, Fullscreen: {_gameSettings.IsFullscreen}");
            
            // Only notify about settings that actually changed
            // The resolution and fullscreen events will be triggered by GameSettings.ApplyPendingSettings()
            // which only fires events for actual changes
            
            System.Diagnostics.Debug.WriteLine("UIManager: Settings applied successfully");
        }
        
        private void OnSettingsReset()
        {
            // Notify game about settings reset
            OnResetSettings?.Invoke();
            System.Diagnostics.Debug.WriteLine("UIManager: Settings reset event published");
        }
    }
} 