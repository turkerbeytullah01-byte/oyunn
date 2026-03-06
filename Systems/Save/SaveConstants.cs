using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Constants used throughout the save system
    /// </summary>
    public static class SaveConstants
    {
        // Version
        public const string CURRENT_VERSION = "1.0.0";
        
        // Save Keys
        public const string SAVE_KEY = "ProjectAegis_SaveData";
        public const string BACKUP_KEY = "ProjectAegis_SaveBackup";
        public const string VERSION_KEY = "ProjectAegis_Version";
        public const string LAST_SAVE_TIME_KEY = "ProjectAegis_LastSaveTime";
        public const string SAVE_INTEGRITY_KEY = "ProjectAegis_SaveIntegrity";
        
        // Backup Keys
        public const string BACKUP_SAVE_KEY = "ProjectAegis_SaveData_Backup";
        public const string BACKUP_VERSION_KEY = "ProjectAegis_Version_Backup";
        
        // PlayerPrefs Critical Data Keys (for backup)
        public const string CRITICAL_MONEY_KEY = "ProjectAegis_Critical_Money";
        public const string CRITICAL_REPUTATION_KEY = "ProjectAegis_Critical_Reputation";
        public const string CRITICAL_LEVEL_KEY = "ProjectAegis_Critical_Level";
        public const string CRITICAL_COMPANY_NAME_KEY = "ProjectAegis_Critical_CompanyName";
        
        // File Names
        public const string SAVE_FILE_NAME = "save.json";
        public const string BACKUP_FILE_NAME = "save_backup.json";
        public const string EMERGENCY_BACKUP_FILE_NAME = "save_emergency.json";
        
        // Auto-save Settings
        public const float DEFAULT_AUTO_SAVE_INTERVAL_MINUTES = 5f;
        public const float MIN_AUTO_SAVE_INTERVAL_MINUTES = 1f;
        public const float MAX_AUTO_SAVE_INTERVAL_MINUTES = 30f;
        
        // Offline Progress
        public const float MAX_OFFLINE_HOURS = 4f;
        public const float MIN_OFFLINE_MINUTES = 1f;
        
        // Save Limits
        public const int MAX_SAVE_BACKUPS = 3;
        public const int MAX_PLAYERPREFS_SIZE = 1000000; // ~1MB
        
        // Integrity Check
        public const string INTEGRITY_HASH_ALGORITHM = "SHA256";
        
        // Migration
        public const bool ENABLE_AUTOMATIC_MIGRATION = true;
        public const bool KEEP_OLD_VERSION_BACKUP = true;
        
        // Debug
        public const bool DEBUG_SAVE_OPERATIONS = true;
        public const bool DEBUG_MIGRATION = true;
        
        // Retry Settings
        public const int SAVE_RETRY_ATTEMPTS = 3;
        public const float SAVE_RETRY_DELAY_SECONDS = 0.5f;
        
        // Compression
        public const bool COMPRESS_SAVE_DATA = false; // Enable for large saves
        public const int COMPRESSION_THRESHOLD_BYTES = 50000; // Compress if larger than 50KB
    }
}
