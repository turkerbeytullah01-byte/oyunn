using System;
using System.IO;
using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// JSON file-based save strategy
    /// Saves to Application.persistentDataPath
    /// </summary>
    public class JsonFileSaveStrategy : ISaveStrategy
    {
        private readonly string _saveDirectory;
        private readonly object _fileLock = new object();
        
        public string StorageType => "JsonFile";
        
        public bool IsAvailable => true; // Always available on all platforms
        
        public JsonFileSaveStrategy(string customDirectory = null)
        {
            _saveDirectory = customDirectory ?? Application.persistentDataPath;
            EnsureDirectoryExists();
        }
        
        /// <summary>
        /// Saves data to a JSON file
        /// </summary>
        public void Save(string key, string data)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[JsonFileSaveStrategy] Cannot save with empty key");
                return;
            }
            
            lock (_fileLock)
            {
                try
                {
                    string filePath = GetFilePath(key);
                    string directory = Path.GetDirectoryName(filePath);
                    
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    // Write to temp file first, then move (atomic operation)
                    string tempFile = filePath + ".tmp";
                    File.WriteAllText(tempFile, data);
                    
                    // If backup exists, delete it
                    if (File.Exists(filePath))
                    {
                        string backupPath = filePath + ".old";
                        if (File.Exists(backupPath))
                        {
                            File.Delete(backupPath);
                        }
                        File.Move(filePath, backupPath);
                    }
                    
                    // Move temp file to final location
                    File.Move(tempFile, filePath);
                    
                    if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                    {
                        Debug.Log($"[JsonFileSaveStrategy] Saved to: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonFileSaveStrategy] Save failed: {ex.Message}");
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Loads data from a JSON file
        /// </summary>
        public string Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[JsonFileSaveStrategy] Cannot load with empty key");
                return null;
            }
            
            lock (_fileLock)
            {
                try
                {
                    string filePath = GetFilePath(key);
                    
                    if (!File.Exists(filePath))
                    {
                        // Try to load from backup
                        string backupPath = filePath + ".old";
                        if (File.Exists(backupPath))
                        {
                            Debug.LogWarning($"[JsonFileSaveStrategy] Loading from backup: {backupPath}");
                            return File.ReadAllText(backupPath);
                        }
                        
                        return null;
                    }
                    
                    string data = File.ReadAllText(filePath);
                    
                    if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                    {
                        Debug.Log($"[JsonFileSaveStrategy] Loaded from: {filePath}");
                    }
                    
                    return data;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonFileSaveStrategy] Load failed: {ex.Message}");
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Checks if save file exists
        /// </summary>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            string filePath = GetFilePath(key);
            return File.Exists(filePath) || File.Exists(filePath + ".old");
        }
        
        /// <summary>
        /// Deletes save file
        /// </summary>
        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
                
            lock (_fileLock)
            {
                try
                {
                    string filePath = GetFilePath(key);
                    
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    
                    // Also delete backup
                    string backupPath = filePath + ".old";
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    
                    if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                    {
                        Debug.Log($"[JsonFileSaveStrategy] Deleted: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonFileSaveStrategy] Delete failed: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Gets the full file path for a key
        /// </summary>
        private string GetFilePath(string key)
        {
            // Sanitize key for file system
            string safeKey = SanitizeFileName(key);
            return Path.Combine(_saveDirectory, $"{safeKey}.json");
        }
        
        /// <summary>
        /// Sanitizes a string for use as a file name
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
        
        /// <summary>
        /// Ensures the save directory exists
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }
        
        /// <summary>
        /// Gets the size of a save file in bytes
        /// </summary>
        public long GetFileSize(string key)
        {
            string filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }
            return 0;
        }
        
        /// <summary>
        /// Gets all save files in the directory
        /// </summary>
        public string[] GetAllSaveFiles()
        {
            if (!Directory.Exists(_saveDirectory))
                return new string[0];
                
            return Directory.GetFiles(_saveDirectory, "*.json");
        }
        
        /// <summary>
        /// Clears all save files
        /// </summary>
        public void ClearAllSaves()
        {
            lock (_fileLock)
            {
                try
                {
                    string[] files = GetAllSaveFiles();
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                    
                    Debug.Log($"[JsonFileSaveStrategy] Cleared {files.Length} save files");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonFileSaveStrategy] Clear all failed: {ex.Message}");
                }
            }
        }
    }
}
