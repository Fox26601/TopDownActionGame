using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Manages save and load operations for the game
    /// </summary>
    public class SaveManager : ISaveManager
    {
        private readonly string _saveDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public event Action<bool, string> OnSaveCompleted;
        public event Action<bool, string> OnLoadCompleted;
        
        public SaveManager()
        {
            _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IsometricActionGame", "Saves");
            Directory.CreateDirectory(_saveDirectory);
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        public List<SaveFileInfo> GetSaveFiles()
        {
            var saveFiles = new List<SaveFileInfo>();
            
            try
            {
                var files = Directory.GetFiles(_saveDirectory, "*.json");
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var saveName = Path.GetFileNameWithoutExtension(file);
                        
                        // Try to read save file info
                        var json = File.ReadAllText(file);
                        var gameData = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);
                        
                        if (gameData != null)
                        {
                            saveFiles.Add(new SaveFileInfo
                            {
                                SaveName = saveName,
                                SaveTime = gameData.SaveTime,
                                PlayTime = gameData.WorldData.TotalPlayTime,
                                PlayerLevel = gameData.PlayerData.Level,
                                CurrentLevel = gameData.WorldData.CurrentLevel,
                                PlayerName = "Player", // Could be customizable in the future
                                FileSize = fileInfo.Length
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other files
                        Console.WriteLine($"Error reading save file {file}: {ex.Message}");
                    }
                }
                
                // Sort by save time (newest first)
                return saveFiles.OrderByDescending(f => f.SaveTime).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting save files: {ex.Message}");
                return new List<SaveFileInfo>();
            }
        }
        
        public async Task<bool> SaveGameAsync(string saveName, GameSaveData gameData)
        {
            try
            {
                gameData.SaveTime = DateTime.Now;
                var filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
                
                // Create backup of existing save if it exists
                if (File.Exists(filePath))
                {
                    var backupPath = Path.Combine(_saveDirectory, $"{saveName}_backup.json");
                    File.Copy(filePath, backupPath, true);
                }
                
                // Serialize and save
                var json = JsonSerializer.Serialize(gameData, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
                
                OnSaveCompleted?.Invoke(true, "Game saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(false, $"Save failed: {ex.Message}");
                return false;
            }
        }
        
        public async Task<GameSaveData> LoadGameAsync(string saveName)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
                
                if (!File.Exists(filePath))
                {
                    OnLoadCompleted?.Invoke(false, "Save file not found");
                    return null;
                }
                
                var json = await File.ReadAllTextAsync(filePath);
                var gameData = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);
                
                if (gameData != null)
                {
                    OnLoadCompleted?.Invoke(true, "Game loaded successfully");
                    return gameData;
                }
                else
                {
                    OnLoadCompleted?.Invoke(false, "Failed to deserialize save data");
                    return null;
                }
            }
            catch (Exception ex)
            {
                OnLoadCompleted?.Invoke(false, $"Load failed: {ex.Message}");
                return null;
            }
        }
        
        public bool DeleteSave(string saveName)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting save {saveName}: {ex.Message}");
                return false;
            }
        }
        
        public bool SaveExists(string saveName)
        {
            var filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
            return File.Exists(filePath);
        }
        
        public SaveFileInfo GetSaveFileInfo(string saveName)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
                
                if (!File.Exists(filePath))
                    return null;
                
                var json = File.ReadAllText(filePath);
                var gameData = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);
                
                if (gameData != null)
                {
                    var fileInfo = new FileInfo(filePath);
                    return new SaveFileInfo
                    {
                        SaveName = saveName,
                        SaveTime = gameData.SaveTime,
                        PlayTime = gameData.WorldData.TotalPlayTime,
                        PlayerLevel = gameData.PlayerData.Level,
                        CurrentLevel = gameData.WorldData.CurrentLevel,
                        PlayerName = "Player",
                        FileSize = fileInfo.Length
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting save file info for {saveName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get the save directory path
        /// </summary>
        public string GetSaveDirectory()
        {
            return _saveDirectory;
        }
        
        /// <summary>
        /// Create a quick save
        /// </summary>
        public async Task<bool> QuickSaveAsync(GameSaveData gameData)
        {
            return await SaveGameAsync("quicksave", gameData);
        }
        
        /// <summary>
        /// Load the quick save
        /// </summary>
        public async Task<GameSaveData> QuickLoadAsync()
        {
            return await LoadGameAsync("quicksave");
        }
        
        /// <summary>
        /// Create an auto save
        /// </summary>
        public async Task<bool> AutoSaveAsync(GameSaveData gameData)
        {
            return await SaveGameAsync("autosave", gameData);
        }
    }
}
