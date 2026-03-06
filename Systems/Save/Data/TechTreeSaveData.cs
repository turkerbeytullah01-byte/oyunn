using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Technology tree save data
    /// </summary>
    [Serializable]
    public class TechTreeSaveData
    {
        // Unlocked technology IDs
        public List<string> unlockedTechnologies;
        
        // Progress in each technology category
        // Using serializable list instead of Dictionary for Unity JSON
        public List<CategoryProgressEntry> categoryProgressList;
        
        // Tech points available
        public int availableTechPoints;
        
        // Total tech points earned
        public int totalTechPointsEarned;
        
        // Special unlocks
        public List<string> specialUnlocks;
        
        /// <summary>
        /// Creates default tech tree data for new game
        /// </summary>
        public static TechTreeSaveData CreateDefault()
        {
            return new TechTreeSaveData
            {
                unlockedTechnologies = new List<string>(),
                categoryProgressList = new List<CategoryProgressEntry>(),
                availableTechPoints = 0,
                totalTechPointsEarned = 0,
                specialUnlocks = new List<string>()
            };
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            unlockedTechnologies ??= new List<string>();
            categoryProgressList ??= new List<CategoryProgressEntry>();
            specialUnlocks ??= new List<string>();
            
            availableTechPoints = Mathf.Max(0, availableTechPoints);
            totalTechPointsEarned = Mathf.Max(availableTechPoints, totalTechPointsEarned);
            
            // Clean up null entries
            unlockedTechnologies.RemoveAll(string.IsNullOrEmpty);
            specialUnlocks.RemoveAll(string.IsNullOrEmpty);
            
            // Validate category progress entries
            foreach (var entry in categoryProgressList)
            {
                entry?.Validate();
            }
            
            // Remove invalid entries
            categoryProgressList.RemoveAll(e => e == null || string.IsNullOrEmpty(e.category));
        }
        
        /// <summary>
        /// Gets category progress as dictionary
        /// </summary>
        public Dictionary<string, float> GetCategoryProgressDictionary()
        {
            var dict = new Dictionary<string, float>();
            foreach (var entry in categoryProgressList)
            {
                if (!string.IsNullOrEmpty(entry.category))
                {
                    dict[entry.category] = entry.progress;
                }
            }
            return dict;
        }
        
        /// <summary>
        /// Sets category progress from dictionary
        /// </summary>
        public void SetCategoryProgressFromDictionary(Dictionary<string, float> dict)
        {
            categoryProgressList = new List<CategoryProgressEntry>();
            foreach (var kvp in dict)
            {
                categoryProgressList.Add(new CategoryProgressEntry
                {
                    category = kvp.Key,
                    progress = kvp.Value
                });
            }
        }
        
        /// <summary>
        /// Gets progress for a specific category
        /// </summary>
        public float GetCategoryProgress(string category)
        {
            foreach (var entry in categoryProgressList)
            {
                if (entry.category == category)
                {
                    return entry.progress;
                }
            }
            return 0f;
        }
        
        /// <summary>
        /// Sets progress for a specific category
        /// </summary>
        public void SetCategoryProgress(string category, float progress)
        {
            foreach (var entry in categoryProgressList)
            {
                if (entry.category == category)
                {
                    entry.progress = Mathf.Clamp01(progress);
                    return;
                }
            }
            
            // Add new entry
            categoryProgressList.Add(new CategoryProgressEntry
            {
                category = category,
                progress = Mathf.Clamp01(progress)
            });
        }
        
        /// <summary>
        /// Checks if a technology is unlocked
        /// </summary>
        public bool IsTechnologyUnlocked(string techId)
        {
            return !string.IsNullOrEmpty(techId) && unlockedTechnologies.Contains(techId);
        }
        
        /// <summary>
        /// Unlocks a technology
        /// </summary>
        public void UnlockTechnology(string techId)
        {
            if (!string.IsNullOrEmpty(techId) && !unlockedTechnologies.Contains(techId))
            {
                unlockedTechnologies.Add(techId);
            }
        }
    }
    
    /// <summary>
    /// Serializable category progress entry
    /// </summary>
    [Serializable]
    public class CategoryProgressEntry
    {
        public string category;
        public float progress;
        
        public void Validate()
        {
            progress = Mathf.Clamp01(progress);
        }
    }
}
