using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Example usage of the Risk System
    /// Demonstrates research risk rolls, contract bidding, and mitigation strategies
    /// </summary>
    public class RiskSystemExamples : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool runExamplesOnStart = false;

        private void Start()
        {
            if (runExamplesOnStart)
            {
                RunAllExamples();
            }
        }

        [ContextMenu("Run All Examples")]
        public void RunAllExamples()
        {
            Debug.Log("=== RISK SYSTEM EXAMPLES ===\n");

            Example1_BasicRiskProfile();
            Example2_CalculateFailureChance();
            Example3_RollForSuccess();
            Example4_ResearchRiskRoll();
            Example5_ContractBidEvaluation();
            Example6_RiskMitigation();
            Example7_FailureConsequences();
            Example8_RiskVisualization();
        }

        [ContextMenu("Example 1: Basic Risk Profile")]
        public void Example1_BasicRiskProfile()
        {
            Debug.Log("\n=== EXAMPLE 1: Basic Risk Profile ===\n");

            // Create a risk profile for a research project
            var researchRisk = new RiskProfile
            {
                technicalRisk = 65f,   // Complex technology
                financialRisk = 40f,   // Moderate cost
                securityRisk = 30f,    // Some data sensitivity
                description = "Advanced Drone AI Research",
                sourceContext = "Research"
            };

            Debug.Log("Created Risk Profile:");
            Debug.Log($"  Technical Risk: {researchRisk.technicalRisk}%");
            Debug.Log($"  Financial Risk: {researchRisk.financialRisk}%");
            Debug.Log($"  Security Risk: {researchRisk.securityRisk}%");
            Debug.Log($"  Overall Risk: {researchRisk.GetOverallRisk():F1}%");
            Debug.Log($"  Highest Category: {researchRisk.GetHighestRiskCategory()}");
        }

        [ContextMenu("Example 2: Calculate Failure Chance")]
        public void Example2_CalculateFailureChance()
        {
            Debug.Log("\n=== EXAMPLE 2: Calculate Failure Chance ===\n");

            // Create base risk profile
            var riskProfile = RiskProfile.FromValues(60f, 45f, 35f);

            // Create modifiers (company stats)
            var modifiers = new RiskModifiers
            {
                techLevelReduction = 15f,      // Good tech level
                reputationReduction = 10f,     // Decent reputation
                securityInvestment = 5f,       // Some security investment
                timelineModifier = 5f          // Slight time pressure
            };

            // Calculate failure chance
            float baseChance = riskProfile.GetBaseFailureChance();
            float modifiedChance = RiskCalculator.CalculateFailureChance(riskProfile, modifiers);

            Debug.Log("Risk Calculation:");
            Debug.Log($"  Base Risk: {baseChance:F1}%");
            Debug.Log($"  Total Reduction: {modifiers.GetTotalReduction():F1}%");
            Debug.Log($"  Total Increase: {modifiers.GetTotalIncrease():F1}%");
            Debug.Log($"  Net Modifier: {modifiers.GetNetModifier():F1}%");
            Debug.Log($"  Final Failure Chance: {modifiedChance:F1}%");
            Debug.Log($"  Success Chance: {RiskCalculator.GetSuccessChance(modifiedChance):F1}%");
        }

        [ContextMenu("Example 3: Roll for Success")]
        public void Example3_RollForSuccess()
        {
            Debug.Log("\n=== EXAMPLE 3: Roll for Success ===\n");

            float failureChance = 35f;

            Debug.Log($"Rolling against {failureChance}% failure chance...");

            // Roll multiple times to show distribution
            int successes = 0;
            int failures = 0;

            for (int i = 0; i < 10; i++)
            {
                RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

                if (result.Success)
                {
                    successes++;
                    Debug.Log($"  Roll {i + 1}: SUCCESS (rolled {result.Roll:F1}%)");
                }
                else
                {
                    failures++;
                    Debug.Log($"  Roll {i + 1}: FAILURE (rolled {result.Roll:F1}%)");
                }
            }

            Debug.Log($"\nResults: {successes} successes, {failures} failures");
        }

        [ContextMenu("Example 4: Research Risk Roll")]
        public void Example4_ResearchRiskRoll()
        {
            Debug.Log("\n=== EXAMPLE 4: Research Risk Roll ===\n");

            // Create a research project
            var researchProject = new ResearchProjectData
            {
                Id = "drone_ai_v2",
                Name = "Advanced Drone AI v2.0",
                Description = "Next-generation autonomous navigation system",
                Difficulty = 70f,
                CostRisk = 50f,
                SecurityRequirement = 40f,
                Complexity = 4,
                InnovationLevel = 0.8f,
                BaseCost = 5000f,
                BaseTimeMinutes = 120f
            };

            // Company stats
            float companyTechLevel = 60f;
            float companyReputation = 45f;
            float securityInvestment = 10f;

            // Calculate risk
            var riskProfile = ResearchRiskIntegration.Instance?.CalculateResearchRisk(
                researchProject, companyTechLevel, companyReputation, securityInvestment)
                ?? new RiskProfile
                {
                    technicalRisk = researchProject.Difficulty * 0.8f,
                    financialRisk = researchProject.CostRisk,
                    securityRisk = researchProject.SecurityRequirement
                };

            var modifiers = new RiskModifiers
            {
                techLevelReduction = companyTechLevel * 0.3f,
                reputationReduction = companyReputation * 0.2f,
                securityInvestment = securityInvestment
            };

            float failureChance = RiskCalculator.CalculateResearchFailureChance(riskProfile, modifiers);

            Debug.Log($"Research Project: {researchProject.Name}");
            Debug.Log($"Difficulty: {researchProject.Difficulty}%");
            Debug.Log($"Complexity: {researchProject.Complexity}/5");
            Debug.Log($"Innovation: {researchProject.InnovationLevel:P0}");
            Debug.Log("");
            Debug.Log("Risk Profile:");
            Debug.Log($"  Technical: {riskProfile.technicalRisk:F1}%");
            Debug.Log($"  Financial: {riskProfile.financialRisk:F1}%");
            Debug.Log($"  Security: {riskProfile.securityRisk:F1}%");
            Debug.Log($"  Overall: {riskProfile.GetOverallRisk():F1}%");
            Debug.Log("");
            Debug.Log($"Failure Chance: {failureChance:F1}%");
            Debug.Log($"Success Chance: {100f - failureChance:F1}%");

            // Perform the roll
            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);
            Debug.Log("");
            Debug.Log($"ROLL RESULT: {(result.Success ? "SUCCESS!" : "FAILURE!")}");
            Debug.Log($"  Rolled: {result.Roll:F1}% (needed > {failureChance:F1}%)");
            Debug.Log($"  Margin: {result.Margin:F1}%");
            Debug.Log($"  Critical: {(result.IsCritical ? "Yes" : "No")}");
        }

        [ContextMenu("Example 5: Contract Bid Evaluation")]
        public void Example5_ContractBidEvaluation()
        {
            Debug.Log("\n=== EXAMPLE 5: Contract Bid Evaluation ===\n");

            // Create a contract
            var contract = new ContractData
            {
                Id = "military_patrol_01",
                Name = "Military Patrol Drone Contract",
                Description = "Supply 10 autonomous patrol drones",
                ClientName = "Defense Corp",
                TechRequirement = 75f,
                FinancialRequirement = 60f,
                SecurityRequirement = 80f,
                ClientReputation = 0.7f,
                EstimatedValue = 50000f,
                DeadlineMinutes = 2880f, // 2 days
                PenaltyPerMinute = 10f
            };

            // Bid parameters
            float bidPrice = 45000f;  // Underbid to win
            float estimatedTime = 2500f; // Tight schedule
            float companyTechLevel = 65f;
            float companyReputation = 50f;

            // Calculate bid risk
            var riskProfile = RiskProfile.FromValues(
                contract.TechRequirement * 0.9f,
                contract.FinancialRequirement * 1.1f,
                contract.SecurityRequirement * 1.0f
            );

            // Apply deadline pressure
            float deadlinePressure = Mathf.Max(0, (estimatedTime - contract.DeadlineMinutes) / contract.DeadlineMinutes);
            riskProfile.technicalRisk *= 1f + deadlinePressure * 1.5f;
            riskProfile.financialRisk *= 1f + deadlinePressure * 1.5f;

            // Apply underpricing
            float underpricing = Mathf.Max(0, 1f - bidPrice / contract.EstimatedValue);
            riskProfile.financialRisk *= 1f + underpricing * 1.3f;

            var modifiers = new RiskModifiers
            {
                reputationReduction = companyReputation * 0.25f,
                timelineModifier = deadlinePressure * 20f
            };

            float failureChance = RiskCalculator.CalculateContractFailureChance(riskProfile, modifiers);

            Debug.Log($"Contract: {contract.Name}");
            Debug.Log($"Client: {contract.ClientName}");
            Debug.Log($"Estimated Value: ${contract.EstimatedValue:N0}");
            Debug.Log($"Your Bid: ${bidPrice:N0}");
            Debug.Log($"Estimated Time: {estimatedTime:F0} min");
            Debug.Log($"Deadline: {contract.DeadlineMinutes:F0} min");
            Debug.Log("");
            Debug.Log("Bid Risk Analysis:");
            Debug.Log($"  Deadline Pressure: {deadlinePressure:P0}");
            Debug.Log($"  Underpricing: {underpricing:P0}");
            Debug.Log("");
            Debug.Log($"Technical Risk: {riskProfile.technicalRisk:F1}%");
            Debug.Log($"Financial Risk: {riskProfile.financialRisk:F1}%");
            Debug.Log($"Security Risk: {riskProfile.securityRisk:F1}%");
            Debug.Log("");
            Debug.Log($"FAILURE CHANCE: {failureChance:F1}%");
            Debug.Log($"Recommendation: {(failureChance < 50f ? "BID ACCEPTABLE" : "HIGH RISK - RECONSIDER")}");
        }

        [ContextMenu("Example 6: Risk Mitigation")]
        public void Example6_RiskMitigation()
        {
            Debug.Log("\n=== EXAMPLE 6: Risk Mitigation ===\n");

            // Base risk profile
            var baseRisk = RiskProfile.FromValues(70f, 55f, 60f);
            float baseFailureChance = baseRisk.GetOverallRisk();

            Debug.Log("Base Risk Profile:");
            Debug.Log(RiskDisplay.FormatRiskProfile(baseRisk));
            Debug.Log($"Base Failure Chance: {baseFailureChance:F1}%");
            Debug.Log("");

            // Apply mitigation strategies
            var options = new MitigationOptions
            {
                SecurityInvestment = 5000f,
                HireConsultant = true,
                ConsultantCost = 3000f,
                ConsultantSpecialization = RiskCategory.Technical,
                TimelineExtensionPercent = 20f,
                QualityInvestment = 4000f,
                ProjectValue = 50000f
            };

            var result = RiskMitigation.ApplyCombinedMitigation(baseRisk, options);

            Debug.Log("Mitigation Applied:");
            Debug.Log($"  Security Investment: ${options.SecurityInvestment:N0} → {result.SecurityReduction:F1}% reduction");
            Debug.Log($"  Consultant: ${options.ConsultantCost:N0} → {result.ConsultantReduction:F1}% reduction");
            Debug.Log($"  Timeline Extension: {options.TimelineExtensionPercent}% → {result.TimelineReduction:F1}% reduction");
            Debug.Log($"  Quality Investment: ${options.QualityInvestment:N0} → {result.QualityReduction:F1}% reduction");
            Debug.Log("");
            Debug.Log($"Total Cost: ${result.TotalCost:N0}");
            Debug.Log($"Total Risk Reduction: {result.TotalRiskReduction:F1}%");
            Debug.Log("");

            float mitigatedFailureChance = result.FinalProfile.GetOverallRisk();
            Debug.Log("Mitigated Risk Profile:");
            Debug.Log(RiskDisplay.FormatRiskProfile(result.FinalProfile));
            Debug.Log($"New Failure Chance: {mitigatedFailureChance:F1}%");
            Debug.Log($"Improvement: {baseFailureChance - mitigatedFailureChance:F1}%");
        }

        [ContextMenu("Example 7: Failure Consequences")]
        public void Example7_FailureConsequences()
        {
            Debug.Log("\n=== EXAMPLE 7: Failure Consequences ===\n");

            RiskLevel riskLevel = RiskLevel.High;

            Debug.Log($"Showing consequences for {RiskProfile.RiskLevelToString(riskLevel)} risk level:\n");

            foreach (FailureType type in System.Enum.GetValues(typeof(FailureType)))
            {
                FailureResult consequence = FailureConsequences.GetConsequences(type, riskLevel);

                Debug.Log($"--- {type} ---");
                Debug.Log($"  Money Lost: ${consequence.moneyLost:N0}");
                Debug.Log($"  Recovery Cost: ${consequence.recoveryCost:N0}");
                Debug.Log($"  Reputation Lost: {consequence.reputationLost:F1}");
                Debug.Log($"  Time Delay: {consequence.TimeDelayFormatted}");
                Debug.Log($"  Progress Lost: {consequence.progressLost * 100:F0}%");
                Debug.Log($"  Recoverable: {(consequence.isRecoverable ? "Yes" : "No")}");
                Debug.Log("");
            }
        }

        [ContextMenu("Example 8: Risk Visualization")]
        public void Example8_RiskVisualization()
        {
            Debug.Log("\n=== EXAMPLE 8: Risk Visualization ===\n");

            // Show risk bars for different risk levels
            Debug.Log("Risk Bars:");
            for (int i = 0; i <= 100; i += 20)
            {
                string bar = RiskDisplay.GetRiskBar(i);
                string colorHex = RiskDisplay.GetRiskColorHex(i);
                string label = RiskDisplay.GetRiskLabel(i);
                Debug.Log($"  {i,3}% {bar} - {label}");
            }

            Debug.Log("");

            // Show multi-category risk
            var profile = RiskProfile.FromValues(75f, 45f, 60f);
            Debug.Log("Multi-Category Risk Display:");
            Debug.Log(RiskDisplay.GetMultiCategoryRiskBar(profile));

            Debug.Log("");

            // Show risk advice
            Debug.Log("Risk Advice:");
            Debug.Log(RiskDisplay.GetBriefRiskAdvice(profile));
        }

        [ContextMenu("Full Research Flow")]
        public void FullResearchFlow()
        {
            Debug.Log("\n=== FULL RESEARCH FLOW EXAMPLE ===\n");

            // Step 1: Define research
            var research = new ResearchProjectData
            {
                Id = "stealth_tech",
                Name = "Stealth Coating Technology",
                Difficulty = 80f,
                CostRisk = 60f,
                SecurityRequirement = 70f,
                Complexity = 5,
                InnovationLevel = 0.9f,
                BaseCost = 10000f,
                BaseTimeMinutes = 240f
            };

            // Step 2: Calculate initial risk
            var riskProfile = RiskProfile.FromValues(
                research.Difficulty * 0.9f,
                research.CostRisk,
                research.SecurityRequirement * 1.1f
            );

            float initialFailureChance = riskProfile.GetOverallRisk();

            Debug.Log($"Research: {research.Name}");
            Debug.Log($"Initial Failure Chance: {initialFailureChance:F1}%");
            Debug.Log(RiskDisplay.FormatRiskProfile(riskProfile));
            Debug.Log("");

            // Step 3: Show mitigation options
            Debug.Log("Recommended Mitigation:");
            Debug.Log(RiskDisplay.GetRiskAdvice(riskProfile));
            Debug.Log("");

            // Step 4: Apply mitigation
            var mitigation = new MitigationOptions
            {
                SecurityInvestment = 8000f,
                HireConsultant = true,
                ConsultantCost = 5000f,
                QualityInvestment = 6000f
            };

            var mitigatedResult = RiskMitigation.ApplyCombinedMitigation(riskProfile, mitigation);
            float mitigatedFailureChance = mitigatedResult.FinalProfile.GetOverallRisk();

            Debug.Log($"After Mitigation (Cost: ${mitigatedResult.TotalCost:N0}):");
            Debug.Log($"New Failure Chance: {mitigatedFailureChance:F1}%");
            Debug.Log("");

            // Step 5: Roll for success
            RiskRollResult roll = RiskCalculator.RollDetailed(mitigatedFailureChance);

            Debug.Log("ROLLING...");
            Debug.Log($"Need to roll > {mitigatedFailureChance:F1}%");
            Debug.Log($"Rolled: {roll.Roll:F1}%");
            Debug.Log($"Result: {(roll.Success ? "SUCCESS!" : "FAILURE")}");

            if (!roll.Success)
            {
                Debug.Log("");
                FailureResult failure = FailureConsequences.GetResearchConsequences(
                    FailureType.PartialFailure,
                    RiskProfile.ValueToRiskLevel(mitigatedFailureChance),
                    research.BaseCost,
                    research.BaseTimeMinutes
                );
                Debug.Log("Consequences:");
                Debug.Log(failure.GetSummary());
            }
        }
    }
}
