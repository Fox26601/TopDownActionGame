using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using IsometricActionGame.UI;

namespace IsometricActionGame
{
    // Displays console messages with fade-out effect
    public class ConsoleDisplay
    {
        private readonly List<ConsoleMessage> _messages;
        private readonly Queue<ConsoleMessage> _messageQueue;
        private SpriteFont _font;
        private Vector2 _position;
        private int _screenWidth;
        private int _screenHeight;
        private float _updateAccumulator;
        private bool _layoutDirty = true;
        
        private const float UpdateIntervalSeconds = GameConstants.Timing.CONSOLE_FADE_DURATION / GameConstants.Timing.CONSOLE_FULL_OPACITY_DURATION;
        private const int MaxMessages = 10;
        private const float MessageSpacing = 5f;
        private const float FadeStartSeconds = 15f;
        private const float FadeEndSeconds = 20f;
        
        public ConsoleDisplay(Vector2 position)
        {
            _position = position;
            _messages = new List<ConsoleMessage>();
            _messageQueue = new Queue<ConsoleMessage>();
        }
        
        // Initialize console display with screen dimensions for scaling
        public void Initialize(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Update position for current resolution
            UpdatePosition();
        }
        
        // Update console position for current screen resolution
        private void UpdatePosition()
        {
            // Position console in top-right corner with scaled margins
            int marginX = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_MEDIUM);
            int marginY = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_MEDIUM);
            
            _position = new Vector2(_screenWidth - 350 - marginX, marginY); // Increased width for more messages
        }
        
        // Set console position manually
        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void LoadContent(SpriteFont font)
        {
            _font = font;
        }

        public void AddMessage(string text, Color color)
        {
            _messageQueue.Enqueue(new ConsoleMessage(text, color));
        }

        public void Update(GameTime gameTime)
        {
            var currentTime = DateTime.Now;
            _updateAccumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_updateAccumulator >= UpdateIntervalSeconds)
            {
                // process in fixed one-second steps to avoid drift
                _updateAccumulator -= UpdateIntervalSeconds;
                
                // Process all queued messages at once
                while (_messageQueue.Count > 0)
                {
                    var next = _messageQueue.Dequeue();
                    // Reset timestamp to the display time so fade timings start now
                    next.TimeStamp = DateTime.Now;
                    _messages.Add(next);
                }
                
                // Keep only the last MaxMessages
                if (_messages.Count > MaxMessages)
                {
                    _messages.RemoveRange(0, _messages.Count - MaxMessages);
                }
                
                // Remove expired messages (older than 20 seconds)
                _messages.RemoveAll(m => (currentTime - m.TimeStamp).TotalSeconds > 20);
                
                // Mark layout dirty so it will be recomputed lazily during draw
                _layoutDirty = true;
            }
        }

        private void RecomputeLayout(DateTime currentTime)
        {
            float maxWidth = 0f;
            float totalHeight = 0f;
            
            for (int i = 0; i < _messages.Count; i++)
            {
                var msg = _messages[i];
                float alpha = msg.GetAlpha(currentTime);
                if (alpha <= 0f) continue;
                Vector2 size = _font.MeasureString(msg.Text);
                if (size.X > maxWidth) maxWidth = size.X;
                totalHeight += size.Y + MessageSpacing;
            }
            if (totalHeight > 0f)
            {
                totalHeight -= MessageSpacing; // remove last extra spacing
            }
            
            // Update layout bounds
            _layoutBounds = new Rectangle((int)_position.X, (int)_position.Y, (int)maxWidth, (int)totalHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null || _messages.Count == 0) return;
            
            var currentTime = DateTime.Now;
            
            // Recompute layout if dirty
            if (_layoutDirty)
            {
                RecomputeLayout(currentTime);
                _layoutDirty = false;
            }
            
            // Draw messages from bottom to top (newest at bottom)
            float currentY = _position.Y;
            
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                var msg = _messages[i];
                float alpha = msg.GetAlpha(currentTime);
                
                if (alpha <= 0f) continue;
                
                var color = new Color(msg.Color, alpha);
                var position = new Vector2(_position.X, currentY);
                
                spriteBatch.DrawString(_font, msg.Text, position, color);
                
                currentY += _font.MeasureString(msg.Text).Y + MessageSpacing;
            }
        }
        
        private Rectangle _layoutBounds;
        
        // Get current layout bounds for collision detection
        public Rectangle GetBounds()
        {
            return _layoutBounds;
        }
        
        // Clear all messages
        public void Clear()
        {
            _messages.Clear();
            _messageQueue.Clear();
            _layoutDirty = true;
        }
    }
    
    // Represents a console message with timestamp and color
    public class ConsoleMessage
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public DateTime TimeStamp { get; set; }
        
        public ConsoleMessage(string text, Color color)
        {
            Text = text;
            Color = color;
            TimeStamp = DateTime.Now;
        }
        
        // Calculate alpha value based on message age for fade effect
        public float GetAlpha(DateTime currentTime)
        {
            var age = (currentTime - TimeStamp).TotalSeconds;
            
            if (age < 15f) return 1f;
            if (age > 20f) return 0f;
            
            // Fade from 1 to 0 over 5 seconds
            return 1f - (float)((age - 15f) / 5f);
        }
    }
} 