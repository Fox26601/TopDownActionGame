using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Data structure for serialized save information
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string SaveId { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public DateTime SaveTime { get; set; }
        
        public SaveData()
        {
            Data = new Dictionary<string, object>();
            SaveTime = DateTime.Now;
        }
        
        public SaveData(string saveId) : this()
        {
            SaveId = saveId;
        }
        
        public void SetValue<T>(string key, T value)
        {
            Data[key] = value;
        }
        
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (Data.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        public bool HasValue(string key)
        {
            return Data.ContainsKey(key);
        }
    }
    
    /// <summary>
    /// Player save data structure
    /// </summary>
    [Serializable]
    public class PlayerSaveData : SaveData
    {
        public Vector2 Position { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int Gold { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public List<InventoryItemSaveData> InventoryItems { get; set; }
        
        public PlayerSaveData() : base("Player")
        {
            InventoryItems = new List<InventoryItemSaveData>();
        }
    }
    
    /// <summary>
    /// Inventory item save data structure
    /// </summary>
    [Serializable]
    public class InventoryItemSaveData
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public int SlotIndex { get; set; }
        public Dictionary<string, object> CustomData { get; set; }
        
        public InventoryItemSaveData()
        {
            CustomData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Quest save data structure
    /// </summary>
    [Serializable]
    public class QuestSaveData : SaveData
    {
        public string QuestId { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTurnedIn { get; set; }
        public bool IsRefused { get; set; }
        public Dictionary<string, object> ProgressData { get; set; }
        
        public QuestSaveData() : base("Quest")
        {
            ProgressData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Game world save data structure
    /// </summary>
    [Serializable]
    public class WorldSaveData : SaveData
    {
        public int CurrentLevel { get; set; }
        public List<EntitySaveData> Entities { get; set; }
        public List<DroppedItemSaveData> DroppedItems { get; set; }
        public Dictionary<string, bool> InteractableStates { get; set; }
        public Dictionary<string, int> EnemyKillCounts { get; set; }
        public DateTime GameStartTime { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        
        public WorldSaveData() : base("World")
        {
            Entities = new List<EntitySaveData>();
            DroppedItems = new List<DroppedItemSaveData>();
            InteractableStates = new Dictionary<string, bool>();
            EnemyKillCounts = new Dictionary<string, int>();
        }
    }
    
    /// <summary>
    /// Entity save data structure
    /// </summary>
    [Serializable]
    public class EntitySaveData
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public Vector2 Position { get; set; }
        public int CurrentHealth { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, object> CustomData { get; set; }
        
        public EntitySaveData()
        {
            CustomData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Dropped item save data structure
    /// </summary>
    [Serializable]
    public class DroppedItemSaveData
    {
        public string ItemId { get; set; }
        public Vector2 Position { get; set; }
        public int Quantity { get; set; }
        public bool IsCollected { get; set; }
    }
}
