using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using IsometricActionGame.Events;
using IsometricActionGame.Settings;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.UI
{
    public class PauseMenu : IUIElement
    {
        private SpriteFont _font;
        private Button _resumeButton;
        private Button _saveButton;
        private Button _loadButton;
        private Button _menuButton;
        private Button _exitButton;
        private bool _isVisible;
        private KeyboardState _previousKeyboardState;
        private int _screenWidth;
        private int _screenHeight;
        private ConfirmationDialog _menuConfirmationDialog;
        private ConfirmationDialog _exitConfirmationDialog;

        
        // UI Layout constants (will be scaled by UIScalingManager) - using GameConstants
        private const int PANEL_WIDTH = GameConstants.UI.PANEL_WIDTH;
        private const int TITLE_TOP_MARGIN = GameConstants.UI.MARGIN_MEDIUM;
        private const int BUTTON_LIFT_OFFSET = 5;
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        public bool IsPaused => _isVisible;
        public bool HasConfirmationDialogVisible => _menuConfirmationDialog?.IsVisible == true || _exitConfirmationDialog?.IsVisible == true;
        
        /// <summary>
        /// Close confirmation dialogs when ESC is pressed
        /// </summary>
        public void CloseConfirmationDialogs()
        {
            _menuConfirmationDialog?.Hide();
            _exitConfirmationDialog?.Hide();
        }
        public event Action OnResume;
        public event Action OnSave;
        public event Action OnLoad;
        public event Action OnMenu;
        public event Action OnExit;

        public PauseMenu(SpriteFont font, int screenWidth, int screenHeight)
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
            int liftOffset = UIScalingManager.ScaleValue(BUTTON_LIFT_OFFSET);
            
            // Get centered positions for buttons
            var centerPos = UIScalingManager.GetCenteredPosition(buttonWidth, buttonHeight);
            int centerX = (int)centerPos.X;
            int centerY = (int)centerPos.Y;
            
            // Create buttons with scaled positions and lift offset
            _resumeButton = new Button(
                new Rectangle(centerX, centerY - 2 * (buttonHeight + spacing) - liftOffset, buttonWidth, buttonHeight),
                "Resume"
            );
            
            _saveButton = new Button(
                new Rectangle(centerX, centerY - buttonHeight - spacing - liftOffset, buttonWidth, buttonHeight),
                "Save Game"
            );
            
            _loadButton = new Button(
                new Rectangle(centerX, centerY - liftOffset, buttonWidth, buttonHeight),
                "Load Game"
            );
            
            _menuButton = new Button(
                new Rectangle(centerX, centerY + buttonHeight + spacing - liftOffset, buttonWidth, buttonHeight),
                "Menu"
            );
            
            _exitButton = new Button(
                new Rectangle(centerX, centerY + 2 * (buttonHeight + spacing) - liftOffset, buttonWidth, buttonHeight),
                "Exit"
            );
            
            // Create confirmation dialogs
            _menuConfirmationDialog = new ConfirmationDialog(_font, _screenWidth, _screenHeight);
            _menuConfirmationDialog.OnConfirm += () => OnMenu?.Invoke();
            _menuConfirmationDialog.OnCancel += () => { /* Dialog auto-hides */ };
            
            _exitConfirmationDialog = new ConfirmationDialog(_font, _screenWidth, _screenHeight);
            _exitConfirmationDialog.OnConfirm += () => OnExit?.Invoke();
            _exitConfirmationDialog.OnCancel += () => { /* Dialog auto-hides */ };
            
            // Subscribe to button events
            _resumeButton.OnClick += () => { Hide(); OnResume?.Invoke(); };
            _saveButton.OnClick += () => OnSave?.Invoke();
            _loadButton.OnClick += () => OnLoad?.Invoke();
            _menuButton.OnClick += () => _menuConfirmationDialog.Show("Exit to Menu", "Are you sure you want to exit to menu?\nAll unsaved progress will be lost.");
            _exitButton.OnClick += () => _exitConfirmationDialog.Show("Exit Game", "Are you sure you want to quit the game?\nAll unsaved progress will be lost.");
        }

        public void Show()
        {
            _isVisible = true;
            GameEventSystem.Instance.Publish<string>(GameEvents.GAME_PAUSED, "Pause Menu Opened");
        }

        public void Hide()
        {
            _isVisible = false;
            GameEventSystem.Instance.Publish<object>(GameEvents.GAME_RESUMED, (object)null);
        }

        public void TogglePause()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        public void ResetEscapeState()
        {
            _previousKeyboardState = Keyboard.GetState();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Load content for all buttons
            _resumeButton.LoadContent(content);
            _saveButton.LoadContent(content);
            _loadButton.LoadContent(content);
            _menuButton.LoadContent(content);
            _exitButton.LoadContent(content);
            
            // Load content for confirmation dialogs
            _menuConfirmationDialog.LoadContent(content, graphicsDevice);
            _exitConfirmationDialog.LoadContent(content, graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            if (!IsEnabled) return;
            
            // Update confirmation dialogs first (have higher priority)
            _menuConfirmationDialog.Update(gameTime);
            _exitConfirmationDialog.Update(gameTime);
            
            // If any confirmation dialog is visible, don't process pause menu input
            if (_menuConfirmationDialog.IsVisible || _exitConfirmationDialog.IsVisible) return;
            
            // No longer directly checking for Escape key here
            // Input is handled by Game1 and published via event system

            _resumeButton.Update(gameTime);
            _saveButton.Update(gameTime);
            _loadButton.Update(gameTime);
            _menuButton.Update(gameTime);
            _exitButton.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;
            
            // Semi-transparent background covering full screen
            var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), screenRect, Color.Black * GameConstants.UI.PAUSE_OVERLAY_OPACITY);
            
            // Panel background with dynamic dimensions to accommodate all buttons
            int panelWidth = UIScalingManager.ScaleValue(PANEL_WIDTH);
            
            // Calculate dynamic height based on button count and spacing
            int buttonHeight = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_HEIGHT);
            int spacing = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE);
            int totalButtonHeight = 5 * buttonHeight + 4 * spacing; // 5 buttons with 4 spacings
            int panelHeight = totalButtonHeight + UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE * 3); // Extra space for title and padding
            
            var panelCenterPos = UIScalingManager.GetCenteredPosition(panelWidth, panelHeight);
            var panelRect = new Rectangle(
                (int)panelCenterPos.X, 
                (int)panelCenterPos.Y, 
                panelWidth, 
                panelHeight
            );
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), panelRect, Color.DarkGray * GameConstants.UI.PANEL_OPACITY);
            
            // Panel border
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
            
            // Title "Paused" with scaled positioning
            string title = "PAUSED";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(
                (_screenWidth - titleSize.X) / 2,
                panelRect.Y + UIScalingManager.ScaleValue(TITLE_TOP_MARGIN)
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);
            
            // Buttons
            _resumeButton.Draw(spriteBatch);
            _saveButton.Draw(spriteBatch);
            _loadButton.Draw(spriteBatch);
            _menuButton.Draw(spriteBatch);
            _exitButton.Draw(spriteBatch);
            
            // Draw confirmation dialogs on top
            _menuConfirmationDialog.Draw(spriteBatch);
            _exitConfirmationDialog.Draw(spriteBatch);
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }


    }
}