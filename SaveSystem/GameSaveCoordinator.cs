using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using IsometricActionGame.Quests;
using IsometricActionGame.Inventory;
using IsometricActionGame.Items;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Coordinates save and load operations across all game systems
    /// </summary>
    public class GameSaveCoordinator
    {
        private readonly ISaveManager _saveManager;
        private readonly List<ISaveable> _saveableObjects;
        private DateTime _gameStartTime;
        private bool _isGameStarted;
        
        public event Action<bool, string> OnSaveCompleted;
        public event Action<bool, string> OnLoadCompleted;
        public event Action OnGameStateChanged;
        
        public GameSaveCoordinator(ISaveManager saveManager)
        {
            _saveManager = saveManager;
            _saveableObjects = new List<ISaveable>();
            _gameStartTime = DateTime.Now;
            
            // Subscribe to save manager events
            _saveManager.OnSaveCompleted += (success, message) => OnSaveCompleted?.Invoke(success, message);
            _saveManager.OnLoadCompleted += (success, message) => OnLoadCompleted?.Invoke(success, message);
        }
        
        /// <summary>
        /// Register a saveable object
        /// </summary>
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!_saveableObjects.Contains(saveable))
            {
                _saveableObjects.Add(saveable);
            }
        }
        
        /// <summary>
        /// Unregister a saveable object
        /// </summary>
        public void UnregisterSaveable(ISaveable saveable)
        {
            _saveableObjects.Remove(saveable);
        }
        
        /// <summary>
        /// Start the game session
        /// </summary>
        public void StartGame()
        {
            _gameStartTime = DateTime.Now;
            _isGameStarted = true;
            OnGameStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Create save data from current game state
        /// </summary>
        public GameSaveData CreateSaveData(Player player, QuestManager questManager, GameMap gameMap, 
            List<IEntity> entities, List<DroppedItem> droppedItems, int currentLevel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: CreateSaveData started");
                
                var gameData = new GameSaveData();
                
                if (gameData == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Failed to create GameSaveData");
                    return null;
                }
            
            // Save player data - use ISaveable interface for consistency
            if (player != null)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Saving player data - Position: {player.WorldPosition}, Health: {player.CurrentHealth}/{player.MaxHealth}");
                
                // Use ISaveable interface for player data
                var playerData = player.Serialize();
                if (playerData != null)
                {
                    gameData.GlobalData[player.SaveId] = playerData;
                    System.Diagnostics.Debug.WriteLine("DEBUG: Player data saved via ISaveable interface");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Player.Serialize() returned null");
                }
                
                // Also save to PlayerData for backward compatibility
                gameData.PlayerData.Position = player.WorldPosition;
                gameData.PlayerData.CurrentHealth = player.CurrentHealth;
                gameData.PlayerData.MaxHealth = player.MaxHealth;
                gameData.PlayerData.Gold = player.Gold;
                gameData.PlayerData.Experience = player.Experience;
                gameData.PlayerData.Level = player.Level;
                
                // Save inventory - simplified for now
                if (player.Inventory != null)
                {
                    gameData.PlayerData.InventoryItems.Clear();
        
                    // For now, we'll save basic inventory data
                }
            }
            
            // Save world data
            gameData.WorldData.CurrentLevel = currentLevel;
            gameData.WorldData.GameStartTime = _gameStartTime;
            gameData.WorldData.TotalPlayTime = DateTime.Now - _gameStartTime;
            
            // Save entities
            gameData.WorldData.Entities.Clear();
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity is ISaveable saveableEntity)
                    {
                        var entityData = saveableEntity.Serialize();
                        var entitySaveData = new EntitySaveData
                        {
                            EntityId = saveableEntity.SaveId,
                            EntityType = entity.GetType().Name,
                            Position = entity.WorldPosition,
                            IsActive = true // Could be more sophisticated
                        };
                        
                        // Store serialized data in CustomData
                        if (entityData != null)
                        {
                            foreach (var kvp in entityData.Data)
                            {
                                entitySaveData.CustomData[kvp.Key] = kvp.Value;
                            }
                        }
                        
                        gameData.WorldData.Entities.Add(entitySaveData);
                    }
                }
            }
            
            // Save dropped items
            gameData.WorldData.DroppedItems.Clear();
            if (droppedItems != null)
            {
                foreach (var item in droppedItems)
                {
                    gameData.WorldData.DroppedItems.Add(new DroppedItemSaveData
                    {
                        ItemId = item.Item.Name, // Use Name as ID for now
                        Position = item.WorldPosition,
                        Quantity = item.Item.Quantity,
                        IsCollected = false
                    });
                }
            }
            
            // Save quest data
            gameData.QuestData.Clear();
            if (questManager != null)
            {
                foreach (var quest in questManager.GetAllQuests())
                {
                    if (quest is ISaveable saveableQuest)
                    {
                        var questData = saveableQuest.Serialize();
                        gameData.QuestData.Add(new QuestSaveData
                        {
                            QuestId = saveableQuest.SaveId,
                            IsActive = quest.IsActive,
                            IsCompleted = quest.IsCompleted,
                            IsTurnedIn = quest.IsTurnedIn,
                            IsRefused = quest.IsRefused
                        });
                    }
                }
            }
            
            // Save global data from all saveable objects
            foreach (var saveable in _saveableObjects)
            {
                var data = saveable.Serialize();
                if (data != null && !string.IsNullOrEmpty(data.SaveId))
                {
                    gameData.GlobalData[saveable.SaveId] = data;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("DEBUG: CreateSaveData completed successfully");
            return gameData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in CreateSaveData: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
            return null;
        }
        }
        
        /// <summary>
        /// Apply loaded save data to game state
        /// </summary>
        public void ApplySaveData(GameSaveData gameData, Player player, QuestManager questManager, 
            GameMap gameMap, List<IEntity> entities, List<DroppedItem> droppedItems)
        {
            if (gameData == null) return;
            
            System.Diagnostics.Debug.WriteLine("DEBUG: ApplySaveData started");
            
            // Apply world data first
            if (gameData.WorldData != null)
            {
                _gameStartTime = gameData.WorldData.GameStartTime;
                _isGameStarted = true;
                System.Diagnostics.Debug.WriteLine($"DEBUG: Applied world data - GameStartTime: {_gameStartTime}");
            }
            
            // Apply global data to saveable objects (including player)
            System.Diagnostics.Debug.WriteLine($"DEBUG: Applying global data to {_saveableObjects.Count} saveable objects");
            foreach (var saveable in _saveableObjects)
            {
                if (gameData.GlobalData.TryGetValue(saveable.SaveId, out object data) && data is SaveData saveData)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Applying data to {saveable.SaveId}");
                    saveable.Deserialize(saveData);
                    
                    // Special handling for player to log position
                    if (saveable is Player playerObj)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Player position after deserialize: {playerObj.WorldPosition}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: No data found for {saveable.SaveId}");
                }
            }
            
            // Apply player data from PlayerData as fallback (for backward compatibility)
            if (player != null && gameData.PlayerData != null)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Applying fallback player data - Position: {gameData.PlayerData.Position}");
                
                // Only apply if not already applied via ISaveable
                if (!gameData.GlobalData.ContainsKey(player.SaveId))
                {
                    player.WorldPosition = gameData.PlayerData.Position;
                    player.SetHealth(gameData.PlayerData.CurrentHealth);
                    player.Gold = gameData.PlayerData.Gold;
                    player.Experience = gameData.PlayerData.Experience;
                    player.Level = gameData.PlayerData.Level;
                    System.Diagnostics.Debug.WriteLine("DEBUG: Applied fallback player data");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Player data already applied via ISaveable, skipping fallback");
                }
                
                // Load inventory - simplified for now
                if (player.Inventory != null && gameData.PlayerData.InventoryItems != null)
                {
        
                    // For now, we'll skip inventory loading
                }
            }
            
            System.Diagnostics.Debug.WriteLine("DEBUG: ApplySaveData completed");
            OnGameStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Save game asynchronously
        /// </summary>
        public async Task<bool> SaveGameAsync(string saveName, Player player, QuestManager questManager, 
            GameMap gameMap, List<IEntity> entities, List<DroppedItem> droppedItems, int currentLevel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: GameSaveCoordinator.SaveGameAsync called for {saveName}");
                
                if (player == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Player is null in SaveGameAsync");
                    return false;
                }
                
                var gameData = CreateSaveData(player, questManager, gameMap, entities, droppedItems, currentLevel);
                
                if (gameData == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: CreateSaveData returned null");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"DEBUG: Created save data, calling SaveManager");
                var result = await _saveManager.SaveGameAsync(saveName, gameData);
                System.Diagnostics.Debug.WriteLine($"DEBUG: SaveManager result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in SaveGameAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Load game asynchronously
        /// </summary>
        public async Task<GameSaveData> LoadGameAsync(string saveName)
        {
            return await _saveManager.LoadGameAsync(saveName);
        }
        
        /// <summary>
        /// Quick save
        /// </summary>
        public async Task<bool> QuickSaveAsync(Player player, QuestManager questManager, 
            GameMap gameMap, List<IEntity> entities, List<DroppedItem> droppedItems, int currentLevel)
        {
            var gameData = CreateSaveData(player, questManager, gameMap, entities, droppedItems, currentLevel);
            return await _saveManager.QuickSaveAsync(gameData);
        }
        
        /// <summary>
        /// Quick load
        /// </summary>
        public async Task<GameSaveData> QuickLoadAsync()
        {
            return await _saveManager.QuickLoadAsync();
        }
        
        /// <summary>
        /// Auto save
        /// </summary>
        public async Task<bool> AutoSaveAsync(Player player, QuestManager questManager, 
            GameMap gameMap, List<IEntity> entities, List<DroppedItem> droppedItems, int currentLevel)
        {
            var gameData = CreateSaveData(player, questManager, gameMap, entities, droppedItems, currentLevel);
            return await _saveManager.AutoSaveAsync(gameData);
        }
        
        /// <summary>
        /// Get save files
        /// </summary>
        public List<SaveFileInfo> GetSaveFiles()
        {
            return _saveManager.GetSaveFiles();
        }
        
        /// <summary>
        /// Delete save
        /// </summary>
        public bool DeleteSave(string saveName)
        {
            return _saveManager.DeleteSave(saveName);
        }
        
        /// <summary>
        /// Check if save exists
        /// </summary>
        public bool SaveExists(string saveName)
        {
            return _saveManager.SaveExists(saveName);
        }
        
        /// <summary>
        /// Get current play time
        /// </summary>
        public TimeSpan GetCurrentPlayTime()
        {
            if (!_isGameStarted)
                return TimeSpan.Zero;
                
            return DateTime.Now - _gameStartTime;
        }
    }
}
