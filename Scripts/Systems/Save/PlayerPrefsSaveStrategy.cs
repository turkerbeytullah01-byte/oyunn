using System;
using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// PlayerPrefs-based save strategy
    /// Used for critical data backup and small saves
    /// Limited by PlayerPrefs size (~1MB on most platforms)
    /// </summary>
    public class PlayerPrefsSaveStrategy : ISaveStrategy
    {
        public string StorageType => "PlayerPrefs";
        
        public bool IsAvailable => true; // Always available in Unity
        
        private const int MAX_KEY_LENGTH = 250; // PlayerPrefs key length limit
        private const int MAX_VALUE_LENGTH = 1000000; // Approximate value limit
        
        /// <summary>
        /// Saves data to PlayerPrefs
        /// </summary>
        public void Save(string key, string data)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PlayerPrefsSaveStrategy] Cannot save with empty key");
                return;
            }
            
            if (data == null)
            {
                Debug.LogError("[PlayerPrefsSaveStrategy] Cannot save null data");
                return;
            }
            
            try
            {
                // Check key length
                if (key.Length > MAX_KEY_LENGTH)
                {
                    Debug.LogWarning($"[PlayerPrefsSaveStrategy] Key too long, truncating: {key}");
                    key = key.Substring(0, MAX_KEY_LENGTH);
                }
                
                // Check data size
                if (data.Length > MAX_VALUE_LENGTH)
                {
                    Debug.LogError($"[PlayerPrefsSaveStrategy] Data too large for PlayerPrefs: {data.Length} bytes");
                }
                
                PlayerPrefs.SetString(key, data);
                
                // Also save a hash for integrity
                string hashKey = key + "_hash";
                string hash = SerializationHelper.GenerateHash(data);
                PlayerPrefs.SetString(hashKey, hash);
                
                // Save timestamp
                string timeKey = key + "_time";
                PlayerPrefs.SetString(timeKey, DateTime.UtcNow.ToString("O"));
                
                // Immediate save
                bool saved = PlayerPrefs.Save();
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[PlayerPrefsSaveStrategy] Saved to key: {key}, Size: {data.Length} bytes, Success: {saved}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Save failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Loads data from PlayerPrefs
        /// </summary>
        public string Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PlayerPrefsSaveStrategy] Cannot load with empty key");
                return null;
            }
            
            try
            {
                // Check key length
                if (key.Length > MAX_KEY_LENGTH)
                {
                    key = key.Substring(0, MAX_KEY_LENGTH);
                }
                
                if (!PlayerPrefs.HasKey(key))
                {
                    return null;
                }
                
                string data = PlayerPrefs.GetString(key);
                
                // Verify integrity if hash exists
                string hashKey = key + "_hash";
                if (PlayerPrefs.HasKey(hashKey))
                {
                    string storedHash = PlayerPrefs.GetString(hashKey);
                    string actualHash = SerializationHelper.GenerateHash(data);
                    
                    if (!storedHash.Equals(actualHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning("[PlayerPrefsSaveStrategy] Data integrity check failed!");
                    }
                }
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[PlayerPrefsSaveStrategy] Loaded from key: {key}, Size: {data?.Length ?? 0} bytes");
                }
                
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Load failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Checks if data exists in PlayerPrefs
        /// </summary>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            if (key.Length > MAX_KEY_LENGTH)
            {
                key = key.Substring(0, MAX_KEY_LENGTH);
            }
            
            return PlayerPrefs.HasKey(key);
        }
        
        /// <summary>
        /// Deletes data from PlayerPrefs
        /// </summary>
        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
                
            try
            {
                if (key.Length > MAX_KEY_LENGTH)
                {
                    key = key.Substring(0, MAX_KEY_LENGTH);
                }
                
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.DeleteKey(key + "_hash");
                PlayerPrefs.DeleteKey(key + "_time");
                
                PlayerPrefs.Save();
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[PlayerPrefsSaveStrategy] Deleted key: {key}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Delete failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Saves critical player data separately for redundancy
        /// </summary>
        public void SaveCriticalData(float money, float reputation, int level, string companyName)
        {
            try
            {
                PlayerPrefs.SetFloat(SaveConstants.CRITICAL_MONEY_KEY, money);
                PlayerPrefs.SetFloat(SaveConstants.CRITICAL_REPUTATION_KEY, reputation);
                PlayerPrefs.SetInt(SaveConstants.CRITICAL_LEVEL_KEY, level);
                PlayerPrefs.SetString(SaveConstants.CRITICAL_COMPANY_NAME_KEY, companyName ?? "Aegis Corp");
                PlayerPrefs.SetString(SaveConstants.LAST_SAVE_TIME_KEY, DateTime.UtcNow.ToString("O"));
                
                PlayerPrefs.Save();
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log("[PlayerPrefsSaveStrategy] Critical data saved");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Critical save failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Loads critical player data
        /// </summary>
        public (float money, float reputation, int level, string companyName, DateTime? lastSaveTime) LoadCriticalData()
        {
            try
            {
                float money = PlayerPrefs.GetFloat(SaveConstants.CRITICAL_MONEY_KEY, 0f);
                float reputation = PlayerPrefs.GetFloat(SaveConstants.CRITICAL_REPUTATION_KEY, 0f);
                int level = PlayerPrefs.GetInt(SaveConstants.CRITICAL_LEVEL_KEY, 1);
                string companyName = PlayerPrefs.GetString(SaveConstants.CRITICAL_COMPANY_NAME_KEY, "Aegis Corp");
                
                DateTime? lastSaveTime = null;
                string timeStr = PlayerPrefs.GetString(SaveConstants.LAST_SAVE_TIME_KEY, "");
                if (!string.IsNullOrEmpty(timeStr))
                {
                    if (DateTime.TryParse(timeStr, out DateTime parsedTime))
                    {
                        lastSaveTime = parsedTime;
                    }
                }
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log("[PlayerPrefsSaveStrategy] Critical data loaded");
                }
                
                return (money, reputation, level, companyName, lastSaveTime);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Critical load failed: {ex.Message}");
                return (0f, 0f, 1, "Aegis Corp", null);
            }
        }
        
        /// <summary>
        /// Clears all PlayerPrefs data for this game
        /// </summary>
        public void ClearAllData()
        {
            try
            {
                PlayerPrefs.DeleteKey(SaveConstants.SAVE_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.SAVE_KEY + "_hash");
                PlayerPrefs.DeleteKey(SaveConstants.SAVE_KEY + "_time");
                PlayerPrefs.DeleteKey(SaveConstants.BACKUP_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.BACKUP_KEY + "_hash");
                PlayerPrefs.DeleteKey(SaveConstants.VERSION_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.CRITICAL_MONEY_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.CRITICAL_REPUTATION_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.CRITICAL_LEVEL_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.CRITICAL_COMPANY_NAME_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.LAST_SAVE_TIME_KEY);
                PlayerPrefs.DeleteKey(SaveConstants.SAVE_INTEGRITY_KEY);
                
                PlayerPrefs.Save();
                
                Debug.Log("[PlayerPrefsSaveStrategy] All data cleared");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerPrefsSaveStrategy] Clear all failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets the last save timestamp
        /// </summary>
        public DateTime? GetLastSaveTime(string key)
        {
            string timeKey = key + "_time";
            if (PlayerPrefs.HasKey(timeKey))
            {
                string timeStr = PlayerPrefs.GetString(timeKey);
                if (DateTime.TryParse(timeStr, out DateTime time))
                {
                    return time;
                }
            }
            return null;
        }
    }
}
