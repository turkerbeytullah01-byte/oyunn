namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Interface for save/load strategies
    /// Allows swapping between different storage methods
    /// </summary>
    public interface ISaveStrategy
    {
        /// <summary>
        /// Saves data with the specified key
        /// </summary>
        /// <param name="key">Unique identifier for the save data</param>
        /// <param name="data">JSON string to save</param>
        void Save(string key, string data);
        
        /// <summary>
        /// Loads data with the specified key
        /// </summary>
        /// <param name="key">Unique identifier for the save data</param>
        /// <returns>JSON string or null if not found</returns>
        string Load(string key);
        
        /// <summary>
        /// Checks if save data exists for the specified key
        /// </summary>
        /// <param name="key">Unique identifier for the save data</param>
        /// <returns>True if data exists</returns>
        bool Exists(string key);
        
        /// <summary>
        /// Deletes save data with the specified key
        /// </summary>
        /// <param name="key">Unique identifier for the save data</param>
        void Delete(string key);
        
        /// <summary>
        /// Gets the storage type name
        /// </summary>
        string StorageType { get; }
        
        /// <summary>
        /// Checks if this storage method is available on current platform
        /// </summary>
        bool IsAvailable { get; }
    }
}
