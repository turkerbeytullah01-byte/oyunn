using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectAegis.Systems.Save.Data;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Handles save data migration between versions
    /// </summary>
    public static class SaveMigration
    {
        // Migration registry: version -> migration function
        private static readonly Dictionary<string, Func<GameSaveData, GameSaveData>> _migrations = 
            new Dictionary<string, Func<GameSaveData, GameSaveData>>
        {
            { "0.0.0", MigrateFrom_0_0_0_To_1_0_0 },
            { "0.1.0", MigrateFrom_0_1_0_To_1_0_0 },
            { "0.9.0", MigrateFrom_0_9_0_To_1_0_0 },
            // Add more migrations here as versions change
        };
        
        /// <summary>
        /// Migrates save data from one version to current version
        /// </summary>
        /// <param name="oldSave">The save data to migrate</param>
        /// <param name="fromVersion">The version to migrate from</param>
        /// <returns>Migrated save data</returns>
        public static GameSaveData Migrate(GameSaveData oldSave, string fromVersion)
        {
            if (oldSave == null)
            {
                Debug.LogError("[SaveMigration] Cannot migrate null save data");
                return null;
            }
            
            if (string.IsNullOrEmpty(fromVersion))
            {
                fromVersion = "0.0.0";
            }
            
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log($"[SaveMigration] Starting migration from {fromVersion} to {SaveConstants.CURRENT_VERSION}");
            }
            
            // Keep reference to old version for backup
            string originalVersion = fromVersion;
            
            // Create backup before migration
            if (SaveConstants.KEEP_OLD_VERSION_BACKUP)
            {
                CreateVersionBackup(oldSave, fromVersion);
            }
            
            // Apply migrations sequentially
            GameSaveData currentSave = oldSave;
            
            // Get sorted list of versions to migrate through
            List<string> versionsToMigrate = GetVersionsToMigrate(fromVersion);
            
            foreach (string targetVersion in versionsToMigrate)
            {
                if (_migrations.TryGetValue(targetVersion, out var migrationFunc))
                {
                    if (SaveConstants.DEBUG_MIGRATION)
                    {
                        Debug.Log($"[SaveMigration] Applying migration to {targetVersion}");
                    }
                    
                    try
                    {
                        currentSave = migrationFunc(currentSave);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SaveMigration] Migration to {targetVersion} failed: {ex.Message}");
                        // Continue with current state, don't lose progress
                    }
                }
            }
            
            // Always run final validation migration
            currentSave = ApplyFinalMigration(currentSave);
            
            // Update version
            currentSave.version = SaveConstants.CURRENT_VERSION;
            
            // Validate the migrated data
            currentSave.Validate();
            
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log($"[SaveMigration] Migration complete. Updated from {originalVersion} to {SaveConstants.CURRENT_VERSION}");
            }
            
            return currentSave;
        }
        
        /// <summary>
        /// Gets list of versions that need to be migrated through
        /// </summary>
        private static List<string> GetVersionsToMigrate(string fromVersion)
        {
            var versions = new List<string>();
            
            // Parse version numbers
            Version current = ParseVersion(SaveConstants.CURRENT_VERSION);
            Version start = ParseVersion(fromVersion);
            
            // Add all intermediate versions
            foreach (var kvp in _migrations)
            {
                Version migrationVersion = ParseVersion(kvp.Key);
                if (migrationVersion > start && migrationVersion <= current)
                {
                    versions.Add(kvp.Key);
                }
            }
            
            // Sort by version
            versions.Sort((a, b) => ParseVersion(a).CompareTo(ParseVersion(b)));
            
            return versions;
        }
        
        /// <summary>
        /// Parses version string to Version object
        /// </summary>
        private static Version ParseVersion(string versionString)
        {
            try
            {
                return new Version(versionString);
            }
            catch
            {
                return new Version(0, 0, 0);
            }
        }
        
        /// <summary>
        /// Creates a backup of the old version save
        /// </summary>
        private static void CreateVersionBackup(GameSaveData save, string version)
        {
            try
            {
                string backupKey = $"{SaveConstants.BACKUP_KEY}_{version}";
                string json = SerializationHelper.ToJson(save, false);
                
                var prefsStrategy = new PlayerPrefsSaveStrategy();
                prefsStrategy.Save(backupKey, json);
                
                if (SaveConstants.DEBUG_MIGRATION)
                {
                    Debug.Log($"[SaveMigration] Created version backup: {backupKey}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveMigration] Failed to create version backup: {ex.Message}");
            }
        }
        
        #region Specific Migrations
        
        /// <summary>
        /// Migration from 0.0.0 to 1.0.0
        /// Initial migration for very old saves or corrupted saves
        /// </summary>
        private static GameSaveData MigrateFrom_0_0_0_To_1_0_0(GameSaveData save)
        {
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log("[SaveMigration] Applying 0.0.0 -> 1.0.0 migration");
            }
            
            // Ensure all data objects exist
            save.playerData ??= PlayerSaveData.CreateDefault();
            save.researchData ??= ResearchSaveData.CreateDefault();
            save.techTreeData ??= TechTreeSaveData.CreateDefault();
            save.contractsData ??= ContractsSaveData.CreateDefault();
            save.productionData ??= ProductionSaveData.CreateDefault();
            save.eventsData ??= EventsSaveData.CreateDefault();
            save.statisticsData ??= StatisticsSaveData.CreateDefault();
            save.offlineProgressData ??= OfflineProgressData.CreateDefault();
            
            // Add default unlocked items for new players
            if (save.productionData.unlockedDroneModels.Count == 0)
            {
                save.productionData.unlockedDroneModels.Add("basic_scout");
            }
            
            return save;
        }
        
        /// <summary>
        /// Migration from 0.1.0 to 1.0.0
        /// Adds new fields introduced in 1.0.0
        /// </summary>
        private static GameSaveData MigrateFrom_0_1_0_To_1_0_0(GameSaveData save)
        {
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log("[SaveMigration] Applying 0.1.0 -> 1.0.0 migration");
            }
            
            // Add new player settings if not present
            save.playerData.settings ??= PlayerSettingsSaveData.CreateDefault();
            
            // Add new research fields
            save.researchData.researchPoints = save.researchData.researchPoints; // Already exists, ensure initialized
            save.researchData.unlockedCategories ??= new List<string> { "basic" };
            
            // Add new production fields
            save.productionData.globalEfficiencyMultiplier = 
                Mathf.Max(1f, save.productionData.globalEfficiencyMultiplier);
            save.productionData.globalQualityMultiplier = 
                Mathf.Max(1f, save.productionData.globalQualityMultiplier);
            
            // Add new events fields
            save.eventsData.pendingEvents ??= new List<PendingEventSaveData>();
            
            // Add new statistics fields
            save.statisticsData.achievementProgress ??= new List<AchievementProgressEntry>();
            save.statisticsData.completedMilestones ??= new List<string>();
            
            return save;
        }
        
        /// <summary>
        /// Migration from 0.9.0 to 1.0.0
        /// Final beta to release migration
        /// </summary>
        private static GameSaveData MigrateFrom_0_9_0_To_1_0_0(GameSaveData save)
        {
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log("[SaveMigration] Applying 0.9.0 -> 1.0.0 migration");
            }
            
            // Ensure integrity hash is generated
            save.integrityHash = null;
            
            // Reset any beta-specific flags
            // (none for this version)
            
            return save;
        }
        
        #endregion
        
        /// <summary>
        /// Final migration that runs on all saves
        /// Ensures data consistency and adds any missing fields
        /// </summary>
        private static GameSaveData ApplyFinalMigration(GameSaveData save)
        {
            if (SaveConstants.DEBUG_MIGRATION)
            {
                Debug.Log("[SaveMigration] Applying final migration");
            }
            
            // Ensure device ID
            if (string.IsNullOrEmpty(save.deviceId))
            {
                save.deviceId = SystemInfo.deviceUniqueIdentifier;
            }
            
            // Ensure timestamps
            if (string.IsNullOrEmpty(save.saveTimestampSerialized))
            {
                save.saveTimestamp = DateTime.UtcNow;
            }
            
            if (string.IsNullOrEmpty(save.lastLogoutTimeSerialized))
            {
                save.lastLogoutTime = DateTime.UtcNow;
            }
            
            // Ensure statistics
            if (save.statisticsData.firstPlayDate == DateTime.MinValue)
            {
                save.statisticsData.firstPlayDate = DateTime.UtcNow;
            }
            
            // Validate all data structures
            save.playerData?.Validate();
            save.researchData?.Validate();
            save.techTreeData?.Validate();
            save.contractsData?.Validate();
            save.productionData?.Validate();
            save.eventsData?.Validate();
            save.statisticsData?.Validate();
            save.offlineProgressData?.Validate();
            
            return save;
        }
        
        /// <summary>
        /// Attempts to recover a corrupted or partially loaded save
        /// </summary>
        public static GameSaveData AttemptRecovery(GameSaveData corruptedSave)
        {
            Debug.Log("[SaveMigration] Attempting save data recovery");
            
            if (corruptedSave == null)
            {
                // Complete data loss - create new
                Debug.LogWarning("[SaveMigration] Save data completely lost, creating new");
                return GameSaveData.CreateNew();
            }
            
            // Try to preserve what we can
            var recoveredSave = new GameSaveData
            {
                version = SaveConstants.CURRENT_VERSION,
                saveTimestamp = DateTime.UtcNow,
                lastLogoutTime = DateTime.UtcNow,
                deviceId = SystemInfo.deviceUniqueIdentifier,
                
                // Try to preserve player data
                playerData = corruptedSave.playerData ?? PlayerSaveData.CreateDefault(),
                
                // Reset other systems to default but keep what we can
                researchData = corruptedSave.researchData ?? ResearchSaveData.CreateDefault(),
                techTreeData = corruptedSave.techTreeData ?? TechTreeSaveData.CreateDefault(),
                contractsData = corruptedSave.contractsData ?? ContractsSaveData.CreateDefault(),
                productionData = corruptedSave.productionData ?? ProductionSaveData.CreateDefault(),
                eventsData = corruptedSave.eventsData ?? EventsSaveData.CreateDefault(),
                statisticsData = corruptedSave.statisticsData ?? StatisticsSaveData.CreateDefault(),
                offlineProgressData = OfflineProgressData.CreateDefault()
            };
            
            // Validate everything
            recoveredSave.Validate();
            
            Debug.Log("[SaveMigration] Recovery complete");
            return recoveredSave;
        }
        
        /// <summary>
        /// Checks if a version string is valid
        /// </summary>
        public static bool IsValidVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return false;
                
            try
            {
                var v = new Version(version);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Compares two version strings
        /// </summary>
        /// <returns>Negative if v1 < v2, 0 if equal, positive if v1 > v2</returns>
        public static int CompareVersions(string v1, string v2)
        {
            try
            {
                var version1 = new Version(v1);
                var version2 = new Version(v2);
                return version1.CompareTo(version2);
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.Ordinal);
            }
        }
    }
}
