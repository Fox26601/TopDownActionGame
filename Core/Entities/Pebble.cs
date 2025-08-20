using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using IsometricActionGame.Events;
using IsometricActionGame.SaveSystem;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;

namespace IsometricActionGame
{
    public class Pebble : IEntity, IAttackable, IPausable, ISaveable
    {
        public Vector2 WorldPosition { get; set; }
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition); // Use grid-based depth calculation
        public Vector2 FacingDirection { get; } = Vector2.Zero; // Pebbles don't have facing direction
        public float Scale => GameConstants.SpriteScale.PEBBLE_SCALE;
        public float BaseHitboxRadius => GameConstants.Combat.PEBBLE_HITBOX_RADIUS;
        public int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 32;
        public float HitboxRadius => BaseHitboxRadius * Scale;
        
        public event Action<IAttackable, int> OnDamage;
        public event Action<IAttackable, int> OnHeal;
        public event Action<IAttackable> OnDeath;
        
        private float _shootCooldown;
        private float _currentShootCooldown;
        private int _damage;
        
        private HealthSystem _healthSystem;
        public int MaxHealth => _healthSystem.MaxHealth;
        public int CurrentHealth => _healthSystem.CurrentHealth;
        public bool IsAlive => _healthSystem.IsAlive;
        
        public EnemyState State { get; private set; } = EnemyState.Idle;
        
        public bool IsPaused { get; set; } = false;
        
        /// <summary>
        /// Check if death animation has finished
        /// </summary>
        public bool IsDeathAnimationFinished => !IsAlive && State == EnemyState.Dead && _currentAnimation?.IsFinished == true;
        
        private AnimatedSprite _idleAnimation;
        private AnimatedSprite _hitAnimation;
        private AnimatedSprite _deathAnimation;
        private AnimatedSprite _currentAnimation;

        public Pebble(Vector2 position, float shootCooldown, int damage)
        {
            System.Diagnostics.Debug.WriteLine($"Pebble constructor called at position {position}");
            
            WorldPosition = position;
            _shootCooldown = shootCooldown;
            _currentShootCooldown = 0f;
            _damage = damage;
            FacingDirection = Vector2.Zero;
            
            _healthSystem = new HealthSystem(GameConstants.Health.PEBBLE_MAX_HEALTH);
            
            _healthSystem.OnDamage += (health, damage) => 
            {
                OnDamage?.Invoke(this, damage);
                GameEventSystem.Instance?.Publish(GameEvents.DAMAGE_DEALT, new { Entity = this, Damage = damage });
            };
            _healthSystem.OnHeal += (health, amount) => 
            {
                OnHeal?.Invoke(this, amount);
            };
            _healthSystem.OnDeath += (health) => 
            {
                OnDeath?.Invoke(this);
            };
            
            // Initialize with idle state
            State = EnemyState.Idle;
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent called, content is null: {content == null}");
                
                if (content == null)
                {
                    System.Diagnostics.Debug.WriteLine("Pebble.LoadContent: ContentManager is null!");
                    return;
                }
                
                // Pebble_Idle - 4 frames (1 row, 4 columns)
                System.Diagnostics.Debug.WriteLine("Pebble.LoadContent: Loading Pebble_Idle texture");
                var pebbleIdleTexture = content.Load<Texture2D>("Pebble_Idle");
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: Pebble_Idle loaded, texture is null: {pebbleIdleTexture == null}");
                if (pebbleIdleTexture != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: Pebble_Idle texture size: {pebbleIdleTexture.Width}x{pebbleIdleTexture.Height}");
                }
                _idleAnimation = new AnimatedSprite(pebbleIdleTexture, 4, 1, GameConstants.Animation.PEBBLE_FRAME_TIME, true);
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: _idleAnimation created, is null: {_idleAnimation == null}");
                
                // Pebble_Hit - 5 frames (2 rows, 4 columns, using only first 5 frames)
                System.Diagnostics.Debug.WriteLine("Pebble.LoadContent: Loading Pebble_Hit texture");
                var pebbleHitTexture = content.Load<Texture2D>("Pebble_Hit");
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: Pebble_Hit loaded, texture is null: {pebbleHitTexture == null}");
                _hitAnimation = new AnimatedSprite(pebbleHitTexture, 4, 2, 5, GameConstants.Animation.HIT_FRAME_TIME, false);
                
                // Pebble_Death - 7 frames (2 rows, 4 columns, using only first 7 frames)
                System.Diagnostics.Debug.WriteLine("Pebble.LoadContent: Loading Pebble_Death texture");
                var pebbleDeathTexture = content.Load<Texture2D>("Pebble_Death");
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: Pebble_Death loaded, texture is null: {pebbleDeathTexture == null}");
                if (pebbleDeathTexture != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent: Pebble_Death texture size: {pebbleDeathTexture.Width}x{pebbleDeathTexture.Height}");
                }
                // Pebble_Death - 7 frames from 2x4 grid (first 4 from row 0, first 3 from row 1)
                _deathAnimation = new AnimatedSprite(pebbleDeathTexture, 4, 2, 7, GameConstants.Animation.DEATH_FRAME_TIME, false);
                
                _currentAnimation = _idleAnimation;
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent completed successfully. _currentAnimation is null: {_currentAnimation == null}");
                
                // Ensure we have a valid animation
                if (_currentAnimation == null && _idleAnimation != null)
                {
                    _currentAnimation = _idleAnimation;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Pebble animations: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent exception: {ex}");
                System.Diagnostics.Debug.WriteLine($"Pebble.LoadContent stack trace: {ex.StackTrace}");
            }
        }

        public void Update(GameTime gameTime)
        {
            Update(gameTime, IsPaused);
        }

        public void Update(GameTime gameTime, bool isPaused = false)
        {
            _currentAnimation?.Update(gameTime);
            
            if (isPaused || IsPaused)
            {
                return;
            }
            
            // Debug death animation state
            if (!IsAlive && State == EnemyState.Dead && gameTime.TotalGameTime.TotalSeconds % GameConstants.Timing.CONSOLE_FADE_DURATION < GameConstants.Animation.IDLE_FRAME_TIME)
            {
                System.Diagnostics.Debug.WriteLine($"Pebble.Update: Death animation state - IsFinished={_currentAnimation?.IsFinished}, CurrentFrame={_currentAnimation?.CurrentFrame}, FrameCount={_currentAnimation?.FrameCount}");
            }
            
            // Handle death animation completion
            if (!IsAlive && State == EnemyState.Dead && _currentAnimation != null && _currentAnimation.IsFinished)
            {
                // Death animation finished, entity should be removed
                // This will be handled by the game manager checking this state
                System.Diagnostics.Debug.WriteLine("Pebble.Update: Death animation finished, ready for removal");
                return;
            }
            
            if (IsAlive)
            {
                if (_currentShootCooldown > 0)
                {
                    _currentShootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                
                switch (State)
                {
                    case EnemyState.Hit:
                        if (_currentAnimation.IsFinished)
                        {
                            State = EnemyState.Idle;
                            _currentAnimation = _idleAnimation;
                        }
                        break;
                        
                    case EnemyState.Idle:
                        break;
                }
            }
        }

        public void ShootAtPlayer(Player player)
        {
            if (_currentShootCooldown <= 0 && IsAlive)
            {
                ProjectileManager.Instance.CreateFireball(WorldPosition, player.WorldPosition, _damage);
                _currentShootCooldown = _shootCooldown;
                
                GameEventSystem.Instance?.Publish(GameEvents.PROJECTILE_CREATED, new { Type = "Fireball", Position = WorldPosition, Target = player.WorldPosition });
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            if (_currentAnimation != null)
            {
                _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, LayerDepth, Scale);
            }
        }

        public void TakeDamage(int amount, Vector2? attackerPosition = null)
        {
            if (!IsAlive) return;
            
            _healthSystem.TakeDamage(amount, attackerPosition);
            
            GameEventSystem.Instance?.Publish(GameEvents.DAMAGE_DEALT, new { Target = this, Damage = amount, AttackerPosition = attackerPosition });
            
            if (!IsAlive)
            {
                State = EnemyState.Dead;
                _currentAnimation = _deathAnimation;
                _currentAnimation.Reset();
                
                System.Diagnostics.Debug.WriteLine($"Pebble.TakeDamage: Death animation set. Animation null: {_currentAnimation == null}, IsFinished: {_currentAnimation?.IsFinished}, FrameCount: {_currentAnimation?.FrameCount}");
                
                GameEventSystem.Instance?.Publish(GameEvents.ENEMY_DEFEATED, new { EnemyType = "Pebble", Position = WorldPosition });
            }
            else
            {
                State = EnemyState.Hit;
                _currentAnimation = _hitAnimation;
                _currentAnimation.Reset();
            }
        }

        public void Heal(int amount)
        {
            _healthSystem.Heal(amount);
        }
        
        /// <summary>
        /// Reset the pebble to its initial state
        /// </summary>
        public void Reset()
        {
            _healthSystem.Reset();
            State = EnemyState.Idle;
            _currentAnimation = _idleAnimation;
            _currentShootCooldown = 0f;
            
            if (_currentAnimation != null)
            {
                _currentAnimation.Reset();
            }
        }
        
        // ISaveable implementation
        public string SaveId => "Pebble";
        
        /// <summary>
        /// Reset Pebble to starting state (position, health, state, etc.)
        /// </summary>
        public void ResetToStartingState()
        {
            // Reset position to starting position
            WorldPosition = GameConstants.World.PEBBLE_START_POSITION;
            
            // Reset health to full
            _healthSystem.RestoreFullHealth();
            
            // Reset state
            State = EnemyState.Idle;
            
            // Reset cooldowns
            _currentShootCooldown = 0f;
            
            // Reset animation to idle
            _currentAnimation = _idleAnimation;
            _currentAnimation?.Reset();
            
            System.Diagnostics.Debug.WriteLine("Pebble reset to starting state");
        }
        
        public SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            data.SetValue("Position", WorldPosition);
            data.SetValue("CurrentHealth", CurrentHealth);
            data.SetValue("MaxHealth", MaxHealth);
            data.SetValue("State", State.ToString());
            data.SetValue("ShootCooldown", _shootCooldown);
            data.SetValue("Damage", _damage);
            return data;
        }
        
        public void Deserialize(SaveData data)
        {
            if (data == null) return;
            
            WorldPosition = data.GetValue<Vector2>("Position", WorldPosition);
            var currentHealth = data.GetValue<int>("CurrentHealth", CurrentHealth);
            var maxHealth = data.GetValue<int>("MaxHealth", MaxHealth);
            _shootCooldown = data.GetValue<float>("ShootCooldown", _shootCooldown);
            _damage = data.GetValue<int>("Damage", _damage);
            
            // Restore health
            _healthSystem.SetHealth(currentHealth);
            
            // Restore state
            if (data.HasValue("State"))
            {
                var stateString = data.GetValue<string>("State", "Idle");
                if (Enum.TryParse<EnemyState>(stateString, out var state))
                {
                    State = state;
                }
            }
        }
    }
}
