using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame
{
    // Centralized projectile manager with object pooling for efficient projectile management
    public class ProjectileManager
    {
        private static ProjectileManager _instance;
        public static ProjectileManager Instance => _instance ??= new ProjectileManager();

        // Object pool for fireballs
        private readonly Queue<Fireball> _fireballPool;
        private readonly List<Fireball> _activeFireballs;
        private readonly int _maxPoolSize = GameConstants.Projectiles.MAX_POOL_SIZE;
        private readonly int _maxActiveProjectiles = 100;

        // Projectile constants
        private const float FIREBALL_LIFETIME = GameConstants.Timing.CONSOLE_FULL_OPACITY_DURATION; // Fireball lifetime
        private const float FIREBALL_SPEED = GameConstants.Movement.FIREBALL_SPEED; // Fireball speed
        private const float FIREBALL_DAMAGE = GameConstants.Damage.FIREBALL_DAMAGE; // Fireball damage

        // Content management
        private ContentManager _content;
        private bool _contentLoaded = false;

        public event Action<Fireball> OnProjectileCreated;
        public event Action<Fireball> OnProjectileDestroyed;

        private ProjectileManager()
        {
            _fireballPool = new Queue<Fireball>();
            _activeFireballs = new List<Fireball>();
        }

        // Initialize the projectile manager with content manager
        public void Initialize(ContentManager content)
        {
            _content = content;
            LoadContent();
        }

        // Load content for all projectile types
        private void LoadContent()
        {
            if (_contentLoaded || _content == null) 
            {
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: LoadContent skipped - contentLoaded: {_contentLoaded}, content null: {_content == null}");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("ProjectileManager: Starting content load...");
                
                // Pre-load some fireballs into the pool
                for (int i = 0; i < 10; i++)
                {
                    var fireball = CreateNewFireball();
                    System.Diagnostics.Debug.WriteLine($"ProjectileManager: Loading content for fireball {i + 1}/10");
                    fireball.LoadContent(_content);
                    _fireballPool.Enqueue(fireball);
                    System.Diagnostics.Debug.WriteLine($"ProjectileManager: Created and queued fireball {i + 1}/10");
                }

                _contentLoaded = true;
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Content loaded successfully. Pool size: {_fireballPool.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Failed to load content: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Stack trace: {ex.StackTrace}");
            }
        }

        // Create a new fireball projectile
        public Fireball CreateFireball(Vector2 position, Vector2 targetPosition, int damage)
        {
            System.Diagnostics.Debug.WriteLine($"ProjectileManager: Creating fireball at {position} with target {targetPosition}");
            System.Diagnostics.Debug.WriteLine($"ProjectileManager: Pool size: {_fireballPool.Count}, Active: {_activeFireballs.Count}, Content loaded: {_contentLoaded}");
            
            Vector2 direction = Vector2.Normalize(targetPosition - position);
            
                            // Try to get from pool first
            if (_fireballPool.Count > 0)
            {
                var fireball = _fireballPool.Dequeue();
                fireball.Reset(position, direction, damage, FIREBALL_SPEED, FIREBALL_LIFETIME);
                
                // Always ensure content is loaded for reused fireballs
                if (_content != null)
                {
                    System.Diagnostics.Debug.WriteLine("ProjectileManager: Ensuring content is loaded for reused fireball");
                    fireball.LoadContent(_content);
                }
                
                _activeFireballs.Add(fireball);
                OnProjectileCreated?.Invoke(fireball);
                
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Reused fireball from pool. Active: {_activeFireballs.Count}, Pool: {_fireballPool.Count}");
                return fireball;
            }

            // Create new if pool is empty and we haven't reached max active limit
            if (_activeFireballs.Count < _maxActiveProjectiles)
            {
                System.Diagnostics.Debug.WriteLine("ProjectileManager: Creating new fireball (pool empty)");
                var fireball = CreateNewFireball();
                fireball.Reset(position, direction, damage, FIREBALL_SPEED, FIREBALL_LIFETIME);
                
                // Always ensure content is loaded for new fireballs
                if (_content != null)
                {
                    System.Diagnostics.Debug.WriteLine("ProjectileManager: Loading content for new fireball");
                    fireball.LoadContent(_content);
                }
                
                _activeFireballs.Add(fireball);
                OnProjectileCreated?.Invoke(fireball);
                
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Created new fireball. Active: {_activeFireballs.Count}, Pool: {_fireballPool.Count}");
                return fireball;
            }

            System.Diagnostics.Debug.WriteLine("ProjectileManager: Cannot create fireball - max limit reached");
            return null;
        }

        /// <summary>
        /// Create a new fireball instance
        /// </summary>
        private Fireball CreateNewFireball()
        {
            return new Fireball(Vector2.Zero, Vector2.Zero, FIREBALL_DAMAGE, FIREBALL_SPEED, FIREBALL_LIFETIME);
        }

        /// <summary>
        /// Update all active projectiles
        /// </summary>
        public void Update(GameTime gameTime)
        {
            Update(gameTime, false);
        }

        /// <summary>
        /// Update all active projectiles with pause state
        /// </summary>
        public void Update(GameTime gameTime, bool isPaused)
        {
            if (_activeFireballs.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Updating {_activeFireballs.Count} active fireballs, Paused: {isPaused}");
            }
            
            for (int i = _activeFireballs.Count - 1; i >= 0; i--)
            {
                var fireball = _activeFireballs[i];
                fireball.Update(gameTime, isPaused);

                // Check if projectile should be returned to pool
                if (!fireball.IsActive)
                {
                    ReturnToPool(fireball, i);
                }
            }
        }

        /// <summary>
        /// Return projectile to pool for reuse
        /// </summary>
        private void ReturnToPool(Fireball fireball, int index)
        {
            _activeFireballs.RemoveAt(index);
            
            // Only add to pool if we haven't reached max pool size
            if (_fireballPool.Count < _maxPoolSize)
            {
                _fireballPool.Enqueue(fireball);
                System.Diagnostics.Debug.WriteLine($"ProjectileManager: Returned fireball to pool. Active: {_activeFireballs.Count}, Pool: {_fireballPool.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ProjectileManager: Pool full, discarding fireball");
            }
            
            OnProjectileDestroyed?.Invoke(fireball);
        }

        /// <summary>
        /// Get all active fireballs
        /// </summary>
        public List<Fireball> GetActiveFireballs()
        {
            return new List<Fireball>(_activeFireballs);
        }

        /// <summary>
        /// Draw all active projectiles
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            System.Diagnostics.Debug.WriteLine($"ProjectileManager.Draw: Called with {_activeFireballs.Count} active fireballs");
            
            if (_activeFireballs.Count > 0)
            {
                foreach (var fireball in _activeFireballs)
                {
                    System.Diagnostics.Debug.WriteLine($"ProjectileManager.Draw: Fireball at {fireball.WorldPosition}, IsActive: {fireball.IsActive}, HasHit: {fireball.HasHit}");
                    if (fireball.IsActive)
                    {
                        fireball.Draw(spriteBatch);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all projectiles (for game reset)
        /// </summary>
        public void Clear()
        {
            _activeFireballs.Clear();
            _fireballPool.Clear();
            System.Diagnostics.Debug.WriteLine("ProjectileManager: All projectiles cleared");
        }

        /// <summary>
        /// Get statistics for debugging
        /// </summary>
        public (int active, int pooled) GetStats()
        {
            return (_activeFireballs.Count, _fireballPool.Count);
        }
    }
}
