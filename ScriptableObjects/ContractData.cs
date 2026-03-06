using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Contract difficulty levels
    /// </summary>
    public enum ContractDifficulty
    {
        Easy,
        Medium,
        Hard,
        Elite
    }

    /// <summary>
    /// Contract type categories
    /// </summary>
    public enum ContractType
    {
        Surveillance,
        Patrol,
        Reconnaissance,
        Escort,
        Defense,
        Emergency
    }

    /// <summary>
    /// ScriptableObject for contract definitions
    /// </summary>
    [CreateAssetMenu(fileName = "NewContract", menuName = "Project Aegis/Contract Data")]
    public class ContractData : ScriptableObject
    {
        [Header("Basic Info")]
        public string contractId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public ContractType contractType;
        public ContractDifficulty difficulty;
        public Sprite contractIcon;
        
        [Header("Requirements")]
        public string[] requiredDroneTypes;
        public int requiredDroneCount;
        public int minimumReputationLevel;
        public string[] requiredTechnologies;
        
        [Header("Duration & Timing")]
        public float durationSeconds;
        public bool hasDeadline;
        public float deadlineSeconds;
        
        [Header("Rewards")]
        public float baseReward;
        public float reputationReward;
        public float researchPointsReward;
        public string[] bonusUnlocks;
        
        [Header("Penalties")]
        public float failurePenalty;
        public float reputationLossOnFail;
        
        [Header("Success Conditions")]
        public float minimumSuccessChance;
        public StatRequirement[] statRequirements;
        
        [Header("Visual")]
        public Color contractColor = Color.white;
        public GameObject contractEffect;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(contractId))
            {
                contractId = name.ToLower().Replace(" ", "_");
            }
        }
    }

    [System.Serializable]
    public class StatRequirement
    {
        public string statName;
        public float minimumValue;
    }
}
