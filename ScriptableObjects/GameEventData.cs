using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Event type categories
    /// </summary>
    public enum EventCategory
    {
        Positive,   // Good events
        Negative,   // Bad events
        Neutral,    // Informational
        Choice,     // Player decision required
        Emergency   // Urgent response needed
    }

    /// <summary>
    /// ScriptableObject for game events
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvent", menuName = "Project Aegis/Event Data")]
    public class GameEventData : ScriptableObject
    {
        [Header("Basic Info")]
        public string eventId;
        public string displayName;
        [TextArea(3, 6)] public string description;
        [TextArea(2, 4)] public string flavorText;
        public EventCategory category;
        public Sprite eventIcon;
        
        [Header("Trigger Conditions")]
        public float baseTriggerChance;
        public float cooldownSeconds;
        public string[] prerequisiteEvents;
        public string[] blockedByEvents;
        public int minimumReputationLevel;
        public int maximumReputationLevel = 100;
        
        [Header("Timing")]
        public bool isTimed;
        public float responseTimeSeconds;
        
        [Header("Effects - Positive")]
        public float currencyReward;
        public float researchPointsReward;
        public float reputationReward;
        public string[] unlocksTechnologies;
        public string[] unlocksDrones;
        
        [Header("Effects - Negative")]
        public float currencyPenalty;
        public float researchPointsPenalty;
        public float reputationPenalty;
        public float productionDelaySeconds;
        public string[] locksTechnologies;
        
        [Header("Choices")]
        public EventChoice[] choices;
        
        [Header("Visual & Audio")]
        public Color eventColor = Color.white;
        public AudioClip eventSound;
        public GameObject eventEffect;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(eventId))
            {
                eventId = name.ToLower().Replace(" ", "_");
            }
        }
    }

    [System.Serializable]
    public class EventChoice
    {
        public string choiceId;
        public string choiceText;
        [TextArea(2, 4)] public string outcomeDescription;
        
        [Header("Effects")]
        public float currencyEffect;
        public float researchPointsEffect;
        public float reputationEffect;
        public string[] unlocks;
        public string[] triggersEvent;
        
        [Header("Requirements")]
        public float requiredCurrency;
        public string[] requiredTechnologies;
    }
}
