using System;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Player-related save data
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        // Core resources
        public float money;
        public float reputation;
        
        // Company info
        public string companyName;
        public int playerLevel;
        public float experience;
        public float experienceToNextLevel;
        
        // Play time tracking
        public float totalPlayTimeMinutes;
        public float sessionPlayTimeMinutes;
        
        // Premium currency (if applicable)
        public int premiumCurrency;
        
        // Tutorial progress
        public bool tutorialCompleted;
        public int tutorialStep;
        
        // Settings
        public PlayerSettingsSaveData settings;
        
        /// <summary>
        /// Creates default player data for new game
        /// </summary>
        public static PlayerSaveData CreateDefault()
        {
            return new PlayerSaveData
            {
                money = 10000f, // Starting capital
                reputation = 0f,
                companyName = "Aegis Corp",
                playerLevel = 1,
                experience = 0f,
                experienceToNextLevel = 100f,
                totalPlayTimeMinutes = 0f,
                sessionPlayTimeMinutes = 0f,
                premiumCurrency = 0,
                tutorialCompleted = false,
                tutorialStep = 0,
                settings = PlayerSettingsSaveData.CreateDefault()
            };
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            money = Mathf.Max(0, money);
            reputation = Mathf.Clamp(reputation, 0f, 100f);
            playerLevel = Mathf.Max(1, playerLevel);
            experience = Mathf.Max(0, experience);
            experienceToNextLevel = Mathf.Max(1, experienceToNextLevel);
            totalPlayTimeMinutes = Mathf.Max(0, totalPlayTimeMinutes);
            sessionPlayTimeMinutes = Mathf.Max(0, sessionPlayTimeMinutes);
            premiumCurrency = Mathf.Max(0, premiumCurrency);
            tutorialStep = Mathf.Max(0, tutorialStep);
            
            if (string.IsNullOrEmpty(companyName))
            {
                companyName = "Aegis Corp";
            }
            
            settings?.Validate();
        }
    }
    
    /// <summary>
    /// Player settings save data
    /// </summary>
    [Serializable]
    public class PlayerSettingsSaveData
    {
        // Audio
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        
        // Graphics
        public int qualityLevel;
        public bool showParticleEffects;
        
        // Gameplay
        public bool autoSaveEnabled;
        public float autoSaveIntervalMinutes;
        public bool notificationsEnabled;
        public bool offlineProgressEnabled;
        
        // UI
        public string selectedLanguage;
        public bool compactMode;
        
        public static PlayerSettingsSaveData CreateDefault()
        {
            return new PlayerSettingsSaveData
            {
                masterVolume = 1f,
                musicVolume = 0.7f,
                sfxVolume = 0.8f,
                qualityLevel = 2, // Medium
                showParticleEffects = true,
                autoSaveEnabled = true,
                autoSaveIntervalMinutes = SaveConstants.DEFAULT_AUTO_SAVE_INTERVAL_MINUTES,
                notificationsEnabled = true,
                offlineProgressEnabled = true,
                selectedLanguage = "en",
                compactMode = false
            };
        }
        
        public void Validate()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            qualityLevel = Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1);
            autoSaveIntervalMinutes = Mathf.Clamp(autoSaveIntervalMinutes, 
                SaveConstants.MIN_AUTO_SAVE_INTERVAL_MINUTES, 
                SaveConstants.MAX_AUTO_SAVE_INTERVAL_MINUTES);
            
            if (string.IsNullOrEmpty(selectedLanguage))
            {
                selectedLanguage = "en";
            }
        }
    }
}
