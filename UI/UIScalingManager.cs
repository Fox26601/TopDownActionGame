using Microsoft.Xna.Framework;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// Manages UI scaling for different screen resolutions
    /// Uses a reference resolution (1280x720) and scales all UI elements proportionally
    /// </summary>
    public static class UIScalingManager
    {
        // Reference resolution (base resolution for UI design)
        private const int REFERENCE_WIDTH = 1280;
        private const int REFERENCE_HEIGHT = 720;
        
        // Current screen dimensions
        private static int _currentWidth;
        private static int _currentHeight;
        
        // Scaling factors
        private static float _scaleX;
        private static float _scaleY;
        private static float _scaleFactor; // Use the smaller scale to maintain aspect ratio
        
        // Minimum and maximum scale limits
        private const float MIN_SCALE = GameConstants.LayerDepth.DEFAULT_DEPTH;
        private const float MAX_SCALE = GameConstants.Graphics.MAX_SPRITE_SCALE;
        
        /// <summary>
        /// Initialize the scaling manager with current screen dimensions
        /// </summary>
        public static void Initialize(int screenWidth, int screenHeight)
        {
            _currentWidth = screenWidth;
            _currentHeight = screenHeight;
            
            // Calculate scaling factors
            _scaleX = (float)_currentWidth / REFERENCE_WIDTH;
            _scaleY = (float)_currentHeight / REFERENCE_HEIGHT;
            
            // Use the smaller scale to maintain aspect ratio and prevent UI overflow
            _scaleFactor = MathHelper.Min(_scaleX, _scaleY);
            
            // Clamp scale factor to reasonable limits
            _scaleFactor = MathHelper.Clamp(_scaleFactor, MIN_SCALE, MAX_SCALE);
        }
        
        /// <summary>
        /// Scale a value based on the current screen resolution
        /// </summary>
        public static float ScaleValue(float value)
        {
            return value * _scaleFactor;
        }
        
        /// <summary>
        /// Scale an integer value based on the current screen resolution
        /// </summary>
        public static int ScaleValue(int value)
        {
            return (int)(value * _scaleFactor);
        }
        
        /// <summary>
        /// Scale a Vector2 based on the current screen resolution
        /// </summary>
        public static Vector2 ScaleVector(Vector2 vector)
        {
            return vector * _scaleFactor;
        }
        
        /// <summary>
        /// Scale a Rectangle based on the current screen resolution
        /// </summary>
        public static Rectangle ScaleRectangle(Rectangle rectangle)
        {
            return new Rectangle(
                ScaleValue(rectangle.X),
                ScaleValue(rectangle.Y),
                ScaleValue(rectangle.Width),
                ScaleValue(rectangle.Height)
            );
        }
        
        /// <summary>
        /// Scale a Point based on the current screen resolution
        /// </summary>
        public static Point ScalePoint(Point point)
        {
            return new Point(
                ScaleValue(point.X),
                ScaleValue(point.Y)
            );
        }
        
        /// <summary>
        /// Get a scaled position that maintains relative positioning from reference resolution
        /// </summary>
        public static Vector2 GetScaledPosition(float referenceX, float referenceY)
        {
            return new Vector2(
                referenceX * _scaleX,
                referenceY * _scaleY
            );
        }
        
        /// <summary>
        /// Get a centered position for UI elements
        /// </summary>
        public static Vector2 GetCenteredPosition(float elementWidth, float elementHeight)
        {
            return new Vector2(
                (_currentWidth - elementWidth) / 2f,
                (_currentHeight - elementHeight) / 2f
            );
        }
        
        /// <summary>
        /// Get a centered position for UI elements with offset
        /// </summary>
        public static Vector2 GetCenteredPositionWithOffset(float elementWidth, float elementHeight, float offsetX, float offsetY)
        {
            var centerPos = GetCenteredPosition(elementWidth, elementHeight);
            return centerPos + new Vector2(offsetX, offsetY);
        }
        
        /// <summary>
        /// Get current scale factor
        /// </summary>
        public static float GetScaleFactor()
        {
            return _scaleFactor;
        }
        
        /// <summary>
        /// Get current screen dimensions
        /// </summary>
        public static (int Width, int Height) GetScreenDimensions()
        {
            return (_currentWidth, _currentHeight);
        }
        
        /// <summary>
        /// Get reference resolution
        /// </summary>
        public static (int Width, int Height) GetReferenceResolution()
        {
            return (REFERENCE_WIDTH, REFERENCE_HEIGHT);
        }
        
        /// <summary>
        /// Scale font size based on current resolution
        /// </summary>
        public static float ScaleFontSize(float baseFontSize)
        {
            return baseFontSize * _scaleFactor;
        }
        
        /// <summary>
        /// Get scaled margins for UI elements
        /// </summary>
        public static (float Top, float Bottom, float Left, float Right) GetScaledMargins(float baseMargin)
        {
            float scaledMargin = ScaleValue(baseMargin);
            return (scaledMargin, scaledMargin, scaledMargin, scaledMargin);
        }
        
        /// <summary>
        /// Get scaled spacing between UI elements
        /// </summary>
        public static float GetScaledSpacing(float baseSpacing)
        {
            return ScaleValue(baseSpacing);
        }
    }
}

