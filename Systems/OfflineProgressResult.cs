using System;
using System.Collections.Generic;

namespace ProjectAegis.Systems.Idle
{
    /// <summary>
    /// Represents the calculated progress gained while the player was offline.
    /// This is the result of the idle calculation and contains all resources,
    /// research progress, and completion states.
    /// </summary>
    [Serializable]
    public class OfflineProgressResult
    {
        /// <summary>
        /// Total time the player was offline (before capping)
        /// </summary>
        public TimeSpan offlineDuration;
        
        /// <summary>
        /// Actual duration used for calculations (capped at max offline time)
        /// </summary>
        public float cappedDurationMinutes;
        
        /// <summary>
        /// Research progress added during offline time (researchId → progress added 0-1)
        /// </summary>
        public Dictionary<string, float> researchProgress;
        
        /// <summary>
        /// Total money earned during offline time
        /// </summary>
        public float moneyEarned;
        
        /// <summary>
        /// List of research IDs that were completed during offline time
        /// </summary>
        public List<string> completedResearches;
        
        /// <summary>
        /// Timestamp when this calculation was performed
        /// </summary>
        public DateTime calculationTime;
        
        /// <summary>
        /// Whether time rollback was detected (potential cheating)
        /// </summary>
        public bool timeRollbackDetected;
        
        /// <summary>
        /// Whether the offline time was capped
        /// </summary>
        public bool wasCapped;
        
        /// <summary>
        /// Original logout time used for calculation
        /// </summary>
        public DateTime logoutTime;
        
        /// <summary>
        /// Login time when calculation was performed
        /// </summary>
        public DateTime loginTime;
        
        /// <summary>
        /// Production line contributions (lineId → money earned)
        /// </summary>
        public Dictionary<string, float> productionContributions;
        
        /// <summary>
        /// Number of completed researches that triggered notifications
        /// </summary>
        public int notificationCount;

        public OfflineProgressResult()
        {
            researchProgress = new Dictionary<string, float>();
            completedResearches = new List<string>();
            productionContributions = new Dictionary<string, float>();
            calculationTime = DateTime.UtcNow;
            timeRollbackDetected = false;
            wasCapped = false;
            notificationCount = 0;
        }

        /// <summary>
        /// Gets a formatted string of the offline duration for display
        /// </summary>
        public string GetFormattedDuration()
        {
            if (offlineDuration.TotalHours >= 1)
            {
                return $"{offlineDuration.TotalHours:F1} hours";
            }
            return $"{offlineDuration.TotalMinutes:F0} minutes";
        }

        /// <summary>
        /// Gets a formatted string of the capped duration for display
        /// </summary>
        public string GetFormattedCappedDuration()
        {
            if (wasCapped)
            {
                return $"{cappedDurationMinutes:F0} minutes (capped)";
            }
            return $"{cappedDurationMinutes:F0} minutes";
        }

        /// <summary>
        /// Returns true if any meaningful progress was made
        /// </summary>
        public bool HasProgress()
        {
            return moneyEarned > 0 || completedResearches.Count > 0 || researchProgress.Count > 0;
        }

        /// <summary>
        /// Gets a summary of all completed researches
        /// </summary>
        public string GetCompletedResearchesSummary()
        {
            if (completedResearches.Count == 0)
                return "None";
            
            if (completedResearches.Count == 1)
                return completedResearches[0];
            
            return $"{completedResearches.Count} researches completed";
        }

        /// <summary>
        /// Creates an empty result for when offline progress is disabled
        /// </summary>
        public static OfflineProgressResult CreateEmpty()
        {
            return new OfflineProgressResult
            {
                offlineDuration = TimeSpan.Zero,
                cappedDurationMinutes = 0,
                moneyEarned = 0,
                wasCapped = false
            };
        }

        /// <summary>
        /// Creates a result when time rollback is detected
        /// </summary>
        public static OfflineProgressResult CreateRollbackDetected(DateTime logout, DateTime login)
        {
            return new OfflineProgressResult
            {
                offlineDuration = TimeSpan.Zero,
                cappedDurationMinutes = 0,
                moneyEarned = 0,
                timeRollbackDetected = true,
                logoutTime = logout,
                loginTime = login,
                wasCapped = true
            };
        }
    }
}
