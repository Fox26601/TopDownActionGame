using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Quests;
using IsometricActionGame.Inventory;
using IsometricActionGame.Items;
using IsometricActionGame.UI;
using IsometricActionGame.NPCs;
using IsometricActionGame.Dialogue;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Manages save and load operations for the game
    /// </summary>
    public class GameSaveLoadManager
    {
        private readonly GameSaveCoordinator _saveCoordinator;
        private readonly SaveLoadPanel _saveLoadPanel;
        private readonly MessageDisplay _messageDisplay;
        
        // Game references
        private Player _player;
        private QuestManager _questManager;
        private GameMap _gameMap;
        private List<IEntity> _entities;
        private List<IInteractable> _interactables;
        private List<DroppedItem> _droppedItems;
        private int _currentLevel;
        private LevelSystem _levelSystem;
        private ContentManager _content;
        private DialogueManager _dialogueManager;
        
        // Auto-save settings
        private float _autoSaveTimer = 0f;
        private const float AUTO_SAVE_INTERVAL = 300f; // 5 minutes
        private bool _autoSaveEnabled = true;
        
        // Events
        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;
        
        public GameSaveLoadManager(ISaveManager saveManager, SaveLoadPanel saveLoadPanel, MessageDisplay messageDisplay)
        {
            _saveCoordinator = new GameSaveCoordinator(saveManager);
            _saveLoadPanel = saveLoadPanel;
            _messageDisplay = messageDisplay;
            
            // Subscribe to events only if SaveLoadPanel is provided
            if (_saveLoadPanel != null)
            {
                _saveLoadPanel.OnSaveRequested += HandleSaveRequest;
                _saveLoadPanel.OnLoadRequested += HandleLoadRequest;
                _saveLoadPanel.OnDeleteRequested += HandleDeleteRequest;
                _saveLoadPanel.OnPanelClosed += HandlePanelClosed;
            }
            
            _saveCoordinator.OnSaveCompleted += (success, message) =>
            {
                                 if (success)
                 {
                     // _messageDisplay.ShowMessage("Game saved successfully!", Color.Green);
                     OnSaveCompleted?.Invoke();
                 }
                 else
                 {
                     // _messageDisplay.ShowMessage($"Save failed: {message}", Color.Red);
                     OnSaveError?.Invoke(message);
                 }
            };
            
            _saveCoordinator.OnLoadCompleted += (success, message) =>
            {
                                 if (success)
                 {
                     // _messageDisplay.ShowMessage("Game loaded successfully!", Color.Green);
                     OnLoadCompleted?.Invoke();
                 }
                 else
                 {
                     // _messageDisplay.ShowMessage($"Load failed: {message}", Color.Red);
                     OnLoadError?.Invoke(message);
                 }
            };
        }
        
        /// <summary>
        /// Initialize game references
        /// </summary>
        public void Initialize(Player player, QuestManager questManager, GameMap gameMap, 
            List<IEntity> entities, List<IInteractable> interactables, List<DroppedItem> droppedItems, int currentLevel, 
            LevelSystem levelSystem = null, ContentManager content = null, DialogueManager dialogueManager = null)
        {
            _player = player;
            _questManager = questManager;
            _gameMap = gameMap;
            _entities = entities;
            _interactables = interactables;
            _droppedItems = droppedItems;
            _currentLevel = currentLevel;
            _levelSystem = levelSystem;
            _content = content;
            _dialogueManager = dialogueManager;
            
            // Register saveable objects
            System.Diagnostics.Debug.WriteLine($"DEBUG: Registering saveable objects - Player: {_player != null}, QuestManager: {_questManager != null}");
            
            if (_player is ISaveable saveablePlayer)
            {
                _saveCoordinator.RegisterSaveable(saveablePlayer);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Registered player as saveable with SaveId: {saveablePlayer.SaveId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Player does not implement ISaveable");
            }
            
            if (_questManager is ISaveable saveableQuestManager)
            {
                _saveCoordinator.RegisterSaveable(saveableQuestManager);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Registered quest manager as saveable with SaveId: {saveableQuestManager.SaveId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: QuestManager does not implement ISaveable");
            }
            
            // Register entities as saveable objects
            if (_entities != null)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Registering {_entities.Count} entities as saveable objects");
                foreach (var entity in _entities)
                {
                    if (entity is ISaveable saveableEntity)
                    {
                        _saveCoordinator.RegisterSaveable(saveableEntity);
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Registered entity {entity.GetType().Name} as saveable with SaveId: {saveableEntity.SaveId}");
                    }
                }
            }
            
            // Start game session
            _saveCoordinator.StartGame();
        }
        
        /// <summary>
        /// Update the manager
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Update auto-save timer
            if (_autoSaveEnabled)
            {
                _autoSaveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
                {
                    _autoSaveTimer = 0f;
                    AutoSave();
                }
            }
        }
        
        /// <summary>
        /// Show save/load panel
        /// </summary>
        public void ShowSaveLoadPanel(Rectangle bounds)
        {
            _saveLoadPanel?.Show(bounds);
        }
        
        /// <summary>
        /// Hide save/load panel
        /// </summary>
        public void HideSaveLoadPanel()
        {
            _saveLoadPanel?.Hide();
        }
        
        /// <summary>
        /// Quick save
        /// </summary>
        public async Task<bool> QuickSave()
        {
            return await _saveCoordinator.QuickSaveAsync(_player, _questManager, _gameMap, _entities, _droppedItems, _currentLevel);
        }
        
        /// <summary>
        /// Quick load
        /// </summary>
        public async Task<bool> QuickLoad()
        {
            var gameData = await _saveCoordinator.QuickLoadAsync();
            if (gameData != null)
            {
                ApplyLoadedData(gameData);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Auto save
        /// </summary>
        private async void AutoSave()
        {
            await _saveCoordinator.AutoSaveAsync(_player, _questManager, _gameMap, _entities, _droppedItems, _currentLevel);
        }
        
                /// <summary>
        /// Handle save request from UI
        /// </summary>
        private async void HandleSaveRequest(string saveName)
        {
                         if (string.IsNullOrWhiteSpace(saveName))
             {
                 // _messageDisplay.ShowMessage("Please enter a save name", Color.Yellow);
                 return;
             }
             
             var success = await _saveCoordinator.SaveGameAsync(saveName, _player, _questManager, _gameMap, _entities, _droppedItems, _currentLevel);
             if (success)
             {
                 _saveLoadPanel?.Hide();
             }
         }
        
        /// <summary>
        /// Handle load request from UI
        /// </summary>
        private async void HandleLoadRequest(string saveName)
        {
            var gameData = await _saveCoordinator.LoadGameAsync(saveName);
            if (gameData != null)
            {
                ApplyLoadedData(gameData);
                _saveLoadPanel?.Hide();
            }
        }
        
        /// <summary>
        /// Handle delete request from UI
        /// </summary>
        private void HandleDeleteRequest(string saveName)
        {
                         var success = _saveCoordinator.DeleteSave(saveName);
             if (success)
             {
                 // _messageDisplay.ShowMessage($"Save '{saveName}' deleted", Color.Green);
             }
             else
             {
                 // _messageDisplay.ShowMessage($"Failed to delete save '{saveName}'", Color.Red);
             }
        }
        
        /// <summary>
        /// Handle panel closed event
        /// </summary>
        private void HandlePanelClosed()
        {
            // Panel was closed, no action needed
        }
        
        /// <summary>
        /// Apply loaded save data to game state
        /// </summary>
        private void ApplyLoadedData(GameSaveData gameData)
        {
            if (gameData == null) return;
            
            // Debug: Log player position and health before loading
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player position before loading: {_player?.WorldPosition}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player health before loading: {_player?.CurrentHealth}/{_player?.MaxHealth}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player reference: {_player?.GetHashCode()}");
            
            // Set flag to prevent position reset during level loading
            if (_levelSystem != null)
            {
                _levelSystem.SetLoadingFromSave(true);
                System.Diagnostics.Debug.WriteLine("DEBUG: SetLoadingFromSave(true) called");
            }
            
            // Apply data to game systems (this restores player data including health)
            _saveCoordinator.ApplySaveData(gameData, _player, _questManager, _gameMap, _entities, _droppedItems);
            
            // Debug: Log player position and health after applying save data
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player position after applying save data: {_player?.WorldPosition}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player health after applying save data: {_player?.CurrentHealth}/{_player?.MaxHealth}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player reference after save data: {_player?.GetHashCode()}");
            
            // Update current level
            if (gameData.WorldData != null)
            {
                _currentLevel = gameData.WorldData.CurrentLevel;
            }
            
            // Restore entities (but skip player since it's already restored)
            RestoreEntities(gameData.WorldData?.Entities);
            
            // Restore dropped items
            RestoreDroppedItems(gameData.WorldData?.DroppedItems);
            
            // Restore quests
            RestoreQuests(gameData.QuestData);
            
            // Debug: Log player position and health after all restoration
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player position after all restoration: {_player?.WorldPosition}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player health after all restoration: {_player?.CurrentHealth}/{_player?.MaxHealth}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Player reference after all restoration: {_player?.GetHashCode()}");
            
            // Reset flag after loading is complete
            if (_levelSystem != null)
            {
                _levelSystem.SetLoadingFromSave(false);
                System.Diagnostics.Debug.WriteLine("DEBUG: SetLoadingFromSave(false) called");
            }
            
            // Trigger load completed event
            OnLoadCompleted?.Invoke();
        }
        
        /// <summary>
        /// Restore entities from save data
        /// </summary>
        private void RestoreEntities(List<EntitySaveData> entityData)
        {
            if (entityData == null || _entities == null) return;
            
            System.Diagnostics.Debug.WriteLine($"DEBUG: Restoring {entityData.Count} entities from save data");
            
            // Clear existing entities first, but preserve player
            var playerToPreserve = _entities.FirstOrDefault(e => e is Player) as Player;
            _entities.Clear();
            
            // Add player back if it exists
            if (playerToPreserve != null)
            {
                _entities.Add(playerToPreserve);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Preserved existing player at position: {playerToPreserve.WorldPosition}");
            }
            
            // Restore each entity from save data (skip player since it's already handled)
            foreach (var entitySaveData in entityData)
            {
                // Skip player entity - it's already restored by GameSaveCoordinator
                if (entitySaveData.EntityId == "Player")
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Skipping player entity restoration - already handled");
                    continue;
                }
                
                System.Diagnostics.Debug.WriteLine($"DEBUG: Restoring entity: {entitySaveData.EntityId} at position: {entitySaveData.Position}");
                
                // Create entity based on type and add to appropriate lists
                CreateEntityFromSaveData(entitySaveData);
                
                System.Diagnostics.Debug.WriteLine($"DEBUG: Entity {entitySaveData.EntityId} restored successfully");
            }
            
            System.Diagnostics.Debug.WriteLine($"DEBUG: Total entities after restoration: {_entities.Count}");
        }
        
        /// <summary>
        /// Create entity instance from save data and add to appropriate lists
        /// </summary>
        private void CreateEntityFromSaveData(EntitySaveData entityData)
        {
            if (_content == null)
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Content manager is null, cannot create entity");
                return;
            }
            
            // This method should create the appropriate entity type based on the save data
            // and add it to the appropriate list (entities or interactables)
            
            switch (entityData.EntityId)
            {
                case "Player":
                    // Player should already exist, just update position
                    if (_player != null)
                    {
                        _player.WorldPosition = entityData.Position;
                    }
                    break;
                    
                case "Pebble":
                    // Create new Pebble instance and add to entities
                    var pebble = new Pebble(entityData.Position, GameConstants.Timing.PEBBLE_SHOOT_COOLDOWN, GameConstants.Damage.PEBBLE_DAMAGE);
                    System.Diagnostics.Debug.WriteLine("GameSaveLoadManager: Loading pebble content");
                    pebble.LoadContent(_content);
                    _entities?.Add(pebble);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Created Pebble at position: {entityData.Position}");
                    break;
                    
                case "Door":
                    // Find existing door and update its state instead of creating a new one
                    var existingDoor = _interactables?.OfType<Door>().FirstOrDefault();
                    if (existingDoor != null)
                    {
                        System.Diagnostics.Debug.WriteLine("GameSaveLoadManager: Updating existing door from save data");
                        existingDoor.SetPosition(entityData.Position);
                        // Deserialize door state if it implements ISaveable
                        if (existingDoor is ISaveable saveableDoor)
                        {
                            var doorData = new SaveData("Door");
                            doorData.SetValue("Position", entityData.Position);
                            // Add other door properties as needed
                            saveableDoor.Deserialize(doorData);
                        }
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Updated existing Door at position: {entityData.Position}");
                    }
                    else
                    {
                        // Only create new door if none exists (shouldn't happen in normal flow)
                        System.Diagnostics.Debug.WriteLine("GameSaveLoadManager: No existing door found, creating new one");
                        var door = new Door(entityData.Position);
                        door.LoadContent(_content);
                        _interactables?.Add(door);
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Created new Door at position: {entityData.Position}");
                    }
                    break;
                    
                case "Mayor":
                    // Create new Mayor instance and add to interactables
                    var mayor = new Mayor(entityData.Position, _dialogueManager, _questManager);
                    mayor.LoadContent(_content);
                    mayor.SetPlayerReference(_player);
                    _interactables?.Add(mayor);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Created Mayor at position: {entityData.Position}");
                    break;
                    
                case "Vendor":
                    // Create new Vendor instance and add to interactables
                    var vendor = new Vendor(entityData.Position, _dialogueManager);
                    vendor.LoadContent(_content);
                    vendor.SetPlayerReference(_player);
                    _interactables?.Add(vendor);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Created Vendor at position: {entityData.Position}");
                    break;
                    
                case "Chest":
                    // Create new Chest instance and add to interactables
                    var chest = new Chest(entityData.Position);
                    chest.LoadContent(_content);
                    _interactables?.Add(chest);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Created Chest at position: {entityData.Position}");
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Unknown entity type: {entityData.EntityId}");
                    break;
            }
        }
        
        /// <summary>
        /// Restore dropped items from save data
        /// </summary>
        private void RestoreDroppedItems(List<DroppedItemSaveData> itemData)
        {
            if (itemData == null || _droppedItems == null) return;
            
            // Clear existing dropped items
            _droppedItems.Clear();
            
            // Restore dropped items
            foreach (var itemSaveData in itemData)
            {
                if (!itemSaveData.IsCollected)
                {
                    // This would need to be implemented based on your item system
                    // var item = ItemFactory.CreateItem(itemSaveData.ItemId);
                    // if (item != null)
                    // {
                    //     item.Quantity = itemSaveData.Quantity;
                    //     var droppedItem = new DroppedItem(item, itemSaveData.Position);
                    //     _droppedItems.Add(droppedItem);
                    // }
                }
            }
        }
        
        /// <summary>
        /// Restore quests from save data
        /// </summary>
        private void RestoreQuests(List<QuestSaveData> questData)
        {
            if (questData == null || _questManager == null) return;
            
            foreach (var questSaveData in questData)
            {
                var quest = _questManager.GetQuestById(questSaveData.QuestId);
                if (quest != null && quest is ISaveable saveableQuest)
                {
                    var data = new SaveData(questSaveData.QuestId);
                    data.SetValue("IsActive", questSaveData.IsActive);
                    data.SetValue("IsCompleted", questSaveData.IsCompleted);
                    data.SetValue("IsTurnedIn", questSaveData.IsTurnedIn);
                    data.SetValue("IsRefused", questSaveData.IsRefused);
                    
                    saveableQuest.Deserialize(data);
                }
            }
        }
        
        /// <summary>
        /// Get save files for UI display
        /// </summary>
        public List<SaveFileInfo> GetSaveFiles()
        {
            return _saveCoordinator.GetSaveFiles();
        }
        
        /// <summary>
        /// Enable or disable auto-save
        /// </summary>
        public void SetAutoSaveEnabled(bool enabled)
        {
            _autoSaveEnabled = enabled;
            if (enabled)
            {
                _autoSaveTimer = 0f;
            }
        }
        
        /// <summary>
        /// Get current play time
        /// </summary>
        public TimeSpan GetCurrentPlayTime()
        {
            return _saveCoordinator.GetCurrentPlayTime();
        }
        
        /// <summary>
        /// Update current level
        /// </summary>
        public void UpdateCurrentLevel(int level)
        {
            _currentLevel = level;
        }
        
        /// <summary>
        /// Update entities list
        /// </summary>
        public void UpdateEntities(List<IEntity> entities)
        {
            _entities = entities;
        }
        
        /// <summary>
        /// Update dropped items list
        /// </summary>
        public void UpdateDroppedItems(List<DroppedItem> droppedItems)
        {
            _droppedItems = droppedItems;
        }
        
        /// <summary>
        /// Save game to specific slot
        /// </summary>
        public async Task<bool> SaveGameAsync(string saveName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Starting save to {saveName}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Player: {_player != null}, QuestManager: {_questManager != null}, Entities: {_entities?.Count ?? 0}");
                
                if (_player == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Player is null, cannot save");
                    return false;
                }
                
                var result = await _saveCoordinator.SaveGameAsync(saveName, _player, _questManager, _gameMap, _entities, _droppedItems, _currentLevel);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Save result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Save exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Load game from specific slot
        /// </summary>
        public async Task<bool> LoadGameAsync(string saveName)
        {
            var gameData = await _saveCoordinator.LoadGameAsync(saveName);
            if (gameData != null)
            {
                ApplyLoadedData(gameData);
                return true;
            }
            return false;
        }
    }
}
