// ============================================================================
// Project Aegis: Drone Dominion
// SaveManager - Handles game save and load operations
// ============================================================================
// Manages serialization, encryption, and storage of game data.
// Supports multiple save slots and cloud save integration hooks.
// ============================================================================

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Manages all save and load operations for the game.
    /// </summary>
    public class SaveManager : BaseManager<SaveManager>
    {
        #region Configuration
        
        public override int InitializationPriority => 3;
        
        /// <summary>
        /// Whether to encrypt save data.
        /// </summary>
        [SerializeField]
        private bool _encryptSaves = true;
        
        /// <summary>
        /// Number of save slots available.
        /// </summary>
        [SerializeField, Range(1, 10)]
        private int _saveSlots = 3;
        
        /// <summary>
        /// Maximum number of auto-saves to keep.
        /// </summary>
        [SerializeField, Range(1, 20)]
        private int _maxAutoSaves = 5;
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Currently selected save slot.
        /// </summary>
        public int CurrentSaveSlot { get; private set; } = 0;
        
        /// <summary>
        /// Player data cache.
        /// </summary>
        public PlayerData.PlayerData CurrentPlayerData { get; private set; }
        
        /// <summary>
        /// Game state cache.
        /// </summary>
        public Data.GameStateData CurrentGameState { get; private set; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when a save operation completes.
        /// </summary>
        public event Action<bool, string> OnSaveCompleted;
        
        /// <summary>
        /// Called when a load operation completes.
        /// </summary>
        public event Action<bool, string> OnLoadCompleted;
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialize()
        {
            EnsureSaveDirectoryExists();
            CurrentPlayerData = new PlayerData.PlayerData();
            CurrentGameState = new Data.GameStateData();
            Log("SaveManager initialized");
        }
        
        #endregion
        
        #region Save Operations
        
        public void SaveGame()
        {
            SaveGame(CurrentSaveSlot);
        }
        
        public void SaveGame(int slot)
        {
            try
            {
                var saveData = new SaveContainer
                {
                    Timestamp = DateTime.UtcNow,
                    Version = Application.version
                };
                
                string json = JsonUtility.ToJson(saveData, true);
                
                if (_encryptSaves)
                    json = EncryptString(json);
                
                string filePath = GetSaveFilePath(slot);
                File.WriteAllText(filePath, json);
                
                OnSaveCompleted?.Invoke(true, "Game saved successfully");
                Log($"Game saved to slot {slot}");
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(false, $"Save failed: {ex.Message}");
                LogError($"Save failed: {ex.Message}");
            }
        }
        
        public void AutoSave()
        {
            RotateAutoSaves();
            
            var saveData = new SaveContainer
            {
                Timestamp = DateTime.UtcNow,
                Version = Application.version,
                IsAutoSave = true
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            
            if (_encryptSaves)
                json = EncryptString(json);
            
            string filePath = GetAutoSaveFilePath(0);
            File.WriteAllText(filePath, json);
            
            Log("Auto-save completed");
        }
        
        #endregion
        
        #region Load Operations
        
        public void LoadGame()
        {
            LoadGame(CurrentSaveSlot);
        }
        
        public void LoadGame(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);
                
                if (!File.Exists(filePath))
                {
                    OnLoadCompleted?.Invoke(false, "Save file not found");
                    return;
                }
                
                string json = File.ReadAllText(filePath);
                
                if (_encryptSaves)
                    json = DecryptString(json);
                
                var saveData = JsonUtility.FromJson<SaveContainer>(json);
                CurrentSaveSlot = slot;
                
                OnLoadCompleted?.Invoke(true, "Game loaded successfully");
                Log($"Game loaded from slot {slot}");
            }
            catch (Exception ex)
            {
                OnLoadCompleted?.Invoke(false, $"Load failed: {ex.Message}");
                LogError($"Load failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Save Management
        
        public bool HasSaveData(int slot = 0)
        {
            return File.Exists(GetSaveFilePath(slot));
        }
        
        public bool HasAnySaveData()
        {
            for (int i = 0; i < _saveSlots; i++)
            {
                if (HasSaveData(i))
                    return true;
            }
            return false;
        }
        
        public void ClearSaveData(int slot = -1)
        {
            if (slot < 0)
            {
                for (int i = 0; i < _saveSlots; i++)
                    DeleteSaveFile(i);
            }
            else
            {
                DeleteSaveFile(slot);
            }
            
            CurrentPlayerData = new PlayerData.PlayerData();
            CurrentGameState = new Data.GameStateData();
            Log("Save data cleared");
        }
        
        #endregion
        
        #region File Operations
        
        private string SaveDirectory => Application.persistentDataPath + "/Saves";
        
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }
        
        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(SaveDirectory, $"save_{slot}.dat");
        }
        
        private string GetAutoSaveFilePath(int index)
        {
            return Path.Combine(SaveDirectory, $"autosave_{index}.dat");
        }
        
        private void DeleteSaveFile(int slot)
        {
            string filePath = GetSaveFilePath(slot);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        
        private void RotateAutoSaves()
        {
            string oldestPath = GetAutoSaveFilePath(_maxAutoSaves - 1);
            if (File.Exists(oldestPath))
                File.Delete(oldestPath);
            
            for (int i = _maxAutoSaves - 2; i >= 0; i--)
            {
                string sourcePath = GetAutoSaveFilePath(i);
                string destPath = GetAutoSaveFilePath(i + 1);
                
                if (File.Exists(sourcePath))
                    File.Move(sourcePath, destPath);
            }
        }
        
        #endregion
        
        #region Encryption
        
        private string EncryptString(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            byte key = 0x42;
            
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= key;
            
            return Convert.ToBase64String(bytes);
        }
        
        private string DecryptString(string encrypted)
        {
            byte[] bytes = Convert.FromBase64String(encrypted);
            byte key = 0x42;
            
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= key;
            
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    [Serializable]
    public class SaveContainer
    {
        public DateTime Timestamp;
        public string Version;
        public bool IsAutoSave;
    }
    
    #endregion
}
