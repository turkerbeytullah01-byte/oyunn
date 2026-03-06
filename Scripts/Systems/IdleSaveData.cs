using System;
using System.Collections.Generic;

namespace ProjectAegis.Systems.Idle
{
    /// <summary>
    /// Complete save data for the idle system
    /// </summary>
    [Serializable]
    public class IdleSaveData
    {
        // Version for migration support
        public int version = 1;
        
        // Timestamps
        public DateTime lastLogoutTime;
        public DateTime lastSaveTime;
        public DateTime firstInstallTime;
        
        // Session tracking
        public int totalSessions;
        public float totalPlayTimeMinutes;
        public float totalOfflineTimeMinutes;
        
        // Production
        public List<ProductionLineSaveData> productionLines;
        public float globalProductionMultiplier;
        public float premiumMultiplier;
        
        // Research progress at logout
        public Dictionary<string, ResearchProgressData> researchProgressAtLogout;
        
        // Partial progress values
        public Dictionary<string, float> partialProgressValues;
        
        // Anti-cheat data
        public int detectedRollbackCount;
        public DateTime? firstRollbackTime;
        public bool wasFlaggedForCheating;
        
        // Settings
        public bool offlineProgressEnabled = true;
        public float maxOfflineHours = 4f;
        
        // Statistics
        public float totalOfflineEarnings;
        public int offlineSessionsCount;
        public DateTime? lastOfflineCalculation;

        public IdleSaveData()
        {
            productionLines = new List<ProductionLineSaveData>();
            researchProgressAtLogout = new Dictionary<string, ResearchProgressData>();
            partialProgressValues = new Dictionary<string, float>();
            lastLogoutTime = DateTime.MinValue;
            lastSaveTime = DateTime.MinValue;
            firstInstallTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Research progress data for saving
    /// </summary>
    [Serializable]
    public class ResearchProgressData
    {
        public string researchId;
        public float progress; // 0-1
        public float timeInvestedMinutes;
        public DateTime startTime;
        public bool isPaused;
        public float baseResearchRate; // progress per minute
    }

    /// <summary>
    /// Login reward data
    /// </summary>
    [Serializable]
    public class LoginRewardData
    {
        public DateTime lastLoginTime;
        public int consecutiveLogins;
        public int totalLogins;
        public Dictionary<int, DateTime> claimedRewards; // day -> claim time
        
        public LoginRewardData()
        {
            claimedRewards = new Dictionary<int, DateTime>();
        }
    }

    /// <summary>
    /// Offline calculation snapshot for debugging
    /// </summary>
    [Serializable]
    public class OfflineCalculationSnapshot
    {
        public DateTime logoutTime;
        public DateTime loginTime;
        public TimeSpan calculatedDuration;
        public TimeSpan cappedDuration;
        public float moneyEarned;
        public int completedResearchCount;
        public bool wasCapped;
        public bool rollbackDetected;
        public string errorMessage;
    }
}
