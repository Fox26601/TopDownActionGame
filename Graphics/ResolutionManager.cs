using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Settings;
using System;
using System.Diagnostics;

namespace IsometricActionGame.Graphics
{
    /// <summary>
    /// Robust resolution manager implementing safe graphics operations
    /// Follows Singleton pattern for centralized graphics management
    /// Implements proper error handling and fallback mechanisms
    /// </summary>
    public sealed class ResolutionManager : IResolutionManager
    {
        private static ResolutionManager _instance;
        private static readonly object _lock = new object();
        
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private bool _isInitialized = false;
        
        // Safe fallback resolution
        private static readonly Resolution FALLBACK_RESOLUTION = new Resolution(1280, 720);
        private const int MIN_WIDTH = 800;
        private const int MIN_HEIGHT = 600;
        private const int MAX_WIDTH = 7680;  // 8K width
        private const int MAX_HEIGHT = 4320; // 8K height
        
        public Rectangle CurrentViewport { get; private set; }
        public Resolution CurrentResolution { get; private set; }
        public bool IsFullscreen { get; private set; }
        public bool IsGraphicsDeviceReady => _graphicsDevice != null && !_graphicsDevice.IsDisposed;
        
        public event Action<Resolution> OnResolutionChanged;
        public event Action<bool> OnFullscreenChanged;
        public event Action<Viewport> OnViewportChanged;
        
        /// <summary>
        /// Singleton instance getter with thread safety
        /// </summary>
        public static ResolutionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ResolutionManager();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private ResolutionManager()
        {
            CurrentResolution = FALLBACK_RESOLUTION;
            IsFullscreen = false;
            CurrentViewport = new Rectangle(0, 0, FALLBACK_RESOLUTION.Width, FALLBACK_RESOLUTION.Height);
        }
        
        public void Initialize(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            
            _graphics = graphics;
            _graphicsDevice = graphicsDevice;
            
            // Note: GraphicsDevice events are handled by MonoGame internally
            // We'll update viewport manually when needed
            
            UpdateViewport();
            _isInitialized = true;
            

        }
        
        public bool ApplyResolution(Resolution resolution, bool isFullscreen)
        {
            if (!_isInitialized || _graphics == null)
            {
                return false;
            }
            
            if (!IsResolutionSupported(resolution))
            {
                resolution = GetSafeFallbackResolution();
            }
            
            try
            {
                // Store previous values for rollback
                var prevResolution = CurrentResolution;
                var prevFullscreen = IsFullscreen;
                
                // Apply new settings
                _graphics.PreferredBackBufferWidth = resolution.Width;
                _graphics.PreferredBackBufferHeight = resolution.Height;
                _graphics.IsFullScreen = isFullscreen;
                
                _graphics.ApplyChanges();
                
                // Verify the changes were applied successfully
                if (IsGraphicsDeviceReady)
                {
                    UpdateViewport();
                    
                    // Update internal state only after successful application
                    if (CurrentResolution != prevResolution)
                    {
                        CurrentResolution = resolution;
                        OnResolutionChanged?.Invoke(resolution);
                    }
                    
                    if (IsFullscreen != prevFullscreen)
                    {
                        IsFullscreen = isFullscreen;
                        OnFullscreenChanged?.Invoke(isFullscreen);
                    }
                    
                    return true;
                }
                else
                {
                    // Rollback on failure
                    RollbackSettings(prevResolution, prevFullscreen);
                    return false;
                }
            }
            catch (Exception)
            {
                // Emergency fallback to safe settings
                try
                {
                    ResetToDefaults();
                }
                catch (Exception)
                {
                    // Critical error - even fallback failed
                }
                
                return false;
            }
        }
        
        public bool ApplyFullscreenMode(bool isFullscreen)
        {
            return ApplyResolution(CurrentResolution, isFullscreen);
        }
        
        public bool IsResolutionSupported(Resolution resolution)
        {
            return resolution.Width >= MIN_WIDTH && 
                   resolution.Height >= MIN_HEIGHT && 
                   resolution.Width <= MAX_WIDTH && 
                   resolution.Height <= MAX_HEIGHT;
        }
        
        public Resolution GetSafeFallbackResolution()
        {
            return FALLBACK_RESOLUTION;
        }
        
        public void UpdateViewport()
        {
            if (!IsGraphicsDeviceReady)
            {
                return;
            }
            
            var viewport = _graphicsDevice.Viewport;
            CurrentViewport = new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
            
            // Update current resolution to match actual viewport
            var actualResolution = new Resolution(viewport.Width, viewport.Height);
            if (CurrentResolution != actualResolution)
            {
                CurrentResolution = actualResolution;
            }
            
            OnViewportChanged?.Invoke(viewport);
        }
        
        public void ResetToDefaults()
        {
            if (!_isInitialized || _graphics == null)
                return;
            
            try
            {
                _graphics.PreferredBackBufferWidth = FALLBACK_RESOLUTION.Width;
                _graphics.PreferredBackBufferHeight = FALLBACK_RESOLUTION.Height;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();
                
                UpdateViewport();
                
                CurrentResolution = FALLBACK_RESOLUTION;
                IsFullscreen = false;
                

            }
            catch (Exception)
            {
                // Failed to reset to defaults
            }
        }
        
        public bool ResetAndApply()
        {
            if (!_isInitialized || _graphics == null)
            {
                return false;
            }
            
            try
            {
                // Store previous values for event notification
                var prevResolution = CurrentResolution;
                var prevFullscreen = IsFullscreen;
                
                // Apply default settings immediately
                _graphics.PreferredBackBufferWidth = FALLBACK_RESOLUTION.Width;
                _graphics.PreferredBackBufferHeight = FALLBACK_RESOLUTION.Height;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();
                
                // Verify the changes were applied successfully
                if (IsGraphicsDeviceReady)
                {
                    UpdateViewport();
                    
                    // Update internal state and trigger events
                    if (CurrentResolution != FALLBACK_RESOLUTION)
                    {
                        CurrentResolution = FALLBACK_RESOLUTION;
                        OnResolutionChanged?.Invoke(FALLBACK_RESOLUTION);
                    }
                    
                    if (IsFullscreen != false)
                    {
                        IsFullscreen = false;
                        OnFullscreenChanged?.Invoke(false);
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private void RollbackSettings(Resolution prevResolution, bool prevFullscreen)
        {
            try
            {
                _graphics.PreferredBackBufferWidth = prevResolution.Width;
                _graphics.PreferredBackBufferHeight = prevResolution.Height;
                _graphics.IsFullScreen = prevFullscreen;
                _graphics.ApplyChanges();
                
                UpdateViewport();
            }
            catch (Exception)
            {
                // Last resort - try to reset to defaults
                ResetToDefaults();
            }
        }
        

        
        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            _isInitialized = false;
        }
    }
}
