using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;

namespace IsometricActionGame
{
    public class Fireball : IEntity, IPausable
    {
        public Vector2 WorldPosition { get; private set; }
        public float LayerDepth => GameConstants.LayerDepth.DEBUG_DEPTH; // Maximum priority rendering
        public Vector2 FacingDirection => _direction; // Direction for sprite mirroring
        public float Scale => GameConstants.SpriteScale.FIREBALL_SCALE; // Updated to SpriteScale
        public float BaseHitboxRadius => GameConstants.Combat.FIREBALL_HITBOX_RADIUS; // Updated to Combat
        public int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 16;
        public float HitboxRadius => BaseHitboxRadius * Scale;
        private Vector2 _direction;
        private float _speed = 8f;
        private float _damage = 15f;
        private float _lifetime = 5f; // 5 seconds lifetime
        private float _currentLifetime = 0f;
        
        public bool IsActive { get; private set; } = true;
        public bool HasHit { get; private set; } = false;
        
        // IPausable implementation
        public bool IsPaused { get; set; } = false;
        
        // Animations
        private AnimatedSprite _flyAnimation;
        private AnimatedSprite _hitAnimation;
        private AnimatedSprite _currentAnimation;
        
        public event Action<Fireball, float> OnHit; // Fireball, damage

        public Fireball(Vector2 position, Vector2 direction, float damage, float speed = 8f, float lifetime = 5f)
        {
            Reset(position, direction, damage, speed, lifetime);
        }

        /// <summary>
        /// Reset fireball for reuse from object pool
        /// </summary>
        public void Reset(Vector2 position, Vector2 direction, float damage, float speed, float lifetime)
        {
            WorldPosition = position;
            _direction = Vector2.Normalize(direction);
            _damage = damage;
            _speed = speed;
            _lifetime = lifetime;
            _currentLifetime = 0f;
            IsActive = true;
            HasHit = false;
            IsPaused = false;
            _currentAnimation = _flyAnimation;
            _currentAnimation?.Reset(); // Reset animation to start from beginning
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                // Bones_SingleSkull_Fly - 8 frames (2 rows, 4 columns)
                var flyTexture = content.Load<Texture2D>("Bones_SingleSkull_Fly");
                _flyAnimation = new AnimatedSprite(flyTexture, 4, 2, GameConstants.Animation.FIREBALL_FRAME_TIME, true);
                
                // Bones_SingleSkull_Hit - 4 frames (1 row, 4 columns)
                var hitTexture = content.Load<Texture2D>("Bones_SingleSkull_Hit");
                _hitAnimation = new AnimatedSprite(hitTexture, 4, 1, GameConstants.Animation.FIREBALL_FRAME_TIME, false);
                
                _currentAnimation = _flyAnimation;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load Fireball animations");
            }
        }

        public void Update(GameTime gameTime)
        {
            Update(gameTime, IsPaused);
        }

        public void Update(GameTime gameTime, bool isPaused = false)
        {
            if (!IsActive) return;
            
            // If paused, only update animation
            if (isPaused || IsPaused)
            {
                _currentAnimation?.Update(gameTime);
                return;
            }
            
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentLifetime += deltaTime;
            
            // Deactivate if lifetime exceeded
            if (_currentLifetime >= _lifetime)
            {
                IsActive = false;
                return;
            }
            
            // Move fireball
            Vector2 movement = _direction * _speed * deltaTime;
            WorldPosition += movement;
            
            // Check map boundaries
            if (WorldPosition.X < 0 || WorldPosition.X >= GameMap.Width || 
                WorldPosition.Y < 0 || WorldPosition.Y >= GameMap.Height)
            {
                Hit();
            }
            
            _currentAnimation?.Update(gameTime);
        }

        public void Hit()
        {
            if (HasHit) return;
            
            HasHit = true;
            _currentAnimation = _hitAnimation;
            _currentAnimation?.Reset();
            OnHit?.Invoke(this, _damage);
            
            // Deactivate after hit animation
            IsActive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;
            
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            // Determine sprite effects based on facing direction
            var spriteEffects = SpriteEffects.None;
            if (_direction.X < 0) // Moving left
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            
            if (_currentAnimation != null)
            {
                _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, spriteEffects, LayerDepth, Scale);
            }
            else
            {
                // Fallback debug rectangle if animation not loaded
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.White });
                var debugSize = 80;
                var debugRect = new Rectangle((int)screenPos.X - debugSize/2, (int)screenPos.Y - debugSize/2, debugSize, debugSize);
                spriteBatch.Draw(debugTexture, debugRect, null, Color.Magenta, 0f, Vector2.Zero, spriteEffects, LayerDepth);
            }
        }
    }
} 