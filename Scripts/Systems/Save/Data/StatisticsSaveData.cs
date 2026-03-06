using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Game statistics save data
    /// </summary>
    [Serializable]
    public class StatisticsSaveData
    {
        // Session info
        public string firstPlayDateSerialized;
        public string lastPlayDateSerialized;
        public int totalSessions;
        
        // Time statistics
        public float totalPlayTimeHours;
        public float longestSessionHours;
        public float averageSessionHours;
        
        // Financial statistics
        public float highestMoney;
        public float totalMoneyEarned;
        public float totalMoneySpent;
        public float highestReputation;
        
        // Production statistics
        public int totalDronesProduced;
        public int totalDronesSold;
        public int uniqueDroneModelsUnlocked;
        public int productionLinesBuilt;
        
        // Research statistics
        public int totalResearchCompleted;
        public float totalResearchTimeHours;
        public int uniqueTechnologiesUnlocked;
        
        // Contract statistics
        public int totalContractsAccepted;
        public int totalContractsCompleted;
        public int totalContractsFailed;
        public int totalContractsExpired;
        public float bestContractValue;
        
        // Event statistics
        public int totalEventsTriggered;
        public int totalEventsCompleted;
        public int totalEventsIgnored;
        
        // Achievement tracking
        public List<string> unlockedAchievements;
        public List<AchievementProgressEntry> achievementProgress;
        
        // Milestones
        public List<string> completedMilestones;
        
        // Temporary DateTime fields
        [NonSerialized]
        public DateTime firstPlayDate;
        
        [NonSerialized]
        public DateTime lastPlayDate;
        
        /// <summary>
        /// Creates default statistics data for new game
        /// </summary>
        public static StatisticsSaveData CreateDefault()
        {
            return new StatisticsSaveData
            {
                firstPlayDate = DateTime.UtcNow,
                lastPlayDate = DateTime.UtcNow,
                totalSessions = 0,
                totalPlayTimeHours = 0,
                longestSessionHours = 0,
                averageSessionHours = 0,
                highestMoney = 0,
                totalMoneyEarned = 0,
                totalMoneySpent = 0,
                highestReputation = 0,
                totalDronesProduced = 0,
                totalDronesSold = 0,
                uniqueDroneModelsUnlocked = 0,
                productionLinesBuilt = 0,
                totalResearchCompleted = 0,
                totalResearchTimeHours = 0,
                uniqueTechnologiesUnlocked = 0,
                totalContractsAccepted = 0,
                totalContractsCompleted = 0,
                totalContractsFailed = 0,
                totalContractsExpired = 0,
                bestContractValue = 0,
                totalEventsTriggered = 0,
                totalEventsCompleted = 0,
                totalEventsIgnored = 0,
                unlockedAchievements = new List<string>(),
                achievementProgress = new List<AchievementProgressEntry>(),
                completedMilestones = new List<string>()
            };
        }
        
        /// <summary>
        /// Serializes DateTime fields
        /// </summary>
        public void SerializeDateTimes()
        {
            firstPlayDateSerialized = SerializationHelper.DateTimeToString(firstPlayDate);
            lastPlayDateSerialized = SerializationHelper.DateTimeToString(lastPlayDate);
        }
        
        /// <summary>
        /// Deserializes DateTime fields
        /// </summary>
        public void DeserializeDateTimes()
        {
            firstPlayDate = SerializationHelper.StringToDateTime(firstPlayDateSerialized);
            lastPlayDate = SerializationHelper.StringToDateTime(lastPlayDateSerialized);
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            totalSessions = Mathf.Max(0, totalSessions);
            totalPlayTimeHours = Mathf.Max(0, totalPlayTimeHours);
            longestSessionHours = Mathf.Max(0, longestSessionHours);
            averageSessionHours = Mathf.Max(0, averageSessionHours);
            
            highestMoney = Mathf.Max(0, highestMoney);
            totalMoneyEarned = Mathf.Max(0, totalMoneyEarned);
            totalMoneySpent = Mathf.Max(0, totalMoneySpent);
            highestReputation = Mathf.Max(0, highestReputation);
            
            totalDronesProduced = Mathf.Max(0, totalDronesProduced);
            totalDronesSold = Mathf.Max(0, totalDronesSold);
            uniqueDroneModelsUnlocked = Mathf.Max(0, uniqueDroneModelsUnlocked);
            productionLinesBuilt = Mathf.Max(0, productionLinesBuilt);
            
            totalResearchCompleted = Mathf.Max(0, totalResearchCompleted);
            totalResearchTimeHours = Mathf.Max(0, totalResearchTimeHours);
            uniqueTechnologiesUnlocked = Mathf.Max(0, uniqueTechnologiesUnlocked);
            
            totalContractsAccepted = Mathf.Max(0, totalContractsAccepted);
            totalContractsCompleted = Mathf.Max(0, totalContractsCompleted);
            totalContractsFailed = Mathf.Max(0, totalContractsFailed);
            totalContractsExpired = Mathf.Max(0, totalContractsExpired);
            bestContractValue = Mathf.Max(0, bestContractValue);
            
            totalEventsTriggered = Mathf.Max(0, totalEventsTriggered);
            totalEventsCompleted = Mathf.Max(0, totalEventsCompleted);
            totalEventsIgnored = Mathf.Max(0, totalEventsIgnored);
            
            unlockedAchievements ??= new List<string>();
            achievementProgress ??= new List<AchievementProgressEntry>();
            completedMilestones ??= new List<string>();
            
            // Clean up null entries
            unlockedAchievements.RemoveAll(string.IsNullOrEmpty);
            completedMilestones.RemoveAll(string.IsNullOrEmpty);
            
            // Validate achievement progress
            foreach (var progress in achievementProgress)
            {
                progress?.Validate();
            }
            
            achievementProgress.RemoveAll(p => p == null || string.IsNullOrEmpty(p.achievementId));
            
            // Recalculate average session time
            if (totalSessions > 0)
            {
                averageSessionHours = totalPlayTimeHours / totalSessions;
            }
        }
        
        /// <summary>
        /// Records a new session
        /// </summary>
        public void RecordSession(float sessionHours)
        {
            totalSessions++;
            totalPlayTimeHours += sessionHours;
            
            if (sessionHours > longestSessionHours)
            {
                longestSessionHours = sessionHours;
            }
            
            if (totalSessions > 0)
            {
                averageSessionHours = totalPlayTimeHours / totalSessions;
            }
            
            lastPlayDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Updates highest money if current is higher
        /// </summary>
        public void UpdateHighestMoney(float currentMoney)
        {
            if (currentMoney > highestMoney)
            {
                highestMoney = currentMoney;
            }
        }
        
        /// <summary>
        /// Updates highest reputation if current is higher
        /// </summary>
        public void UpdateHighestReputation(float currentReputation)
        {
            if (currentReputation > highestReputation)
            {
                highestReputation = currentReputation;
            }
        }
        
        /// <summary>
        /// Unlocks an achievement
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!string.IsNullOrEmpty(achievementId) && !unlockedAchievements.Contains(achievementId))
            {
                unlockedAchievements.Add(achievementId);
            }
        }
        
        /// <summary>
        /// Checks if an achievement is unlocked
        /// </summary>
        public bool IsAchievementUnlocked(string achievementId)
        {
            return !string.IsNullOrEmpty(achievementId) && unlockedAchievements.Contains(achievementId);
        }
        
        /// <summary>
        /// Updates achievement progress
        /// </summary>
        public void UpdateAchievementProgress(string achievementId, float progress)
        {
            foreach (var entry in achievementProgress)
            {
                if (entry.achievementId == achievementId)
                {
                    entry.currentProgress = Mathf.Max(entry.currentProgress, progress);
                    return;
                }
            }
            
            // Add new entry
            achievementProgress.Add(new AchievementProgressEntry
            {
                achievementId = achievementId,
                currentProgress = progress,
                targetProgress = 1f
            });
        }
        
        /// <summary>
        /// Completes a milestone
        /// </summary>
        public void CompleteMilestone(string milestoneId)
        {
            if (!string.IsNullOrEmpty(milestoneId) && !completedMilestones.Contains(milestoneId))
            {
                completedMilestones.Add(milestoneId);
            }
        }
        
        /// <summary>
        /// Gets days since first play
        /// </summary>
        public int GetDaysSinceFirstPlay()
        {
            return (int)(DateTime.UtcNow - firstPlayDate).TotalDays;
        }
        
        /// <summary>
        /// Gets contract success rate
        /// </summary>
        public float GetContractSuccessRate()
        {
            int total = totalContractsCompleted + totalContractsFailed + totalContractsExpired;
            if (total <= 0) return 100f;
            return (totalContractsCompleted / (float)total) * 100f;
        }
    }
    
    /// <summary>
    /// Achievement progress entry
    /// </summary>
    [Serializable]
    public class AchievementProgressEntry
    {
        public string achievementId;
        public float currentProgress;
        public float targetProgress;
        public string lastUpdateSerialized;
        
        [NonSerialized]
        public DateTime lastUpdate;
        
        public void SerializeDateTime()
        {
            lastUpdateSerialized = SerializationHelper.DateTimeToString(lastUpdate);
        }
        
        public void DeserializeDateTime()
        {
            lastUpdate = SerializationHelper.StringToDateTime(lastUpdateSerialized);
        }
        
        public void Validate()
        {
            currentProgress = Mathf.Max(0, currentProgress);
            targetProgress = Mathf.Max(1, targetProgress);
        }
        
        public float GetProgressPercentage()
        {
            if (targetProgress <= 0) return 0f;
            return Mathf.Clamp01(currentProgress / targetProgress) * 100f;
        }
        
        public bool IsComplete()
        {
            return currentProgress >= targetProgress;
        }
    }
}
