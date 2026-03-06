using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Main controller for the Risk System
    /// Handles risk calculations, failure determination, and consequence application
    /// </summary>
    public class RiskManager : MonoBehaviour
    {
        public static RiskManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableRiskSystem = true;
        [SerializeField] private bool logRiskRolls = true;
        [SerializeField] private bool enableCriticalFailures = true;

        [Header("Tracking")]
        [SerializeField] private int consecutiveSuccesses = 0;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private int totalProjectsCompleted = 0;
        [SerializeField] private int totalProjectsFailed = 0;

        [Header("Events")]
        public Action<RiskRollResult> OnRiskRollCompleted;
        public Action<FailureResult> OnFailureOccurred;
        public Action<string, float> OnSuccessAchieved;
        public Action<RiskProfile, float> OnRiskCalculated;

        // Success streak bonus (capped at 15%)
        private const float MaxSuccessBonus = 15f;
        private const float SuccessBonusPerStreak = 3f;

        // Failure penalty (capped at 10%)
        private const float MaxFailurePenalty = 10f;
        private const float FailurePenaltyPerStreak = 2f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #region Risk Calculation

        /// <summary>
        /// Calculates the final failure chance for a risk profile with modifiers
        /// </summary>
        public float CalculateFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (!enableRiskSystem) return 0f;

            // Add streak bonuses/penalties to modifiers
            modifiers = ApplyStreakModifiers(modifiers);

            float failureChance = RiskCalculator.CalculateFailureChance(profile, modifiers);

            OnRiskCalculated?.Invoke(profile, failureChance);

            return failureChance;
        }

        /// <summary>
        /// Calculates failure chance specifically for research projects
        /// </summary>
        public float CalculateResearchFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (!enableRiskSystem) return 0f;

            modifiers = ApplyStreakModifiers(modifiers);
            return RiskCalculator.CalculateResearchFailureChance(profile, modifiers);
        }

        /// <summary>
        /// Calculates failure chance specifically for contracts
        /// </summary>
        public float CalculateContractFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (!enableRiskSystem) return 0f;

            modifiers = ApplyStreakModifiers(modifiers);
            return RiskCalculator.CalculateContractFailureChance(profile, modifiers);
        }

        /// <summary>
        /// Gets a mitigated risk profile by applying modifiers
        /// </summary>
        public RiskProfile GetMitigatedRisk(RiskProfile baseProfile, RiskModifiers modifiers)
        {
            if (baseProfile == null) return new RiskProfile();

            RiskProfile mitigated = baseProfile.Clone();

            float reduction = modifiers?.GetTotalReduction() ?? 0f;
            float increase = modifiers?.GetTotalIncrease() ?? 0f;
            float netEffect = reduction - increase;

            // Apply net effect proportionally to each risk category
            float factor = 1f - (netEffect / 100f);
            factor = Mathf.Clamp(factor, 0.1f, 2f);

            mitigated.technicalRisk *= factor;
            mitigated.financialRisk *= factor;
            mitigated.securityRisk *= factor;

            return mitigated;
        }

        #endregion

        #region Risk Rolling

        /// <summary>
        /// Rolls for success against a failure chance
        /// Returns true if successful
        /// </summary>
        public bool RollForSuccess(float failureChance)
        {
            if (!enableRiskSystem) return true;

            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

            if (logRiskRolls)
            {
                Debug.Log($"[RiskManager] {result}");
            }

            OnRiskRollCompleted?.Invoke(result);

            if (result.Success)
            {
                consecutiveSuccesses++;
                consecutiveFailures = 0;
            }
            else
            {
                consecutiveFailures++;
                consecutiveSuccesses = 0;
            }

            return result.Success;
        }

        /// <summary>
        /// Performs a detailed risk roll with full result information
        /// </summary>
        public RiskRollResult RollDetailed(RiskProfile profile, RiskModifiers modifiers)
        {
            float failureChance = CalculateFailureChance(profile, modifiers);
            return RiskCalculator.RollDetailed(failureChance);
        }

        /// <summary>
        /// Rolls for research project completion
        /// </summary>
        public RiskRollResult RollForResearch(RiskProfile profile, RiskModifiers modifiers, string researchId)
        {
            float failureChance = CalculateResearchFailureChance(profile, modifiers);
            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

            if (logRiskRolls)
            {
                Debug.Log($"[RiskManager] Research '{researchId}': {result}");
            }

            OnRiskRollCompleted?.Invoke(result);

            if (result.Success)
            {
                ApplySuccessBonuses("research", researchId);
            }
            else
            {
                FailureResult failure = GenerateFailureResult(profile, result, "research");
                ApplyFailureConsequences(failure);
            }

            return result;
        }

        /// <summary>
        /// Rolls for contract delivery
        /// </summary>
        public RiskRollResult RollForContract(RiskProfile profile, RiskModifiers modifiers, string contractId)
        {
            float failureChance = CalculateContractFailureChance(profile, modifiers);
            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

            if (logRiskRolls)
            {
                Debug.Log($"[RiskManager] Contract '{contractId}': {result}");
            }

            OnRiskRollCompleted?.Invoke(result);

            if (result.Success)
            {
                ApplySuccessBonuses("contract", contractId);
            }
            else
            {
                FailureResult failure = GenerateFailureResult(profile, result, "contract");
                ApplyFailureConsequences(failure);
            }

            return result;
        }

        #endregion

        #region Success Handling

        /// <summary>
        /// Applies bonuses for successful completion
        /// </summary>
        public void ApplySuccessBonuses(string context, string id = "")
        {
            consecutiveSuccesses++;
            consecutiveFailures = 0;
            totalProjectsCompleted++;

            float reputationGain = CalculateReputationGain(context);
            OnSuccessAchieved?.Invoke(context, reputationGain);

            if (logRiskRolls)
            {
                Debug.Log($"[RiskManager] Success! Context: {context}, Reputation Gain: +{reputationGain:F1}");
            }
        }

        /// <summary>
        /// Calculates reputation gain from success
        /// </summary>
        private float CalculateReputationGain(string context)
        {
            float baseGain = context.ToLower() switch
            {
                "research" => 2f,
                "contract" => 5f,
                "operation" => 1f,
                _ => 1f
            };

            // Bonus for consecutive successes
            float streakBonus = Mathf.Min(consecutiveSuccesses * 0.5f, 5f);

            return baseGain + streakBonus;
        }

        #endregion

        #region Failure Handling

        /// <summary>
        /// Generates a failure result based on roll and context
        /// </summary>
        private FailureResult GenerateFailureResult(RiskProfile profile, RiskRollResult roll, string context)
        {
            consecutiveFailures++;
            consecutiveSuccesses = 0;
            totalProjectsFailed++;

            // Determine failure type based on how badly the roll failed
            FailureType type = DetermineFailureType(roll, profile);

            // Get base consequences
            FailureResult result = FailureConsequences.GetConsequences(type, RiskProfile.ValueToRiskLevel(profile.GetOverallRisk()));

            // Scale based on context
            result = ScaleFailureForContext(result, context);

            return result;
        }

        /// <summary>
        /// Determines the type of failure based on roll margin and risk level
        /// </summary>
        private FailureType DetermineFailureType(RiskRollResult roll, RiskProfile profile)
        {
            float riskLevel = profile.GetOverallRisk();
            float failureMargin = roll.FailureChance - roll.Roll;

            // Critical failures only if enabled and very bad roll
            if (enableCriticalFailures && failureMargin > 30f && riskLevel > 75f)
                return FailureType.Catastrophic;

            if (failureMargin > 20f || riskLevel > 85f)
                return FailureType.MajorFailure;

            if (failureMargin > 10f || riskLevel > 60f)
                return FailureType.PartialFailure;

            if (failureMargin > 5f || riskLevel > 40f)
                return FailureType.CostOverrun;

            if (failureMargin > 2f || riskLevel > 25f)
                return FailureType.Delay;

            return FailureType.MinorSetback;
        }

        /// <summary>
        /// Scales failure consequences based on context
        /// </summary>
        private FailureResult ScaleFailureForContext(FailureResult result, string context)
        {
            float multiplier = context.ToLower() switch
            {
                "research" => 0.8f,  // Research failures are less severe
                "contract" => 1.2f,  // Contract failures are more severe
                "operation" => 1.0f,
                _ => 1.0f
            };

            result.moneyLost *= multiplier;
            result.reputationLost *= multiplier;
            result.timeDelayMinutes *= multiplier;

            return result;
        }

        /// <summary>
        /// Applies failure consequences to the game state
        /// </summary>
        public void ApplyFailureConsequences(FailureResult failure)
        {
            OnFailureOccurred?.Invoke(failure);

            if (logRiskRolls)
            {
                Debug.Log($"[RiskManager] Failure Applied: {failure}");
            }
        }

        #endregion

        #region Streak Modifiers

        /// <summary>
        /// Applies streak-based modifiers to risk calculations
        /// </summary>
        private RiskModifiers ApplyStreakModifiers(RiskModifiers modifiers)
        {
            if (modifiers == null) modifiers = RiskModifiers.Default;

            RiskModifiers modified = modifiers.Clone();

            // Success streak bonus (reduces failure chance)
            if (consecutiveSuccesses > 0)
            {
                float successBonus = Mathf.Min(consecutiveSuccesses * SuccessBonusPerStreak, MaxSuccessBonus);
                modified.previousSuccessBonus = successBonus;
            }

            // Failure streak penalty (increases failure chance)
            if (consecutiveFailures > 0)
            {
                float failurePenalty = Mathf.Min(consecutiveFailures * FailurePenaltyPerStreak, MaxFailurePenalty);
                modified.recentFailurePenalty = failurePenalty;
            }

            return modified;
        }

        /// <summary>
        /// Resets the success/failure streaks
        /// </summary>
        public void ResetStreaks()
        {
            consecutiveSuccesses = 0;
            consecutiveFailures = 0;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the current success streak
        /// </summary>
        public int GetSuccessStreak() => consecutiveSuccesses;

        /// <summary>
        /// Gets the current failure streak
        /// </summary>
        public int GetFailureStreak() => consecutiveFailures;

        /// <summary>
        /// Gets total completed projects
        /// </summary>
        public int GetTotalCompleted() => totalProjectsCompleted;

        /// <summary>
        /// Gets total failed projects
        /// </summary>
        public int GetTotalFailed() => totalProjectsFailed;

        /// <summary>
        /// Gets success rate as percentage
        /// </summary>
        public float GetSuccessRate()
        {
            int total = totalProjectsCompleted + totalProjectsFailed;
            if (total == 0) return 100f;
            return (totalProjectsCompleted / (float)total) * 100f;
        }

        /// <summary>
        /// Gets risk color for UI display
        /// </summary>
        public Color GetRiskColor(float risk)
        {
            return RiskDisplay.GetRiskColor(risk);
        }

        /// <summary>
        /// Gets risk description
        /// </summary>
        public string GetRiskDescription(RiskLevel level)
        {
            return RiskDisplay.GetRiskDescription(level);
        }

        /// <summary>
        /// Gets advice for a risk profile
        /// </summary>
        public string GetRiskAdvice(RiskProfile profile)
        {
            return RiskDisplay.GetRiskAdvice(profile);
        }

        /// <summary>
        /// Checks if the system is enabled
        /// </summary>
        public bool IsEnabled() => enableRiskSystem;

        /// <summary>
        /// Enables or disables the risk system
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableRiskSystem = enabled;
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for the risk manager
        /// </summary>
        public RiskManagerSaveData GetSaveData()
        {
            return new RiskManagerSaveData
            {
                ConsecutiveSuccesses = consecutiveSuccesses,
                ConsecutiveFailures = consecutiveFailures,
                TotalProjectsCompleted = totalProjectsCompleted,
                TotalProjectsFailed = totalProjectsFailed,
                EnableRiskSystem = enableRiskSystem
            };
        }

        /// <summary>
        /// Loads save data into the risk manager
        /// </summary>
        public void LoadSaveData(RiskManagerSaveData data)
        {
            consecutiveSuccesses = data.ConsecutiveSuccesses;
            consecutiveFailures = data.ConsecutiveFailures;
            totalProjectsCompleted = data.TotalProjectsCompleted;
            totalProjectsFailed = data.TotalProjectsFailed;
            enableRiskSystem = data.EnableRiskSystem;
        }

        #endregion
    }

    /// <summary>
    /// Save data for RiskManager
    /// </summary>
    [Serializable]
    public struct RiskManagerSaveData
    {
        public int ConsecutiveSuccesses;
        public int ConsecutiveFailures;
        public int TotalProjectsCompleted;
        public int TotalProjectsFailed;
        public bool EnableRiskSystem;
    }
}
