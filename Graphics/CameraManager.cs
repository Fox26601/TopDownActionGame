using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using System;
using System.Diagnostics;

namespace IsometricActionGame.Graphics
{
    /// <summary>
    /// Advanced camera manager with viewport awareness and smooth following
    /// Implements proper OOP principles with encapsulation and single responsibility
    /// Automatically handles viewport changes for seamless resolution switching
    /// </summary>
    public sealed class CameraManager : ICameraManager
    {
        private Viewport _viewport;
        private Vector2 _currentPosition;
        private Vector2 _targetPosition;
        private bool _isInitialized = false;
        
        // Default camera settings from GameConstants
        private const float DEFAULT_LERP_FACTOR = GameConstants.Timing.CAMERA_LERP_FACTOR;
        
        public Matrix Transform { get; private set; }
        public Vector2 Position => _currentPosition;
        public Viewport CurrentViewport => _viewport;
        public float LerpFactor { get; set; } = DEFAULT_LERP_FACTOR;
        
        public event Action<Vector2> OnPositionChanged;
        
        public void Initialize(Viewport viewport)
        {
            _viewport = viewport;
            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            Transform = Matrix.Identity;
            _isInitialized = true;
            
            Debug.WriteLine($"CameraManager initialized with viewport: {viewport.Width}x{viewport.Height}");
            UpdateTransform();
        }
        
        public void UpdateViewport(Viewport newViewport)
        {
            if (!_isInitialized)
            {
                Initialize(newViewport);
                return;
            }
            
            var oldViewport = _viewport;
            _viewport = newViewport;
            
            // Maintain relative camera position when viewport changes
            if (oldViewport.Width > 0 && oldViewport.Height > 0)
            {
                // Calculate position ratio relative to old viewport center
                float ratioX = (_currentPosition.X + oldViewport.Width / 2f) / oldViewport.Width;
                float ratioY = (_currentPosition.Y + oldViewport.Height / 2f) / oldViewport.Height;
                
                // Apply the same ratio to new viewport
                _currentPosition.X = (ratioX * newViewport.Width) - newViewport.Width / 2f;
                _currentPosition.Y = (ratioY * newViewport.Height) - newViewport.Height / 2f;
                _targetPosition = _currentPosition;
            }
            
            UpdateTransform();
            OnPositionChanged?.Invoke(_currentPosition);
            
            Debug.WriteLine($"CameraManager viewport updated: {oldViewport.Width}x{oldViewport.Height} → {newViewport.Width}x{newViewport.Height}");
        }
        
        public void Follow(Vector2 targetScreenPosition)
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("CameraManager: Cannot follow - not initialized");
                return;
            }
            
            _targetPosition = targetScreenPosition;
            
            // Smoothly interpolate the camera's position towards the target
            _currentPosition = Vector2.Lerp(_currentPosition, _targetPosition, LerpFactor);
            
            UpdateTransform();
            OnPositionChanged?.Invoke(_currentPosition);
        }
        
        public void SetPosition(Vector2 screenPosition)
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("CameraManager: Cannot set position - not initialized");
                return;
            }
            
            _currentPosition = screenPosition;
            _targetPosition = screenPosition;
            
            UpdateTransform();
            OnPositionChanged?.Invoke(_currentPosition);
        }
        
        public void Reset()
        {
            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            UpdateTransform();
            OnPositionChanged?.Invoke(_currentPosition);
            
            Debug.WriteLine("CameraManager: Reset to origin");
        }
        
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            // Use GridHelper for consistent coordinate conversion
            return GridHelper.WorldToScreen(worldPosition);
        }
        
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            // Use GridHelper for consistent coordinate conversion
            return GridHelper.ScreenToWorld(screenPosition);
        }
        
        public bool IsVisible(Vector2 worldPosition, float margin = 0f)
        {
            if (!_isInitialized)
                return false;
            
            var screenPosition = WorldToScreen(worldPosition);
            var transformedPosition = Vector2.Transform(screenPosition, Transform);
            
            return transformedPosition.X >= -margin &&
                   transformedPosition.Y >= -margin &&
                   transformedPosition.X <= _viewport.Width + margin &&
                   transformedPosition.Y <= _viewport.Height + margin;
        }
        
        private void UpdateTransform()
        {
            if (!_isInitialized)
                return;
            
            // Round the position to avoid sub-pixel jittering which can make sprites look shaky
            var roundedPosition = new Vector3(
                -(float)Math.Round(_currentPosition.X), 
                -(float)Math.Round(_currentPosition.Y), 
                0);
            
            var position = Matrix.CreateTranslation(roundedPosition);
            var offset = Matrix.CreateTranslation(_viewport.Width / 2f, _viewport.Height / 2f, 0);
            
            Transform = position * offset;
        }
    }
}
