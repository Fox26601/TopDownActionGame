using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.Inventory;
using IsometricActionGame.Items;
using IsometricActionGame.SaveSystem;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using System;

namespace IsometricActionGame
{
    public enum PlayerState
    {
        Idle,
        Running,
        Attacking,
        Hit,
        Dead
    }

    public class Player : IEntity, IAttackable, IPausable, ISaveable
    {
        public Vector2 WorldPosition { get; set; }
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition); // Use grid-based depth calculation
        public Vector2 FacingDirection => GetFacingDirection(); // Direction for sprite mirroring
        public float Scale => GameConstants.SpriteScale.PLAYER_SCALE;
        public float BaseHitboxRadius => GameConstants.Combat.PLAYER_HITBOX_RADIUS;
        public int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 32;
        public float HitboxRadius => BaseHitboxRadius * Scale;
        public bool HasKey { get; set; } = false;
        private bool _canMove = true;
        private bool _facingRight = true;
        private Vector2 _lastMoveDirection = Vector2.Zero;
        
    
        
        // Current movement direction state - preserved between frames to maintain facing direction
        private bool _isMovingLeft = false;
        private bool _isMovingRight = false;
        private bool _isMovingUp = false;
        private bool _isMovingDown = false;
        
        // Store last non-zero movement state for maintaining direction when idle
        private bool _lastIsMovingLeft = false;
        private bool _lastIsMovingRight = false;
        private bool _lastIsMovingUp = false;
        private bool _lastIsMovingDown = false;
        
        // Animations
        private AnimatedSprite _idleDownAnimation;      // Row 1 - facing down
        private AnimatedSprite _idleUpAnimation;        // Row 2 - facing up  
        private AnimatedSprite _idleRightAnimation;     // Row 3 - facing right
        private AnimatedSprite _runDownAnimation;       // Row 4 - running down
        private AnimatedSprite _runUpAnimation;         // Row 5 - running up
        private AnimatedSprite _runRightAnimation;      // Row 6 - running right
        private AnimatedSprite _attackDownAnimation;    // Row 7 - attack down
        private AnimatedSprite _attackRightAnimation;   // Row 8 - attack right
        private AnimatedSprite _attackUpAnimation;      // Row 9 - attack up
        private AnimatedSprite _deathAnimation;
        private AnimatedSprite _currentAnimation;
        
        // Player state
        private PlayerState _state = PlayerState.Idle;
        private float _hitAnimationTimer = 0f;
        private float _attackCooldown = 0f;
        private ConsoleDisplay _console;
        
        // IPausable implementation
        public bool IsPaused { get; set; } = false;
        

        
    
        private HealthSystem _healthSystem;
        public int MaxHealth => _healthSystem.MaxHealth;
        public int CurrentHealth => _healthSystem.CurrentHealth;
        public bool IsAlive => _healthSystem.IsAlive;
        
        // Inventory system
        public Inventory.Inventory Inventory { get; private set; }
        public Microsoft.Xna.Framework.Content.ContentManager Content { get; private set; }
        
        // Game map reference for walkability checks
        private GameMap _gameMap;

        // Events
        public event Action<Vector2, Vector2> OnAttack; // Player position, attack direction
        
        // IAttackable events
        public event Action<IAttackable, int> OnDamage;
        public event Action<IAttackable, int> OnHeal;
        public event Action<IAttackable> OnDeath;
        
        // ISaveable implementation
        public string SaveId => "Player";
        
        /// <summary>
        /// Handle movement event from InputHandler
        /// </summary>
        public void HandleMovementEvent(Vector2 direction, bool isMovingLeft, bool isMovingRight, bool isMovingUp, bool isMovingDown, GameTime gameTime)
        {
            if (_state == PlayerState.Attacking || _state == PlayerState.Hit || !_canMove || !IsAlive) return;
            
            // Update movement states
            _isMovingLeft = isMovingLeft;
            _isMovingRight = isMovingRight;
            _isMovingUp = isMovingUp;
            _isMovingDown = isMovingDown;
            
            // Update facing direction
            if (isMovingLeft) _facingRight = false;
            if (isMovingRight) _facingRight = true;
            
            // Check if there's actual movement
            bool isMoving = isMovingLeft || isMovingRight || isMovingUp || isMovingDown;
            
            // Update last movement state ONLY when there's actual movement
            // This preserves the last direction for idle animation
            if (isMoving)
            {
                _lastIsMovingLeft = _isMovingLeft;
                _lastIsMovingRight = _isMovingRight;
                _lastIsMovingUp = _isMovingUp;
                _lastIsMovingDown = _isMovingDown;
            }
            
            if (isMoving)
            {
                // Calculate movement
                float speedMultiplier = GridHelper.GetMovementSpeedMultiplier();
                
                var oldPosition = WorldPosition;
                var newPosition = WorldPosition + direction * GameConstants.Movement.PLAYER_SPEED * speedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Clamp to map boundaries
                newPosition.X = MathHelper.Clamp(newPosition.X, 0, GameMap.Width - 1);
                newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, GameMap.Height - 1);
                
                // Check if the new position is walkable
                bool isWalkable = _gameMap?.IsWalkable(newPosition) ?? true;
                
                // Only move if the position is walkable
                if (isWalkable)
                {
                    WorldPosition = newPosition;
                }
                _lastMoveDirection = direction;
                
                // Update animation to running
                if (_state != PlayerState.Hit)
                {
                    _state = PlayerState.Running;
                    var newRunAnimation = GetRunAnimation(_isMovingRight, _isMovingLeft, _isMovingUp, _isMovingDown);
                    if (_currentAnimation != newRunAnimation)
                    {
                        _currentAnimation = newRunAnimation;
                        _currentAnimation?.Reset();
                    }
                }
            }
            else
            {
                // No movement - reset to idle animation but keep last direction
                if (_state != PlayerState.Hit && _state != PlayerState.Attacking)
                {
                    _state = PlayerState.Idle;
                    var newIdleAnimation = GetIdleAnimation();
                    if (_currentAnimation != newIdleAnimation)
                    {
                        _currentAnimation = newIdleAnimation;
                        _currentAnimation?.Reset();
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle attack event from InputHandler
        /// </summary>
        public void HandleAttackEvent()
        {
            if (_state == PlayerState.Attacking || _state == PlayerState.Hit || _state == PlayerState.Dead || _attackCooldown > 0) return;
            
            _state = PlayerState.Attacking;
            _currentAnimation = GetAttackAnimation();
            _currentAnimation.Reset();
            _attackCooldown = GameConstants.Timing.HIT_ANIMATION_DURATION; // Attack cooldown
            
            OnAttack?.Invoke(WorldPosition, GetFacingDirection());
            _console?.AddMessage("Player attacks!", Color.Cyan);
        }
        
        // Player stats for saving
        public int Gold { get; set; } = 0;
        public int Experience { get; set; } = 0;
        public int Level { get; set; } = 1;

        public Player(Vector2 startPosition, GameMap gameMap = null)
        {
            WorldPosition = startPosition;
            Inventory = new Inventory.Inventory();
            _healthSystem = new HealthSystem(GameConstants.Health.PLAYER_MAX_HEALTH);
            _gameMap = gameMap;
            
            // Subscribe to health system events
            _healthSystem.OnDamage += (health, damage) => OnDamage?.Invoke(this, damage);
            _healthSystem.OnHeal += (health, amount) => OnHeal?.Invoke(this, amount);
            _healthSystem.OnDeath += (health) => OnDeath?.Invoke(this);
            
            // Add starting health potion (will be loaded in LoadContent)
            var startingHealthPotion = ItemFactory.CreateHealthPotion();
            Inventory.AddItem(startingHealthPotion);
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }
        
        public void RemoveKey()
        {
            try
            {
                bool keyFound = false;
                
                // Remove key from inventory grid
                for (int x = 0; x < IsometricActionGame.Inventory.Inventory.INVENTORY_WIDTH; x++)
                {
                    for (int y = 0; y < IsometricActionGame.Inventory.Inventory.INVENTORY_HEIGHT; y++)
                    {
                        var item = Inventory.GetItem(x, y);
                        if (item is KeyItem)
                        {
                            Inventory.RemoveItem(x, y);
                            keyFound = true;
                            if (_console != null)
                            {
                                _console.AddMessage("Key used and removed from inventory!", GameConstants.Colors.CONSOLE_GRAY);
                            }
                            break;
                        }
                    }
                    if (keyFound) break;
                }
                
                // Remove key from quick access slots if not found in inventory
                if (!keyFound)
                {
                    for (int i = 0; i < IsometricActionGame.Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
                    {
                        var item = Inventory.GetQuickAccessItem(i);
                        if (item is KeyItem)
                        {
                            Inventory.RemoveQuickAccessItem(i);
                            keyFound = true;
                            if (_console != null)
                            {
                                _console.AddMessage("Key used and removed from quick access!", GameConstants.Colors.CONSOLE_GRAY);
                            }
                            break;
                        }
                    }
                }
                
                // Only set HasKey to false if we actually found and removed a key
                if (keyFound)
                {
                    HasKey = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Player.RemoveKey: {ex.Message}");
            }
        }
        
    
        
        public void SetConsole(ConsoleDisplay console)
        {
            _console = console;
        }
        
        public void TakeDamage(int damage, Vector2? attackerPosition = null)
        {
            // Player doesn't take damage if already dead
            if (!IsAlive || _state == PlayerState.Dead) return;
            
            _healthSystem.TakeDamage(damage, attackerPosition);
            
            if (!IsAlive)
            {
                _state = PlayerState.Dead;
                _currentAnimation = _deathAnimation;
                _currentAnimation.Reset();
                _console?.AddMessage($"Player died!", Color.Red);
            }
            else
            {
                _state = PlayerState.Hit;
                _currentAnimation = GetIdleAnimation(); // Use appropriate idle animation for hit state
                _currentAnimation.Reset();
                _hitAnimationTimer = GameConstants.Timing.HIT_ANIMATION_DURATION; // Hit animation duration
            }
        }
        
        public void Heal(int amount)
        {
            _healthSystem.Heal(amount);
            _console?.AddMessage($"Player healed {amount}! Health: {CurrentHealth}/{MaxHealth}", Color.Green);
        }
        
        public void RestoreFullHealth()
        {
            _healthSystem.RestoreFullHealth();
            _console?.AddMessage("Player fully healed!", Color.Green);
        }
        
        /// <summary>
        /// Reset player to starting state (position, health, inventory, etc.)
        /// </summary>
        public void ResetToStartingState()
        {
            // Reset position to starting position
            WorldPosition = GameConstants.World.PLAYER_START_POSITION;
            
            // Reset health to full
            _healthSystem.RestoreFullHealth();
            
            // Reset state
            _state = PlayerState.Idle;
            _hitAnimationTimer = 0f;
            _attackCooldown = 0f;
            _canMove = true;
            
            // Reset movement state
            _isMovingLeft = false;
            _isMovingRight = false;
            _isMovingUp = false;
            _isMovingDown = false;
            _lastMoveDirection = Vector2.Zero;
            
            // Reset facing direction
            _facingRight = true;
            
            // Reset key status
            HasKey = false;
            
            // Reset inventory to starting state
            Inventory.Clear();
            var startingHealthPotion = ItemFactory.CreateHealthPotion();
            Inventory.AddItem(startingHealthPotion);
            
            // Reset animation to idle
            _currentAnimation = _idleDownAnimation;
            _currentAnimation?.Reset();
            
            _console?.AddMessage("Player reset to starting state", GameConstants.Colors.CONSOLE_CYAN);
        }
        
        public Vector2 GetFacingDirection()
        {
            // Use simple cardinal directions for attack - these should match player expectations
            Vector2 facingDirection;
            
            if (_lastIsMovingUp && !_lastIsMovingDown) // W
            {
                facingDirection = GameConstants.Directions.UP; // Simple up
            }
            else if (_lastIsMovingDown && !_lastIsMovingUp) // S
            {
                facingDirection = GameConstants.Directions.DOWN; // Simple down
            }
            else if (_lastIsMovingRight && !_lastIsMovingLeft) // D
            {
                facingDirection = GameConstants.Directions.RIGHT; // Simple right
            }
            else if (_lastIsMovingLeft && !_lastIsMovingRight) // A
            {
                facingDirection = new Vector2(-1, 0); // Simple left
            }
            else if (_lastMoveDirection != Vector2.Zero)
            {
                // For diagonal movement, choose the dominant direction
                Vector2 normalized = Vector2.Normalize(_lastMoveDirection);
                if (Math.Abs(normalized.X) > Math.Abs(normalized.Y))
                {
                    // Horizontal movement is dominant
                    facingDirection = normalized.X > 0 ? GameConstants.Directions.RIGHT : GameConstants.Directions.LEFT;
                }
                else
                {
                    // Vertical movement is dominant
                    facingDirection = normalized.Y > 0 ? GameConstants.Directions.DOWN : GameConstants.Directions.UP;
                }
            }
            else
            {
                // Default direction based on facing
                facingDirection = _facingRight ? GameConstants.Directions.RIGHT : GameConstants.Directions.LEFT;
            }
            
            return facingDirection;
        }
        
        private AnimatedSprite GetIdleAnimation()
        {
            // Use last movement direction to determine idle animation
            // SPRITE SHEET LAYOUT (10x6) - IDLE ANIMATIONS:
            // Row 1 (index 0): Idle Down (S) - лицом к кадру (вниз)
            // Row 2 (index 1): Idle Side (D/A) - в сторону (D без зеркала, A с зеркалом)
            // Row 3 (index 2): Idle Up (W) - спиной к кадру (вверх)
            
            if (_lastIsMovingUp && !_lastIsMovingDown && !_lastIsMovingLeft && !_lastIsMovingRight) // W
                return _idleUpAnimation; // Row 3 (index 2) - спиной к кадру
            else if (_lastIsMovingDown && !_lastIsMovingUp && !_lastIsMovingLeft && !_lastIsMovingRight) // S
                return _idleDownAnimation; // Row 1 (index 0) - лицом к кадру
            else if (_lastIsMovingRight && !_lastIsMovingLeft && !_lastIsMovingUp && !_lastIsMovingDown) // D
                return _idleRightAnimation; // Row 2 (index 1) - no mirroring
            else if (_lastIsMovingLeft && !_lastIsMovingRight && !_lastIsMovingUp && !_lastIsMovingDown) // A
                return _idleRightAnimation; // Row 2 (index 1) - will be mirrored
            else if (_lastIsMovingUp && _lastIsMovingRight && !_lastIsMovingDown && !_lastIsMovingLeft) // W + D
                return _idleUpAnimation; // Row 3 (index 2) - спиной к кадру
            else if (_lastIsMovingUp && _lastIsMovingLeft && !_lastIsMovingDown && !_lastIsMovingRight) // W + A
                return _idleUpAnimation; // Row 3 (index 2) - спиной к кадру
            else if (_lastIsMovingDown && _lastIsMovingRight && !_lastIsMovingUp && !_lastIsMovingLeft) // S + D
                return _idleRightAnimation; // Row 2 (index 1) - no mirroring
            else if (_lastIsMovingDown && _lastIsMovingLeft && !_lastIsMovingUp && !_lastIsMovingRight) // S + A
                return _idleRightAnimation; // Row 2 (index 1) - will be mirrored
            else
                return _idleDownAnimation; // Default to down idle
        }

        private AnimatedSprite GetAttackAnimation()
        {
            // SPRITE SHEET LAYOUT (10x6) - ATTACK ANIMATIONS:
            // Row 7 (index 6): Attack Down (S) - лицом к кадру (вниз)
            // Row 8 (index 7): Attack Side (D/A) - в сторону (D без зеркала, A с зеркалом)
            // Row 9 (index 8): Attack Up (W) - спиной к кадру (вверх)
            
            if (_lastIsMovingUp && !_lastIsMovingDown && !_lastIsMovingLeft && !_lastIsMovingRight) // W
                return _attackUpAnimation; // Row 9 (index 8) - спиной к кадру
            else if (_lastIsMovingDown && !_lastIsMovingUp && !_lastIsMovingLeft && !_lastIsMovingRight) // S
                return _attackDownAnimation; // Row 7 (index 6) - лицом к кадру
            else if (_lastIsMovingRight && !_lastIsMovingLeft && !_lastIsMovingUp && !_lastIsMovingDown) // D
                return _attackRightAnimation; // Row 8 (index 7) - no mirroring
            else if (_lastIsMovingLeft && !_lastIsMovingRight && !_lastIsMovingUp && !_lastIsMovingDown) // A
                return _attackRightAnimation; // Row 8 (index 7) - will be mirrored
            else if (_lastIsMovingUp && _lastIsMovingRight && !_lastIsMovingDown && !_lastIsMovingLeft) // W + D
                return _attackUpAnimation; // Row 9 (index 8) - спиной к кадру
            else if (_lastIsMovingUp && _lastIsMovingLeft && !_lastIsMovingDown && !_lastIsMovingRight) // W + A
                return _attackUpAnimation; // Row 9 (index 8) - спиной к кадру
            else if (_lastIsMovingDown && _lastIsMovingRight && !_lastIsMovingUp && !_lastIsMovingLeft) // S + D
                return _attackRightAnimation; // Row 8 (index 7) - no mirroring
            else if (_lastIsMovingDown && _lastIsMovingLeft && !_lastIsMovingUp && !_lastIsMovingRight) // S + A
                return _attackRightAnimation; // Row 8 (index 7) - will be mirrored
            else
                return _attackDownAnimation;
        }

        private AnimatedSprite GetRunAnimation(bool isMovingRight, bool isMovingLeft, bool isMovingUp, bool isMovingDown)
        {
            // SPRITE SHEET LAYOUT (10x6) - RUN ANIMATIONS:
            // Row 4 (index 3): Run Down (S) - лицом к кадру (вниз)
            // Row 5 (index 4): Run Side (D/A) - в сторону (D без зеркала, A с зеркалом)
            // Row 6 (index 5): Run Up (W) - спиной к кадру (вверх)
            
            if (isMovingRight && !isMovingLeft && !isMovingUp && !isMovingDown) // D
                return _runRightAnimation; // Row 5 (index 4) - no mirroring
            else if (isMovingLeft && !isMovingRight && !isMovingUp && !isMovingDown) // A
                return _runRightAnimation; // Row 5 (index 4) - will be mirrored
            else if (isMovingUp && !isMovingDown && !isMovingLeft && !isMovingRight) // W
                return _runUpAnimation; // Row 6 (index 5) - спиной к кадру
            else if (isMovingDown && !isMovingUp && !isMovingLeft && !isMovingRight) // S
                return _runDownAnimation; // Row 4 (index 3) - лицом к кадру
            else if (isMovingLeft && isMovingDown && !isMovingRight && !isMovingUp) // A + S
                return _runRightAnimation; // Row 5 (index 4) - will be mirrored
            else if (isMovingUp && isMovingLeft && !isMovingDown && !isMovingRight) // W + A
                return _runUpAnimation; // Row 6 (index 5) - спиной к кадру
            else if (isMovingDown && isMovingRight && !isMovingUp && !isMovingLeft) // S + D
                return _runRightAnimation; // Row 5 (index 4) - no mirroring
            else if (isMovingUp && isMovingRight && !isMovingDown && !isMovingLeft) // W + D
                return _runUpAnimation; // Row 6 (index 5) - спиной к кадру
            else
                return _runDownAnimation;
        }


        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            Content = content;
            
            // Load content for all items in inventory
            for (int x = 0; x < IsometricActionGame.Inventory.Inventory.INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < IsometricActionGame.Inventory.Inventory.INVENTORY_HEIGHT; y++)
                {
                    var item = Inventory.GetItem(x, y);
                    if (item != null)
                    {
                        item.LoadContent(content);
                    }
                }
            }
            
            // Load content for quick access items
            for (int i = 0; i < IsometricActionGame.Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
            {
                var item = Inventory.GetQuickAccessItem(i);
                if (item != null)
                {
                    item.LoadContent(content);
                }
            }
            
            try
            {
                var playerTexture = content.Load<Texture2D>("Player");
                
                // Player animations: 320x192 texture (10x6 grid)
                // Rows 1-6: 6 frames each (6 columns) - frame size: 53x19 pixels
                // Rows 7-10: 4 frames from 6 columns (only first 4 used) - frame size: 53x19 pixels
                
                // ============================================================================
                // SPRITE SHEET LAYOUT (10x6) - Player.png (320x192 pixels)
                // ============================================================================
                // Row 1 (index 0): Idle Down (S) - лицом к кадру (вниз)
                // Row 2 (index 1): Idle Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                // Row 3 (index 2): Idle Up (W) - спиной к кадру (вверх)
                // Row 4 (index 3): Run Down (S) - лицом к кадру (вниз)
                // Row 5 (index 4): Run Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                // Row 6 (index 5): Run Up (W) - спиной к кадру (вверх)
                // Row 7 (index 6): Attack Down (S) - лицом к кадру (вниз)
                // Row 8 (index 7): Attack Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                // Row 9 (index 8): Attack Up (W) - спиной к кадру (вверх)
                // Row 10 (index 9): Death - анимация смерти
                // ============================================================================
                // ВАЖНО: Каждая строка содержит 6 кадров (кроме строк 7-10, где используется только 4 кадра)
                // Размер кадра: 53x19 пикселей
                // ============================================================================
                
                // ============================================================================
                // IDLE ANIMATIONS (6 кадров каждая, зацикленные)
                // ============================================================================
                
                // Row 1 (index 0) - Idle Down (S) - лицом к кадру (вниз)
                _idleDownAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_IDLE_FRAMES, 0, GameConstants.Animation.IDLE_FRAME_TIME, true);
                
                // Row 2 (index 1) - Idle Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                _idleRightAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_IDLE_FRAMES, 1, GameConstants.Animation.IDLE_FRAME_TIME, true);
                
                // Row 3 (index 2) - Idle Up (W) - спиной к кадру (вверх)
                _idleUpAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_IDLE_FRAMES, 2, GameConstants.Animation.IDLE_FRAME_TIME, true);
                
                // ============================================================================
                // RUN ANIMATIONS (6 кадров каждая, зацикленные)
                // ============================================================================
                
                // Row 4 (index 3) - Run Down (S) - лицом к кадру (вниз)
                _runDownAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_RUN_FRAMES, 3, GameConstants.Animation.RUN_FRAME_TIME, true);
                
                // Row 5 (index 4) - Run Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                _runRightAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_RUN_FRAMES, 4, GameConstants.Animation.RUN_FRAME_TIME, true);
                
                // Row 6 (index 5) - Run Up (W) - спиной к кадру (вверх)
                _runUpAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_RUN_FRAMES, 5, GameConstants.Animation.RUN_FRAME_TIME, true);
                
                // ============================================================================
                // ATTACK ANIMATIONS (4 кадра каждая, НЕ зацикленные)
                // ============================================================================
                
                // Row 7 (index 6) - Attack Down (S) - лицом к кадру (вниз)
                _attackDownAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_ATTACK_FRAMES, 6, GameConstants.Animation.ATTACK_FRAME_TIME, false);
                
                // Row 8 (index 7) - Attack Side (D/A) - в сторону (D без зеркала, A с зеркалом)
                _attackRightAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_ATTACK_FRAMES, 7, GameConstants.Animation.ATTACK_FRAME_TIME, false);
                
                // Row 9 (index 8) - Attack Up (W) - спиной к кадру (вверх)
                _attackUpAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_ATTACK_FRAMES, 8, GameConstants.Animation.ATTACK_FRAME_TIME, false);
                
                // ============================================================================
                // DEATH ANIMATION (4 кадра, НЕ зацикленная)
                // ============================================================================
                
                // Row 10 (index 9) - Death - анимация смерти
                _deathAnimation = new AnimatedSprite(playerTexture, GameConstants.SpriteFrames.PLAYER_COLUMNS, GameConstants.SpriteFrames.PLAYER_ROWS, GameConstants.SpriteFrames.PLAYER_DEATH_FRAMES, 9, GameConstants.Animation.DEATH_FRAME_TIME, false);
                
                // ============================================================================
                // ИНИЦИАЛИЗАЦИЯ
                // ============================================================================
                
                _currentAnimation = _idleDownAnimation; // По умолчанию смотрим вниз (лицом к кадру)
                
                // ============================================================================
                // ВАЖНЫЕ ЗАМЕЧАНИЯ:
                // 1. Side анимации (Row 2, 5, 8) используются для D (без зеркала) и A (с зеркалом)
                // 2. Зеркалирование обрабатывается в ShouldFlipHorizontally() и применяется в Draw()
                // 3. Attack анимации НЕ зациклены (isLooping = false) - проигрываются один раз
                // 4. Idle и Run анимации зациклены (isLooping = true)
                // ============================================================================
            }
            catch (Exception ex)
            {
                // Animation loading failed - this is a critical error
                throw new InvalidOperationException($"Failed to load Player animations: {ex.Message}", ex);
            }
        }

        public void Update(GameTime gameTime)
        {
        
            // The pausing and dialogue logic is now controlled from Game1.cs.
            Update(gameTime, isDialogueActive: false, isPaused: false); // Default to not paused and no dialogue
        }

        public void Update(GameTime gameTime, bool isPaused)
        {
        
            Update(gameTime, isDialogueActive: false, isPaused: isPaused); // Default to no dialogue
        }

        public void Update(GameTime gameTime, bool isDialogueActive = false, bool isPaused = false)
        {
            // Update inventory with dialogue state
            Inventory.Update(gameTime, this, isDialogueActive);
            
            // Always update animation (including death animation)
            _currentAnimation?.Update(gameTime);
            
            // If paused, only update animations
            if (isPaused || IsPaused || !_canMove || !IsAlive) return;
            
            // Input handling is now centralized in InputHandler
            // Player only processes movement and attack events
            
            // Update attack cooldown
            if (_attackCooldown > 0)
            {
                _attackCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            
            // Update hit animation timer
            if (_state == PlayerState.Hit)
            {
                _hitAnimationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_hitAnimationTimer <= 0)
                {
                    _state = PlayerState.Idle;
                    _currentAnimation = GetIdleAnimation();
                }
            }
            
            // Check if attack animation finished
            if (_state == PlayerState.Attacking && _currentAnimation.IsFinished)
            {
                _state = PlayerState.Idle;
                _currentAnimation = GetIdleAnimation();
            }
        }

        private bool ShouldFlipHorizontally()
        {
        
            // A (Left): Row 2 5 8 (right animation) + horizontal mirroring
            // D (Right): Row 2 5 8 (right animation) without mirroring
            // W (Up): Row 3 6 9 (up animation)
            // S (Down): Row 1 4 7 (down animation)
            // A + S: Row 2 5 8 (right animation) + horizontal mirroring
            // S + D: Row 2 5 8 (right animation) without mirroring
            
            // Mirror for left-facing movements (A key)
            if (_lastIsMovingLeft && !_lastIsMovingRight && !_lastIsMovingUp && !_lastIsMovingDown) // A
                return true;
                
            // Mirror for diagonal left combinations
            if (_lastIsMovingLeft && _lastIsMovingUp && !_lastIsMovingRight && !_lastIsMovingDown) // W + A
                return true;
            if (_lastIsMovingLeft && _lastIsMovingDown && !_lastIsMovingRight && !_lastIsMovingUp) // S + A (A + S)
                return true;
                
            return false; // No mirroring for right, up, down, S+D movements
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            if (_currentAnimation != null)
            {
                var spriteEffects = ShouldFlipHorizontally() ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, spriteEffects, LayerDepth, Scale);
            }
            // Animation is null - this should not happen in normal operation
        }
        
        // ISaveable implementation
        public SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            data.SetValue("Position", WorldPosition);
            data.SetValue("CurrentHealth", CurrentHealth);
            data.SetValue("MaxHealth", MaxHealth);
            data.SetValue("Gold", Gold);
            data.SetValue("Experience", Experience);
            data.SetValue("Level", Level);
            data.SetValue("HasKey", HasKey);
            data.SetValue("State", _state.ToString());
            return data;
        }
        
        public void Deserialize(SaveData data)
        {
            if (data == null) return;
            
            WorldPosition = data.GetValue<Vector2>("Position", WorldPosition);
            var currentHealth = data.GetValue<int>("CurrentHealth", CurrentHealth);
            var maxHealth = data.GetValue<int>("MaxHealth", MaxHealth);
            Gold = data.GetValue<int>("Gold", Gold);
            Experience = data.GetValue<int>("Experience", Experience);
            Level = data.GetValue<int>("Level", Level);
            HasKey = data.GetValue<bool>("HasKey", HasKey);
            
            // Restore health
            _healthSystem.SetHealth(currentHealth);
            
            // Restore state
            if (data.HasValue("State"))
            {
                var stateString = data.GetValue<string>("State", "Idle");
                if (Enum.TryParse<PlayerState>(stateString, out var state))
                {
                    _state = state;
                }
            }
        }
        
        public void SetHealth(int health)
        {
            _healthSystem.SetHealth(health);
        }
    }
}