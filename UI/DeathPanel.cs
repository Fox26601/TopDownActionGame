using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Input;
using System;
using IsometricActionGame.SaveSystem;

namespace IsometricActionGame.UI
{
    public class DeathPanel : IUIElement
    {
        private SpriteFont _font;
        private Button _restartButton;
        private Button _menuButton;
        private Button _exitButton;
        private Button _loadGameButton;
        private Button _quickLoadButton;
        private bool _isVisible;
        private int _screenWidth;
        private int _screenHeight;
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        public event Action OnRestart;
        public event Action OnMenu;
        public event Action OnExit;
        public event Action OnLoadGame;
        public event Action OnQuickLoad;

        public DeathPanel(SpriteFont font, int screenWidth, int screenHeight)
        {
            _font = font;
            _isVisible = false;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Initialize UI scaling
            UIScalingManager.Initialize(screenWidth, screenHeight);
            
            // Use scaled button dimensions
            int buttonWidth = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_WIDTH);
            int buttonHeight = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_HEIGHT);
            int spacing = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE);
            
            // Get centered positions for buttons
            var centerPos = UIScalingManager.GetCenteredPosition(buttonWidth, buttonHeight);
            int centerX = (int)centerPos.X;
            int centerY = (int)centerPos.Y;
            
            // Create buttons with scaled positions
            _restartButton = new Button(
                new Rectangle(centerX, centerY - 2 * (buttonHeight + spacing), buttonWidth, buttonHeight),
                "Restart"
            );
            
            _loadGameButton = new Button(
                new Rectangle(centerX, centerY - buttonHeight - spacing, buttonWidth, buttonHeight),
                "Load Game"
            );
            
            _quickLoadButton = new Button(
                new Rectangle(centerX, centerY, buttonWidth, buttonHeight),
                "Quick Load"
            );
            
            _menuButton = new Button(
                new Rectangle(centerX, centerY + buttonHeight + spacing, buttonWidth, buttonHeight),
                "Menu"
            );
            
            _exitButton = new Button(
                new Rectangle(centerX, centerY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight),
                "Exit"
            );
            
            // Subscribe to button events
            _restartButton.OnClick += () => OnRestart?.Invoke();
            _loadGameButton.OnClick += () => OnLoadGame?.Invoke();
            _quickLoadButton.OnClick += () => OnQuickLoad?.Invoke();
            _menuButton.OnClick += () => OnMenu?.Invoke();
            _exitButton.OnClick += () => OnExit?.Invoke();
        }

        public void Show()
        {
            System.Diagnostics.Debug.WriteLine($"DeathPanel.Show: Called - Stack trace: {System.Environment.StackTrace}");
            _isVisible = true;
        }

        public void Hide()
        {
            System.Diagnostics.Debug.WriteLine($"DeathPanel.Hide: Called - Stack trace: {System.Environment.StackTrace}");
            _isVisible = false;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            _restartButton.LoadContent(content);
            _loadGameButton.LoadContent(content);
            _quickLoadButton.LoadContent(content);
            _menuButton.LoadContent(content);
            _exitButton.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            if (!_isVisible || !IsEnabled) return;
            
            _restartButton.Update(gameTime);
            _loadGameButton.Update(gameTime);
            _quickLoadButton.Update(gameTime);
            _menuButton.Update(gameTime);
            _exitButton.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;
            
            // Semi-transparent background covering entire screen
            var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), screenRect, Color.Black * GameConstants.UI.OVERLAY_OPACITY);
            
            // Panel background with scaled dimensions
            int panelWidth = UIScalingManager.ScaleValue(GameConstants.UI.PANEL_WIDTH);
            int panelHeight = UIScalingManager.ScaleValue(GameConstants.UI.PANEL_HEIGHT);
            var panelCenterPos = UIScalingManager.GetCenteredPosition(panelWidth, panelHeight);
            var panelRect = new Rectangle(
                (int)panelCenterPos.X, 
                (int)panelCenterPos.Y, 
                panelWidth, 
                panelHeight
            );
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), panelRect, Color.DarkRed * GameConstants.UI.PANEL_OPACITY);
            
            // Panel border
            var borderThickness = 3;
            var borderColor = Color.Red;
            
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
            
            // Title "You Died" with scaled positioning
            string title = "YOU DIED";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(
                (_screenWidth - titleSize.X) / 2,
                panelRect.Y + UIScalingManager.ScaleValue(40)
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);
            
            // Buttons
            _restartButton.Draw(spriteBatch);
            _loadGameButton.Draw(spriteBatch);
            _quickLoadButton.Draw(spriteBatch);
            _menuButton.Draw(spriteBatch);
            _exitButton.Draw(spriteBatch);
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
    }
}