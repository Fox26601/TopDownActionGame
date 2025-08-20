using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace IsometricActionGame.UI
{
    public class Button
    {
        public Rectangle Bounds { get; set; }
        public Texture2D Texture { get; set; }
        public Color FillColor { get; set; } = Color.LightGray;
        public Color HoverColor { get; set; } = Color.Gray;
        public string Text { get; set; }
        public Color TextColor { get; set; } = Color.Black;
        public int TextSize { get; set; } = 16;
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public event Action OnClick;

        private SpriteFont _font;
        private bool _isHovered;
        private bool _wasMouseDown;

        public Button(Rectangle bounds, string text = null)
        {
            Bounds = bounds;
            Text = text;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _font = content.Load<SpriteFont>("buttonFont");
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible || !IsEnabled) return;
            var mouse = Mouse.GetState();
            _isHovered = Bounds.Contains(mouse.Position);
            bool isMouseDown = mouse.LeftButton == ButtonState.Pressed;
            if (_isHovered && isMouseDown && !_wasMouseDown)
            {
                OnClick?.Invoke();
            }
            _wasMouseDown = isMouseDown;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            Color drawColor = _isHovered ? HoverColor : FillColor;
            if (Texture != null)
                spriteBatch.Draw(Texture, Bounds, drawColor);
            else
            {
                // Solid color fill
                Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
                spriteBatch.Draw(pixel, Bounds, drawColor);
            }
            if (!string.IsNullOrEmpty(Text) && _font != null)
            {
                Vector2 textSize = _font.MeasureString(Text);
                Vector2 textPos = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(_font, Text, textPos, TextColor);
            }
        }
        
        /// <summary>
        /// Update button bounds for new screen resolution
        /// </summary>
        public void UpdateBounds(Rectangle newBounds)
        {
            Bounds = newBounds;
        }
    }
} 