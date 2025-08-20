using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using IsometricActionGame.Events;
using IsometricActionGame.Settings;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// Modal confirmation dialog for important game decisions
    /// </summary>
    public class ConfirmationDialog : IUIElement
    {
        private SpriteFont _font;
        private Button _confirmButton;
        private Button _cancelButton;
        private bool _isVisible;
        private int _screenWidth;
        private int _screenHeight;
        private string _message;
        private string _title;
        private bool _enterPressed;
        // private bool _escapePressed; // Removed unused field
        private InputSettings _inputSettings;
        
        // UI Layout constants
        private const int DIALOG_WIDTH = GameConstants.UILayout.CONFIRMATION_DIALOG_WIDTH;
        private const int DIALOG_HEIGHT = GameConstants.UILayout.CONFIRMATION_DIALOG_HEIGHT;
        private const int BUTTON_WIDTH = 120;
        private const int BUTTON_HEIGHT = 40;
        private const int BUTTON_SPACING = 20;
        private const int TEXT_MARGIN = 20;
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        
        public event Action OnConfirm;
        public event Action OnCancel;

        public ConfirmationDialog(SpriteFont font, int screenWidth, int screenHeight)
        {
            _font = font;
            _isVisible = false;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _message = "";
            _title = "";
            _inputSettings = InputSettings.Instance;
            
            // Initialize UI scaling
            UIScalingManager.Initialize(screenWidth, screenHeight);
            
            // Use scaled dimensions
            int scaledDialogWidth = UIScalingManager.ScaleValue(DIALOG_WIDTH);
            int scaledDialogHeight = UIScalingManager.ScaleValue(DIALOG_HEIGHT);
            int scaledButtonWidth = UIScalingManager.ScaleValue(BUTTON_WIDTH);
            int scaledButtonHeight = UIScalingManager.ScaleValue(BUTTON_HEIGHT);
            int scaledButtonSpacing = UIScalingManager.ScaleValue(BUTTON_SPACING);
            int scaledTextMargin = UIScalingManager.ScaleValue(TEXT_MARGIN);
            
            // Dialog centering with scaled dimensions
            int centerX = _screenWidth / 2;
            int centerY = _screenHeight / 2;
            
            // Button positioning with scaled dimensions
            int buttonY = centerY + (scaledDialogHeight / 2) - scaledButtonHeight - scaledTextMargin;
            int confirmButtonX = centerX - scaledButtonWidth - (scaledButtonSpacing / 2);
            int cancelButtonX = centerX + (scaledButtonSpacing / 2);
            
            // Create buttons with scaled dimensions
            _confirmButton = new Button(
                new Rectangle(confirmButtonX, buttonY, scaledButtonWidth, scaledButtonHeight),
                "Yes"
            );
            
            _cancelButton = new Button(
                new Rectangle(cancelButtonX, buttonY, scaledButtonWidth, scaledButtonHeight),
                "No"
            );
            
            // Subscribe to button events
            _confirmButton.OnClick += () => {
                Hide();
                OnConfirm?.Invoke();
            };
            
            _cancelButton.OnClick += () => {
                Hide();
                OnCancel?.Invoke();
            };
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            _confirmButton.LoadContent(content);
            _cancelButton.LoadContent(content);
        }

        public void Show(string title, string message)
        {
            _title = title;
            _message = message;
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isVisible || !IsEnabled) return;
            
            var keyboardState = Keyboard.GetState();
            
            // Handle Enter key to confirm
            if (keyboardState.IsKeyDown(_inputSettings.Confirm) && !_enterPressed)
            {
                _enterPressed = true;
                Hide();
                OnConfirm?.Invoke();
            }
            else if (!keyboardState.IsKeyDown(_inputSettings.Confirm))
            {
                _enterPressed = false;
            }
            
            // ESC is now handled by InputHandler through event system
            // Remove direct ESC handling to avoid conflicts
            
            _confirmButton.Update(gameTime);
            _cancelButton.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;
            
            // Draw overlay background
            var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), screenRect, Color.Black * GameConstants.UI.OVERLAY_OPACITY);
            
            // Use scaled dimensions
            int scaledDialogWidth = UIScalingManager.ScaleValue(DIALOG_WIDTH);
            int scaledDialogHeight = UIScalingManager.ScaleValue(DIALOG_HEIGHT);
            int scaledTextMargin = UIScalingManager.ScaleValue(TEXT_MARGIN);
            
            // Dialog positioning with scaled dimensions
            int centerX = _screenWidth / 2;
            int centerY = _screenHeight / 2;
            var dialogRect = new Rectangle(
                centerX - scaledDialogWidth / 2,
                centerY - scaledDialogHeight / 2,
                scaledDialogWidth,
                scaledDialogHeight
            );
            
            // Draw dialog background
            spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), dialogRect, Color.DarkGray * GameConstants.UI.PANEL_OPACITY);
            
            // Draw dialog border
            DrawBorder(spriteBatch, dialogRect, Color.White, UIScalingManager.ScaleValue(2));
            
            // Draw title with text wrapping
            if (!string.IsNullOrEmpty(_title))
            {
                var maxTitleWidth = (float)scaledDialogWidth - UIScalingManager.ScaleValue(40); // Leave margins
                var wrappedTitle = WrapText(_title, maxTitleWidth);
                var titleY = dialogRect.Y + scaledTextMargin;
                
                foreach (var line in wrappedTitle)
                {
                    var titleSize = _font.MeasureString(line);
                    var titlePos = new Vector2(
                        centerX - titleSize.X / 2,
                        titleY
                    );
                    spriteBatch.DrawString(_font, line, titlePos, Color.White);
                    titleY += (int)(titleSize.Y + UIScalingManager.ScaleValue(2));
                }
            }
            
            // Draw message with text wrapping
            if (!string.IsNullOrEmpty(_message))
            {
                var maxMessageWidth = (float)scaledDialogWidth - UIScalingManager.ScaleValue(40); // Leave margins
                var wrappedMessage = WrapText(_message, maxMessageWidth);
                var messageY = centerY - (int)(wrappedMessage.Count * _font.LineSpacing) / 2;
                
                foreach (var line in wrappedMessage)
                {
                    var messageSize = _font.MeasureString(line);
                    var messagePos = new Vector2(
                        centerX - messageSize.X / 2,
                        messageY
                    );
                    spriteBatch.DrawString(_font, line, messagePos, Color.LightGray);
                    messageY += (int)(messageSize.Y + UIScalingManager.ScaleValue(2));
                }
            }
            
            // Draw buttons
            _confirmButton.Draw(spriteBatch);
            _cancelButton.Draw(spriteBatch);
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            var pixel = CreatePixelTexture(spriteBatch.GraphicsDevice);
            
            // Top border
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom border
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left border
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right border
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
        
        /// <summary>
        /// Wrap text to fit within specified width
        /// </summary>
        private List<string> WrapText(string text, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";
            
            foreach (var word in words)
            {
                var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                var testSize = _font.MeasureString(testLine);
                
                if (testSize.X <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        // Single word is too long, add it anyway
                        lines.Add(word);
                    }
                }
            }
            
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine);
            }
            
            return lines;
        }
    }
}