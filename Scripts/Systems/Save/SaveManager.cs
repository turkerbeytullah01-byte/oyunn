using System;
using System.Collections;
using UnityEngine;
using ProjectAegis.Systems.Save.Data;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Main save system manager
    /// Handles all save/load operations, backups, and data integrity
    /// </summary>
    public class SaveManager : BaseManager<SaveManager>
    {
        #region Events
        
        /// <summary>
        /// Called when game is saved successfully
        /// </summary>
        public event Action OnGameSaved;
        
        /// <summary>
        /// Called when game is loaded successfully
        /// </summary>
        public event Action<GameSaveData> OnGameLoaded;
        
        /// <summary>
        /// Called when save operation fails
        /// </summary>
        public event Action<string> OnSaveFailed;
        
        /// <summary>
        /// Called when load operation fails
        /// </summary>
        public event Action<string> OnLoadFailed;
        
        #endregion
        
        #region Fields
        
        // Save strategies
        private ISaveStrategy _primaryStrategy;
        private ISaveStrategy _backupStrategy;
        
        // Current save data
        private GameSaveData _currentSaveData;
        
        // Save state
        private bool _isSaving;
        private bool _isLoading;
        private DateTime _lastSaveTime;
        private int _saveAttempts;
        
        // Settings
        [SerializeField] private bool _usePlayerPrefsBackup = true;
        [SerializeField] private bool _verifyIntegrity = true;
        [SerializeField] private int _maxRetryAttempts = SaveConstants.SAVE_RETRY_ATTEMPTS;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the current save data
        /// </summary>
        public GameSaveData CurrentSaveData => _currentSaveData;
        
        /// <summary>
        /// Gets whether a save operation is in progress
        /// </summary>
        public bool IsSaving => _isSaving;
        
        /// <summary>
        /// Gets whether a load operation is in progress
        /// </summary>
        public bool IsLoading => _isLoading;
        
        /// <summary>
        /// Gets the time of the last successful save
        /// </summary>
        public DateTime LastSaveTime => _lastSaveTime;
        
        /// <summary>
        /// Gets time since last save
        /// </summary>
        public TimeSpan TimeSinceLastSave => DateTime.UtcNow - _lastSaveTime;
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // Initialize save strategies
            _primaryStrategy = new JsonFileSaveStrategy();
            _backupStrategy = new PlayerPrefsSaveStrategy();
            
            _lastSaveTime = DateTime.MinValue;
            
            Debug.Log("[SaveManager] Initialized");
        }
        
        #endregion
        
        #region Save Operations
        
        /// <summary>
        /// Saves the current game state
        /// </summary>
        /// <returns>True if save was successful</returns>
        public bool SaveGame()
        {
            if (_isSaving)
            {
                Debug.LogWarning("[SaveManager] Save already in progress");
                return false;
            }
            
            if (_currentSaveData == null)
            {
                Debug.LogError("[SaveManager] Cannot save - no save data exists");
                OnSaveFailed?.Invoke("No save data exists");
                return false;
            }
            
            _isSaving = true;
            bool success = false;
            
            try
            {
                // Prepare save data
                _currentSaveData.PrepareForSave();
                
                // Serialize to JSON
                string json = SerializationHelper.ToJson(_currentSaveData, false);
                
                if (string.IsNullOrEmpty(json) || json == "{}")
                {
                    throw new Exception("Serialization produced empty data");
                }
                
                // Save with primary strategy
                _primaryStrategy.Save(SaveConstants.SAVE_KEY, json);
                
                // Backup to PlayerPrefs if enabled
                if (_usePlayerPrefsBackup)
                {
                    try
                    {
                        _backupStrategy.Save(SaveConstants.BACKUP_KEY, json);
                        
                        // Also save critical data separately
                        if (_backupStrategy is PlayerPrefsSaveStrategy prefsStrategy)
                        {
                            prefsStrategy.SaveCriticalData(
                                _currentSaveData.playerData.money,
                                _currentSaveData.playerData.reputation,
                                _currentSaveData.playerData.playerLevel,
                                _currentSaveData.playerData.companyName
                            );
                        }
                    }
                    catch (Exception backupEx)
                    {
                        Debug.LogWarning($"[SaveManager] Backup save failed: {backupEx.Message}");
                        // Don't fail the whole save if backup fails
                    }
                }
                
                _lastSaveTime = DateTime.UtcNow;
                _saveAttempts = 0;
                success = true;
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[SaveManager] Game saved successfully. Size: {json.Length} bytes");
                }
                
                OnGameSaved?.Invoke();
            }
            catch (Exception ex)
            {
                _saveAttempts++;
                Debug.LogError($"[SaveManager] Save failed (attempt {_saveAttempts}): {ex.Message}");
                
                if (_saveAttempts < _maxRetryAttempts)
                {
                    // Retry
                    _isSaving = false;
                    return SaveGame();
                }
                
                OnSaveFailed?.Invoke(ex.Message);
            }
            finally
            {
                _isSaving = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// Saves game asynchronously (for UI responsiveness)
        /// </summary>
        public void SaveGameAsync(Action<bool> callback = null)
        {
            StartCoroutine(SaveGameAsyncCoroutine(callback));
        }
        
        private IEnumerator SaveGameAsyncCoroutine(Action<bool> callback)
        {
            yield return null; // Wait one frame
            bool result = SaveGame();
            callback?.Invoke(result);
        }
        
        #endregion
        
        #region Load Operations
        
        /// <summary>
        /// Loads the saved game state
        /// </summary>
        /// <returns>The loaded save data, or null if no save exists</returns>
        public GameSaveData LoadGame()
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SaveManager] Load already in progress");
                return null;
            }
            
            _isLoading = true;
            GameSaveData loadedData = null;
            
            try
            {
                string json = null;
                bool loadedFromBackup = false;
                
                // Try primary strategy first
                if (_primaryStrategy.Exists(SaveConstants.SAVE_KEY))
                {
                    json = _primaryStrategy.Load(SaveConstants.SAVE_KEY);
                }
                
                // If primary failed, try backup
                if (string.IsNullOrEmpty(json) && _backupStrategy.Exists(SaveConstants.BACKUP_KEY))
                {
                    Debug.LogWarning("[SaveManager] Loading from backup strategy");
                    json = _backupStrategy.Load(SaveConstants.BACKUP_KEY);
                    loadedFromBackup = true;
                }
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log("[SaveManager] No save data found");
                    _isLoading = false;
                    return null;
                }
                
                // Deserialize
                loadedData = SerializationHelper.FromJson<GameSaveData>(json);
                
                if (loadedData == null)
                {
                    throw new Exception("Deserialization returned null");
                }
                
                // Prepare for load (deserialize DateTimes)
                loadedData.PrepareForLoad();
                
                // Verify integrity if enabled
                if (_verifyIntegrity && !loadedData.VerifyIntegrity())
                {
                    Debug.LogWarning("[SaveManager] Integrity check failed, attempting recovery");
                    
                    // Try to load from backup file
                    if (!loadedFromBackup)
                    {
                        loadedData = TryLoadFromBackupFile();
                    }
                    
                    if (loadedData == null)
                    {
                        throw new Exception("Save data integrity check failed and recovery failed");
                    }
                }
                
                // Handle version migration
                if (loadedData.version != SaveConstants.CURRENT_VERSION)
                {
                    if (SaveConstants.ENABLE_AUTOMATIC_MIGRATION)
                    {
                        Debug.Log($"[SaveManager] Migrating save from {loadedData.version} to {SaveConstants.CURRENT_VERSION}");
                        loadedData = SaveMigration.Migrate(loadedData, loadedData.version);
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveManager] Save version mismatch: {loadedData.version} vs {SaveConstants.CURRENT_VERSION}");
                    }
                }
                
                // Validate data
                loadedData.Validate();
                
                _currentSaveData = loadedData;
                
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[SaveManager] Game loaded successfully. Version: {loadedData.version}");
                }
                
                OnGameLoaded?.Invoke(loadedData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed: {ex.Message}");
                OnLoadFailed?.Invoke(ex.Message);
                loadedData = null;
            }
            finally
            {
                _isLoading = false;
            }
            
            return loadedData;
        }
        
        /// <summary>
        /// Tries to load from backup file
        /// </summary>
        private GameSaveData TryLoadFromBackupFile()
        {
            try
            {
                // The JsonFileSaveStrategy automatically tries .old backup
                string json = _primaryStrategy.Load(SaveConstants.SAVE_KEY);
                
                if (!string.IsNullOrEmpty(json))
                {
                    var data = SerializationHelper.FromJson<GameSaveData>(json);
                    if (data != null)
                    {
                        Debug.Log("[SaveManager] Recovered from backup file");
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Backup file recovery failed: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Loads game asynchronously
        /// </summary>
        public void LoadGameAsync(Action<GameSaveData> callback = null)
        {
            StartCoroutine(LoadGameAsyncCoroutine(callback));
        }
        
        private IEnumerator LoadGameAsyncCoroutine(Action<GameSaveData> callback)
        {
            yield return null; // Wait one frame
            var result = LoadGame();
            callback?.Invoke(result);
        }
        
        #endregion
        
        #region New Game
        
        /// <summary>
        /// Creates a new game save
        /// </summary>
        public GameSaveData CreateNewGame()
        {
            _currentSaveData = GameSaveData.CreateNew();
            
            Debug.Log("[SaveManager] New game created");
            
            return _currentSaveData;
        }
        
        #endregion
        
        #region Backup Operations
        
        /// <summary>
        /// Creates a backup of the current save
        /// </summary>
        public bool CreateBackup()
        {
            if (_currentSaveData == null)
            {
                Debug.LogWarning("[SaveManager] Cannot create backup - no save data");
                return false;
            }
            
            try
            {
                // Serialize current data
                string json = SerializationHelper.ToJson(_currentSaveData, false);
                
                // Save to backup key
                _backupStrategy.Save(SaveConstants.BACKUP_SAVE_KEY, json);
                
                Debug.Log("[SaveManager] Backup created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Backup creation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Restores from backup
        /// </summary>
        public bool RestoreFromBackup()
        {
            try
            {
                if (!_backupStrategy.Exists(SaveConstants.BACKUP_SAVE_KEY))
                {
                    Debug.LogWarning("[SaveManager] No backup exists to restore from");
                    return false;
                }
                
                string json = _backupStrategy.Load(SaveConstants.BACKUP_SAVE_KEY);
                
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("Backup data is empty");
                }
                
                var loadedData = SerializationHelper.FromJson<GameSaveData>(json);
                
                if (loadedData == null)
                {
                    throw new Exception("Failed to deserialize backup");
                }
                
                loadedData.PrepareForLoad();
                loadedData.Validate();
                
                _currentSaveData = loadedData;
                
                // Save restored data as primary
                SaveGame();
                
                Debug.Log("[SaveManager] Restored from backup successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Restore from backup failed: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Delete Operations
        
        /// <summary>
        /// Deletes all save data
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                _primaryStrategy.Delete(SaveConstants.SAVE_KEY);
                _backupStrategy.Delete(SaveConstants.BACKUP_KEY);
                _backupStrategy.Delete(SaveConstants.BACKUP_SAVE_KEY);
                
                if (_backupStrategy is PlayerPrefsSaveStrategy prefsStrategy)
                {
                    prefsStrategy.ClearAllData();
                }
                
                _currentSaveData = null;
                _lastSaveTime = DateTime.MinValue;
                
                Debug.Log("[SaveManager] All save data deleted");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Delete save failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Query Operations
        
        /// <summary>
        /// Checks if save data exists
        /// </summary>
        public bool HasSaveData()
        {
            return _primaryStrategy.Exists(SaveConstants.SAVE_KEY) ||
                   _backupStrategy.Exists(SaveConstants.BACKUP_KEY);
        }
        
        /// <summary>
        /// Gets the save version
        /// </summary>
        public string GetSaveVersion()
        {
            if (_currentSaveData != null)
            {
                return _currentSaveData.version;
            }
            
            // Try to load just the version
            try
            {
                string json = _primaryStrategy.Load(SaveConstants.SAVE_KEY);
                if (string.IsNullOrEmpty(json))
                {
                    json = _backupStrategy.Load(SaveConstants.BACKUP_KEY);
                }
                
                if (!string.IsNullOrEmpty(json))
                {
                    // Quick parse for version field
                    int versionIndex = json.IndexOf("\"version\":");
                    if (versionIndex >= 0)
                    {
                        int start = json.IndexOf('"', versionIndex + 10) + 1;
                        int end = json.IndexOf('"', start);
                        if (start > 0 && end > start)
                        {
                            return json.Substring(start, end - start);
                        }
                    }
                }
            }
            catch { }
            
            return null;
        }
        
        /// <summary>
        /// Checks if save needs migration
        /// </summary>
        public bool NeedsMigration()
        {
            string saveVersion = GetSaveVersion();
            return !string.IsNullOrEmpty(saveVersion) && saveVersion != SaveConstants.CURRENT_VERSION;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets the current save data, creating new if none exists
        /// </summary>
        public GameSaveData GetOrCreateSaveData()
        {
            if (_currentSaveData == null)
            {
                if (HasSaveData())
                {
                    LoadGame();
                }
                else
                {
                    CreateNewGame();
                }
            }
            
            return _currentSaveData;
        }
        
        /// <summary>
        /// Updates the current save data reference
        /// </summary>
        public void SetSaveData(GameSaveData saveData)
        {
            _currentSaveData = saveData;
        }
        
        /// <summary>
        /// Marks logout time in save data
        /// </summary>
        public void MarkLogout()
        {
            _currentSaveData?.MarkLogout();
            SaveGame();
        }
        
        /// <summary>
        /// Forces an immediate save
        /// </summary>
        public void ForceSave()
        {
            SaveGame();
        }
        
        #endregion
        
        #region Application Lifecycle
        
        protected override void OnApplicationPause(bool pause)
        {
            base.OnApplicationPause(pause);
            
            if (pause && _currentSaveData != null)
            {
                // App going to background - save immediately
                Debug.Log("[SaveManager] App paused - saving game");
                MarkLogout();
            }
        }
        
        protected override void OnApplicationFocus(bool focus)
        {
            base.OnApplicationFocus(focus);
            
            if (!focus && _currentSaveData != null)
            {
                // Lost focus - save
                Debug.Log("[SaveManager] App lost focus - saving game");
                SaveGame();
            }
        }
        
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            
            if (_currentSaveData != null)
            {
                // App quitting - save
                Debug.Log("[SaveManager] App quitting - saving game");
                MarkLogout();
            }
        }
        
        #endregion
    }
}
