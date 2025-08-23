using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using IsometricActionGame.UI;
using IsometricActionGame.NPCs;
using IsometricActionGame.Quests;
using IsometricActionGame.Dialogue;
using IsometricActionGame.Items;
using IsometricActionGame.Inventory;
using IsometricActionGame.Settings;
using IsometricActionGame.Graphics;
using IsometricActionGame.Events;
using IsometricActionGame.Events.EventHandlers;
using IsometricActionGame.Events.EventData;
using IsometricActionGame.SaveSystem;
using IsometricActionGame.Input;

using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

using IsometricActionGame;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Factories;

namespace IsometricActionGame;

public class Game1 : Game
{
    // Static instance for global access
    public static Game1 Instance { get; private set; }
    
    // Public property for player access
    public Player Player => _player;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameMap _map;
    public Player _player;
    private ICameraManager _camera;
    public IResolutionManager _resolutionManager;
    public UIManager _uiManager;
    private List<IEntity> _entities;
    private List<IInteractable> _interactables;
    private List<DroppedItem> _droppedItems;
    private BatSpawnManager _batSpawnManager;

    private Chest _chest;
    private DroppedItem _key;
    private Door _door;
    
    // Flags to prevent duplicate death messages
    
    private Texture2D _tileTexture;
    private Texture2D _backgroundTexture;
    private Dictionary<TileType, Color> _tileColors;
    private bool _gameStarted = false;
    private ConsoleDisplay _console;
    // Input handling is now centralized in InputHandler
    private InputHandler _inputHandler;
    private Input.Handlers.DialogueInputHandler _dialogueInputHandler;
    

        public QuestManager _questManager;
        public DialogueManager _dialogueManager;
    private Mayor _mayor;
    private Vendor _vendor;
    private SpriteFont _uiFont;
    

    private AttackSystem _attackSystem;
    
    // UI Panels
    public DeathPanel _deathPanel;
    public PauseMenu _pauseMenu;
    private SaveSlotManager _saveSlotManager;

    private bool _isGamePaused = false;
    private bool _isLevelTransitionActive = false;
    
    // Game Settings
    private GameSettings _gameSettings;
    
    // Unified Event System
    private UnifiedEventSystem _unifiedEventSystem;
    
    // Game Systems
    private GameRestartManager _restartManager;
    private LevelSystem _levelSystem;
    
    
    // Save System
    private GameSaveLoadManager _saveLoadManager;
    private SaveManager _saveManager;
    private Dictionary<string, int> _enemyKillCounts;
    
    // Input handling
    private object _pendingMovementData = null;



    public Game1()
    {
        // Set static instance
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        // Initialize Event System as early as possible
        // This ensures GameEventSystem.Instance is available from the start
        var eventSystem = GameEventSystem.Instance;
    }

    private void LogToFile(string message)
    {
        try
        {
            string logPath = "game_log.txt";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] {message}";
            File.AppendAllText(logPath, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore logging errors
        }
    }

    protected override void Initialize()
    {
        try
        {
            LogToFile("Game1.Initialize: Starting initialization...");
            
            _gameSettings = new GameSettings();
            _gameSettings.LoadSettings();
            LogToFile("Game1.Initialize: GameSettings loaded");

            // Initialize resolution manager first
            _resolutionManager = ResolutionManager.Instance;
            LogToFile("Game1.Initialize: ResolutionManager initialized");
            
            _map = new GameMap();
            LogToFile("Game1.Initialize: GameMap created");
            
            // Debug: Log detailed map visualization
            // MapDebugHelper.LogDetailedMapToConsole(_map); // Removed - MapDebugHelper was deleted
            
            _player = EntityFactory.CreatePlayer(_map);
            LogToFile("Game1.Initialize: Player created");
            
            _camera = new CameraManager();
            LogToFile("Game1.Initialize: CameraManager created");
            
            _uiManager = new UIManager();
            LogToFile("Game1.Initialize: UIManager created");
            
            _entities = new List<IEntity>();
            _interactables = new List<IInteractable>();
            _droppedItems = new List<DroppedItem>();
            LogToFile("Game1.Initialize: Lists initialized");

            _console = new ConsoleDisplay(new Vector2(GameConstants.UI.CONSOLE_DEFAULT_X, GameConstants.UI.CONSOLE_DEFAULT_Y));
            _console.Initialize(GameConstants.UI.CONSOLE_DEFAULT_WIDTH, GameConstants.UI.CONSOLE_DEFAULT_HEIGHT);
            LogToFile("Game1.Initialize: ConsoleDisplay created");
            
            // Initialize RPG systems
            _questManager = new QuestManager();
            LogToFile("Game1.Initialize: QuestManager created");
            
            // Subscribe QuestManager to Pebble events
            LogToFile("Game1.Initialize: QuestManager Pebble events setup");
            
            _dialogueManager = new DialogueManager();
            LogToFile("Game1.Initialize: DialogueManager created");
            
            _mayor = EntityFactory.CreateMayor(_dialogueManager, _questManager);
            LogToFile("Game1.Initialize: Mayor created");
            
            _vendor = EntityFactory.CreateVendor(_dialogueManager);
            LogToFile("Game1.Initialize: Vendor created");
            
            // Initialize Attack System
            _attackSystem = new AttackSystem();
            LogToFile("Game1.Initialize: AttackSystem created");
            
            // Initialize Input Handler
            _inputHandler = new InputHandler();
            LogToFile("Game1.Initialize: InputHandler created");
            
            // Initialize level entities FIRST
            _batSpawnManager = new BatSpawnManager(_questManager);
            LogToFile("Game1.Initialize: BatSpawnManager created");
            
            var pebble = EntityFactory.CreatePebble(GameConstants.Timing.PEBBLE_SHOOT_COOLDOWN, GameConstants.Damage.PEBBLE_DAMAGE);
            
            // Initialize Game Systems AFTER entities are created
            _restartManager = new GameRestartManager(Content, GraphicsDevice, _resolutionManager);
            LogToFile("Game1.Initialize: GameRestartManager created");
            
            _levelSystem = new LevelSystem(Content, GraphicsDevice, _resolutionManager);
            LogToFile("Game1.Initialize: LevelSystem created");
            
            // Initialize Event System AFTER LevelSystem is created
            InitializeEventSystem();
            LogToFile("Game1.Initialize: EventSystem initialized");
            
            // Set references for restart manager AFTER all entities are created
            _restartManager.SetGameReferences(_console, _levelSystem);
            LogToFile("Game1.Initialize: RestartManager references set");
            
            // Initialize Save System
            _saveManager = new SaveManager();
            _enemyKillCounts = new Dictionary<string, int>();
            _chest = null; // Chest spawns only when Pebble is defeated
            _key = null; // Key spawns only from chest
            LogToFile("Game1.Initialize: Save system initialized");
            
            LogToFile("Game1.Initialize: Creating door...");
            _door = EntityFactory.CreateDoor();
            LogToFile($"Game1.Initialize: Door created at position {_door.WorldPosition}");

            _entities.Add(_player);
            _entities.Add(pebble);
            
            _entities.Add(_mayor);
            _entities.Add(_vendor);
            _entities.Add(_door);
            System.Diagnostics.Debug.WriteLine($"Game1.Initialize: Added door to entities list. Total entities: {_entities.Count}");
            
            _interactables.Add(_door);
            System.Diagnostics.Debug.WriteLine($"Game1.Initialize: Added door to interactables list. Total interactables: {_interactables.Count}");
            
            // Set player reference in NPCs for dynamic depth calculation
            _mayor.SetPlayerReference(_player);
            _vendor.SetPlayerReference(_player);
            LogToFile("Game1.Initialize: Player references set in NPCs");
            
            // Register attackable entities in Attack System
            _attackSystem.RegisterEntity(_player);
            _attackSystem.RegisterEntity(pebble);
            LogToFile("Game1.Initialize: Entities registered in AttackSystem");
            
            // Set UIManager reference for settings check
            _levelSystem.SetUIManager(_uiManager);
            LogToFile("Game1.Initialize: UIManager reference set");
            
            // Set save data for level system
            LogToFile($"Game1: Setting save data - _enemyKillCounts count: {_enemyKillCounts?.Count ?? 0}");
            _levelSystem.SetSaveData(_enemyKillCounts, TimeSpan.Zero);
            LogToFile("Game1.Initialize: Save data set");
            
            // Start level 1
            LogToFile("Game1.Initialize: Starting level 1...");
            _levelSystem.StartLevel(1);
            LogToFile("Game1.Initialize: Level 1 started");
            
            // Initialize ProjectileManager
            ProjectileManager.Instance.Initialize(Content);
            LogToFile("Game1.Initialize: ProjectileManager initialized");
            
            _tileColors = new Dictionary<TileType, Color>
            {
                { TileType.Grass, GameConstants.Colors.TILE_GRASS },
                { TileType.Wall, GameConstants.Colors.TILE_WALL },

                { TileType.Door, GameConstants.Colors.TILE_DOOR },
                { TileType.Chest, GameConstants.Colors.TILE_CHEST },
                { TileType.EnemySpawn, GameConstants.Colors.TILE_ENEMY_SPAWN },
                { TileType.KeySpawn, GameConstants.Colors.TILE_KEY_SPAWN }
            };
            LogToFile("Game1.Initialize: Tile colors initialized");

            // Subscribe to Pebble events
            SubscribeToPebbleEvents();
            LogToFile("Game1.Initialize: Pebble events subscribed");

            LogToFile("Game1.Initialize: Calling base.Initialize()...");
            base.Initialize();
            LogToFile("Game1.Initialize: Initialization completed successfully!");
        }
        catch (Exception ex)
        {
            LogToFile($"Game1.Initialize: CRITICAL ERROR during initialization: {ex.Message}");
            LogToFile($"Game1.Initialize: Exception details: {ex}");
            throw; // Re-throw to see the error
        }
    }
    

    
    private void OnPlayerAttack(Vector2 playerPos, Vector2 attackDirection)
    {
        // Create attack using new system
        var attack = new Attack(
            _player.WorldPosition,
            _player.FacingDirection,
            GameConstants.Attack.PLAYER_ATTACK_RANGE, // Attack range in world units
            GameConstants.Attack.PLAYER_ATTACK_CONE_ANGLE, // Cone angle from constants
            GameConstants.Damage.PLAYER_ATTACK_DAMAGE, // Damage from constants
            "melee" // Attack type
        );
        
        // Execute attack using Attack System
        var hitTargets = _attackSystem.ExecuteAttack(attack);
        
        if (hitTargets.Count > 0)
        {
            foreach (var target in hitTargets)
            {
                string targetName = target.GetType().Name;
                
                _console?.AddMessage($"Player hit {targetName}!", GameConstants.Colors.CONSOLE_ORANGE);
            }
        }
        else
        {
            _console?.AddMessage("Attack missed!", GameConstants.Colors.CONSOLE_GRAY);
        }
    }

    protected override void LoadContent()
    {
        // Initialize resolution manager with graphics device
        _resolutionManager.Initialize(_graphics, GraphicsDevice);
        

        
        // Apply initial graphics settings safely
        bool applied = _resolutionManager.ApplyResolution(_gameSettings.CurrentResolution, _gameSettings.IsFullscreen);
        if (!applied)
        {
            // Failed to apply initial resolution, using fallback
        }
        
        // Initialize camera with current viewport
        _camera.Initialize(GraphicsDevice.Viewport);
        
        // Initialize UI scaling system with current resolution
        var currentResolution = _resolutionManager.CurrentResolution;
        UIScalingManager.Initialize(currentResolution.Width, currentResolution.Height);
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _tileTexture = new Texture2D(GraphicsDevice, 1, 1);
        _tileTexture.SetData(new[] { GameConstants.Colors.CONSOLE_WHITE });
        
        try
        {
            _backgroundTexture = Content.Load<Texture2D>("background");
        }
        catch (Exception)
        {
            // Failed to load background texture
        }
        
        try
        {
            _player.LoadContent(Content);
        }
        catch (Exception)
        {
            // Failed to load player content
        }
        
        // Load BatSpawnManager content
        _batSpawnManager.LoadContent(Content);
        
        try
        {
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent: Loading pebble content");
            var pebble = _entities.OfType<Pebble>().FirstOrDefault();
            if (pebble != null)
            {
                pebble.LoadContent(Content);
                System.Diagnostics.Debug.WriteLine("Game1.LoadContent: Pebble content loaded successfully");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Game1.LoadContent: Failed to load pebble content: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Game1.LoadContent: Pebble load exception: {ex}");
        }
        
        try
        {
            _mayor.LoadContent(Content);
        }
        catch (Exception)
        {
            // Failed to load mayor content
        }
        
        try
        {
            _vendor.LoadContent(Content);
        }
        catch (Exception)
        {
            // Failed to load vendor content
        }
        
        try
        {
            _dialogueManager.LoadContent(Content);
        }
        catch (Exception)
        {
            // Failed to load dialogue manager content
        }
        
        // Create and connect DialogueInputHandler to DialogueManager
        try
        {
            _dialogueInputHandler = new Input.Handlers.DialogueInputHandler();
            _dialogueManager.SetInputHandler(_dialogueInputHandler);
            System.Diagnostics.Debug.WriteLine("Game1: DialogueInputHandler created and connected to DialogueManager");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Game1: Failed to create DialogueInputHandler: {ex.Message}");
        }
        
        // UI font will be loaded with console font later
        System.Diagnostics.Debug.WriteLine("Game1.LoadContent: UI font will be loaded with console font");
        
        try
        {
            var currentRes = _resolutionManager.CurrentResolution;
            _uiManager.Initialize(Content, GraphicsDevice, currentRes.Width, currentRes.Height, _gameSettings);
        }
        catch (Exception)
        {
            // Failed to initialize UIManager
        }
        
        // Load console font and initialize new chat system
        try
        {
            var consoleFont = Content.Load<SpriteFont>("messageFont");
            _console.LoadContent(consoleFont);
            _console.AddMessage("Game started! Press Space to attack.", GameConstants.Colors.CONSOLE_GREEN);
            _player.SetConsole(_console); // Set console for Player
            
            // Use the same font for UI
            _uiFont = consoleFont;
            _console.AddMessage("UI font set from console font", Color.Green);
            
            // Set references for level system AFTER UI font is loaded
            LogToFile("Game1.LoadContent: Setting level system references...");
            _levelSystem.SetGameReferences(
                _player, _entities, _interactables, _droppedItems, _batSpawnManager, _chest, _key, _door,
                _mayor, _vendor, _questManager, _dialogueManager, _attackSystem, _console,
                _deathPanel, _pauseMenu, _uiFont);
            LogToFile("Game1.LoadContent: Level system references set");
            
            // Subscribe to player events
            SubscribeToPlayerEvents();
            
            // Initialize UI panels with scaling
            var currentRes = _resolutionManager.CurrentResolution;
            
            _deathPanel = new DeathPanel(_uiFont, currentResolution.Width, currentResolution.Height);
            _pauseMenu = new PauseMenu(_uiFont, currentResolution.Width, currentResolution.Height);
            
            // Initialize dialogue manager with scaling
            _dialogueManager.Initialize(currentRes.Width, currentRes.Height, GraphicsDevice);
            
            // Initialize inventory with scaling
            _player.Inventory.Initialize(currentRes.Width, currentRes.Height);
            
            _console.Initialize(currentRes.Width, currentRes.Height);
            
            // Load content for UI panels
            _deathPanel.LoadContent(Content, GraphicsDevice);
            _pauseMenu.LoadContent(Content, GraphicsDevice);
            
            // Initialize Save System UI
            var backgroundTexture = CreateBackgroundTexture();
            var buttonTexture = CreateButtonTexture();
            _saveSlotManager = new SaveSlotManager(_saveManager, _uiFont, backgroundTexture, buttonTexture);
            _saveSlotManager.LoadContent(Content, GraphicsDevice);
            
            // Initialize Save/Load Manager
            _saveLoadManager = new GameSaveLoadManager(_saveManager, null, null); // No SaveLoadPanel and message display for now
            
            // Initialize save system with game references
            _saveLoadManager.Initialize(_player, _questManager, _map, _entities, _interactables, _droppedItems, _levelSystem?.CurrentLevel ?? 1, _levelSystem, Content, _dialogueManager);
            
            
            
            // Integrate all events with the universal event system
            IntegrateAllEvents();
            
    

            // Initialize Input Handler with all components
            _inputHandler.Initialize(
                _player,
                _dialogueManager,
                _pauseMenu,
                _deathPanel,
                _uiManager,
                _levelSystem?.LevelTransitionPanel, // Access through level system
                _saveSlotManager,
                _saveLoadManager);
            
            // Set InputHandler reference in LevelSystem for updating LevelTransitionPanel
            System.Diagnostics.Debug.WriteLine($"Game1.LoadContent: About to set InputHandler in LevelSystem - _levelSystem: {_levelSystem != null}, _inputHandler: {_inputHandler != null}");
            _levelSystem?.SetInputHandler(_inputHandler);
        }
            catch (Exception)
        {
                // Failed to load console
        }
        
        // Load door content at the end, after all other content is loaded
        try
        {
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent: Starting to load door content...");
            _door.LoadContent(Content);
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent: Door content loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Game1.LoadContent: Failed to load door content: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Game1.LoadContent: Exception details: {ex}");
        }
    }

    private void SubscribeToPlayerEvents()
    {
        if (_player == null) return;
        

        
        // Subscribe to player inventory events
        if (_player.Inventory != null)
        {
        _player.Inventory.OnItemDiscarded += DropItemAtPlayerPosition;
        }
    }
    
    private void SubscribeToPebbleEvents()
    {
        var pebble = _entities.OfType<Pebble>().FirstOrDefault();
        if (pebble == null) return;
        

        System.Diagnostics.Debug.WriteLine("Game1: SubscribeToPebbleEvents called - chest creation delegated to GameRestartManager");
    }
    

    
    private void OnPlayerDeath(IAttackable player)
    {
        _deathPanel.Show();
        
        // Disable player movement during death
        _player?.SetCanMove(false);
    }
    
    private void HandleRestartFromDeath()
    {
        try
        {
            // Handle restart from death
            
            // Hide death panel
            _deathPanel?.Hide();
            
            // Use existing comprehensive reset method
            ReinitializeGameSystems();
            
            // Enable player movement
            _player?.SetCanMove(true);
            
            _console?.AddMessage("Game restarted from death!", GameConstants.Colors.CONSOLE_GREEN);
        }
        catch (Exception ex)
        {
            // Error in restart from death
            _console?.AddMessage($"Error restarting from death: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void ExitGame()
    {
        Exit();
    }
    
    
    

            // publishing GameEvents.ENEMY_DEFEATED, or by event handlers (e.g., UnifiedGameplayEventHandler).
    // The chest spawning logic for Pebble defeat is handled in Game1.Initialize or RestartGame/ResetGameToInitialState
    // by subscribing to _pebble.OnDeath directly there.
    
    private void ResumeGame()
    {
        _isGamePaused = false;
    }

    private void OnDialogueEnded()
    {
        // Reset pause menu ESC state to prevent immediate pause when dialogue closes
        _pauseMenu.ResetEscapeState();
        
        // The dialogue system now handles choices automatically through SetDialogueWithPendingChoices
        // No need to check for pending choices anymore
    }
    
    /// <summary>
    /// Initialize the universal event system and integrate existing events
    /// </summary>
    private void InitializeEventSystem()
    {
        // Initialize Unified Event System
        _unifiedEventSystem = UnifiedEventSystem.Instance;
        _unifiedEventSystem.Initialize(
            _console, _player, _questManager, _dialogueManager, _uiManager, 
            _levelSystem, _restartManager, Content, _pauseMenu, _deathPanel, _levelSystem?.LevelTransitionPanel);
        
        // Initialize Health Potion Auto Assigner
        
        
        // Subscribe QuestManager to Pebble events
        _questManager.SubscribeToPebbleEvents();

        
        // Integration will happen in LoadContent after all components are initialized
    }
    
    /// <summary>
    /// Integrate all existing C# events with the universal event system
    /// </summary>
    private void IntegrateAllEvents()
    {
        var eventSystem = GameEventSystem.Instance;
        
        // Integrate Player events using Unified Event System
        if (_player != null)
        {
            _player.OnAttack += (pos, dir) => 
            {
                // Use Unified Event System for type-safe event publishing
                _unifiedEventSystem.PublishPlayerAttack(pos, dir);
                // Execute attack logic
                OnPlayerAttack(pos, dir);
            };
            // Player events are now handled by UnifiedEventSystem
            // Only handle death panel display here
            _player.OnDeath += (entity) => 
            {
                // Show death panel
                OnPlayerDeath(entity);
            };
            
            // Inventory events using Unified Event System
            if (_player.Inventory != null)
            {
                _player.Inventory.OnGoldChanged += (gold) => 
                {
                    var oldGold = _player.Inventory.Gold - gold;
                    _unifiedEventSystem.PublishGoldChanged(oldGold, _player.Inventory.Gold, "Transaction");
                };
    
                // _player.Inventory.OnInventoryChanged += () => 
                // {
                //     // Calculate total items in inventory
                //     int totalItems = 0;
                //     for (int x = 0; x < Inventory.Inventory.INVENTORY_WIDTH; x++)
                //     {
                //         for (int y = 0; y < Inventory.Inventory.INVENTORY_HEIGHT; y++)
                //         {
                //             if (_player.Inventory.GetItem(x, y) != null)
                //                 totalItems++;
                //         }
                //     }
                //     // Add quick access items
                //     for (int i = 0; i < Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
                //     {
                //         if (_player.Inventory.GetQuickAccessItem(i) != null)
                //             totalItems++;
                //     }
                //     _unifiedEventSystem.PublishInventoryChanged(totalItems, 0, false);
                // };
                _player.Inventory.OnItemDiscarded += (item) => 
                {
                    _unifiedEventSystem.PublishItemDiscarded(item, _player.WorldPosition, "Manual");
                };
            }
        }
        
        // Integrate Enemy events

        
        // Pebble.cs now directly publishes GameEvents.ENEMY_DEFEATED upon death.
        // This section previously duplicated the event publishing.
        // Any item drop logic related to pebble defeat should be in UnifiedGameplayEventHandler.OnEnemyDefeated.
        
        // Integrate UI events using Unified Event System
        if (_uiManager != null)
        {

            _uiManager.OnResetSettings += () => _unifiedEventSystem.PublishSettingsReset();
            // UnifiedEventSystem already handles OnSettings subscription
            // _uiManager.OnSettings += () => _unifiedEventSystem.PublishSettingsOpened();
        }
        
        // Integrate Dialogue events
        if (_dialogueManager != null)
        {
    
        }
        
        // Integrate Quest events using Unified Event System
        if (_questManager != null)
        {
            _questManager.OnQuestStarted += (quest) => _unifiedEventSystem.PublishQuestStarted(quest);
            _questManager.OnQuestCompleted += (quest) => _unifiedEventSystem.PublishQuestCompleted(quest);
            _questManager.OnQuestTurnedIn += (quest) => _unifiedEventSystem.PublishQuestTurnedIn(quest);
            _questManager.OnQuestRefused += (quest) => _unifiedEventSystem.PublishQuestRefused(quest);
        }
        
        // Integrate Environment events
        if (_door != null)
        {
            _door.OnOpened += (d) => 
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Door opened! Position: {d.WorldPosition}, GameStarted: {_gameStarted}, SettingsActive: {_uiManager?.SettingsActive}");
                // Use Unified Event System for door opened event
                _unifiedEventSystem.PublishGameStateChanged("Playing", "Completed", "Door Opened");
                // Publish DOOR_OPENED event for other systems
                eventSystem.Publish(GameEvents.DOOR_OPENED, new { Door = d, Position = d.WorldPosition });
                // Level completion messages are handled by LevelSystem
            };
        }
        
        // Integrate Resolution events
        if (_resolutionManager != null)
        {
            _resolutionManager.OnResolutionChanged += (res) => 
            {
                eventSystem.Publish(GameEvents.RESOLUTION_CHANGED, res);
                OnResolutionManagerResolutionChanged(res);
            };
            _resolutionManager.OnFullscreenChanged += (fullscreen) => 
            {
                eventSystem.Publish(GameEvents.FULLSCREEN_CHANGED, fullscreen);
                OnResolutionManagerFullscreenChanged(fullscreen);
            };
            _resolutionManager.OnViewportChanged += OnViewportChanged;
        }
        
        // Integrate UI Panel events
        if (_deathPanel != null)
        {
            _deathPanel.OnRestart += () => HandleRestartFromDeath();
            _deathPanel.OnLoadGame += () => HandleLoadFromDeathPanel();
            _deathPanel.OnMenu += () => eventSystem.Publish<object>(GameEvents.RETURN_TO_MENU, null);
            _deathPanel.OnExit += () => eventSystem.Publish<object>(GameEvents.GAME_EXIT, null);
        }
        
        if (_pauseMenu != null)
        {
            _pauseMenu.OnResume += () => eventSystem.Publish<object>(GameEvents.GAME_RESUMED, null);
            _pauseMenu.OnSave += () => ShowSaveMenu();
            _pauseMenu.OnLoad += () => ShowLoadMenu();
            _pauseMenu.OnMenu += () => eventSystem.Publish<object>(GameEvents.RETURN_TO_MENU, null);
            _pauseMenu.OnExit += () => eventSystem.Publish<object>(GameEvents.GAME_EXIT, null);
        }

        // Subscribe to game exit event
        eventSystem.Subscribe<object>(GameEvents.GAME_EXIT, (data) => ExitGame());
        
        
        // Integrate Save System events
        if (_saveSlotManager != null)
        {
            _saveSlotManager.OnSaveRequested += (slot) => HandleSaveRequest(slot);
            _saveSlotManager.OnLoadRequested += (slot) => HandleLoadRequest(slot);
            _saveSlotManager.OnPanelClosed += () => HideSaveMenu();
        }
        
        // Subscribe to save system events
        if (_saveLoadManager != null)
        {
            _saveLoadManager.OnLoadCompleted += () => 
            {
                // Close save menu panel after successful load
                HideSaveMenu();
                // Close pause menu if it's open to return to gameplay immediately
                if (_pauseMenu.IsVisible)
                {
                    _pauseMenu.Hide();
                }
            };
        }
        
        // Integrate Main Menu events
        if (_uiManager != null)
        {
            _uiManager.OnLoadGame += () => ShowLoadMenu();
            _uiManager.OnStartGame += () => HandleStartNewGame();
            _uiManager.OnCloseGame += () => ExitGame();
        }
        
        // Subscribe to GameSettings events
        if (_gameSettings != null)
        {
            _gameSettings.OnResolutionChanged += OnResolutionChanged;
            _gameSettings.OnFullscreenChanged += OnFullscreenChanged;
        }
        
        // Subscribe to chest creation events
        eventSystem.Subscribe<object>(GameEvents.CHEST_CREATED, OnChestCreated);
        
        // Subscribe to item dropped events (for enemy drops)
        eventSystem.Subscribe<ItemDroppedEventData>(GameEvents.ITEM_DROPPED, OnItemDropped);
        

        

        
        // Subscribe to load game events from death panel
        eventSystem.Subscribe<object>(GameEvents.LOAD_GAME_REQUESTED, _ => ShowLoadMenu());
        
        // Ensure load panel is never shown together with settings: hide it on settings opened
        eventSystem.Subscribe<object>(GameEvents.SETTINGS_OPENED, _ => HideSaveMenu());
        
        // Subscribe to player interaction event
        eventSystem.Subscribe<object>(GameEvents.PLAYER_INTERACTION_REQUESTED, _ => HandlePlayerInteraction());
        
        // Subscribe to console message events
        eventSystem.Subscribe<string>(GameEvents.CONSOLE_MESSAGE, (message) => _console?.AddMessage(message, GameConstants.Colors.CONSOLE_WHITE));
        
        // Subscribe to player movement and attack events
        eventSystem.Subscribe<object>(GameEvents.PLAYER_ATTACK_REQUESTED, _ => _player?.HandleAttackEvent());
        eventSystem.Subscribe<object>(GameEvents.PLAYER_MOVEMENT_DIRECTION, (data) => 
        {
            // Store movement data for processing in Update
            _pendingMovementData = data;
        });
        
        // Subscribe to level transition panel events
        eventSystem.Subscribe<object>(GameEvents.LEVEL_TRANSITION_PANEL_SHOWN, _ => 
        {
            _isLevelTransitionActive = true;
            _console?.AddMessage("Level transition panel shown - game paused", GameConstants.Colors.CONSOLE_CYAN);
        });
        
        eventSystem.Subscribe<object>(GameEvents.LEVEL_TRANSITION_PANEL_HIDDEN, _ => 
        {
            _isLevelTransitionActive = false;
            _console?.AddMessage("Level transition panel hidden - game resumed", GameConstants.Colors.CONSOLE_CYAN);
        });
        
        // Subscribe to unified ESC handling events
        eventSystem.Subscribe<object>(GameEvents.INVENTORY_TOGGLE_REQUESTED, _ => 
        {
            _console?.AddMessage("Inventory toggle requested via ESC", GameConstants.Colors.CONSOLE_CYAN);
            _player?.Inventory.ToggleInventory();
        });
        
        eventSystem.Subscribe<object>(GameEvents.PAUSE_MENU_OPEN_REQUESTED, _ => 
        {
            _console?.AddMessage("Pause menu open requested via ESC", GameConstants.Colors.CONSOLE_CYAN);
            _pauseMenu?.Show();
        });
        
        eventSystem.Subscribe<object>(GameEvents.PAUSE_MENU_CLOSE_REQUESTED, _ => 
        {
            _console?.AddMessage("Pause menu close requested via ESC", GameConstants.Colors.CONSOLE_CYAN);
            _pauseMenu?.Hide();
        });
        
        eventSystem.Subscribe<object>(GameEvents.PAUSE_MENU_CONFIRMATION_CLOSE_REQUESTED, _ => 
        {
            _console?.AddMessage("Pause menu confirmation close requested via ESC", GameConstants.Colors.CONSOLE_CYAN);
            _pauseMenu?.CloseConfirmationDialogs();
        });
        
        eventSystem.Subscribe<object>(GameEvents.SAVE_SLOT_MENU_CLOSE_REQUESTED, _ => 
        {
            _console?.AddMessage("Save slot menu close requested via ESC", GameConstants.Colors.CONSOLE_CYAN);
            _saveSlotManager?.Hide();
        });
        
        // Subscribe to game state events for menu return
        eventSystem.Subscribe<object>(GameEvents.GAME_STOPPED, _ => HandleGameStopped());
        eventSystem.Subscribe<object>(GameEvents.SCENE_CLEAR_REQUESTED, _ => HandleSceneClear("menu_return"));
        
        // Subscribe to quick access usage events
        eventSystem.Subscribe<object>(GameEvents.QUICK_ACCESS_USED, (data) => 
        {
            var slotIndex = GetPropertyValue<int>(data, "SlotIndex");
            HandleQuickAccessUsed(slotIndex);
        });

    }
    
    /// <summary>
    /// Handle player interaction event from InputHandler
    /// </summary>
    private void HandlePlayerInteraction()
    {
        System.Diagnostics.Debug.WriteLine($"HandlePlayerInteraction: Called, interactables count: {_interactables.Count}");
        bool foundInteractable = false;

        foreach (var interactable in _interactables)
        {
            // Get position from interactable
            Vector2 interactablePosition = interactable switch
            {
                IEntity entity => entity.WorldPosition,
                DroppedItem droppedItem => droppedItem.WorldPosition,
                Chest chest => chest.WorldPosition,
                _ => Vector2.Zero
            };
            
            float distance = Vector2.Distance(_player.WorldPosition, interactablePosition);
            System.Diagnostics.Debug.WriteLine($"HandlePlayerInteraction: {interactable.GetType().Name} at {interactablePosition}, distance={distance:F1}, radius={interactable.InteractionRadius:F1}");
            _console?.AddMessage($"Check {interactable.GetType().Name} dist:{distance:F1} radius:{interactable.InteractionRadius:F1}", GameConstants.Colors.CONSOLE_CYAN);

            if (distance <= interactable.InteractionRadius)
            {
                // Show key status for interactive objects
                if (interactable is Door door)
                {
                    _console?.AddMessage($"Has key: {_player.HasKey}", GameConstants.Colors.CONSOLE_CYAN);
                    if (!door.IsOpen && !_player.HasKey)
                    {
                        _console?.AddMessage("Door is locked! Need a key.", GameConstants.Colors.CONSOLE_RED);
                    }
                    else if (door.IsOpen)
                    {
                        _console?.AddMessage("Door is open - interacting to show level completion!", GameConstants.Colors.CONSOLE_CYAN);
                    }
                }

                // Handle NPC dialogue
                if (interactable is NPC npc)
                {
                    // Close inventory if it's open when dialogue starts
                    if (_player.Inventory.IsOpen)
                    {
                        _player.Inventory.ToggleInventory();
                    }

                    // OnInteract handles all dialogue setup and starting automatically
                    npc.OnInteract(_player);
                }
                else
                {
                    // For non-NPC interactables (including doors), call OnInteract normally
                    interactable.OnInteract(_player);
                    
                    // Check key status after interaction (in case key was used)
                    CheckAndUpdateKeyStatus();
                }
                _console?.AddMessage($"Interacted with {interactable.GetType().Name}!", GameConstants.Colors.CONSOLE_LIME);
                foundInteractable = true;
                break; // Interact with only one object at a time
            }
        }

        if (!foundInteractable)
        {
            _console?.AddMessage("Nothing to interact with nearby", GameConstants.Colors.CONSOLE_GRAY);
        }
    }
    
    /// <summary>
    /// Handle quick access item usage from InputHandler events
    /// </summary>
    private void HandleQuickAccessUsed(int slotIndex)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: Called with slotIndex={slotIndex}");
            
            if (_player == null || !_gameStarted || _isLevelTransitionActive) 
            {
                System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: Early return - player={_player != null}, gameStarted={_gameStarted}, levelTransition={_isLevelTransitionActive}");
                return;
            }
            
            // Debug quick access slots state

            
            // Validate slot index
            if (slotIndex < 0 || slotIndex >= Inventory.Inventory.QUICK_ACCESS_SLOTS)
            {
                _console?.AddMessage($"Invalid quick access slot: {slotIndex + 1}", GameConstants.Colors.CONSOLE_RED);
                System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: Invalid slot index {slotIndex}");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: About to call UseQuickAccessItem");
            
            // Use the item in the quick access slot
            bool used = _player.Inventory.UseQuickAccessItem(slotIndex, _player);
            
            System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: UseQuickAccessItem returned {used}");
            
            if (used)
            {
                _console?.AddMessage($"Used item from quick access slot {slotIndex + 1}", GameConstants.Colors.CONSOLE_CYAN);
            }
            else
            {
                _console?.AddMessage($"No usable item in quick access slot {slotIndex + 1}", GameConstants.Colors.CONSOLE_GRAY);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: Exception occurred: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"HandleQuickAccessUsed: Stack trace: {ex.StackTrace}");
            _console?.AddMessage($"Error using quick access item: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void OnResolutionChanged(Resolution resolution)
    {
        // Use resolution manager for safe resolution changes
        // Pass the current fullscreen state from GameSettings to maintain it
        bool applied = _resolutionManager.ApplyResolution(resolution, _gameSettings.IsFullscreen);
        if (applied)
        {
            _console?.AddMessage($"Resolution changed to {resolution.Width}x{resolution.Height}", GameConstants.Colors.CONSOLE_CYAN);
        }
        else
        {
            _console?.AddMessage("Failed to change resolution", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void OnFullscreenChanged(bool isFullscreen)
    {
        // Apply fullscreen via resolution manager only if it differs
        if (_resolutionManager.IsFullscreen == isFullscreen)
        {
            return;
        }
        
        bool applied = _resolutionManager.ApplyResolution(_resolutionManager.CurrentResolution, isFullscreen);
        if (applied)
        {
            // Sync settings after successful apply
            _gameSettings.SetPendingFullscreen(isFullscreen);
            _gameSettings.ApplyPendingSettings();
            _console?.AddMessage($"Fullscreen {(isFullscreen ? "enabled" : "disabled")}", GameConstants.Colors.CONSOLE_CYAN);
        }
        else
        {
            _console?.AddMessage("Failed to change fullscreen mode", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void OnResolutionManagerResolutionChanged(Resolution resolution)
    {
        // Update UI when resolution manager successfully changes resolution
        UpdateUIAfterResolutionChange(resolution.Width, resolution.Height);
        
        // Update game settings to reflect the change without triggering events
        _gameSettings.UpdateResolution(resolution);
    }
    
    private void OnResolutionManagerFullscreenChanged(bool isFullscreen)
    {
        // Update UI when fullscreen changes (viewport might change)
        UpdateUIAfterResolutionChange(_resolutionManager.CurrentResolution.Width, _resolutionManager.CurrentResolution.Height);
        
        // Update game settings to reflect the change without triggering events
        _gameSettings.UpdateFullscreen(isFullscreen);
    }
    
    private void OnViewportChanged(Viewport newViewport)
    {
        // Update camera with new viewport
        _camera.UpdateViewport(newViewport);
    }
    
    /// <summary>
    /// Common method to update all UI components after resolution change
    /// </summary>
    private void UpdateUIAfterResolutionChange(int width, int height)
    {
        // Update UI scaling
        UIScalingManager.Initialize(width, height);
        
        // Update UI components
        _uiManager.Initialize(Content, GraphicsDevice, width, height, _gameSettings);
        
        // Event subscriptions are handled in IntegrateAllEvents - no need to resubscribe here
        // _uiManager.OnResolutionChanged += OnResolutionChanged;
        // _uiManager.OnFullscreenChanged += OnFullscreenChanged;
        // _uiManager.OnResetSettings += OnResetSettings;
        // _uiManager.OnSettings += OnSettings;
        
        // Use centralized initialization for all entities
        InitializeAllEntitiesWithCurrentDimensions();
    }
    
    private void OnSettings()
    {
        System.Diagnostics.Debug.WriteLine($"Game1.OnSettings: Called - GameStarted: {_gameStarted}, SettingsActive: {_uiManager?.SettingsActive}");
        
        // Publish settings opened event to coordinate UI state
        var eventSystem = GameEventSystem.Instance;
        eventSystem.Publish<object>(GameEvents.SETTINGS_OPENED, null);
        

    }
    
    private void OnResetSettings()
    {
        // Reset settings immediately using resolution manager
        bool resetSuccess = _resolutionManager.ResetAndApply();
        if (resetSuccess)
        {
            _console?.AddMessage("Settings reset to defaults!", GameConstants.Colors.CONSOLE_GREEN);
        }
        else
        {
            _console?.AddMessage("Failed to reset settings", GameConstants.Colors.CONSOLE_RED);
        }
    }
    


    public void DropItemAtPlayerPosition(Item item)
    {
        if (item == null) return;

        // Create a new instance of the item for the dropped item
        // This ensures the dropped item is independent of the inventory item
        Item droppedItemInstance = CreateItemInstance(item);
        if (droppedItemInstance == null) return;

        // Use appropriate scale based on item type
        float scale;
        if (item is HealthPotion)
        {
            scale = GameConstants.SpriteScale.HEALTH_POTION_SCALE;
        }
        else if (item is KeyItem)
        {
            scale = GameConstants.SpriteScale.KEY_SCALE;
        }
        else
        {
            scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
        }
        
        var droppedItem = new DroppedItem(_player.WorldPosition, droppedItemInstance, scale);
        droppedItem.LoadContent(Content);
        _droppedItems.Add(droppedItem);
        _interactables.Add(droppedItem);
        
        _console?.AddMessage($"Dropped {item.Name}", GameConstants.Colors.CONSOLE_ORANGE);
        
        // If a key was dropped, check if player still has a key in inventory
        if (item is KeyItem)
        {
            CheckAndUpdateKeyStatus();
        }
    }

    private void OnItemDropped(ItemDroppedEventData data)
    {
        if (data?.Item == null) return;
        
        // Create DroppedItem from the event data
        float scale = data.Item is HealthPotion ? GameConstants.SpriteScale.HEALTH_POTION_SCALE : GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
        var droppedItem = new DroppedItem(data.DropPosition, data.Item, scale);
        droppedItem.LoadContent(Content);
        
        _droppedItems.Add(droppedItem);
        _interactables.Add(droppedItem);
        
        var sourceText = !string.IsNullOrEmpty(data.DropSource) ? $" from {data.DropSource}" : "";
        _console?.AddMessage($"{data.Item.Name} dropped{sourceText}!", GameConstants.Colors.CONSOLE_ORANGE);
    }
    
    private void OnChestCreated(object chestData)
    {
        // Extract chest from anonymous object
        var chest = GetPropertyValue<Chest>(chestData, "Chest");
        var position = GetPropertyValue<Vector2>(chestData, "Position");
        
        if (chest != null)
        {
            _chest = chest;
            _interactables.Add(chest);
            _console?.AddMessage($"Chest created at {position}", GameConstants.Colors.CONSOLE_GOLD);
            
            // Subscribe to chest events
            SubscribeToChestEvents(chest);
        }
    }
    

    
    private void SubscribeToChestEvents(Chest chest)
    {
        if (chest == null) return;
        
        chest.OnOpened += (c, gotKey) =>
        {
            if (gotKey && _key == null)
            {
                var keyPosition = c.WorldPosition + GameConstants.World.KEY_DROP_OFFSET;
                var newKeyItem = new KeyItem();
                newKeyItem.LoadContent(Content);
                
                _key = new DroppedItem(keyPosition, newKeyItem, GameConstants.SpriteScale.KEY_SCALE);
                
                _key.OnPickedUp += (k) =>
                {
                    _player.HasKey = true;
                    _interactables.Remove(k);
                    _key = null;
                    _console?.AddMessage("Key picked up!", GameConstants.Colors.CONSOLE_GOLD);
                };
                
                _interactables.Add(_key);
                _console?.AddMessage("Key dropped from chest!", GameConstants.Colors.CONSOLE_GOLD);
            }
        };
    }

    private T GetPropertyValue<T>(object obj, string propertyName)
    {
        if (obj == null) return default(T);
        
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null && property.PropertyType == typeof(T))
        {
            return (T)property.GetValue(obj);
        }
        
        return default(T);
    }

    private Item CreateItemInstance(Item originalItem)
    {
        return ItemFactory.CreateItemInstance(originalItem);
    }
    
    private void CheckAndUpdateKeyStatus()
    {
        // Check if player still has a key in inventory
        bool hasKeyInInventory = false;
        
        // Check inventory grid
        for (int x = 0; x < Inventory.Inventory.INVENTORY_WIDTH; x++)
        {
            for (int y = 0; y < Inventory.Inventory.INVENTORY_HEIGHT; y++)
            {
                var item = _player.Inventory.GetItem(x, y);
                if (item is KeyItem)
                {
                    hasKeyInInventory = true;
                    break;
                }
            }
            if (hasKeyInInventory) break;
        }
        
        // Check quick access slots
        if (!hasKeyInInventory)
        {
            for (int i = 0; i < Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
            {
                var item = _player.Inventory.GetQuickAccessItem(i);
                if (item is KeyItem)
                {
                    hasKeyInInventory = true;
                    break;
                }
            }
        }
        
        // Check if player had a key before but doesn't have one now
        bool hadKeyBefore = _player.HasKey;
        
        // Update player's HasKey status
        _player.HasKey = hasKeyInInventory;
        
        // Only show message if player had a key before but doesn't have one now
        if (hadKeyBefore && !hasKeyInInventory)
        {
            _console?.AddMessage("Key removed from inventory - door is now locked!", GameConstants.Colors.CONSOLE_RED);
        }
    }

    /// <summary>
    /// Centralized method to ensure all entities are properly initialized with current screen dimensions
    /// This method should be called after any restart or resolution change
    /// </summary>
    private void InitializeAllEntitiesWithCurrentDimensions()
    {
        var currentResolution = _resolutionManager.CurrentResolution;
        
        // Initialize inventory with current screen dimensions
        if (_player?.Inventory != null)
        {
            _player.Inventory.Initialize(currentResolution.Width, currentResolution.Height);
        }
        
        // Initialize UI systems with current dimensions
        UIScalingManager.Initialize(currentResolution.Width, currentResolution.Height);
        _console?.Initialize(currentResolution.Width, currentResolution.Height);
        _dialogueManager?.Initialize(currentResolution.Width, currentResolution.Height, GraphicsDevice);
        
        // Reinitialize UI panels with current dimensions
        if (_uiFont != null)
        {
            _deathPanel = new DeathPanel(_uiFont, currentResolution.Width, currentResolution.Height);
            _pauseMenu = new PauseMenu(_uiFont, currentResolution.Width, currentResolution.Height);
            
            // Load content for UI panels
            _deathPanel.LoadContent(Content, GraphicsDevice);
            _pauseMenu.LoadContent(Content, GraphicsDevice);
            
    
            // _deathPanel.OnRestart += RestartGame;
            // _deathPanel.OnMenu += GoToMenu;
            // _deathPanel.OnExit += ExitGame;
            
            // _pauseMenu.OnResume += ResumeGame;
            // _pauseMenu.OnMenu += GoToMenu;
            // _pauseMenu.OnExit += ExitGame;
        }
        

    }

    protected override void Update(GameTime gameTime)
    {
        // Update unified event system
        _unifiedEventSystem?.ProcessQueuedEvents();
        
        // Update legacy event system (will be removed after migration)
        GameEventSystem.Instance.ProcessQueuedEvents();
        
        // Debug: Event system testing (F1 key for statistics, F2 key for system test)
        if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
        {
            _unifiedEventSystem?.LogEventStatistics();
            _console?.AddMessage("Event statistics logged to debug output!", GameConstants.Colors.CONSOLE_CYAN);
        }
        
        if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2))
        {
            _unifiedEventSystem?.RunSystemTest();
            _console?.AddMessage("Unified Event System test completed! Check debug output.", GameConstants.Colors.CONSOLE_GREEN);
        }

        // Update game systems
        _restartManager?.Update(gameTime);
        _levelSystem?.Update(gameTime);
        _saveLoadManager?.Update(gameTime);
        
        // GameStateEventHandler functionality moved to UnifiedGameStateEventHandler in UnifiedEventSystem

        // Update pause menu state FIRST
        if (!_dialogueManager.IsDialogueActive && !_isLevelTransitionActive)
        {
            _pauseMenu.Update(gameTime);
            _isGamePaused = _pauseMenu.IsPaused;
        }
        else
        {
            // When dialogue is active or level transition is active, game is paused but not through pause menu
            _isGamePaused = true;
        }

        // Update centralized input handler with current pause state
        _inputHandler.Update(gameTime, _gameStarted, _isGamePaused);
        
        // Handle pending movement data
        if (_pendingMovementData != null)
        {
            try
            {
                var moveData = _pendingMovementData;
                var direction = GetPropertyValue<Vector2>(moveData, "Direction");
                var isMovingLeft = GetPropertyValue<bool>(moveData, "IsMovingLeft");
                var isMovingRight = GetPropertyValue<bool>(moveData, "IsMovingRight");
                var isMovingUp = GetPropertyValue<bool>(moveData, "IsMovingUp");
                var isMovingDown = GetPropertyValue<bool>(moveData, "IsMovingDown");
                
                _player?.HandleMovementEvent(direction, isMovingLeft, isMovingRight, isMovingUp, isMovingDown, gameTime);
            }
            catch (Exception ex)
            {
                _console?.AddMessage($"Movement error: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
            }
            finally
            {
                _pendingMovementData = null;
            }
        }
        
        // Update console FIRST - always update console regardless of game state
        _console?.Update(gameTime);
        
        // Update save slot manager
        _saveSlotManager?.Update(gameTime);



        // Inventory should NOT pause the game - it should work in parallel
        // Only pause menu and dialogue should pause the game

        // Update death panel
        _deathPanel.Update(gameTime);
        
        // Debug: Track death panel visibility
        if (_deathPanel.IsVisible)
        {
            System.Diagnostics.Debug.WriteLine($"Game1.Update: Death panel is visible - GameStarted: {_gameStarted}, MenuActive: {_uiManager?.MenuActive}, SettingsActive: {_uiManager?.SettingsActive}");
        }

        // Update dialogue system first (always update dialogue, even when paused)
        if (_dialogueManager.IsDialogueActive)
        {
            _dialogueInputHandler?.Update(gameTime);
            _dialogueManager.Update(gameTime);
        }

        // If death panel is shown, don't update other game logic
        if (_deathPanel.IsVisible)
        {
            base.Update(gameTime);
            return;
        }

        // If game is paused (pause menu or dialogue), only update UI elements
        if (_isGamePaused)
        {
            // Don't update inventory when pause menu is active - only when dialogue is active
            if (_player.Inventory.IsOpen && _dialogueManager.IsDialogueActive)
            {
                _player.Inventory.Update(gameTime, _player, _dialogueManager.IsDialogueActive);
            }

            base.Update(gameTime);
            return;
        }
        else if (_gameStarted)
        {
            // Cursor state will be managed at the end of Update method
        }

        if (!_gameStarted)
        {
            // Manage quick access panel visibility in main menu
            ManageQuickAccessVisibility();
            
            // Ensure cursor is visible in main menu
            IsMouseVisible = CursorStateManager.ShouldShowCursor(
                _gameStarted,
                _isGamePaused,
                _deathPanel.IsVisible,
                _player.Inventory.IsOpen,
                _uiManager.MenuActive,
                _uiManager.SettingsActive,
                _dialogueManager.IsDialogueActive,
                _uiManager.ExitConfirmationVisible,
                _pauseMenu.IsVisible,
                _levelSystem?.LevelTransitionPanel?.IsVisible == true,
                _saveSlotManager?.IsVisible == true);

            _uiManager.Update(gameTime);

            // The ESC key is now handled by the event system subscribed by UIManager
            // Removed direct ESC key handling from here

            if (!_uiManager.MenuActive && !_uiManager.SettingsActive)
            {
                _gameStarted = true;
                _player.SetCanMove(true);
            }
            return;
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();



        // Update player with dialogue state and pause state
        bool shouldPausePlayer = _player.Inventory.IsOpen || _isGamePaused;
        _player.Update(gameTime, _dialogueManager.IsDialogueActive, shouldPausePlayer);
        
        // Manage quick access panel visibility based on UI state
        ManageQuickAccessVisibility();

        // Update inventory - always update when open, even during normal gameplay
        if (_player.Inventory.IsOpen)
        {
            _player.Inventory.Update(gameTime, _player, _dialogueManager.IsDialogueActive);
        }

        // Update other entities (enemies, NPCs, etc.)
        // Pause enemies when inventory is open
        bool shouldPauseEnemies = _player.Inventory.IsOpen || _isGamePaused;
        
        // Update bat spawn manager and spawn new bats if needed
        _batSpawnManager.Update(gameTime, shouldPauseEnemies);
        if (_batSpawnManager.ShouldSpawnBat())
        {
            var newBat = _batSpawnManager.CreateBat();
            newBat.LoadContent(Content);
            _entities.Add(newBat);
            _attackSystem.RegisterEntity(newBat);
            _batSpawnManager.AddBat(newBat);
            _batSpawnManager.OnBatSpawned();
        }
        
        // Update all active bats
        var activeBats = _batSpawnManager.GetActiveBatCount();
        foreach (var entity in _entities)
        {
            if (entity is Bat bat)
            {
                bat.Update(gameTime, _player, shouldPauseEnemies);
            }
        }
        
        // Update Pebble from entities list
        var pebble = _entities.OfType<Pebble>().FirstOrDefault();
        if (pebble != null)
        {
            pebble.Update(gameTime, shouldPauseEnemies);
        }

        // Update interactables that implement IUpdateable
        foreach (var interactable in _interactables)
        {
            if (interactable is IUpdateable updateable)
            {
                updateable.Update(gameTime);
            }
        }

        // Update key separately (it's also in _interactables but handled specially)
        _key?.Update(gameTime);

        // Update ProjectileManager with pause state
        bool shouldPauseProjectiles = _player.Inventory.IsOpen || _isGamePaused;
        ProjectileManager.Instance.Update(gameTime, shouldPauseProjectiles);

        // Update dropped items and remove picked up ones
        for (int i = _droppedItems.Count - 1; i >= 0; i--)
        {
            var droppedItem = _droppedItems[i];
            droppedItem.Update(gameTime);

            if (droppedItem.IsPickedUp)
            {
                _droppedItems.RemoveAt(i);
                _interactables.Remove(droppedItem);
            }
        }

        // Update key (DroppedItem) and remove if picked up
        if (_key != null && _key.IsPickedUp)
        {
            _interactables.Remove(_key);
            _key = null;
        }

        // Remove dead enemies after death animation completes
        RemoveDeadEnemies();

        // Minimap removed
        _camera.Follow(GridHelper.WorldToScreen(_player.WorldPosition));



        // Interaction handling is now managed by InputHandler through events

        // Enemy logic - only when not paused
        var activePebble = _entities.OfType<Pebble>().FirstOrDefault();
        if (activePebble != null && activePebble.IsAlive && !shouldPauseEnemies)
        {
            // Check if player is within attack range before shooting
            float distanceToPlayer = Vector2.Distance(activePebble.WorldPosition, _player.WorldPosition);
            if (distanceToPlayer <= GameConstants.Attack.PEBBLE_ATTACK_RANGE)
            {
                activePebble.ShootAtPlayer(_player);
            }
        }

        // Check fireball hits on player (now handled by ProjectileManager)
        var fireballs = ProjectileManager.Instance.GetActiveFireballs();
        foreach (var fireball in fireballs)
        {
            if (fireball.IsActive && !fireball.HasHit && Vector2.Distance(_player.WorldPosition, fireball.WorldPosition) < _player.HitboxRadius)
            {
                fireball.Hit();
                _player.TakeDamage(GameConstants.Damage.FIREBALL_DAMAGE); // Player takes fireball damage
            }
        }

        // Update cursor state
        IsMouseVisible = CursorStateManager.ShouldShowCursor(
            _gameStarted,
            _isGamePaused,
            _deathPanel.IsVisible,
            _player.Inventory.IsOpen,
            _uiManager.MenuActive,
            _uiManager.SettingsActive,
            _dialogueManager.IsDialogueActive,
            _uiManager.ExitConfirmationVisible,
            _pauseMenu.IsVisible,
            _levelSystem?.LevelTransitionPanel?.IsVisible == true,
            _saveSlotManager?.IsVisible == true);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        // Draw game field and objects WITH camera transformation
        _spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, transformMatrix: _camera.Transform);
        
        // Draw background image as game field (bottom layer)
        if (_backgroundTexture != null)
        {
            // Position background image at center of game world
            var worldCenter = new Vector2(GameMap.Width / 2, GameMap.Height / 2);
            var screenCenter = GridHelper.WorldToScreen(worldCenter);
            var textureCenter = new Vector2(_backgroundTexture.Width / 2, _backgroundTexture.Height / 2);
            _spriteBatch.Draw(_backgroundTexture, screenCenter, null, GameConstants.Colors.CONSOLE_WHITE, 0f, textureCenter, GameConstants.Graphics.BACKGROUND_SCALE, SpriteEffects.None, GameConstants.Graphics.BACKGROUND_LAYER_DEPTH);
        }
        _spriteBatch.End();

        // Draw game objects SEPARATELY with LayerDepth sorting
        _spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, transformMatrix: _camera.Transform);
        
        // Collect all entities for drawing
        var drawList = new List<IEntity>(_entities);
        
        // Draw interactables that implement IDrawable
        foreach (var interactable in _interactables)
        {
            if (interactable is IEntity entity)
            {
                // Add IEntity objects to drawList for proper sorting
                drawList.Add(entity);
            }
            else if (interactable is IDrawable drawable)
            {
                // Draw non-IEntity objects directly
                try
                {
                    drawable.Draw(_spriteBatch);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Game1.Draw: Failed to draw {interactable.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        // Draw entities (SpriteSortMode.BackToFront automatically sorts by LayerDepth)
        for (int i = 0; i < drawList.Count; i++)
        {
            var entity = drawList[i];
            try
            {
                entity.Draw(_spriteBatch);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Game1.Draw: Failed to draw entity {entity.GetType().Name}: {ex.Message}");
            }
        }
        
        // Draw projectiles using ProjectileManager
        ProjectileManager.Instance.Draw(_spriteBatch);
        
        // Draw bat spawner
        _batSpawnManager.Draw(_spriteBatch);

        _spriteBatch.End();

        // UI
        _spriteBatch.Begin();
        _uiManager.Draw(_spriteBatch);
        
        // Display player health
        if (_gameStarted)
        {
            // Use existing tile texture for health bar
            if (_tileTexture != null)
            {
                // Scaled health display as rectangles
                int barWidth = UIScalingManager.ScaleValue(GameConstants.UI.HEALTH_BAR_WIDTH);
                int barHeight = UIScalingManager.ScaleValue(GameConstants.UI.HEALTH_BAR_HEIGHT);
                int barX = UIScalingManager.ScaleValue(GameConstants.UI.HEALTH_BAR_X);
                int barY = UIScalingManager.ScaleValue(GameConstants.UI.HEALTH_BAR_Y);
                
                // Health bar background
                _spriteBatch.Draw(_tileTexture, new Rectangle(barX, barY, barWidth, barHeight), GameConstants.Colors.TILE_DARK_GRAY);
                
                // Health bar
                int currentBarWidth = (int)((float)_player.CurrentHealth / _player.MaxHealth * barWidth);
                Color healthColor = _player.CurrentHealth > GameConstants.Health.HEALTH_THRESHOLD_HIGH ? GameConstants.Colors.CONSOLE_GREEN : _player.CurrentHealth > GameConstants.Health.HEALTH_THRESHOLD_MEDIUM ? GameConstants.Colors.CONSOLE_YELLOW : GameConstants.Colors.CONSOLE_RED;
                _spriteBatch.Draw(_tileTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), healthColor);
            }
            
            // Display quest progress with scaled positioning
            if (_uiFont != null)
            {
                var questText = _questManager.GetActiveQuestProgress();
                var questTextPos = new Vector2(
                    UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_SMALL), 
                    UIScalingManager.ScaleValue(40));
                _spriteBatch.DrawString(_uiFont, questText, questTextPos, GameConstants.Colors.CONSOLE_WHITE);
            }
            
            // Display console messages
            _console?.Draw(_spriteBatch);
        }
        
        // Font should already be loaded with console font
        
        // Draw inventory and quick access panel
        if (_uiFont != null)
        {
            // Always draw quick access panel, inventory panel only when open
            if (_player.Inventory.IsOpen)
            {
                System.Diagnostics.Debug.WriteLine("Game1.Draw: Drawing inventory");
                _player.Inventory.Draw(_spriteBatch, _uiFont);
            }
            else
            {
                // Draw only quick access panel when inventory is closed
                _player.Inventory.DrawQuickAccessOnly(_spriteBatch, _uiFont);
            }
        }
        
        if (_gameStarted)
        {
            
            // Minimap removed
            
            // Draw UI panels (death panel and pause menu)
            _pauseMenu.Draw(_spriteBatch);
            _deathPanel.Draw(_spriteBatch);
        }
        
        // Draw dialogue LAST - on top of everything
        _dialogueManager.Draw(_spriteBatch);
        
        // Draw save slot manager
        _saveSlotManager?.Draw(_spriteBatch);
        
        // Draw level system UI LAST - on top of everything
        _levelSystem?.Draw(_spriteBatch);
        
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private Vector2 GetSpriteCenter(Vector2 worldPosition)
    {
        var screenPos = GridHelper.WorldToScreen(worldPosition);
        
        // Since sprites are now fully centered (both horizontally and vertically),
        // the center is exactly at the screen position
        return screenPos;
    }

    /// <summary>
    /// Remove dead enemies from the game world after their death animation completes
    
    /// </summary>
    private void RemoveDeadEnemies()
    {
        // Remove dead bats
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            if (_entities[i] is Bat bat && !bat.IsAlive && bat.IsDeathAnimationFinished)
            {
                _entities.RemoveAt(i);
                _attackSystem.UnregisterEntity(bat);
                _batSpawnManager.RemoveBat(bat);
                _console?.AddMessage("Bat defeated and removed!", GameConstants.Colors.CONSOLE_ORANGE);
            }
        }
        
        // Remove dead Pebble
        var deadPebble = _entities.OfType<Pebble>().FirstOrDefault(p => !p.IsAlive && p.IsDeathAnimationFinished);
        if (deadPebble != null)
        {
            System.Diagnostics.Debug.WriteLine($"RemoveDeadEnemies: Removing Pebble - IsAlive={deadPebble.IsAlive}, State={deadPebble.State}, IsDeathAnimationFinished={deadPebble.IsDeathAnimationFinished}");
            
            _entities.Remove(deadPebble);
            _attackSystem.UnregisterEntity(deadPebble);
            
            _console?.AddMessage("Pebble defeated and removed!", GameConstants.Colors.CONSOLE_ORANGE);
        }
    }
    
    // Removed debug drawing methods:
    // DrawHitboxOutlines
    // GetHitboxColor
    // DrawCircle
    // DrawSpriteBoundaries
    // DrawLine
    // DrawPlayerAttackCone
    // (All methods from line 1289 to 1516)

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose event system
            GameEventSystem.Instance?.Dispose();
            
            _dialogueManager?.Dispose();
            _player?.Inventory?.Dispose();
            _tileTexture?.Dispose();
            _backgroundTexture?.Dispose();
            _spriteBatch?.Dispose();
        }
        base.Dispose(disposing);
    }
    
    private Texture2D CreateBackgroundTexture()
    {
        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { new Color(0, 0, 0, 200) }); // Semi-transparent black
        return texture;
    }
    
    private Texture2D CreateButtonTexture()
    {
        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.Gray });
        return texture;
    }
    
    // Save System Methods
    private void ShowSaveMenu()
    {
        if (_saveSlotManager != null && _gameStarted)
        {
            var panelBounds = new Rectangle(100, 100, 600, 400);
            _saveSlotManager.Show(panelBounds, true); // true for save mode
        }
    }
    
    private void ShowLoadMenu()
    {
        System.Diagnostics.Debug.WriteLine($"Game1.ShowLoadMenu: Called - Stack trace: {System.Environment.StackTrace}");
        
        if (_saveSlotManager != null)
        {
            // Use full screen bounds for better visibility
            var bounds = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _saveSlotManager.Show(bounds, false); // false for load mode
            System.Diagnostics.Debug.WriteLine("Game1.ShowLoadMenu: SaveSlotManager.Show called");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Game1.ShowLoadMenu: SaveSlotManager is null");
        }
    }
    
    private void HideSaveMenu()
    {
        _saveSlotManager?.Hide();
    }
    
    private async void HandleSaveRequest(int slot)
    {
        try
        {
            if (_saveLoadManager != null && _gameStarted)
            {
                _console?.AddMessage($"Saving game to slot {slot}...", GameConstants.Colors.CONSOLE_CYAN);
                
                var saveName = $"save{slot}";
                var success = await _saveLoadManager.SaveGameAsync(saveName);
                
                if (success)
                {
                    _console?.AddMessage($"Game saved to slot {slot}!", GameConstants.Colors.CONSOLE_GREEN);
                    HideSaveMenu();
                }
                else
                {
                    _console?.AddMessage($"Failed to save game to slot {slot}", GameConstants.Colors.CONSOLE_RED);
                }
            }
            else
            {
                _console?.AddMessage("Cannot save: Save manager not initialized or game not started", GameConstants.Colors.CONSOLE_RED);
            }
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Save error: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
            System.Diagnostics.Debug.WriteLine($"Save error: {ex}");
        }
    }
    
    private async void HandleLoadRequest(int slot)
    {
        if (_saveLoadManager != null)
        {
            var saveName = $"save{slot}";
            var success = await _saveLoadManager.LoadGameAsync(saveName);
            if (success)
            {
                _console?.AddMessage($"Game loaded from slot {slot}!", GameConstants.Colors.CONSOLE_GREEN);
                HideSaveMenu();
                _gameStarted = true; // Ensure game is started after loading
                
                // Close pause menu if it's open to return to gameplay immediately
                if (_pauseMenu.IsVisible)
                {
                    _pauseMenu.Hide();
                }
                
                // Hide main menu if it's visible
                if (_uiManager != null)
                {
                    _uiManager.DeactivateMenu();
                }
            }
            else
            {
                _console?.AddMessage($"Failed to load game from slot {slot}", GameConstants.Colors.CONSOLE_RED);
            }
        }
    }
    
    // Quick save/load methods are now handled by InputHandler
    
    private void HandleLoadFromDeathPanel()
    {
        System.Diagnostics.Debug.WriteLine($"Game1.HandleLoadFromDeathPanel: Called - DeathPanel.IsVisible: {_deathPanel?.IsVisible}, GameStarted: {_gameStarted}");
        
        // Only allow loading from death panel if death panel is actually visible
        if (_deathPanel?.IsVisible == true)
        {
            System.Diagnostics.Debug.WriteLine("Game1.HandleLoadFromDeathPanel: Death panel is visible, showing load menu");
            // Use universal load method
            ShowLoadMenu();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Game1.HandleLoadFromDeathPanel: Death panel is not visible, ignoring call");
        }
    }

    
    private void HandleStartNewGame()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: HandleStartNewGame called");
            
            // Hide main menu
            _uiManager?.DeactivateMenu();
            
            // Start the game
            _gameStarted = true;
            _isGamePaused = false;
            _isLevelTransitionActive = false;
            
            // Enable player movement
            _player?.SetCanMove(true);
            
            // Reset player to initial state
            if (_player != null)
            {
                _player.WorldPosition = GameConstants.World.PLAYER_START_POSITION;
                _player.Heal(_player.MaxHealth);
                _player.HasKey = false;
            }
            
            // Use level system to restart level 1 (this will properly initialize all entities including Pebble)
            _levelSystem?.StartLevel(1);
            
            _console?.AddMessage("Started new game", GameConstants.Colors.CONSOLE_CYAN);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in HandleStartNewGame: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
            _console?.AddMessage($"Error starting new game: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void HandleReturnToMenu()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: HandleReturnToMenu called");
            
            // Hide all game UI
            _pauseMenu?.Hide();
            _deathPanel?.Hide();
            HideSaveMenu();
            
            // Reset game state
            _gameStarted = false;
            _isGamePaused = false;
            _isLevelTransitionActive = false;
            
            // Reset input states to prevent stuck keys
            _inputHandler?.ResetInputStates();
            
            // Show main menu
            _uiManager?.ActivateMenu();
            
            // Force UI update to ensure menu is responsive
            System.Diagnostics.Debug.WriteLine($"DEBUG: UIManager state after ActivateMenu - MenuActive: {_uiManager?.MenuActive}, SettingsActive: {_uiManager?.SettingsActive}");
            
            // Simple reset without full reinitialization to avoid hangs
            SimpleGameReset();
            
            _console?.AddMessage("Returned to main menu", GameConstants.Colors.CONSOLE_CYAN);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in HandleReturnToMenu: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
            _console?.AddMessage($"Error returning to menu: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void SimpleGameReset()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: SimpleGameReset called");
            
            // Reset player to initial state - only basic properties
            if (_player != null)
            {
                _player.WorldPosition = GameConstants.World.PLAYER_START_POSITION;
                _player.HasKey = false;
                _player.SetCanMove(false);
                
                // Clear inventory safely
                try
                {
                    for (int x = 0; x < Inventory.Inventory.INVENTORY_WIDTH; x++)
                    {
                        for (int y = 0; y < Inventory.Inventory.INVENTORY_HEIGHT; y++)
                        {
                            _player.Inventory.RemoveItem(x, y);
                        }
                    }
                    for (int i = 0; i < Inventory.Inventory.QUICK_ACCESS_SLOTS; i++)
                    {
                        _player.Inventory.RemoveQuickAccessItem(i);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Error clearing inventory: {ex.Message}");
                }
            }
            
            // Clear dropped items
            try
            {
                foreach (var droppedItem in _droppedItems.ToList())
                {
                    _interactables.Remove(droppedItem);
                }
                _droppedItems.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error clearing dropped items: {ex.Message}");
            }
            
            // Clear chest and key
            try
            {
                if (_chest != null)
                {
                    _interactables.Remove(_chest);
                    _chest = null;
                }
                
                if (_key != null)
                {
                    _interactables.Remove(_key);
                    _key = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error clearing chest/key: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine("DEBUG: SimpleGameReset completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in SimpleGameReset: {ex.Message}");
            _console?.AddMessage($"Error in game reset: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    

    
    private void ReinitializeGameSystems()
    {
        try
        {
            _console?.AddMessage("Reinitializing game systems...", GameConstants.Colors.CONSOLE_CYAN);
            
            // Reset player to initial state
            if (_player != null)
            {
                // Use ResetToStartingState to properly restore health even if player is dead
                _player.ResetToStartingState();
                _player.SetCanMove(false);
            }
            
            // Recreate essential entities
            RecreateEssentialEntities();
            
            // Reset level system
            _levelSystem?.StartLevel(1);
            
            // Reset quest system - use proper reset method
            if (_questManager != null)
            {
                // Reset the extended quest directly to avoid collection modification issues
                var extendedQuest = _questManager.GetExtendedKillBatsQuest();
                if (extendedQuest != null)
                {
                    extendedQuest.Reset();
                    // Extended quest reset directly
                }
                
                // Clear quest lists safely (if QuestManager has such methods)
                _questManager.ClearQuests();
            }
            
            // Reset Pebble state through QuestManager
            if (_questManager != null)
            {
                _questManager.ResetPebbleState();
            }
            
            // Reset dialogue system - close any active dialogue
            if (_dialogueManager != null)
            {
                _dialogueManager.EndDialogue();
            }
            
            // Reset bat spawn manager
            _batSpawnManager?.Reset();
            
            // Clear all projectiles - ProjectileManager doesn't have ClearAllProjectiles
            // We'll handle this in the Update loop by checking for active projectiles
            
            _console?.AddMessage("Game systems reinitialized successfully", GameConstants.Colors.CONSOLE_GREEN);
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Error reinitializing game systems: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void RecreateEssentialEntities()
    {
        try
        {
            // Clear existing entities except player and NPCs
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (entity is Bat || entity is Pebble)
                {
                    if (entity is IAttackable attackable)
                    {
                        _attackSystem?.UnregisterEntity(attackable);
                    }
                    _entities.RemoveAt(i);
                }
            }
            
            // Clear dropped items
            foreach (var droppedItem in _droppedItems)
            {
                _interactables.Remove(droppedItem);
            }
            _droppedItems.Clear();
            
            // Clear chest and key
            if (_chest != null)
            {
                _interactables.Remove(_chest);
                _chest = null;
            }
            
            if (_key != null)
            {
                _interactables.Remove(_key);
                _key = null;
            }
            
            // Reset door - Door.IsOpen is read-only, so we can't reset it directly
            // The door will be reset when the level is restarted
            
            // Recreate Pebble
            var pebble = EntityFactory.CreatePebble(GameConstants.Timing.PEBBLE_SHOOT_COOLDOWN, GameConstants.Damage.PEBBLE_DAMAGE);
            pebble.LoadContent(Content);
            _entities.Add(pebble);
            _attackSystem.RegisterEntity(pebble);
            
            // Subscribe to Pebble events again
            SubscribeToPebbleEvents();
            
            _console?.AddMessage("Essential entities recreated", GameConstants.Colors.CONSOLE_GREEN);
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Error recreating entities: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    /// <summary>
    /// Manage quick access panel visibility based on current UI state
    /// </summary>
    private void ManageQuickAccessVisibility()
    {
        if (_player?.Inventory == null) return;
        
        // Hide quick access panel when any UI panel is visible
        bool shouldHideQuickAccess = 
            _uiManager?.MenuActive == true ||
            _uiManager?.SettingsActive == true ||
            _pauseMenu?.IsVisible == true ||
            _deathPanel?.IsVisible == true ||
            _levelSystem?.LevelTransitionPanel?.IsVisible == true ||
            _saveSlotManager?.IsVisible == true ||
            _uiManager?.ExitConfirmationVisible == true ||
            _dialogueManager?.IsDialogueActive == true;
        
        if (shouldHideQuickAccess)
        {
            _player.Inventory.HideQuickAccess();
        }
        else
        {
            _player.Inventory.ShowQuickAccess();
        }
    }
    
    private void HandleGameStopped()
    {
        try
        {
            // Stop the game and reset to initial state
            _gameStarted = false;
            _isGamePaused = false;
            _isLevelTransitionActive = false;
            
            // Hide all UI elements
            _pauseMenu?.Hide();
            _deathPanel?.Hide();
            _levelSystem?.LevelTransitionPanel?.Hide();
            _saveSlotManager?.Hide();
            
            // Close inventory if open
            if (_player?.Inventory?.IsOpen == true)
            {
                _player.Inventory.ToggleInventory();
            }
            
            // Close any active dialogue
            if (_dialogueManager?.IsDialogueActive == true)
            {
                _dialogueManager.EndDialogue();
            }
            
            // Disable player movement
            _player?.SetCanMove(false);
            
            _console?.AddMessage("Game stopped - returning to main menu", GameConstants.Colors.CONSOLE_CYAN);
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Error stopping game: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void HandleSceneClear(string clearType)
    {
        try
        {
            _console?.AddMessage($"Clearing scene: {clearType}", GameConstants.Colors.CONSOLE_CYAN);
            
            if (clearType == "Full")
            {
                // Clear all game entities except essential ones
                ClearGameScene();
            }
            else if (clearType == "menu_return")
            {
                // Special case for returning to main menu - clear everything
                ClearGameScene();
                
                // Also clear any remaining UI elements
                _pauseMenu?.Hide();
                _deathPanel?.Hide();
                _levelSystem?.LevelTransitionPanel?.Hide();
                _saveSlotManager?.Hide();
                
                // Close inventory if open
                if (_player?.Inventory?.IsOpen == true)
                {
                    _player.Inventory.ToggleInventory();
                }
                
                // Close any active dialogue
                if (_dialogueManager?.IsDialogueActive == true)
                {
                    _dialogueManager.EndDialogue();
                }
                
                // Reset game state completely
                _gameStarted = false;
                _isGamePaused = false;
                _isLevelTransitionActive = false;
                
                // Disable player movement
                _player?.SetCanMove(false);
            }
            
            _console?.AddMessage("Scene cleared successfully", GameConstants.Colors.CONSOLE_GREEN);
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Error clearing scene: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
    
    private void ClearGameScene()
    {
        try
        {
            // Reset player to initial state
            if (_player != null)
            {
                _player.WorldPosition = GameConstants.World.PLAYER_START_POSITION;
                _player.Heal(_player.MaxHealth); // Use Heal method instead of direct assignment
                _player.HasKey = false;
                _player.SetCanMove(false);
            }
            
            // Clear enemies but keep essential entities
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (entity is Bat || entity is Pebble)
                {
                    if (entity is IAttackable attackable)
                    {
                        _attackSystem?.UnregisterEntity(attackable);
                    }
                    _entities.RemoveAt(i);
                }
            }
            
            // Clear dropped items
            foreach (var droppedItem in _droppedItems)
            {
                _interactables.Remove(droppedItem);
            }
            _droppedItems.Clear();
            
            // Clear chest and key
            if (_chest != null)
            {
                _interactables.Remove(_chest);
                _chest = null;
            }
            
            if (_key != null)
            {
                _interactables.Remove(_key);
                _key = null;
            }
            
            // Reset door - Door.IsOpen is read-only, so we can't reset it directly
            // The door will be reset when the level is restarted
            
            // Clear projectiles - ProjectileManager doesn't have ClearAllProjectiles
            // Projectiles will be cleared naturally as they expire
            
            // Reset bat spawn manager
            _batSpawnManager?.Reset();
            
            _console?.AddMessage("Game scene cleared and reset", GameConstants.Colors.CONSOLE_GREEN);
        }
        catch (Exception ex)
        {
            _console?.AddMessage($"Error clearing game scene: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
        }
    }
}
