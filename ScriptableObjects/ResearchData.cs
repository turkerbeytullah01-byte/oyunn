using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Research category types
    /// </summary>
    public enum ResearchCategory
    {
        Energy,
        Propulsion,
        Sensors,
        Materials,
        AI,
        Weapons
    }

    /// <summary>
    /// ScriptableObject for research projects
    /// </summary>
    [CreateAssetMenu(fileName = "NewResearch", menuName = "Project Aegis/Research Data")]
    public class ResearchData : ScriptableObject
    {
        [Header("Basic Info")]
        public string researchId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public ResearchCategory category;
        public Sprite icon;
        
        [Header("Requirements")]
        public string[] prerequisiteResearch;
        public int requiredReputationLevel;
        
        [Header("Research Cost")]
        public float researchPointsRequired;
        public float moneyRequired;
        public float researchTimeSeconds;
        
        [Header("Rewards")]
        public string[] unlocksTechnologies;
        public string[] unlocksDrones;
        public StatModifier[] statModifiers;
        
        [Header("Visual")]
        public Color researchColor = Color.blue;
        public GameObject researchEffect;
        
        [Header("Audio")]
        public AudioClip researchCompleteSound;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(researchId))
            {
                researchId = name.ToLower().Replace(" ", "_");
            }
        }
    }

    [System.Serializable]
    public class StatModifier
    {
        public string statName;
        public float modifierValue;
        public ModifierType modifierType;
    }

    public enum ModifierType
    {
        Additive,
        Multiplicative
    }
}
