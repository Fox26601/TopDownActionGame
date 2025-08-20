using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using IsometricActionGame.Events.EventData;
using IsometricActionGame.Events.EventHandlers;
using IsometricActionGame.UI;
using IsometricActionGame.Quests;
using IsometricActionGame.Graphics;
using IsometricActionGame.Items;
using IsometricActionGame.Dialogue;


namespace IsometricActionGame.Events
{
    /// <summary>
    /// Unified Event System that consolidates all event handling
    /// Provides centralized, type-safe, and performance-optimized event management
    /// </summary>
    public class UnifiedEventSystem : IDisposable
    {
        private static UnifiedEventSystem _instance;
        public static UnifiedEventSystem Instance => _instance ??= new UnifiedEventSystem();
        
        // Core event system
        private readonly GameEventSystem _eventSystem;
        
        // Event handlers
        private readonly List<IEventHandler> _eventHandlers;
        
        // System references
        private ConsoleDisplay _console;
        private Player _player;
        private QuestManager _questManager;
        private DialogueManager _dialogueManager;
        private UIManager _uiManager;
        private LevelSystem _levelSystem;
        private GameRestartManager _restartManager;
        private Microsoft.Xna.Framework.Content.ContentManager _contentManager;
        private PauseMenu _pauseMenu;
        private DeathPanel _deathPanel;
        private LevelTransitionPanel _levelTransitionPanel;
        
        // Event statistics
        private readonly Dictionary<string, int> _eventStatistics;
        private bool _disposed = false;
        
        private UnifiedEventSystem()
        {
            _eventSystem = GameEventSystem.Instance;
            _eventHandlers = new List<IEventHandler>();
            _eventStatistics = new Dictionary<string, int>();
        }
        
        #region Initialization and Setup
        
        /// <summary>
        /// Initialize the unified event system with all required dependencies
        /// </summary>
        public void Initialize(
            ConsoleDisplay console,
            Player player,
            QuestManager questManager,
            DialogueManager dialogueManager,
            UIManager uiManager,
            LevelSystem levelSystem,
            GameRestartManager restartManager,
            Microsoft.Xna.Framework.Content.ContentManager contentManager,
            PauseMenu pauseMenu = null,
            DeathPanel deathPanel = null,
            LevelTransitionPanel levelTransitionPanel = null)
        {
            _console = console;
            _player = player;
            _questManager = questManager;
            _dialogueManager = dialogueManager;
            _uiManager = uiManager;
            _levelSystem = levelSystem;
            _restartManager = restartManager;
            _contentManager = contentManager;
            _pauseMenu = pauseMenu;
            _deathPanel = deathPanel;
            _levelTransitionPanel = levelTransitionPanel;
            
            InitializeEventHandlers();
            SubscribeToAllEvents();
        }
        
        /// <summary>
        /// Update all UI event handlers with new LevelTransitionPanel reference
        /// </summary>
        public void UpdateUIEventHandlers()
        {
            System.Diagnostics.Debug.WriteLine("UnifiedEventSystem.UpdateUIEventHandlers: Called");
            System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem.UpdateUIEventHandlers: _eventHandlers count = {_eventHandlers.Count}");
            foreach (var handler in _eventHandlers)
            {
                System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem.UpdateUIEventHandlers: Checking handler type: {handler?.GetType().Name}");
                if (handler is UnifiedUIEventHandler uiHandler)
                {
                    System.Diagnostics.Debug.WriteLine("UnifiedEventSystem.UpdateUIEventHandlers: Found UnifiedUIEventHandler, refreshing reference");
                    uiHandler.RefreshLevelTransitionPanelReference();
                }
            }
        }
        
        /// <summary>
        /// Initialize all event handlers
        /// </summary>
        private void InitializeEventHandlers()
        {
            System.Diagnostics.Debug.WriteLine("UnifiedEventSystem.InitializeEventHandlers: Starting");
            // Clear existing handlers
            _eventHandlers.Clear();
            
            // Create and add handlers
            System.Diagnostics.Debug.WriteLine("UnifiedEventSystem.InitializeEventHandlers: Creating UnifiedUIEventHandler");
            _eventHandlers.Add(new UnifiedGameplayEventHandler(_console, _questManager, _contentManager, _player, _levelSystem));
            _eventHandlers.Add(new UnifiedUIEventHandler(_console, _uiManager, _levelSystem));
            _eventHandlers.Add(new UnifiedGameStateEventHandler(_console, _dialogueManager, _restartManager));
            _eventHandlers.Add(new UnifiedResetEventHandler(_console, _restartManager, _pauseMenu, _deathPanel, _levelTransitionPanel, _uiManager));

            // IMPORTANT: initialize handlers so they subscribe to events
            System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem.InitializeEventHandlers: Initializing {_eventHandlers.Count} handlers");
            foreach (var handler in _eventHandlers)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem.InitializeEventHandlers: Initializing {handler?.GetType().Name}");
                    handler.Initialize();
                    System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem.InitializeEventHandlers: Successfully initialized {handler?.GetType().Name}");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem: Failed to initialize handler {handler?.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Subscribe to all events from various systems
        /// </summary>
        private void SubscribeToAllEvents()
        {
            SubscribeToPlayerEvents();
            SubscribeToCombatEvents();
            SubscribeToInventoryEvents();
            SubscribeToUIEvents();
            SubscribeToQuestEvents();
            SubscribeToDialogueEvents();
            SubscribeToEnvironmentEvents();
            SubscribeToSystemEvents();
        }
        
        #endregion
        
        #region Event Subscriptions
        
        private void SubscribeToPlayerEvents()
        {
            if (_player == null) return;
            
            // Player health events
            _player.OnDamage += (entity, damage) => 
                PublishPlayerDamaged(damage, _player.WorldPosition);
            
            _player.OnHeal += (entity, amount) => 
                PublishPlayerHealed(amount, "HealthPotion");
            
            _player.OnDeath += (entity) => 
                PublishPlayerDeath(_player.WorldPosition, "Combat");
            
            // Player attack events
            _player.OnAttack += (pos, dir) => 
                PublishPlayerAttack(pos, dir);
            
            // Player movement events (if available)
            // _player.OnMoved += (oldPos, newPos, direction) => 
            //     PublishPlayerMoved(oldPos, newPos, direction);
        }
        
        private void SubscribeToCombatEvents()
        {
            // Combat events are published directly from entities
            // This system will handle them through the unified handlers
        }
        
        private void SubscribeToInventoryEvents()
        {
            if (_player?.Inventory == null) return;
            
            _player.Inventory.OnGoldChanged += (gold) => 
                PublishGoldChanged(_player.Inventory.Gold, gold, "Transaction");
            
            _player.Inventory.OnInventoryChanged += () => 
            {
                // Calculate total items in inventory
                int totalItems = 0;
                for (int x = 0; x < Inventory.Inventory.INVENTORY_WIDTH; x++)
                {
                    for (int y = 0; y < Inventory.Inventory.INVENTORY_HEIGHT; y++)
                    {
                        if (_player.Inventory.GetItem(x, y) != null)
                            totalItems++;
                    }
                }
                // Add quick access items
                for (int i = 0; i < Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
                {
                    if (_player.Inventory.GetQuickAccessItem(i) != null)
                        totalItems++;
                }
                PublishInventoryChanged(totalItems, 0, false);
            };
            
            _player.Inventory.OnItemDiscarded += (item) => 
                PublishItemDiscarded(item, _player.WorldPosition, "Manual");
        }
        
        private void SubscribeToUIEvents()
        {
            if (_uiManager == null) return;
            
            _uiManager.OnResetSettings += () => 
                PublishSettingsReset();
            
            _uiManager.OnSettings += () => 
                PublishSettingsOpened();
        }
        
        private void SubscribeToQuestEvents()
        {
            if (_questManager == null) return;
            
            _questManager.OnQuestStarted += (quest) => 
                PublishQuestStarted(quest);
            
            _questManager.OnQuestCompleted += (quest) => 
                PublishQuestCompleted(quest);
            
            _questManager.OnQuestTurnedIn += (quest) => 
                PublishQuestTurnedIn(quest);
            
            _questManager.OnQuestRefused += (quest) => 
                PublishQuestRefused(quest);
        }
        
        private void SubscribeToDialogueEvents()
        {
            if (_dialogueManager == null) return;
            
            // Dialogue events are published directly from DialogueManager
            // This system will handle them through the unified handlers
        }
        
        private void SubscribeToEnvironmentEvents()
        {
            // Environment events are published directly from entities
            // This system will handle them through the unified handlers
        }
        
        private void SubscribeToSystemEvents()
        {
            // System events are published directly from various managers
            // This system will handle them through the unified handlers
        }
        
        #endregion
        
        #region Event Publishing Methods
        
        // Player Events
        public void PublishPlayerAttack(Vector2 position, Vector2 direction)
        {
            var eventData = new PlayerAttackEventData(position, direction);
            _eventSystem.Publish(GameEvents.PLAYER_ATTACK, eventData);
            UpdateStatistics(GameEvents.PLAYER_ATTACK);
        }
        
        public void PublishPlayerDamaged(int damage, Vector2 position, string attackerType = null)
        {
            var eventData = new PlayerDamagedEventData(damage, position, attackerType);
            _eventSystem.Publish(GameEvents.PLAYER_DAMAGED, eventData);
            UpdateStatistics(GameEvents.PLAYER_DAMAGED);
        }
        
        public void PublishPlayerHealed(int amount, string healSource = null)
        {
            var eventData = new PlayerHealedEventData(amount, healSource);
            _eventSystem.Publish(GameEvents.PLAYER_HEALED, eventData);
            UpdateStatistics(GameEvents.PLAYER_HEALED);
        }
        
        public void PublishPlayerDeath(Vector2 deathPosition, string deathCause = null)
        {
            var eventData = new PlayerDeathEventData(deathPosition, deathCause);
            _eventSystem.Publish(GameEvents.PLAYER_DIED, eventData);
            UpdateStatistics(GameEvents.PLAYER_DIED);
        }
        
        public void PublishPlayerMoved(Vector2 oldPosition, Vector2 newPosition, Vector2 direction)
        {
            var eventData = new PlayerMovedEventData(oldPosition, newPosition, direction);
            _eventSystem.Publish(GameEvents.PLAYER_MOVED, eventData);
            UpdateStatistics(GameEvents.PLAYER_MOVED);
        }
        
        // Combat Events
        public void PublishEnemyDefeated(string enemyType, Vector2 position, string defeatedBy = null, int experienceGained = 0)
        {
            var eventData = new EnemyDefeatedEventData(enemyType, position, defeatedBy, experienceGained);
            _eventSystem.Publish(GameEvents.ENEMY_DEFEATED, eventData);
            UpdateStatistics(GameEvents.ENEMY_DEFEATED);
        }
        

        
        public void PublishAttackExecuted(string attackerType, Vector2 attackerPosition, Vector2 targetPosition, int damage, bool isCritical = false)
        {
            var eventData = new AttackExecutedEventData(attackerType, attackerPosition, targetPosition, damage, isCritical);
            _eventSystem.Publish(GameEvents.ATTACK_EXECUTED, eventData);
            UpdateStatistics(GameEvents.ATTACK_EXECUTED);
        }
        
        public void PublishDamageDealt(string attackerType, string targetType, Vector2 targetPosition, int damage, bool isCritical = false)
        {
            var eventData = new DamageDealtEventData(attackerType, targetType, targetPosition, damage, isCritical);
            _eventSystem.Publish(GameEvents.DAMAGE_DEALT, eventData);
            UpdateStatistics(GameEvents.DAMAGE_DEALT);
        }
        
        public void PublishProjectileCreated(string projectileType, Vector2 position, Vector2 direction, float speed)
        {
            var eventData = new ProjectileCreatedEventData(projectileType, position, direction, speed);
            _eventSystem.Publish(GameEvents.PROJECTILE_CREATED, eventData);
            UpdateStatistics(GameEvents.PROJECTILE_CREATED);
        }
        
        public void PublishProjectileDestroyed(string projectileType, Vector2 position, string destroyReason = null)
        {
            var eventData = new ProjectileDestroyedEventData(projectileType, position, destroyReason);
            _eventSystem.Publish(GameEvents.PROJECTILE_DESTROYED, eventData);
            UpdateStatistics(GameEvents.PROJECTILE_DESTROYED);
        }
        
        // Inventory Events
        public void PublishItemPickedUp(Item item, int quantity, Vector2 pickupPosition)
        {
            var eventData = new ItemPickedUpEventData(item, quantity, pickupPosition);
            _eventSystem.Publish(GameEvents.ITEM_PICKED_UP, eventData);
            UpdateStatistics(GameEvents.ITEM_PICKED_UP);
        }
        
        public void PublishItemDiscarded(Item item, Vector2 discardPosition, string reason = null)
        {
            var eventData = new ItemDiscardedEventData(item, discardPosition, reason);
            _eventSystem.Publish(GameEvents.ITEM_DISCARDED, eventData);
            UpdateStatistics(GameEvents.ITEM_DISCARDED);
        }
        
        public void PublishItemDropped(Item item, Vector2 dropPosition, string dropSource = null)
        {
            var eventData = new ItemDroppedEventData(item, dropPosition, dropSource);
            _eventSystem.Publish(GameEvents.ITEM_DROPPED, eventData);
            UpdateStatistics(GameEvents.ITEM_DROPPED);
        }
        
        public void PublishItemUsed(Item item, Vector2 usePosition, string useTarget = null)
        {
            var eventData = new ItemUsedEventData(item, usePosition, useTarget);
            _eventSystem.Publish(GameEvents.ITEM_USED, eventData);
            UpdateStatistics(GameEvents.ITEM_USED);
        }
        
        public void PublishGoldChanged(int oldAmount, int newAmount, string changeReason = null)
        {
            var eventData = new GoldChangedEventData(oldAmount, newAmount, changeReason);
            _eventSystem.Publish(GameEvents.GOLD_CHANGED, eventData);
            UpdateStatistics(GameEvents.GOLD_CHANGED);
        }
        
        public void PublishInventoryChanged(int totalItems, int totalWeight, bool isFull)
        {
            var eventData = new InventoryChangedEventData(totalItems, totalWeight, isFull);
            _eventSystem.Publish(GameEvents.INVENTORY_CHANGED, eventData);
            UpdateStatistics(GameEvents.INVENTORY_CHANGED);
        }
        
        // Game State Events
        public void PublishGameStateChanged(string oldState, string newState, string reason = null)
        {
            var eventData = new GameStateChangedEventData(oldState, newState, reason);
            _eventSystem.Publish(GameEvents.GAME_STATE_CHANGED, eventData);
            UpdateStatistics(GameEvents.GAME_STATE_CHANGED);
        }
        
        public void PublishGamePaused(string pauseReason, bool isPaused = true)
        {
            var eventData = new GamePausedEventData(pauseReason, isPaused);
            _eventSystem.Publish(GameEvents.GAME_PAUSED, eventData);
            UpdateStatistics(GameEvents.GAME_PAUSED);
        }
        
        public void PublishGameRestart(string restartReason, bool isFromSave = false)
        {
            var eventData = new GameRestartEventData(restartReason, isFromSave);
            _eventSystem.Publish(GameEvents.GAME_RESTART, eventData);
            UpdateStatistics(GameEvents.GAME_RESTART);
        }
        
        public void PublishLevelStarted(int levelNumber, string levelName, Vector2 playerStartPosition)
        {
            var eventData = new LevelEventData(levelNumber, levelName, playerStartPosition);
            _eventSystem.Publish(GameEvents.LEVEL_STARTED, eventData);
            UpdateStatistics(GameEvents.LEVEL_STARTED);
        }
        
        public void PublishLevelCompleted(int levelNumber, string levelName, Vector2 playerStartPosition, 
            float completionTime, int enemiesDefeated, int itemsCollected)
        {
            var eventData = new LevelCompletedEventData(levelNumber, levelName, playerStartPosition, 
                completionTime, enemiesDefeated, itemsCollected);
            _eventSystem.Publish(GameEvents.LEVEL_COMPLETED, eventData);
            UpdateStatistics(GameEvents.LEVEL_COMPLETED);
        }
        
        // UI Events
        public void PublishResolutionChanged(object resolution)
        {
            _eventSystem.Publish(GameEvents.RESOLUTION_CHANGED, resolution);
            UpdateStatistics(GameEvents.RESOLUTION_CHANGED);
        }
        
        public void PublishFullscreenChanged(bool fullscreen)
        {
            _eventSystem.Publish(GameEvents.FULLSCREEN_CHANGED, fullscreen);
            UpdateStatistics(GameEvents.FULLSCREEN_CHANGED);
        }
        
        public void PublishSettingsReset()
        {
            _eventSystem.Publish<object>(GameEvents.SETTINGS_RESET, null);
            UpdateStatistics(GameEvents.SETTINGS_RESET);
        }
        
        public void PublishSettingsOpened()
        {
            _eventSystem.Publish<object>(GameEvents.SETTINGS_OPENED, null);
            UpdateStatistics(GameEvents.SETTINGS_OPENED);
        }
        
        // Quest Events
        public void PublishQuestStarted(Quest quest)
        {
            _eventSystem.Publish(GameEvents.QUEST_STARTED, quest);
            UpdateStatistics(GameEvents.QUEST_STARTED);
        }
        
        public void PublishQuestCompleted(Quest quest)
        {
            System.Diagnostics.Debug.WriteLine($"UnifiedEventSystem: Publishing QUEST_COMPLETED event for quest: {quest?.Title}");
            _eventSystem.Publish(GameEvents.QUEST_COMPLETED, quest);
            UpdateStatistics(GameEvents.QUEST_COMPLETED);
        }
        
        public void PublishQuestTurnedIn(Quest quest)
        {
            _eventSystem.Publish(GameEvents.QUEST_TURNED_IN, quest);
            UpdateStatistics(GameEvents.QUEST_TURNED_IN);
        }
        
        public void PublishQuestRefused(Quest quest)
        {
            _eventSystem.Publish(GameEvents.QUEST_REFUSED, quest);
            UpdateStatistics(GameEvents.QUEST_REFUSED);
        }
        
        #endregion
        
        #region Statistics and Debugging
        
        private void UpdateStatistics(string eventName)
        {
            if (!_eventStatistics.ContainsKey(eventName))
            {
                _eventStatistics[eventName] = 0;
            }
            _eventStatistics[eventName]++;
        }
        
        public Dictionary<string, int> GetEventStatistics()
        {
            return new Dictionary<string, int>(_eventStatistics);
        }
        
        public void LogEventStatistics()
        {
            System.Diagnostics.Debug.WriteLine("=== UNIFIED EVENT SYSTEM STATISTICS ===");
            foreach (var kvp in _eventStatistics.OrderByDescending(x => x.Value))
            {
                System.Diagnostics.Debug.WriteLine($"{kvp.Key}: {kvp.Value} times");
            }
            System.Diagnostics.Debug.WriteLine($"Total event handlers: {_eventHandlers.Count}");
        }
        
        #endregion
        
        #region Processing
        
        /// <summary>
        /// Process all queued events (called each frame)
        /// </summary>
        public void ProcessQueuedEvents()
        {
            if (_disposed) return;
            _eventSystem.ProcessQueuedEvents();
        }
        
        /// <summary>
        /// Test method to verify Unified Event System is working
        /// </summary>
        public void RunSystemTest()
        {
            System.Diagnostics.Debug.WriteLine("=== UNIFIED EVENT SYSTEM TEST ===");
            
            // Test player events
            PublishPlayerAttack(new Vector2(100, 100), new Vector2(1, 0));
            PublishPlayerDamaged(5, new Vector2(100, 100), "TestEnemy");
            PublishPlayerHealed(10, "TestHeal");
            
            // Test combat events
            PublishEnemyDefeated("TestBat", new Vector2(200, 200), "Player", 5);
            PublishAttackExecuted("Player", new Vector2(100, 100), new Vector2(200, 200), 8, false);
            
            // Test inventory events
            PublishGoldChanged(0, 10, "TestTransaction");
            PublishInventoryChanged(3, 15, false);
            
            // Test game state events
            PublishGameStateChanged("Playing", "Paused", "Test");
            PublishGamePaused("Test Pause", true);
            
            System.Diagnostics.Debug.WriteLine("=== UNIFIED EVENT SYSTEM TEST COMPLETED ===");
        }
        
        #endregion
        
        public void Dispose()
        {
            if (_disposed) return;
            
            foreach (var handler in _eventHandlers)
            {
                handler?.Dispose();
            }
            
            _eventHandlers.Clear();
            _eventStatistics.Clear();
            _disposed = true;
            _instance = null;
        }
    }
}
