using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Research system save data
    /// </summary>
    [Serializable]
    public class ResearchSaveData
    {
        // Completed research projects
        public List<string> completedResearchIds;
        
        // Currently active research
        public ActiveResearchSaveData activeResearch;
        
        // Research queue (for batch research)
        public List<string> researchQueue;
        
        // Research points accumulated
        public float researchPoints;
        
        // Research speed modifiers
        public float researchSpeedMultiplier;
        
        // Unlocked research categories
        public List<string> unlockedCategories;
        
        /// <summary>
        /// Creates default research data for new game
        /// </summary>
        public static ResearchSaveData CreateDefault()
        {
            return new ResearchSaveData
            {
                completedResearchIds = new List<string>(),
                activeResearch = null,
                researchQueue = new List<string>(),
                researchPoints = 0f,
                researchSpeedMultiplier = 1f,
                unlockedCategories = new List<string> { "basic" }
            };
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            completedResearchIds ??= new List<string>();
            researchQueue ??= new List<string>();
            unlockedCategories ??= new List<string> { "basic" };
            
            researchPoints = Mathf.Max(0, researchPoints);
            researchSpeedMultiplier = Mathf.Max(0.1f, researchSpeedMultiplier);
            
            activeResearch?.Validate();
            
            // Clean up null entries
            completedResearchIds.RemoveAll(string.IsNullOrEmpty);
            researchQueue.RemoveAll(string.IsNullOrEmpty);
        }
        
        /// <summary>
        /// Checks if a research is completed
        /// </summary>
        public bool IsResearchCompleted(string researchId)
        {
            return !string.IsNullOrEmpty(researchId) && completedResearchIds.Contains(researchId);
        }
        
        /// <summary>
        /// Adds a completed research
        /// </summary>
        public void AddCompletedResearch(string researchId)
        {
            if (!string.IsNullOrEmpty(researchId) && !completedResearchIds.Contains(researchId))
            {
                completedResearchIds.Add(researchId);
            }
        }
    }
    
    /// <summary>
    /// Active research save data
    /// </summary>
    [Serializable]
    public class ActiveResearchSaveData
    {
        // Research ID
        public string researchId;
        
        // Progress (0-1)
        public float progress;
        
        // When research started
        public string startTimeSerialized;
        
        // Is research paused
        public bool isPaused;
        
        // Total research time in seconds
        public float totalResearchTime;
        
        // Time spent on research so far
        public float elapsedTime;
        
        // Researcher assignments (if applicable)
        public int assignedResearchers;
        
        // Temporary field for DateTime conversion
        [NonSerialized]
        public DateTime startTime;
        
        /// <summary>
        /// Serializes DateTime to string
        /// </summary>
        public void SerializeDateTime()
        {
            startTimeSerialized = SerializationHelper.DateTimeToString(startTime);
        }
        
        /// <summary>
        /// Deserializes DateTime from string
        /// </summary>
        public void DeserializeDateTime()
        {
            startTime = SerializationHelper.StringToDateTime(startTimeSerialized);
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            progress = Mathf.Clamp01(progress);
            totalResearchTime = Mathf.Max(1f, totalResearchTime);
            elapsedTime = Mathf.Max(0, elapsedTime);
            assignedResearchers = Mathf.Max(0, assignedResearchers);
            
            if (string.IsNullOrEmpty(researchId))
            {
                // Invalid active research
                progress = 0;
            }
        }
    }
}
