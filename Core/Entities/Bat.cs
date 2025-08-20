using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using IsometricActionGame.Events;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;

namespace IsometricActionGame
{
    public class Bat : IEntity, IAttackable, IPausable
    {
        public Vector2 WorldPosition { get; set; }
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition); // Use grid-based depth calculation
        public Vector2 FacingDirection => Vector2.Zero;
        public float Scale => GameConstants.SpriteScale.BAT_SCALE; // Universal scale for sprite, hitbox, and interaction radius
        public float BaseHitboxRadius => GameConstants.Combat.BAT_HITBOX_RADIUS;
        public int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 32;
        public float HitboxRadius => BaseHitboxRadius * Scale;
        
    
        public event Action<IAttackable, int> OnDamage;
        public event Action<IAttackable, int> OnHeal;
        public event Action<IAttackable> OnDeath;
        
        private Vector2 _start;
        private Vector2 _end;
        private Vector2 _direction;
        private float _speed = 2f;
        private float _attackCooldown = 0f;
        private float _attackCooldownValue = 5f; // Store the cooldown value
        public float _attackRange; // Will be set in constructor
        public float _chaseRange; // Will be set in constructor
        private float _chaseSpeed = 3f; // Chase speed
        private float _attackDamage = 20f; // Attack damage
        
    
        private HealthSystem _healthSystem;
        public int MaxHealth => _healthSystem.MaxHealth;
        public int CurrentHealth => _healthSystem.CurrentHealth;
        public bool IsAlive => _healthSystem.IsAlive;
        
        public EnemyState State { get; private set; } = EnemyState.Flying;
        
        // Pause state
        public bool IsPaused { get; set; } = false;
        
        /// <summary>
        /// Check if death animation has finished
        /// </summary>
        public bool IsDeathAnimationFinished => !IsAlive && State == EnemyState.Dead && _currentAnimation?.IsFinished == true;
        
        // Animations
        private AnimatedSprite _flyAnimation;
        private AnimatedSprite _attackAnimation;
        private AnimatedSprite _hitAnimation;
        private AnimatedSprite _deathAnimation;
        private AnimatedSprite _currentAnimation;
        
        public Bat(Vector2 start, Vector2 initialDirection, float attackCooldown = 5f, float speed = 2f, float chaseSpeed = 3f, float attackDamage = 20f)
        {
            System.Diagnostics.Debug.WriteLine($"Bat constructor called at position {start}");
            
            _start = start;
            _end = start; // Not used for patrolling
            WorldPosition = start;
            _direction = Vector2.Normalize(initialDirection);
                    _attackRange = GameConstants.Attack.BAT_ATTACK_RANGE;
        _chaseRange = GameConstants.Attack.BAT_CHASE_RANGE;
            _attackCooldownValue = attackCooldown; // Set attack cooldown value from constructor
            _speed = speed; // Set movement speed from constructor
            _chaseSpeed = chaseSpeed; // Set chase speed from constructor
            _attackDamage = attackDamage; // Set attack damage from constructor
            
            // Initialize health system
            _healthSystem = new HealthSystem(GameConstants.Health.BAT_MAX_HEALTH);
            
            // Subscribe to health system events and publish to universal event system
            _healthSystem.OnDamage += (health, damage) => OnDamage?.Invoke(this, damage);
            _healthSystem.OnHeal += (health, amount) => OnHeal?.Invoke(this, amount);
            _healthSystem.OnDeath += (health) => OnDeath?.Invoke(this);

            // Event subscriptions to GameEventSystem from _healthSystem - now using anonymous objects
            _healthSystem.OnDamage += (health, damage) => GameEventSystem.Instance?.Publish(GameEvents.PLAYER_DAMAGED, new { Entity = this, Damage = damage }); // Corrected to use anonymous object
            _healthSystem.OnHeal += (health, amount) => GameEventSystem.Instance?.Publish(GameEvents.PLAYER_HEALED, new { Entity = this, Amount = amount });   // Corrected to use anonymous object
            _healthSystem.OnDeath += (health) => GameEventSystem.Instance?.Publish(GameEvents.PLAYER_DIED, new { Entity = this }); // Corrected to use anonymous object
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Bat.LoadContent called");
                
                // Bat animations with correct frame counts
                var batFlyTexture = content.Load<Texture2D>("Bat_Fly");
                _flyAnimation = new AnimatedSprite(batFlyTexture, 4, 1, GameConstants.Animation.IDLE_FRAME_TIME, true);
                
                var batAttackTexture = content.Load<Texture2D>("Bat_Attack");
                _attackAnimation = new AnimatedSprite(batAttackTexture, 4, 2, GameConstants.Animation.ATTACK_FRAME_TIME, false);
                
                var batHitTexture = content.Load<Texture2D>("Bat_Hit");
                _hitAnimation = new AnimatedSprite(batHitTexture, 4, 2, GameConstants.Animation.HIT_FRAME_TIME, false);
                
                var batDeathTexture = content.Load<Texture2D>("Bat_Death");
                _deathAnimation = new AnimatedSprite(batDeathTexture, 4, 3, GameConstants.Animation.DEATH_FRAME_TIME, false);
                
                _currentAnimation = _flyAnimation;
                System.Diagnostics.Debug.WriteLine("Bat.LoadContent completed successfully");
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load Bat animations");
                System.Diagnostics.Debug.WriteLine("Bat.LoadContent exception occurred");
            }
        }
        
        public void TakeDamage(int damage, Vector2? attackerPosition = null)
        {
            if (!IsAlive) return;
            
            _healthSystem.TakeDamage(damage, attackerPosition);
            
            if (!IsAlive)
            {
                State = EnemyState.Dead;
                _currentAnimation = _deathAnimation;
                _currentAnimation.Reset();
                // Publish enemy defeated event directly here with anonymous object
                GameEventSystem.Instance?.Publish(GameEvents.ENEMY_DEFEATED, new { EnemyType = "Bat", Position = WorldPosition });
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

        public void Update(GameTime gameTime)
        {
            Update(gameTime, null, false);
        }

        public void Update(GameTime gameTime, Player player)
        {
            Update(gameTime, player, false);
        }

        public void Update(GameTime gameTime, bool isPaused)
        {
            Update(gameTime, null, isPaused);
        }

        public void Update(GameTime gameTime, Player player, bool isPaused = false)
        {
    
            _currentAnimation?.Update(gameTime);
            
    
            if (isPaused || IsPaused || !IsAlive)
            {
                return;
            }
            
    
            if (player == null)
            {
                return;
            }
            
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _attackCooldown -= deltaTime;
            
            // Handle different states
            switch (State)
            {
                case EnemyState.Hit:
                    if (_currentAnimation.IsFinished)
                    {
                        State = EnemyState.Flying;
                        _currentAnimation = _flyAnimation;
                    }
                    break;
                    
                case EnemyState.Attacking:
                    if (_currentAnimation.IsFinished)
                    {
                        State = EnemyState.Flying;
                        _currentAnimation = _flyAnimation;
                    }
                    break;
                    
                case EnemyState.Flying:
                case EnemyState.Chasing:
                    float distanceToPlayer = Vector2.Distance(WorldPosition, player.WorldPosition);
                    
                    if (distanceToPlayer <= _chaseRange && State != EnemyState.Chasing)
                    {
                        // Start chasing
                        State = EnemyState.Chasing;
                    }
                    else if (distanceToPlayer > _chaseRange && State == EnemyState.Chasing)
                    {
                        // Stop chasing, return to patrolling
                        State = EnemyState.Flying;
                        _direction = Vector2.Normalize(_end - WorldPosition);
                    }
                    
                    if (State == EnemyState.Chasing)
                    {
                        // Chase player but stop at attack range
                        if (distanceToPlayer > _attackRange)
                        {
                            _direction = Vector2.Normalize(player.WorldPosition - WorldPosition);
                            var newPosition = WorldPosition + _direction * _chaseSpeed * deltaTime;
                            
                            // Clamp to map boundaries
                            newPosition.X = MathHelper.Clamp(newPosition.X, 0, GameMap.Width - 1);
                            newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, GameMap.Height - 1);
                            
                            WorldPosition = newPosition;
                        }
                        
                        // Attack if in range and cooldown is over
                        if (distanceToPlayer <= _attackRange && _attackCooldown <= 0 && State != EnemyState.Attacking)
                        {
                            Attack(player);
                        }
                    }
                    else
                    {
                        // Normal patrol behavior
                        var newPosition = WorldPosition + _direction * _speed * deltaTime;
                        
                        // Check boundaries and rotate direction 89 degrees right
                        if (newPosition.X <= 0 || newPosition.X >= GameMap.Width - 1 ||
                            newPosition.Y <= 0 || newPosition.Y >= GameMap.Height - 1)
                        {
                            // Rotate direction 89 degrees right (clockwise)
                            float currentAngle = MathF.Atan2(_direction.Y, _direction.X);
                            float newAngle = currentAngle - MathF.PI * 89f / 180f; // 89 degrees in radians (negative for clockwise)
                            _direction = new Vector2(MathF.Cos(newAngle), MathF.Sin(newAngle));
                        }
                        else
                        {
                            WorldPosition = newPosition;
                        }
                    }
                    break;
            }
            
            _currentAnimation?.Update(gameTime);
        }
        
        private void Attack(Player player)
        {
            if (State == EnemyState.Attacking || State == EnemyState.Hit || State == EnemyState.Dead) return;
            if (_attackCooldown > 0) return;
            
            State = EnemyState.Attacking;
            _currentAnimation = _attackAnimation;
            _currentAnimation.Reset();
            _attackCooldown = _attackCooldownValue; // Reset cooldown using stored value
            
            // Deal damage to player
            player.TakeDamage((int)_attackDamage); // Use configurable damage
            GameEventSystem.Instance.Publish(GameEvents.PLAYER_ATTACK, new { Position = WorldPosition, Direction = _direction }); // Corrected to use anonymous object
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            System.Diagnostics.Debug.WriteLine($"Bat.Draw: Position={WorldPosition}, ScreenPos={screenPos}, CurrentAnimation={_currentAnimation != null}, State={State}, IsAlive={IsAlive}");
            
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
                System.Diagnostics.Debug.WriteLine($"Bat.Draw: _currentAnimation is null at position {WorldPosition}");
                
                // Fallback rectangle if animation not loaded
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.White });
                var debugRect = new Rectangle((int)screenPos.X - GameConstants.Items.DEBUG_RECTANGLE_OFFSET, (int)screenPos.Y - GameConstants.Items.DEBUG_RECTANGLE_OFFSET, GameConstants.Items.DEBUG_RECTANGLE_SIZE, GameConstants.Items.DEBUG_RECTANGLE_SIZE);
                spriteBatch.Draw(debugTexture, debugRect, null, Color.DarkRed, 0f, Vector2.Zero, spriteEffects, LayerDepth);
            }
        }
    }
} 