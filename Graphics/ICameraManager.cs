using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace IsometricActionGame.Graphics
{
    /// <summary>
    /// Interface for camera management with viewport awareness
    /// Follows OOP principles for better testability and maintainability
    /// </summary>
    public interface ICameraManager
    {
        /// <summary>
        /// Current camera transform matrix
        /// </summary>
        Matrix Transform { get; }
        
        /// <summary>
        /// Current camera position in world coordinates
        /// </summary>
        Vector2 Position { get; }
        
        /// <summary>
        /// Current viewport the camera is working with
        /// </summary>
        Viewport CurrentViewport { get; }
        
        /// <summary>
        /// Camera movement interpolation speed
        /// </summary>
        float LerpFactor { get; set; }
        
        /// <summary>
        /// Event triggered when camera position changes
        /// </summary>
        event Action<Vector2> OnPositionChanged;
        
        /// <summary>
        /// Initialize camera with viewport information
        /// </summary>
        void Initialize(Viewport viewport);
        
        /// <summary>
        /// Update viewport when resolution changes
        /// </summary>
        void UpdateViewport(Viewport newViewport);
        
        /// <summary>
        /// Smoothly follow a target position
        /// </summary>
        void Follow(Vector2 targetScreenPosition);
        
        /// <summary>
        /// Instantly snap camera to position
        /// </summary>
        void SetPosition(Vector2 screenPosition);
        
        /// <summary>
        /// Reset camera to default position
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Convert world coordinates to screen coordinates
        /// </summary>
        Vector2 WorldToScreen(Vector2 worldPosition);
        
        /// <summary>
        /// Convert screen coordinates to world coordinates
        /// </summary>
        Vector2 ScreenToWorld(Vector2 screenPosition);
        
        /// <summary>
        /// Check if a world position is visible in the current viewport
        /// </summary>
        bool IsVisible(Vector2 worldPosition, float margin = 0f);
    }
}
