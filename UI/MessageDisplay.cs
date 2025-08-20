using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Content;
using System;

namespace IsometricActionGame.UI
{
    public class MessageDisplay : IUIElement
    {
        // Message properties
        private string _currentMessage;
        private float _displayTime;
        private float _maxDisplayTime;
        private bool _isVisible;
        
        // Interface properties
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        
        // Visual properties
        private SpriteFont _font;
        private Color _textColor;
        private Color _backgroundColor;
        private Vector2 _position;
        private float _alpha;
        
        // Animation constants
            private const float FadeInDuration = GameConstants.Animation.IDLE_FRAME_TIME + GameConstants.Animation.IDLE_FRAME_TIME * 0.5f;
        private const float FadeOutDuration = GameConstants.Animation.IDLE_FRAME_TIME + GameConstants.Animation.IDLE_FRAME_TIME * 0.5f;
            private const float MaxAlpha = GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
        
        public MessageDisplay()
        {
            _position = new Vector2(GameConstants.UILayout.MESSAGE_DISPLAY_X, GameConstants.UILayout.MESSAGE_DISPLAY_Y);
            _textColor = Color.White;
            _backgroundColor = new Color(0, 0, 0, 180);
            _alpha = 0f;
        }
        
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("messageFont");
        }
        
        public void ShowMessage(string message, float duration)
        {
            _currentMessage = message;
            _maxDisplayTime = duration;
            _displayTime = 0f;
            _isVisible = true;
            _alpha = 0f; // Start with fade in
        }
        
        public void Update(GameTime gameTime)
        {
            if (!_isVisible)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _displayTime += deltaTime;
            
            // Handle fade in
            if (_displayTime < FadeInDuration)
            {
                _alpha = MathHelper.Lerp(0f, MaxAlpha, _displayTime / FadeInDuration);
            }
            // Handle fade out
            else if (_displayTime > _maxDisplayTime - FadeOutDuration)
            {
                float fadeOutProgress = (_displayTime - (_maxDisplayTime - FadeOutDuration)) / FadeOutDuration;
                _alpha = MathHelper.Lerp(MaxAlpha, 0f, fadeOutProgress);
            }
            else
            {
                _alpha = MaxAlpha;
            }
            
            // Hide message when time is up
            if (_displayTime >= _maxDisplayTime)
            {
                _isVisible = false;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible || string.IsNullOrEmpty(_currentMessage) || _font == null)
                return;
                
            // Calculate message size for background
            Vector2 messageSize = _font.MeasureString(_currentMessage);
            Vector2 backgroundSize = messageSize + new Vector2(GameConstants.UILayout.MESSAGE_PADDING_X, GameConstants.UILayout.MESSAGE_PADDING_Y); // Add padding
            
            // Calculate background position
            Vector2 backgroundPosition = _position - new Vector2(backgroundSize.X / 2, backgroundSize.Y / 2);
            
            // Create background rectangle
            Rectangle backgroundRect = new Rectangle(
                (int)backgroundPosition.X,
                (int)backgroundPosition.Y,
                (int)backgroundSize.X,
                (int)backgroundSize.Y
            );
            
            // Draw background with current alpha
            Color bgColor = _backgroundColor;
            bgColor.A = (byte)(_backgroundColor.A * _alpha);
            
            // Create a simple texture for background
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            spriteBatch.Draw(pixel, backgroundRect, bgColor);
            
            // Draw text with current alpha
            Color textColor = _textColor;
            textColor.A = (byte)(_textColor.A * _alpha);
            
            Vector2 textPosition = _position - messageSize / 2;
            spriteBatch.DrawString(_font, _currentMessage, textPosition, textColor);
        }
        
        public void SetPosition(Vector2 position)
        {
            _position = position;
        }
        
        public void SetColors(Color textColor, Color backgroundColor)
        {
            _textColor = textColor;
            _backgroundColor = backgroundColor;
        }
    }
} 