using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Factory class for creating the 6 MVP game events.
    /// These events can be created in Unity using the CreateAssetMenu.
    /// </summary>
    public static class MVPEvents
    {
        /// <summary>
        /// Creates the "Eureka Moment" event - Research breakthrough that reduces current research time
        /// </summary>
        public static GameEventData CreateEurekaMoment()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "eureka_moment";
            evt.displayName = "Eureka Moment!";
            evt.description = "One of your researchers has a sudden breakthrough! Their insight reduces the remaining time on the current research project.";
            evt.notificationText = "Research breakthrough!";
            evt.eventType = EventType.ResearchBreakthrough;
            evt.rarity = EventRarity.Uncommon;
            evt.weight = 15f;
            evt.minIntervalMinutes = 15f;
            evt.maxIntervalMinutes = 25f;
            evt.cooldownMinutes = 20f;
            evt.isDecisionEvent = false;
            
            evt.effects = new List<EventEffect>
            {
                new EventEffect
                {
                    effectType = EffectType.ResearchTimeReduction,
                    value = 5f, // Reduce by 5 minutes
                    durationMinutes = 0, // Instant effect
                    canStack = true,
                    maxStackCount = 3
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates the "Investor Confidence" event - Temporary production speed boost
        /// </summary>
        public static GameEventData CreateInvestorConfidence()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "investor_confidence";
            evt.displayName = "Investor Confidence";
            evt.description = "Positive market news has boosted investor confidence in your company. Production efficiency increases temporarily.";
            evt.notificationText = "Production boosted!";
            evt.eventType = EventType.FundingBoost;
            evt.rarity = EventRarity.Common;
            evt.weight = 20f;
            evt.minIntervalMinutes = 12f;
            evt.maxIntervalMinutes = 18f;
            evt.cooldownMinutes = 15f;
            evt.isDecisionEvent = false;
            
            evt.effects = new List<EventEffect>
            {
                new EventEffect
                {
                    effectType = EffectType.ProductionSpeedBoost,
                    value = 0.10f, // +10% speed
                    durationMinutes = 10f, // 10 minutes
                    canStack = false,
                    maxStackCount = 1
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates the "Security Breach Attempt" event - Decision event with choices
        /// </summary>
        public static GameEventData CreateSecurityBreachAttempt()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "security_breach_attempt";
            evt.displayName = "Security Breach Attempt";
            evt.description = "Your security team has detected an attempted breach of your research servers. The attackers haven't accessed any data yet, but they're persistent.";
            evt.notificationText = "Security alert!";
            evt.eventType = EventType.SecurityAlert;
            evt.rarity = EventRarity.Rare;
            evt.weight = 8f;
            evt.minIntervalMinutes = 20f;
            evt.maxIntervalMinutes = 30f;
            evt.cooldownMinutes = 45f;
            evt.isDecisionEvent = true;
            
            evt.choices = new List<EventChoice>
            {
                new EventChoice
                {
                    choiceId = "hire_security",
                    displayText = "Hire Emergency Security Team",
                    description = "Spend money to bring in external security experts to handle the threat.",
                    outcomes = new List<EventEffect>
                    {
                        new EventEffect
                        {
                            effectType = EffectType.MoneyBonus,
                            value = -5000f, // Costs money
                            durationMinutes = 0
                        },
                        new EventEffect
                        {
                            effectType = EffectType.ReputationChange,
                            value = 5f, // Gain reputation for handling well
                            durationMinutes = 0
                        }
                    }
                },
                new EventChoice
                {
                    choiceId = "handle_internally",
                    displayText = "Handle Internally",
                    description = "Use your existing team to counter the threat. Riskier but costs nothing.",
                    outcomes = new List<EventEffect>
                    {
                        new EventEffect
                        {
                            effectType = EffectType.ReputationChange,
                            value = -10f, // Lose reputation if it goes wrong
                            durationMinutes = 0
                        },
                        new EventEffect
                        {
                            effectType = EffectType.ResearchTimeReduction,
                            value = -2f, // Lose some research progress
                            durationMinutes = 0
                        }
                    }
                },
                new EventChoice
                {
                    choiceId = "ignore_threat",
                    displayText = "Ignore the Threat",
                    description = "Your firewall should hold... probably.",
                    outcomes = new List<EventEffect>
                    {
                        new EventEffect
                        {
                            effectType = EffectType.ReputationChange,
                            value = -25f, // Major reputation hit
                            durationMinutes = 0
                        },
                        new EventEffect
                        {
                            effectType = EffectType.MoneyBonus,
                            value = -10000f, // Potential data loss costs
                            durationMinutes = 0
                        }
                    }
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates the "Design Flaw Discovered" event - Improves prototype test success
        /// </summary>
        public static GameEventData CreateDesignFlawDiscovered()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "design_flaw_discovered";
            evt.displayName = "Design Flaw Discovered";
            evt.description = "A careful review of your prototype design has revealed a critical flaw before testing. Fixing it now will significantly improve the chances of a successful test.";
            evt.notificationText = "Prototype optimized!";
            evt.eventType = EventType.PrototypeOptimization;
            evt.rarity = EventRarity.Uncommon;
            evt.weight = 12f;
            evt.minIntervalMinutes = 18f;
            evt.maxIntervalMinutes = 25f;
            evt.cooldownMinutes = 30f;
            evt.isDecisionEvent = false;
            
            evt.effects = new List<EventEffect>
            {
                new EventEffect
                {
                    effectType = EffectType.TestSuccessBoost,
                    value = 0.15f, // +15% test success chance
                    durationMinutes = 0, // Applies to next test only
                    targetId = "next_test",
                    canStack = true,
                    maxStackCount = 2
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates the "Emergency Contract" event - Temporary high-value contract opportunity
        /// </summary>
        public static GameEventData CreateEmergencyContract()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "emergency_contract";
            evt.displayName = "Emergency Contract";
            evt.description = "A government agency needs a rush delivery of drone components for an urgent operation. They're offering premium rates for fast completion.";
            evt.notificationText = "High-value contract available!";
            evt.eventType = EventType.MarketOpportunity;
            evt.rarity = EventRarity.Rare;
            evt.weight = 10f;
            evt.minIntervalMinutes = 25f;
            evt.maxIntervalMinutes = 35f;
            evt.cooldownMinutes = 40f;
            evt.isDecisionEvent = false;
            
            evt.effects = new List<EventEffect>
            {
                new EventEffect
                {
                    effectType = EffectType.ContractValueBoost,
                    value = 0.50f, // +50% contract value
                    durationMinutes = 15f, // 15 minutes to accept and complete
                    canStack = false,
                    maxStackCount = 1
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates the "Power Surge" event - Negative event that causes setbacks
        /// </summary>
        public static GameEventData CreatePowerSurge()
        {
            var evt = ScriptableObject.CreateInstance<GameEventData>();
            
            evt.eventId = "power_surge";
            evt.displayName = "Power Surge";
            evt.description = "A power surge in your research facility has corrupted some data. The backup systems prevented total loss, but some progress was rolled back.";
            evt.notificationText = "Equipment malfunction!";
            evt.eventType = EventType.EquipmentMalfunction;
            evt.rarity = EventRarity.Common;
            evt.weight = 15f;
            evt.minIntervalMinutes = 15f;
            evt.maxIntervalMinutes = 22f;
            evt.cooldownMinutes = 25f;
            evt.isDecisionEvent = false;
            
            evt.effects = new List<EventEffect>
            {
                new EventEffect
                {
                    effectType = EffectType.ResearchTimeReduction,
                    value = -2f, // Lose 2 minutes of progress (negative = add time)
                    durationMinutes = 0,
                    canStack = true,
                    maxStackCount = 3
                }
            };
            
            return evt;
        }
        
        /// <summary>
        /// Creates all 6 MVP events as a list
        /// </summary>
        public static List<GameEventData> CreateAllMVPEvents()
        {
            return new List<GameEventData>
            {
                CreateEurekaMoment(),
                CreateInvestorConfidence(),
                CreateSecurityBreachAttempt(),
                CreateDesignFlawDiscovered(),
                CreateEmergencyContract(),
                CreatePowerSurge()
            };
        }
    }
    
    #region Unity Editor Event Creators
    
    #if UNITY_EDITOR
    
    using UnityEditor;
    
    /// <summary>
    /// Editor utility for creating MVP event assets
    /// </summary>
    public static class MVPEventAssetCreator
    {
        [MenuItem("Project Aegis/Events/Create All MVP Events")]
        public static void CreateAllMVPEventAssets()
        {
            string path = "Assets/ProjectAegis/Events/";
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            // Create Eureka Moment
            CreateEventAsset(MVPEvents.CreateEurekaMoment(), path + "EurekaMoment.asset");
            
            // Create Investor Confidence
            CreateEventAsset(MVPEvents.CreateInvestorConfidence(), path + "InvestorConfidence.asset");
            
            // Create Security Breach Attempt
            CreateEventAsset(MVPEvents.CreateSecurityBreachAttempt(), path + "SecurityBreachAttempt.asset");
            
            // Create Design Flaw Discovered
            CreateEventAsset(MVPEvents.CreateDesignFlawDiscovered(), path + "DesignFlawDiscovered.asset");
            
            // Create Emergency Contract
            CreateEventAsset(MVPEvents.CreateEmergencyContract(), path + "EmergencyContract.asset");
            
            // Create Power Surge
            CreateEventAsset(MVPEvents.CreatePowerSurge(), path + "PowerSurge.asset");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Created all 6 MVP event assets at: " + path);
        }
        
        [MenuItem("Project Aegis/Events/Create Eureka Moment")]
        public static void CreateEurekaMomentAsset()
        {
            CreateEventAsset(MVPEvents.CreateEurekaMoment(), "Assets/ProjectAegis/Events/EurekaMoment.asset");
        }
        
        [MenuItem("Project Aegis/Events/Create Investor Confidence")]
        public static void CreateInvestorConfidenceAsset()
        {
            CreateEventAsset(MVPEvents.CreateInvestorConfidence(), "Assets/ProjectAegis/Events/InvestorConfidence.asset");
        }
        
        [MenuItem("Project Aegis/Events/Create Security Breach Attempt")]
        public static void CreateSecurityBreachAttemptAsset()
        {
            CreateEventAsset(MVPEvents.CreateSecurityBreachAttempt(), "Assets/ProjectAegis/Events/SecurityBreachAttempt.asset");
        }
        
        [MenuItem("Project Aegis/Events/Create Design Flaw Discovered")]
        public static void CreateDesignFlawDiscoveredAsset()
        {
            CreateEventAsset(MVPEvents.CreateDesignFlawDiscovered(), "Assets/ProjectAegis/Events/DesignFlawDiscovered.asset");
        }
        
        [MenuItem("Project Aegis/Events/Create Emergency Contract")]
        public static void CreateEmergencyContractAsset()
        {
            CreateEventAsset(MVPEvents.CreateEmergencyContract(), "Assets/ProjectAegis/Events/EmergencyContract.asset");
        }
        
        [MenuItem("Project Aegis/Events/Create Power Surge")]
        public static void CreatePowerSurgeAsset()
        {
            CreateEventAsset(MVPEvents.CreatePowerSurge(), "Assets/ProjectAegis/Events/PowerSurge.asset");
        }
        
        private static void CreateEventAsset(GameEventData evt, string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(evt, path);
            EditorUtility.SetDirty(evt);
            Debug.Log($"Created event asset: {path}");
        }
    }
    
    #endif
    
    #endregion
}
