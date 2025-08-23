using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Events;
using IsometricActionGame.Events.EventHandlers;
using IsometricActionGame.UI;
using IsometricActionGame.NPCs;
using IsometricActionGame.Quests;
using IsometricActionGame.Dialogue;
using IsometricActionGame.Items;
using IsometricActionGame.Graphics;
using IsometricActionGame.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IsometricActionGame
{
    /// <summary>
    /// Comprehensive level system that manages level progression and data
    /// Uses event-driven architecture for modularity and extensibility
    /// </summary>
    public class LevelSystem
    {
        private readonly GameEventSystem _eventSystem;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IResolutionManager _resolutionManager;
        
        // Level management
        private int _currentLevel = 1;
        private int _maxLevel = 5; // Total number of levels
        private bool _isLoadingLevel = false;
        private bool _isLevelCompleted = false;
        private bool _isLoadingFromSave = false; // Flag to prevent position reset when loading from save
        
        // Level data storage
        private readonly Dictionary<int, LevelData> _levelData;
        private readonly Dictionary<int, LevelProgress> _levelProgress;
        
        // References to game systems
        private Player _player;
        private List<IEntity> _entities;
        private List<IInteractable> _interactables;
        private List<DroppedItem> _droppedItems;
        private BatSpawnManager _batSpawnManager;

        private Chest _chest;
        private DroppedItem _key;
        private Door _door;
        private Mayor _mayor;
        private Vendor _vendor;
        private QuestManager _questManager;
        private DialogueManager _dialogueManager;
        private AttackSystem _attackSystem;
        private ConsoleDisplay _console;
        private DeathPanel _deathPanel;
        private PauseMenu _pauseMenu;
        private SpriteFont _uiFont;
        private UIManager _uiManager; // Reference to UIManager for settings check
        private InputHandler _inputHandler; // Reference to InputHandler for updating LevelTransitionPanel
        
        // Save system data
        private Dictionary<string, int> _enemyKillCounts;
        private TimeSpan _totalPlayTime;
        
        // Level timing
        private DateTime _levelStartTime;
        private bool _isLevelTiming = false;
        
        // Level transition UI
        private LevelTransitionPanel _transitionPanel;
        
        public int CurrentLevel => _currentLevel;
        public int MaxLevel => _maxLevel;
        public bool IsLoadingLevel => _isLoadingLevel;
        public bool IsLevelCompleted => _isLevelCompleted;
        public bool IsLoadingFromSave => _isLoadingFromSave;
        public LevelTransitionPanel LevelTransitionPanel => _transitionPanel;
        
        /// <summary>
        /// Set flag to indicate loading from save (prevents position reset)
        /// </summary>
        public void SetLoadingFromSave(bool isLoadingFromSave)
        {
            _isLoadingFromSave = isLoadingFromSave;
        }
        
        /// <summary>
        /// Set UIManager reference for settings check
        /// </summary>
        public void SetUIManager(UIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        /// <summary>
        /// Set InputHandler reference for updating LevelTransitionPanel
        /// </summary>
        public void SetInputHandler(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
            System.Diagnostics.Debug.WriteLine($"LevelSystem.SetInputHandler: _inputHandler set to {(inputHandler != null ? "not null" : "null")}");
        }
        
        // public event Action<int> OnLevelStarted; // Removed unused event
        public event Action<int> OnLevelLoaded;
        public event Action<string> OnLevelFailed;
        
        public LevelSystem(
            ContentManager content, 
            GraphicsDevice graphicsDevice, 
            IResolutionManager resolutionManager)
        {
            _eventSystem = GameEventSystem.Instance;
            _content = content;
            _graphicsDevice = graphicsDevice;
            _resolutionManager = resolutionManager;
            
            _levelData = new Dictionary<int, LevelData>();
            _levelProgress = new Dictionary<int, LevelProgress>();
            
            InitializeLevelData();
            LoadLevelProgress();
            SubscribeToEvents();
        }
        
        /// <summary>
        /// Initialize level data for all levels
        /// </summary>
        private void InitializeLevelData()
        {
            // Level 1 - Tutorial level
            _levelData[1] = new LevelData
            {
                LevelNumber = 1,
                Name = "Tutorial Village",
                Description = "Learn the basics of combat and exploration",
                PlayerStartPosition = GameConstants.World.PLAYER_START_POSITION,
                PebblePosition = GameConstants.World.PEBBLE_START_POSITION,
                DoorPosition = GameConstants.World.DOOR_POSITION,
                MayorPosition = GameConstants.World.MAYOR_POSITION,
                VendorPosition = GameConstants.World.VENDOR_POSITION,
                BatSpawnPosition = GameConstants.World.BAT_START_POSITION,
                RequiredEnemiesKilled = 3,
                RequiredQuestsCompleted = 1,
                BackgroundTexture = "background",
                MusicTrack = "level1_music",
                Difficulty = LevelDifficulty.Easy
            };
            
            // Level 2 - Forest level
            _levelData[2] = new LevelData
            {
                LevelNumber = 2,
                Name = "Dark Forest",
                Description = "Navigate through the mysterious forest",
                PlayerStartPosition = new Vector2(100, 100),
                PebblePosition = new Vector2(800, 600),
                DoorPosition = new Vector2(900, 900),
                MayorPosition = new Vector2(200, 200),
                VendorPosition = new Vector2(300, 300),
                BatSpawnPosition = new Vector2(500, 500),
                RequiredEnemiesKilled = 5,
                RequiredQuestsCompleted = 2,
                BackgroundTexture = "forest_background",
                MusicTrack = "level2_music",
                Difficulty = LevelDifficulty.Medium
            };
            
            // Level 3 - Cave level
            _levelData[3] = new LevelData
            {
                LevelNumber = 3,
                Name = "Ancient Cave",
                Description = "Explore the depths of the ancient cave",
                PlayerStartPosition = new Vector2(150, 150),
                PebblePosition = new Vector2(850, 650),
                DoorPosition = new Vector2(950, 950),
                MayorPosition = new Vector2(250, 250),
                VendorPosition = new Vector2(350, 350),
                BatSpawnPosition = new Vector2(550, 550),
                RequiredEnemiesKilled = 8,
                RequiredQuestsCompleted = 3,
                BackgroundTexture = "cave_background",
                MusicTrack = "level3_music",
                Difficulty = LevelDifficulty.Hard
            };
            
            // Level 4 - Castle level
            _levelData[4] = new LevelData
            {
                LevelNumber = 4,
                Name = "Castle Ruins",
                Description = "Conquer the ancient castle ruins",
                PlayerStartPosition = new Vector2(200, 200),
                PebblePosition = new Vector2(900, 700),
                DoorPosition = new Vector2(1000, 1000),
                MayorPosition = new Vector2(300, 300),
                VendorPosition = new Vector2(400, 400),
                BatSpawnPosition = new Vector2(600, 600),
                RequiredEnemiesKilled = 12,
                RequiredQuestsCompleted = 4,
                BackgroundTexture = "castle_background",
                MusicTrack = "level4_music",
                Difficulty = LevelDifficulty.VeryHard
            };
            
            // Level 5 - Final level
            _levelData[5] = new LevelData
            {
                LevelNumber = 5,
                Name = "Final Challenge",
                Description = "Face the ultimate challenge",
                PlayerStartPosition = new Vector2(250, 250),
                PebblePosition = new Vector2(950, 750),
                DoorPosition = new Vector2(1050, 1050),
                MayorPosition = new Vector2(350, 350),
                VendorPosition = new Vector2(450, 450),
                BatSpawnPosition = new Vector2(650, 650),
                RequiredEnemiesKilled = 15,
                RequiredQuestsCompleted = 5,
                BackgroundTexture = "final_background",
                MusicTrack = "level5_music",
                Difficulty = LevelDifficulty.Extreme
            };
        }
        
        /// <summary>
        /// Load level progress from save file
        /// </summary>
        private void LoadLevelProgress()
        {
            try
            {
                // In a real implementation, this would load from a save file
                // For now, we'll initialize with default values
                for (int i = 1; i <= _maxLevel; i++)
                {
                    _levelProgress[i] = new LevelProgress
                    {
                        LevelNumber = i,
                        IsCompleted = false,
                        IsUnlocked = i == 1, // Only first level is unlocked initially
                        BestTime = TimeSpan.Zero,
                        EnemiesKilled = 0,
                        QuestsCompleted = 0,
                        ItemsCollected = 0
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load level progress: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Subscribe to level-related events
        /// </summary>
        private void SubscribeToEvents()
        {
            _eventSystem.Subscribe<object>(GameEvents.DOOR_OPENED, OnDoorOpened);
            _eventSystem.Subscribe<object>(GameEvents.NEXT_LEVEL_TRIGGERED, OnNextLevelTriggered);
            _eventSystem.Subscribe<object>(GameEvents.ENEMY_DEFEATED, OnEnemyDefeated);
            // Count only return quests (quests turned in to Mayor) to avoid duplication
            _eventSystem.Subscribe<object>(GameEvents.RETURN_QUEST_COMPLETED, OnQuestCompleted);
            _eventSystem.Subscribe<object>(GameEvents.ITEM_PICKED_UP, OnItemPickedUp);
        }
        
        /// <summary>
        /// Set references to game objects
        /// </summary>
        public void SetGameReferences(
            Player player,
            List<IEntity> entities,
            List<IInteractable> interactables,
            List<DroppedItem> droppedItems,
            BatSpawnManager batSpawnManager,
            Chest chest,
            DroppedItem key,
            Door door,
            Mayor mayor,
            Vendor vendor,
            QuestManager questManager,
            DialogueManager dialogueManager,
            AttackSystem attackSystem,
            ConsoleDisplay console,
            DeathPanel deathPanel,
            PauseMenu pauseMenu,
            SpriteFont uiFont)
        {
            _player = player;
            _entities = entities;
            _interactables = interactables;
            _droppedItems = droppedItems;
            _batSpawnManager = batSpawnManager;
            _chest = chest;
            _key = key;
            _door = door;
            _mayor = mayor;
            _vendor = vendor;
            _questManager = questManager;
            _dialogueManager = dialogueManager;
            _attackSystem = attackSystem;
            _console = console;
            _deathPanel = deathPanel;
            _pauseMenu = pauseMenu;
            _uiFont = uiFont;
        }
        
        /// <summary>
        /// Set save system data for level transition panel
        /// </summary>
        public void SetSaveData(Dictionary<string, int> enemyKillCounts, TimeSpan totalPlayTime)
        {
            _enemyKillCounts = enemyKillCounts ?? new Dictionary<string, int>();
            _totalPlayTime = totalPlayTime;
        }
        
        /// <summary>
        /// Get current enemy kill counts
        /// </summary>
        public Dictionary<string, int> GetCurrentEnemyKillCounts()
        {
            return _enemyKillCounts ?? new Dictionary<string, int>();
        }
        
        /// <summary>
        /// Get current total play time
        /// </summary>
        public TimeSpan GetCurrentTotalPlayTime()
        {
            return _totalPlayTime;
        }
        
        /// <summary>
        /// Get current level play time
        /// </summary>
        public TimeSpan GetCurrentLevelPlayTime()
        {
            if (_isLevelTiming)
            {
                return DateTime.UtcNow - _levelStartTime;
            }
            return TimeSpan.Zero;
        }
        
        /// <summary>
        /// Start a specific level
        /// </summary>
        public void StartLevel(int levelNumber)
        {
            if (levelNumber < 1 || levelNumber > _maxLevel)
            {
                _console?.AddMessage($"Invalid level number: {levelNumber}", GameConstants.Colors.CONSOLE_RED);
                return;
            }
            
            if (!_levelProgress[levelNumber].IsUnlocked)
            {
                _console?.AddMessage($"Level {levelNumber} is not unlocked yet!", GameConstants.Colors.CONSOLE_RED);
                return;
            }
            
            _currentLevel = levelNumber;
            _isLevelCompleted = false;
            
            // Start level timing
            _levelStartTime = DateTime.UtcNow;
            _isLevelTiming = true;
            
            _eventSystem.Publish(GameEvents.LEVEL_STARTED, new { LevelNumber = levelNumber, LevelData = _levelData[levelNumber] });
            _console?.AddMessage($"Starting Level {levelNumber}: {_levelData[levelNumber].Name}", GameConstants.Colors.CONSOLE_CYAN);
            
            LoadLevel(levelNumber);
        }
        
        /// <summary>
        /// Load level data and setup entities
        /// </summary>
        private void LoadLevel(int levelNumber)
        {
            _isLoadingLevel = true;
            _eventSystem.Publish(GameEvents.LEVEL_LOADING, new { LevelNumber = levelNumber });
            
            try
            {
                var levelData = _levelData[levelNumber];
                
                // Clear existing entities
                _entities?.Clear();
                _droppedItems?.Clear();
                ProjectileManager.Instance.Clear();
                _attackSystem?.ClearEntities();
                _batSpawnManager?.Reset();
                
                // Reset player position (only if not loading from save)
                if (_player != null)
                {
                    // Don't reset position if loading from save - position will be restored by save system
                    if (!_isLoadingFromSave)
                    {
                        _player.WorldPosition = levelData.PlayerStartPosition;
                    }
                    _player.SetCanMove(true);
                    _entities?.Add(_player);
                    _attackSystem?.RegisterEntity(_player);
                }
                
                // Setup level-specific entities
                SetupLevelEntities(levelData);
                
                // Setup level-specific quests
                SetupLevelQuests(levelData);
                
                // Update level progress
                UpdateLevelProgress(levelNumber);
                
                _isLoadingLevel = false;
                _eventSystem.Publish(GameEvents.LEVEL_LOADED, new { LevelNumber = levelNumber });
                OnLevelLoaded?.Invoke(levelNumber);
                
                _console?.AddMessage($"Level {levelNumber} loaded successfully!", GameConstants.Colors.CONSOLE_GREEN);
            }
            catch (Exception ex)
            {
                _isLoadingLevel = false;
                _eventSystem.Publish(GameEvents.LEVEL_FAILED, ex.Message);
                OnLevelFailed?.Invoke(ex.Message);
                _console?.AddMessage($"Failed to load level {levelNumber}: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
            }
        }
        
        /// <summary>
        /// Setup level-specific entities
        /// </summary>
        private void SetupLevelEntities(LevelData levelData)
        {
            // Setup Pebble - create new one since entities list was cleared
            var pebble = new Pebble(levelData.PebblePosition, GameConstants.Timing.PEBBLE_SHOOT_COOLDOWN, GameConstants.Damage.PEBBLE_DAMAGE);
            
            // Load content for the new Pebble
            try
            {
                pebble.LoadContent(_content);
                _console?.AddMessage("Pebble content loaded successfully", GameConstants.Colors.CONSOLE_GREEN);
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Failed to load Pebble content: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"LevelSystem: Failed to load Pebble content: {ex}");
            }
            
            _entities?.Add(pebble);
            _attackSystem?.RegisterEntity(pebble);
            
            // Subscribe to Pebble death event
            SubscribeToPebbleEvents(pebble);
            
            _console?.AddMessage($"Pebble spawned at {levelData.PebblePosition}", GameConstants.Colors.CONSOLE_CYAN);
            
            // Setup Door
            if (_door != null)
            {
                _door.SetPosition(levelData.DoorPosition);
                _door.Reset();
                
                // Subscribe to door events
                SubscribeToDoorEvents(_door);
            }
            
            // Setup NPCs
            if (_mayor != null)
            {
                _mayor.SetPosition(levelData.MayorPosition);
                _mayor.SetPlayerReference(_player);
                _interactables?.Add(_mayor);
            }
            
            if (_vendor != null)
            {
                _vendor.SetPosition(levelData.VendorPosition);
                _vendor.SetPlayerReference(_player);
                _interactables?.Add(_vendor);
            }
            
            // Reset other entities
            _chest = null;
            _key = null;
            
            // Reset player state for new level
            if (_player != null)
            {
                _player.HasKey = false;
                _player.SetCanMove(true);
            }
        }
        
                 /// <summary>
         /// Setup level-specific quests
         /// </summary>
         private void SetupLevelQuests(LevelData levelData)
         {
             if (_questManager != null)
             {
                 _questManager.ClearQuests();
                 
                 // Quests will be added when player accepts them from Mayor
                 // No automatic quest assignment
             }
         }
        
        /// <summary>
        /// Handle door opened event
        /// </summary>
        private void OnDoorOpened(object data)
        {
            try
            {
                _console?.AddMessage("Door opened! Processing level completion...", GameConstants.Colors.CONSOLE_CYAN);
                
                // Mark level as completed only once
                if (!_isLevelCompleted)
                {
                    _isLevelCompleted = true;
                    
                    // Calculate total play time for the level
                    TimeSpan levelPlayTime = DateTime.UtcNow - _levelStartTime;
                    
                    // Stop level timing
                    _isLevelTiming = false;
                    
                    // Update level progress
                    if (_levelProgress.ContainsKey(_currentLevel))
                    {
                        var progress = _levelProgress[_currentLevel];
                        progress.IsCompleted = true;
                        progress.BestTime = levelPlayTime;
                        
                        // Unlock next level
                        if (_currentLevel < _maxLevel)
                        {
                            _levelProgress[_currentLevel + 1].IsUnlocked = true;
                        }
                        
                        SaveLevelProgress();
                    }
                }
                
                // Always show level completion message and transition panel, even if level was already completed
                ShowLevelCompletionMessage();
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error in OnDoorOpened: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"Error in OnDoorOpened: {ex}");
            }
        }
        
        /// <summary>
        /// Handle enemy defeated event
        /// </summary>
        private void OnEnemyDefeated(object data)
        {
            if (_levelProgress.ContainsKey(_currentLevel))
            {
                _levelProgress[_currentLevel].EnemiesKilled++;
                System.Diagnostics.Debug.WriteLine($"LevelSystem.OnEnemyDefeated: Enemies killed for level {_currentLevel}: {_levelProgress[_currentLevel].EnemiesKilled}");
            }
        }
        
        /// <summary>
        /// Handle return quest completed event (quest turned in to Mayor)
        /// Only counts quests that are actually turned in for rewards to avoid duplication
        /// </summary>
        private void OnQuestCompleted(object data)
        {
            if (_levelProgress.ContainsKey(_currentLevel))
            {
                _levelProgress[_currentLevel].QuestsCompleted++;
                System.Diagnostics.Debug.WriteLine($"LevelSystem.OnQuestCompleted: Return quests completed for level {_currentLevel}: {_levelProgress[_currentLevel].QuestsCompleted}");
            }
        }

        /// <summary>
        /// Handle item picked up event
        /// </summary>
        private void OnItemPickedUp(object data)
        {
            if (_levelProgress.ContainsKey(_currentLevel))
            {
                _levelProgress[_currentLevel].ItemsCollected++;
                System.Diagnostics.Debug.WriteLine($"LevelSystem.OnItemPickedUp: Items collected for level {_currentLevel}: {_levelProgress[_currentLevel].ItemsCollected}");
            }
        }
        
        /// <summary>
        /// Handle next level triggered event
        /// </summary>
        private void OnNextLevelTriggered(object data)
        {
            try
            {
                _console?.AddMessage("Next level event triggered", GameConstants.Colors.CONSOLE_CYAN);
                
                if (_currentLevel < _maxLevel)
                {
                    _console?.AddMessage($"Transitioning to level {_currentLevel + 1}...", GameConstants.Colors.CONSOLE_CYAN);
                    
                    // Hide transition panel if it exists
                    _transitionPanel?.Hide();
                    
                    StartLevel(_currentLevel + 1);
                }
                else
                {
                    // Game completed
                    _console?.AddMessage("Congratulations! You have completed all levels!", GameConstants.Colors.CONSOLE_GOLD);
                    _eventSystem.Publish(GameEvents.GAME_COMPLETED, new { TotalLevels = _maxLevel });
                }
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error transitioning to next level: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"Error in OnNextLevelTriggered: {ex}");
            }
        }
        
        /// <summary>
        /// Show level completion message and transition panel
        /// </summary>
        private void ShowLevelCompletionMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LevelSystem.ShowLevelCompletionMessage: Starting - IsSettingsActive: {IsSettingsActive()}");
                
                // Check if settings are active - don't show transition panel if settings are open
                if (IsSettingsActive())
                {
                    _console?.AddMessage("Level completion message skipped - settings are active", GameConstants.Colors.CONSOLE_GRAY);
                    System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelCompletionMessage: Skipped due to settings being active");
                    return;
                }
                
                var levelData = _levelData[_currentLevel];
                
                _console?.AddMessage($"=== LEVEL {_currentLevel} COMPLETED ===", GameConstants.Colors.CONSOLE_GOLD);
                _console?.AddMessage($"Level: {levelData.Name}", GameConstants.Colors.CONSOLE_GOLD);
                _console?.AddMessage($"Description: {levelData.Description}", GameConstants.Colors.CONSOLE_GOLD);
                
                if (_currentLevel < _maxLevel)
                {
                    _console?.AddMessage("Publishing level completed event...", GameConstants.Colors.CONSOLE_CYAN);
                    
                    // Publish level completed event - UnifiedUIEventHandler will manage transition panel visibility
                    _eventSystem.Publish(GameEvents.LEVEL_COMPLETED, new { LevelNumber = _currentLevel });
                    
                    // Show transition panel (UnifiedUIEventHandler will prevent this if settings are open)
                    System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelCompletionMessage: About to call ShowLevelTransitionPanel");
                    ShowLevelTransitionPanel();
                }
                else
                {
                    _console?.AddMessage("You have completed all levels! Congratulations!", GameConstants.Colors.CONSOLE_GOLD);
                }
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error in ShowLevelCompletionMessage: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"Error in ShowLevelCompletionMessage: {ex}");
            }
        }
        
        /// <summary>
        /// Show level transition panel
        /// </summary>
        private void ShowLevelTransitionPanel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelTransitionPanel: Method called");
                _console?.AddMessage("Initializing level transition panel...", GameConstants.Colors.CONSOLE_CYAN);
                
                if (_transitionPanel == null)
                {
                    var currentRes = _resolutionManager.CurrentResolution;
                    _console?.AddMessage($"Creating transition panel with resolution: {currentRes.Width}x{currentRes.Height}", GameConstants.Colors.CONSOLE_CYAN);
                    
                    // Check if UI font is available
                    if (_uiFont == null)
                    {
                        _console?.AddMessage("Error: UI font is null, cannot create transition panel", GameConstants.Colors.CONSOLE_RED);
                        System.Diagnostics.Debug.WriteLine("Error: _uiFont is null in ShowLevelTransitionPanel");
                        return;
                    }
                    
                    _transitionPanel = new LevelTransitionPanel(_uiFont, currentRes.Width, currentRes.Height);
                    System.Diagnostics.Debug.WriteLine($"LevelSystem.ShowLevelTransitionPanel: Created new LevelTransitionPanel - IsNull: {_transitionPanel == null}");
                    
                    // Check if graphics device is available
                    if (_graphicsDevice == null)
                    {
                        _console?.AddMessage("Error: Graphics device is null, cannot load transition panel content", GameConstants.Colors.CONSOLE_RED);
                        System.Diagnostics.Debug.WriteLine("Error: _graphicsDevice is null in ShowLevelTransitionPanel");
                        return;
                    }
                    
                    _transitionPanel.LoadContent(_content, _graphicsDevice);
                    System.Diagnostics.Debug.WriteLine($"LevelSystem: Setting event system - _eventSystem: {_eventSystem != null}");
                    _transitionPanel.SetEventSystem(_eventSystem);
                    
                    // Update InputHandler reference to the new LevelTransitionPanel
                    System.Diagnostics.Debug.WriteLine($"LevelSystem.ShowLevelTransitionPanel: About to update InputHandler - _inputHandler: {_inputHandler != null}, _transitionPanel: {_transitionPanel != null}");
                    _inputHandler?.UpdateLevelTransitionPanel(_transitionPanel);
                    
                    // Update UnifiedUIEventHandler reference to the new LevelTransitionPanel
                    // This ensures UnifiedUIEventHandler can access the panel for ESC handling
                    System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelTransitionPanel: Updating UI event handlers");
                    // Get UnifiedEventSystem instance and update handlers
                    var unifiedEventSystem = UnifiedEventSystem.Instance;
                    if (unifiedEventSystem != null)
                    {
                        unifiedEventSystem.UpdateUIEventHandlers();
                        System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelTransitionPanel: UI event handlers updated");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("LevelSystem.ShowLevelTransitionPanel: UnifiedEventSystem is null!");
                    }
                    
                    // Subscribe to events
                    _transitionPanel.OnNextLevel += () => 
                    {
                        _console?.AddMessage("Next Level button pressed! Triggering next level...", GameConstants.Colors.CONSOLE_CYAN);
                        _eventSystem.Publish<object>(GameEvents.NEXT_LEVEL_TRIGGERED, null);
                    };
                    
                    _transitionPanel.OnMenu += () => 
                    {
                        _console?.AddMessage("Menu button pressed! Going to menu...", GameConstants.Colors.CONSOLE_CYAN);
                        _eventSystem.Publish<object>(GameEvents.RETURN_TO_MENU, null);
                    };
                    
                    // _transitionPanel.OnPauseMenu += () => 
                    // {
                    //     _console?.AddMessage("Pause Menu button pressed! Showing pause menu...", GameConstants.Colors.CONSOLE_CYAN);
                    //     _eventSystem.Publish<object>(GameEvents.GAME_PAUSED, null);
                    // };
                }
                
                if (_currentLevel < _maxLevel && _levelData.ContainsKey(_currentLevel + 1))
                {
                    var nextLevelData = _levelData[_currentLevel + 1];
                    
                    // Get battle statistics from save data and level progress
                    var levelProgress = _levelProgress[_currentLevel];
                    
                    // Calculate total enemies killed from save data
                    var totalEnemiesKilled = 0;
                    if (_enemyKillCounts != null)
                    {
                        foreach (var killCount in _enemyKillCounts.Values)
                        {
                            totalEnemiesKilled += killCount;
                        }
                    }
                    
                    // Use real-time data instead of level progress data
                    var currentEnemiesKilled = levelProgress.EnemiesKilled;
                    var currentQuestsCompleted = levelProgress.QuestsCompleted;
                    var currentItemsCollected = levelProgress.ItemsCollected;
                    var currentLevelTime = GetCurrentLevelPlayTime();
                    
                    System.Diagnostics.Debug.WriteLine($"LevelSystem.ShowLevelTransitionPanel: Level {_currentLevel} statistics:");
                    System.Diagnostics.Debug.WriteLine($"  Enemies Killed: {currentEnemiesKilled}");
                    System.Diagnostics.Debug.WriteLine($"  Quests Completed: {currentQuestsCompleted}");
                    System.Diagnostics.Debug.WriteLine($"  Items Collected: {currentItemsCollected}");
                    System.Diagnostics.Debug.WriteLine($"  Level Time: {currentLevelTime}");
                    
                    // Show transition panel with level data
                    _transitionPanel.Show(
                        _currentLevel,
                        nextLevelData.Name,
                        nextLevelData.Description,
                        currentEnemiesKilled,
                        currentQuestsCompleted,
                        currentItemsCollected,
                        currentLevelTime
                    );
                    
                    _console?.AddMessage("Level transition panel shown successfully!", GameConstants.Colors.CONSOLE_CYAN);
                }
                else
                {
                    _console?.AddMessage("No next level available or level data missing", GameConstants.Colors.CONSOLE_RED);
                }
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Error in ShowLevelTransitionPanel: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"Error in ShowLevelTransitionPanel: {ex}");
            }
        }

        private bool IsSettingsActive()
        {
            try
            {
                // Check if settings are active through UIManager
                if (_uiManager != null)
                {
                    bool isActive = _uiManager.SettingsActive;
                    System.Diagnostics.Debug.WriteLine($"LevelSystem.IsSettingsActive: UIManager.SettingsActive = {isActive}");
                    return isActive;
                }
                
                // Fallback: check if pause menu is visible
                bool pauseMenuVisible = _pauseMenu?.IsVisible == true;
                System.Diagnostics.Debug.WriteLine($"LevelSystem.IsSettingsActive: Fallback to pause menu, visible = {pauseMenuVisible}");
                return pauseMenuVisible;
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"LevelSystem.IsSettingsActive: Exception = {ex.Message}");
                return false; 
            }
        }
        
        /// <summary>
        /// Update level progress
        /// </summary>
        private void UpdateLevelProgress(int levelNumber)
        {
            if (_levelProgress.ContainsKey(levelNumber))
            {
                var progress = _levelProgress[levelNumber];
                progress.LastPlayed = DateTime.UtcNow;
                progress.PlayCount++;
            }
        }
        
        /// <summary>
        /// Save level progress to file
        /// </summary>
        private void SaveLevelProgress()
        {
            try
            {
                var progressData = new LevelProgressData
                {
                    CurrentLevel = _currentLevel,
                    MaxLevel = _maxLevel,
                    LevelProgress = _levelProgress
                };
                
                var json = JsonSerializer.Serialize(progressData, new JsonSerializerOptions { WriteIndented = true });
                // In a real implementation, this would save to a file
                System.Diagnostics.Debug.WriteLine($"Level progress saved: {json}");
                
                _eventSystem.Publish(GameEvents.LEVEL_PROGRESS_SAVED, new { ProgressData = progressData });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save level progress: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Subscribe to Pebble events
        /// </summary>
        private void SubscribeToPebbleEvents(Pebble pebble)
        {
            if (pebble == null) return;
            
            // Chest creation is now handled by GameRestartManager
            // No need to create chest here to avoid duplication
        }
        
        /// <summary>
        /// Subscribe to door events
        /// </summary>
        private void SubscribeToDoorEvents(Door door)
        {
            if (door == null) return;
            
            door.OnOpened += (d) =>
            {
                _console?.AddMessage("Door opened! Level completed!", GameConstants.Colors.CONSOLE_GOLD);
            };
        }
        
        // Chest events are now handled by GameRestartManager
        // No need to handle chest events here to avoid duplication
        
        /// <summary>
        /// Get current level data
        /// </summary>
        public LevelData GetCurrentLevelData()
        {
            return _levelData.ContainsKey(_currentLevel) ? _levelData[_currentLevel] : null;
        }
        
        /// <summary>
        /// Get level progress for a specific level
        /// </summary>
        public LevelProgress GetLevelProgress(int levelNumber)
        {
            return _levelProgress.ContainsKey(levelNumber) ? _levelProgress[levelNumber] : null;
        }
        
        /// <summary>
        /// Check if a level is unlocked
        /// </summary>
        public bool IsLevelUnlocked(int levelNumber)
        {
            return _levelProgress.ContainsKey(levelNumber) && _levelProgress[levelNumber].IsUnlocked;
        }
        
        /// <summary>
        /// Check if a specific level is completed
        /// </summary>
        public bool IsLevelCompletedByNumber(int levelNumber)
        {
            return _levelProgress.ContainsKey(levelNumber) && _levelProgress[levelNumber].IsCompleted;
        }
        
        /// <summary>
        /// Update the level transition panel
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _transitionPanel?.Update(gameTime);
        }
        
        /// <summary>
        /// Draw the level transition panel
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            _transitionPanel?.Draw(spriteBatch);
        }
    }
    
    /// <summary>
    /// Data structure for level information
    /// </summary>
    public class LevelData
    {
        public int LevelNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector2 PlayerStartPosition { get; set; }
        public Vector2 PebblePosition { get; set; }
        public Vector2 DoorPosition { get; set; }
        public Vector2 MayorPosition { get; set; }
        public Vector2 VendorPosition { get; set; }
        public Vector2 BatSpawnPosition { get; set; }
        public int RequiredEnemiesKilled { get; set; }
        public int RequiredQuestsCompleted { get; set; }
        public string BackgroundTexture { get; set; }
        public string MusicTrack { get; set; }
        public LevelDifficulty Difficulty { get; set; }
    }
    
    /// <summary>
    /// Data structure for level progress
    /// </summary>
    public class LevelProgress
    {
        public int LevelNumber { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsUnlocked { get; set; }
        public TimeSpan BestTime { get; set; }
        public int EnemiesKilled { get; set; }
        public int QuestsCompleted { get; set; }
        public int ItemsCollected { get; set; }
        public DateTime LastPlayed { get; set; }
        public int PlayCount { get; set; }
    }
    
    /// <summary>
    /// Data structure for saving level progress
    /// </summary>
    public class LevelProgressData
    {
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public Dictionary<int, LevelProgress> LevelProgress { get; set; }
    }
    
    /// <summary>
    /// Level difficulty enumeration
    /// </summary>
    public enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard,
        VeryHard,
        Extreme
    }
}
