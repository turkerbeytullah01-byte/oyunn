using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Research
{
    /// <summary>
    /// Risk levels for research projects affecting failure chance and rewards
    /// </summary>
    public enum RiskLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4,
        Critical = 5
    }

    /// <summary>
    /// Technology categories for organizing the research tree
    /// </summary>
    public enum TechCategory
    {
        EnergySystems,
        AISystems,
        Materials,
        Electronics,
        Production,
        Security
    }

    /// <summary>
    /// ScriptableObject definition for a research project.
    /// Contains all static data for a researchable technology.
    /// </summary>
    [CreateAssetMenu(fileName = "ResearchData", menuName = "Project Aegis/Research/Research Data")]
    public class ResearchData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this research")]
        public string researchId;
        
        [Tooltip("Display name shown to players")]
        public string displayName;
        
        [Tooltip("Description of the research and its benefits")]
        [TextArea(3, 5)]
        public string description;

        [Header("Requirements")]
        [Tooltip("Research cost in credits")]
        public float cost;
        
        [Tooltip("Time required to complete in minutes")]
        public float durationMinutes;
        
        [Tooltip("Risk level affecting failure chance")]
        public RiskLevel riskLevel = RiskLevel.Low;

        [Header("Dependencies")]
        [Tooltip("Research IDs that must be completed before this can be started")]
        public List<string> prerequisiteIds = new List<string>();

        [Header("Classification")]
        [Tooltip("Technology category for organization")]
        public TechCategory techCategory = TechCategory.EnergySystems;

        [Header("Unlocks")]
        [Tooltip("Module IDs unlocked by completing this research")]
        public List<string> unlocksModules = new List<string>();
        
        [Tooltip("Research IDs unlocked by completing this research")]
        public List<string> unlocksResearch = new List<string>();

        [Header("Visuals")]
        [Tooltip("Icon for the research in UI")]
        public Sprite icon;

        /// <summary>
        /// Validates the research data
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(researchId) && 
                   !string.IsNullOrEmpty(displayName) &&
                   cost >= 0 &&
                   durationMinutes > 0;
        }

        /// <summary>
        /// Gets the duration as a TimeSpan
        /// </summary>
        public System.TimeSpan GetDuration()
        {
            return System.TimeSpan.FromMinutes(durationMinutes);
        }

        /// <summary>
        /// Gets the failure chance based on risk level (0-1)
        /// </summary>
        public float GetFailureChance()
        {
            return riskLevel switch
            {
                RiskLevel.None => 0f,
                RiskLevel.Low => 0.05f,
                RiskLevel.Medium => 0.10f,
                RiskLevel.High => 0.20f,
                RiskLevel.VeryHigh => 0.35f,
                RiskLevel.Critical => 0.50f,
                _ => 0.05f
            };
        }

        private void OnValidate()
        {
            // Ensure researchId is not empty and is lowercase for consistency
            if (!string.IsNullOrEmpty(researchId))
            {
                researchId = researchId.ToLowerInvariant().Trim().Replace(" ", "_");
            }
        }
    }
}
