using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Quests;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using System.Collections.Generic;

namespace IsometricActionGame
{
    /// <summary>
    /// Manages bat spawning based on quest state and current bat count
    /// </summary>
    public class BatSpawnManager
    {
        private readonly QuestManager _questManager;
        private readonly List<Bat> _activeBats;
        private float _spawnCooldown = 0f;
        
        // Visual spawner
        private Texture2D _spawnerTexture;
        private AnimatedSprite _spawnerAnimation;
        private bool _contentLoaded = false;
        
        public BatSpawnManager(QuestManager questManager)
        {
            _questManager = questManager;
            _activeBats = new List<Bat>();
        }
        
        /// <summary>
        /// Load content for the spawner sprite
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            if (_contentLoaded) return;
            
            try
            {
                // Load rockscavetree.png as 2x2 sprite sheet
                _spawnerTexture = content.Load<Texture2D>("rockscavetree");
                _spawnerAnimation = new AnimatedSprite(_spawnerTexture, 2, 2, GameConstants.Animation.IDLE_FRAME_TIME, false); // 2x2 grid, non-looping
                _spawnerAnimation.SetFrame(1); // Use the 2nd frame from the 1st row (index 1)
                _contentLoaded = true;
            }
            catch (System.Exception)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load BatSpawnManager content");
            }
        }
        
        /// <summary>
        /// Draw the spawner at the spawn position
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_contentLoaded || _spawnerAnimation == null) return;
            
            var spawnPos = GetSpawnPosition();
            var screenPos = GridHelper.WorldToScreen(spawnPos);
            
            // Draw the spawner with appropriate scale and depth
            _spawnerAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, GameConstants.LayerDepth.DEFAULT_DEPTH, GameConstants.SpriteScale.BAT_SCALE * 0.5f);
        }
        
        /// <summary>
        /// Add a bat to the active bats list
        /// </summary>
        public void AddBat(Bat bat)
        {
            if (!_activeBats.Contains(bat))
            {
                _activeBats.Add(bat);
            }
        }
        
        /// <summary>
        /// Remove a bat from the active bats list (when it dies)
        /// </summary>
        public void RemoveBat(Bat bat)
        {
            _activeBats.Remove(bat);
        }
        
        /// <summary>
        /// Get the current number of active bats
        /// </summary>
        public int GetActiveBatCount()
        {
            return _activeBats.Count;
        }
        
        /// <summary>
        /// Reset the spawn manager for new level
        /// </summary>
        public void Reset()
        {
            _activeBats.Clear();
            _spawnCooldown = 0f;
        }
        
        /// <summary>
        /// Check if a new bat should be spawned
        /// </summary>
        public bool ShouldSpawnBat()
        {
            // Don't spawn if quest is completed
            var extendedQuest = _questManager.GetExtendedKillBatsQuest();
            if (extendedQuest.IsCompleted)
            {
                return false;
            }
            
            // Don't spawn if cooldown is still active
            if (_spawnCooldown > 0)
            {
                return false;
            }
            
            int currentBats = GetActiveBatCount();
            
            // If quest is active, limit to MAX_BATS_WITH_QUEST
            if (extendedQuest.IsActive)
            {
                return currentBats < GameConstants.Spawn.MAX_BATS_WITH_QUEST;
            }
            
            // If quest is not active, maintain minimum bat count
            return currentBats < GameConstants.Spawn.MIN_BATS_ALWAYS;
        }
        
        /// <summary>
        /// Update spawn cooldown and check if spawning is needed
        /// </summary>
        public void Update(GameTime gameTime, bool isPaused = false)
        {
            // Don't update if paused
            if (isPaused) return;
            
            if (_spawnCooldown > 0)
            {
                _spawnCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            
            // Update spawner animation
            _spawnerAnimation?.Update(gameTime);
        }
        
        /// <summary>
        /// Mark that a bat was spawned (resets cooldown)
        /// </summary>
        public void OnBatSpawned()
        {
            _spawnCooldown = GameConstants.Spawn.BAT_SPAWN_COOLDOWN;
        }
        
        /// <summary>
        /// Get spawn position for new bat
        /// </summary>
        public Vector2 GetSpawnPosition()
        {
            // Use the original bat spawn position from constants
            return GameConstants.World.BAT_START_POSITION;
        }
        
        /// <summary>
        /// Get spawn direction for new bat
        /// </summary>
        public Vector2 GetSpawnDirection()
        {
            // Use the original bat direction from constants
            return GameConstants.World.BAT_INITIAL_DIRECTION;
        }
        
        /// <summary>
        /// Create a new bat instance
        /// </summary>
        public Bat CreateBat()
        {
            return new Bat(
                GetSpawnPosition(),
                GetSpawnDirection(),
                GameConstants.Timing.BAT_ATTACK_COOLDOWN,
                GameConstants.Movement.BAT_SPEED,
                GameConstants.Movement.BAT_CHASE_SPEED,
                GameConstants.Damage.BAT_ATTACK_DAMAGE
            );
        }
        
        /// <summary>
        /// Clean up dead bats from the list
        /// </summary>
        public void CleanupDeadBats()
        {
            _activeBats.RemoveAll(bat => !bat.IsAlive);
        }
    }
}
