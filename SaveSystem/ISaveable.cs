using System;

namespace IsometricActionGame.SaveSystem
{
    /// <summary>
    /// Interface for objects that can be saved and loaded
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Unique identifier for this saveable object
        /// </summary>
        string SaveId { get; }
        
        /// <summary>
        /// Serialize object data to save format
        /// </summary>
        /// <returns>Serialized data</returns>
        SaveData Serialize();
        
        /// <summary>
        /// Deserialize object data from save format
        /// </summary>
        /// <param name="data">Serialized data to load</param>
        void Deserialize(SaveData data);
    }
}
