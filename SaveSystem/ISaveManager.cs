using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Interface for save manager operations
    /// </summary>
    public interface ISaveManager
    {
        /// <summary>
        /// Event fired when save operation completes
        /// </summary>
        event Action<bool, string> OnSaveCompleted;
        
        /// <summary>
        /// Event fired when load operation completes
        /// </summary>
        event Action<bool, string> OnLoadCompleted;
        
        /// <summary>
        /// Get list of available save files
        /// </summary>
        /// <returns>List of save file information</returns>
        List<SaveFileInfo> GetSaveFiles();
        
        /// <summary>
        /// Save game data asynchronously
        /// </summary>
        /// <param name="saveName">Name of the save file</param>
        /// <param name="gameData">Game data to save</param>
        /// <returns>Task representing the save operation</returns>
        Task<bool> SaveGameAsync(string saveName, GameSaveData gameData);
        
        /// <summary>
        /// Load game data asynchronously
        /// </summary>
        /// <param name="saveName">Name of the save file to load</param>
        /// <returns>Task containing the loaded game data</returns>
        Task<GameSaveData> LoadGameAsync(string saveName);
        
        /// <summary>
        /// Delete a save file
        /// </summary>
        /// <param name="saveName">Name of the save file to delete</param>
        /// <returns>True if deletion was successful</returns>
        bool DeleteSave(string saveName);
        
        /// <summary>
        /// Check if a save file exists
        /// </summary>
        /// <param name="saveName">Name of the save file</param>
        /// <returns>True if save file exists</returns>
        bool SaveExists(string saveName);
        
        /// <summary>
        /// Get save file information
        /// </summary>
        /// <param name="saveName">Name of the save file</param>
        /// <returns>Save file information or null if not found</returns>
        SaveFileInfo GetSaveFileInfo(string saveName);
        
        /// <summary>
        /// Create a quick save
        /// </summary>
        Task<bool> QuickSaveAsync(GameSaveData gameData);
        
        /// <summary>
        /// Load the quick save
        /// </summary>
        Task<GameSaveData> QuickLoadAsync();
        
        /// <summary>
        /// Create an auto save
        /// </summary>
        Task<bool> AutoSaveAsync(GameSaveData gameData);
    }
    
    /// <summary>
    /// Information about a save file
    /// </summary>
    public class SaveFileInfo
    {
        public string SaveName { get; set; }
        public DateTime SaveTime { get; set; }
        public TimeSpan PlayTime { get; set; }
        public int PlayerLevel { get; set; }
        public int CurrentLevel { get; set; }
        public string PlayerName { get; set; }
        public long FileSize { get; set; }
    }
    
    /// <summary>
    /// Complete game save data structure
    /// </summary>
    public class GameSaveData
    {
        public PlayerSaveData PlayerData { get; set; }
        public WorldSaveData WorldData { get; set; }
        public List<QuestSaveData> QuestData { get; set; }
        public Dictionary<string, object> GlobalData { get; set; }
        public DateTime SaveTime { get; set; }
        public string GameVersion { get; set; }
        
        public GameSaveData()
        {
            PlayerData = new PlayerSaveData();
            WorldData = new WorldSaveData();
            QuestData = new List<QuestSaveData>();
            GlobalData = new Dictionary<string, object>();
            SaveTime = DateTime.Now;
            GameVersion = GameConstants.Settings.GAME_VERSION;
        }
    }
}
