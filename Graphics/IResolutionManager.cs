using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Settings;
using System;

namespace IsometricActionGame.Graphics
{
    /// <summary>
    /// Interface for managing graphics resolution and viewport settings
    /// Follows OOP principles with clear abstraction for scalability
    /// </summary>
    public interface IResolutionManager
    {
        /// <summary>
        /// Current screen dimensions
        /// </summary>
        Rectangle CurrentViewport { get; }
        
        /// <summary>
        /// Current resolution settings
        /// </summary>
        Resolution CurrentResolution { get; }
        
        /// <summary>
        /// Whether the game is in fullscreen mode
        /// </summary>
        bool IsFullscreen { get; }
        
        /// <summary>
        /// Whether the graphics device is valid and ready for rendering
        /// </summary>
        bool IsGraphicsDeviceReady { get; }
        
        /// <summary>
        /// Event triggered when resolution changes successfully
        /// </summary>
        event Action<Resolution> OnResolutionChanged;
        
        /// <summary>
        /// Event triggered when fullscreen mode changes
        /// </summary>
        event Action<bool> OnFullscreenChanged;
        
        /// <summary>
        /// Event triggered when viewport changes (for camera updates)
        /// </summary>
        event Action<Viewport> OnViewportChanged;
        
        /// <summary>
        /// Initialize the resolution manager with graphics device
        /// </summary>
        void Initialize(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice);
        
        /// <summary>
        /// Apply resolution settings safely with validation
        /// </summary>
        bool ApplyResolution(Resolution resolution, bool isFullscreen);
        
        /// <summary>
        /// Apply only fullscreen mode change
        /// </summary>
        bool ApplyFullscreenMode(bool isFullscreen);
        
        /// <summary>
        /// Validate resolution compatibility with current hardware
        /// </summary>
        bool IsResolutionSupported(Resolution resolution);
        
        /// <summary>
        /// Get safe fallback resolution
        /// </summary>
        Resolution GetSafeFallbackResolution();
        
        /// <summary>
        /// Update viewport information after graphics device changes
        /// </summary>
        void UpdateViewport();
        
        /// <summary>
        /// Reset to safe default settings
        /// </summary>
        void ResetToDefaults();
        
        /// <summary>
        /// Reset to defaults and immediately apply them
        /// </summary>
        bool ResetAndApply();
    }
}
