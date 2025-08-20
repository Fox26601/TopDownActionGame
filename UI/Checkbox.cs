using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// A checkbox UI element with customizable appearance and behavior
    /// </summary>
    public class Checkbox : IUIElement
    {
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public bool IsChecked { get; private set; }
        
        public Rectangle Bounds { get; private set; }
        public string Label { get; set; }
        public Color CheckColor { get; set; } = Color.White;
        public Color UncheckColor { get; set; } = Color.Gray;
        public Color HoverColor { get; set; } = Color.LightGray;
        public Color DisabledColor { get; set; } = Color.DarkGray;
        public Color TextColor { get; set; } = Color.White;
        public Color LabelColor { get; set; } = Color.White;
        
        // Event removed to prevent premature fullscreen changes
        // Changes are now applied only when "Apply" button is clicked
        
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private bool _isHovered = false;
        private bool _wasPressed = false;
        
        private const int CHECKBOX_SIZE = 20;
        private const int LABEL_MARGIN = 10;
        
        public Checkbox(Rectangle bounds, string label = "")
        {
            Bounds = bounds;
            Label = label;
        }
        
        public Checkbox(Vector2 position, string label = "")
        {
            Bounds = new Rectangle((int)position.X, (int)position.Y, CHECKBOX_SIZE, CHECKBOX_SIZE);
            Label = label;
        }
        
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("buttonFont");
            
            // Create a 1x1 white pixel texture for drawing
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        public void Update(GameTime gameTime)
        {
            if (!IsVisible || !IsEnabled) return;
            
            var mouse = Mouse.GetState();
            var mousePos = new Point(mouse.X, mouse.Y);
            
            // Check if mouse is hovering over checkbox
            _isHovered = Bounds.Contains(mousePos);
            
            // Handle mouse input
            if (_isHovered && mouse.LeftButton == ButtonState.Pressed && !_wasPressed)
            {
                _wasPressed = true;
            }
            else if (_wasPressed && mouse.LeftButton == ButtonState.Released)
            {
                _wasPressed = false;
                if (_isHovered)
                {
                    Toggle();
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            
            // Determine colors based on state
            Color checkboxColor = GetCheckboxColor();
            Color textColor = IsEnabled ? TextColor : DisabledColor;
            Color labelColor = IsEnabled ? LabelColor : DisabledColor;
            
            // Draw checkbox background
            spriteBatch.Draw(_pixelTexture, Bounds, checkboxColor);
            
            // Draw checkbox border
            DrawRectangle(spriteBatch, Bounds, Color.Black, 2);
            
            // Draw checkmark if checked
            if (IsChecked)
            {
                DrawCheckmark(spriteBatch);
            }
            
            // Draw label if provided
            if (!string.IsNullOrEmpty(Label) && _font != null)
            {
                var labelPos = new Vector2(Bounds.Right + LABEL_MARGIN, Bounds.Center.Y - _font.LineSpacing / 2);
                spriteBatch.DrawString(_font, Label, labelPos, labelColor);
            }
        }
        
        public void SetChecked(bool isChecked)
        {
            if (IsChecked != isChecked)
            {
                IsChecked = isChecked;
                // Don't invoke OnCheckedChanged here - let the parent handle when to apply changes
            }
        }
        
        public void Toggle()
        {
            if (IsEnabled)
            {
                IsChecked = !IsChecked;
                // Don't invoke OnCheckedChanged here - let the parent handle when to apply changes
            }
        }
        
        public void SetPosition(Vector2 position)
        {
            Bounds = new Rectangle((int)position.X, (int)position.Y, Bounds.Width, Bounds.Height);
        }
        
        public void SetSize(int width, int height)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
        }
        
        private Color GetCheckboxColor()
        {
            if (!IsEnabled)
                return DisabledColor;
            
            if (_isHovered)
                return HoverColor;
            
            return IsChecked ? CheckColor : UncheckColor;
        }
        
        private void DrawCheckmark(SpriteBatch spriteBatch)
        {
            // Draw a simple checkmark using lines
            var center = Bounds.Center;
            var size = Math.Min(Bounds.Width, Bounds.Height) / 4;
            
            // Draw checkmark lines
            var start1 = new Vector2(center.X - size, center.Y);
            var end1 = new Vector2(center.X, center.Y + size);
            var start2 = new Vector2(center.X, center.Y + size);
            var end2 = new Vector2(center.X + size, center.Y - size);
            
            DrawLine(spriteBatch, start1, end1, Color.Black, 2);
            DrawLine(spriteBatch, start2, end2, Color.Black, 2);
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

