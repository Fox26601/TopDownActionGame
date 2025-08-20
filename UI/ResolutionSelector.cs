using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Settings;
using System;
using System.Collections.Generic;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// A resolution selector with left/right navigation buttons
    /// </summary>
    public class ResolutionSelector : IUIElement
    {
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        
        public Rectangle Bounds { get; private set; }
        public string Label { get; set; }
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public Color BorderColor { get; set; } = Color.Gray;
        public Color TextColor { get; set; } = Color.White;
        public Color LabelColor { get; set; } = Color.White;
        public Color ButtonColor { get; set; } = Color.LightGray;
        public Color ButtonHoverColor { get; set; } = Color.White;
        public Color ButtonDisabledColor { get; set; } = Color.DarkGray;
        
        // Event removed to prevent premature resolution changes
        // Changes are now applied only when "Apply" button is clicked
        
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Button _leftButton;
        private Button _rightButton;
        private List<Resolution> _availableResolutions;
        private int _currentIndex = 0;
        
        private const int BUTTON_WIDTH = 30;
        private const int BUTTON_HEIGHT = 30;
        private const int LABEL_MARGIN = 10;
        private const int BUTTON_MARGIN = 5;
        
        public Resolution CurrentResolution => _availableResolutions[_currentIndex];
        
        public ResolutionSelector(Rectangle bounds, string label, List<Resolution> resolutions)
        {
            Bounds = bounds;
            Label = label;
            _availableResolutions = new List<Resolution>(resolutions);
            
            // Initialize buttons
            var leftButtonBounds = new Rectangle(
                bounds.X, 
                bounds.Y + (bounds.Height - BUTTON_HEIGHT) / 2, 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            var rightButtonBounds = new Rectangle(
                bounds.Right - BUTTON_WIDTH, 
                bounds.Y + (bounds.Height - BUTTON_HEIGHT) / 2, 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            _leftButton = new Button(leftButtonBounds, "<");
            _rightButton = new Button(rightButtonBounds, ">");
        }
        
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("buttonFont");
            
            // Create a 1x1 white pixel texture for drawing
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Load button content
            _leftButton.LoadContent(content);
            _rightButton.LoadContent(content);
            
            // Set button colors
            _leftButton.FillColor = ButtonColor;
            _leftButton.HoverColor = ButtonHoverColor;
            _leftButton.TextColor = Color.Black;
            
            _rightButton.FillColor = ButtonColor;
            _rightButton.HoverColor = ButtonHoverColor;
            _rightButton.TextColor = Color.Black;
            
            // Subscribe to button events
            _leftButton.OnClick += OnLeftButtonClicked;
            _rightButton.OnClick += OnRightButtonClicked;
        }
        
        public void Update(GameTime gameTime)
        {
            if (!IsVisible || !IsEnabled) return;
            
            // Update buttons
            _leftButton.IsEnabled = IsEnabled && _currentIndex > 0;
            _rightButton.IsEnabled = IsEnabled && _currentIndex < _availableResolutions.Count - 1;
            
            _leftButton.Update(gameTime);
            _rightButton.Update(gameTime);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            
            // Draw background
            spriteBatch.Draw(_pixelTexture, Bounds, BackgroundColor);
            
            // Draw border
            DrawRectangle(spriteBatch, Bounds, BorderColor, 2);
            
            // Draw label if provided
            if (!string.IsNullOrEmpty(Label) && _font != null)
            {
                var labelPos = new Vector2(Bounds.X, Bounds.Y - _font.LineSpacing - LABEL_MARGIN);
                spriteBatch.DrawString(_font, Label, labelPos, LabelColor);
            }
            
            // Draw current resolution text
            if (_font != null)
            {
                var resolutionText = CurrentResolution.ToString();
                var textSize = _font.MeasureString(resolutionText);
                var textPos = new Vector2(
                    Bounds.Center.X - textSize.X / 2,
                    Bounds.Center.Y - textSize.Y / 2
                );
                
                var textColor = IsEnabled ? TextColor : ButtonDisabledColor;
                spriteBatch.DrawString(_font, resolutionText, textPos, textColor);
            }
            
            // Draw buttons
            _leftButton.Draw(spriteBatch);
            _rightButton.Draw(spriteBatch);
        }
        
        public void SetResolution(Resolution resolution)
        {
            System.Diagnostics.Debug.WriteLine($"ResolutionSelector.SetResolution: Called with {resolution.Width}x{resolution.Height}");
            
            int index = _availableResolutions.IndexOf(resolution);
            if (index >= 0 && index != _currentIndex)
            {
                System.Diagnostics.Debug.WriteLine($"ResolutionSelector.SetResolution: Found resolution at index {index}, updating from index {_currentIndex}");
                _currentIndex = index;
                // Don't invoke OnResolutionChanged here - let the parent handle when to apply changes
            }
            else if (index < 0)
            {
                System.Diagnostics.Debug.WriteLine($"ResolutionSelector.SetResolution: Resolution {resolution.Width}x{resolution.Height} not found in available resolutions");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ResolutionSelector.SetResolution: Resolution already at current index {_currentIndex}");
            }
        }
        
        public void SetResolutions(List<Resolution> resolutions)
        {
            _availableResolutions = new List<Resolution>(resolutions);
            _currentIndex = Math.Min(_currentIndex, _availableResolutions.Count - 1);
        }
        
        public void SetPosition(Vector2 position)
        {
            Bounds = new Rectangle((int)position.X, (int)position.Y, Bounds.Width, Bounds.Height);
            UpdateButtonPositions();
        }
        
        public void SetSize(int width, int height)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
            UpdateButtonPositions();
        }
        
        private void UpdateButtonPositions()
        {
            var leftButtonBounds = new Rectangle(
                Bounds.X, 
                Bounds.Y + (Bounds.Height - BUTTON_HEIGHT) / 2, 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            var rightButtonBounds = new Rectangle(
                Bounds.Right - BUTTON_WIDTH, 
                Bounds.Y + (Bounds.Height - BUTTON_HEIGHT) / 2, 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            _leftButton.Bounds = leftButtonBounds;
            _rightButton.Bounds = rightButtonBounds;
        }
        
        private void OnLeftButtonClicked()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                // Don't invoke OnResolutionChanged here - let the parent handle when to apply changes
            }
        }
        
        private void OnRightButtonClicked()
        {
            if (_currentIndex < _availableResolutions.Count - 1)
            {
                _currentIndex++;
                // Don't invoke OnResolutionChanged here - let the parent handle when to apply changes
            }
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
