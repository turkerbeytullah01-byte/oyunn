using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Represents the result of a risk simulation run
    /// Used for analyzing risk calculations and balancing
    /// </summary>
    [System.Serializable]
    public class RiskSimulationResult
    {
        #region Basic Statistics
        [Header("Basic Statistics")]
        [Tooltip("Total number of simulation runs")]
        public int totalRuns;
        
        [Tooltip("Number of successful outcomes")]
        public int successes;
        
        [Tooltip("Number of failed outcomes")]
        public int failures;
        
        [Tooltip("Success rate as percentage")]
        public float successRate;
        
        [Tooltip("Failure rate as percentage")]
        public float failureRate;
        #endregion

        #region Risk Parameters
        [Header("Risk Parameters")]
        [Tooltip("Technical risk level tested")]
        public RiskLevel testedTechnicalRisk;
        
        [Tooltip("Financial risk level tested")]
        public RiskLevel testedFinancialRisk;
        
        [Tooltip("Security risk level tested")]
        public RiskLevel testedSecurityRisk;
        
        [Tooltip("Reputation value used in simulation")]
        public float testedReputation;
        
        [Tooltip("Technology level used in simulation")]
        public float testedTechLevel;
        
        [Tooltip("Budget used in simulation")]
        public float testedBudget;
        #endregion

        #region Failure Analysis
        [Header("Failure Analysis")]
        [Tooltip("Distribution of failure types")]
        public Dictionary<FailureType, int> failureDistribution;
        
        [Tooltip("Average consequence severity (0-1)")]
        public float averageConsequenceSeverity;
        
        [Tooltip("Maximum consequence severity observed")]
        public float maxConsequenceSeverity;
        
        [Tooltip("Minimum consequence severity observed")]
        public float minConsequenceSeverity;
        
        [Tooltip("Standard deviation of consequence severity")]
        public float consequenceStdDeviation;
        #endregion

        #region Detailed Results
        [Header("Detailed Results")]
        [Tooltip("List of individual simulation results")]
        public List<SingleSimulationResult> individualResults;
        
        [Tooltip("Results grouped by outcome")]
        public Dictionary<SimulationOutcome, List<SingleSimulationResult>> resultsByOutcome;
        
        [Tooltip("Timestamp when simulation completed")]
        public DateTime simulationTimestamp;
        
        [Tooltip("Duration of simulation in seconds")]
        public float simulationDuration;
        #endregion

        #region Risk Metrics
        [Header("Risk Metrics")]
        [Tooltip("Calculated overall risk score (0-100)")]
        public float overallRiskScore;
        
        [Tooltip("Risk-adjusted return on investment")]
        public float riskAdjustedROI;
        
        [Tooltip("Value at Risk (95% confidence)")]
        public float valueAtRisk95;
        
        [Tooltip("Value at Risk (99% confidence)")]
        public float valueAtRisk99;
        
        [Tooltip("Expected monetary value")]
        public float expectedMonetaryValue;
        
        [Tooltip("Maximum potential loss")]
        public float maximumPotentialLoss;
        #endregion

        #region Constructors
        public RiskSimulationResult()
        {
            failureDistribution = new Dictionary<FailureType, int>();
            individualResults = new List<SingleSimulationResult>();
            resultsByOutcome = new Dictionary<SimulationOutcome, List<SingleSimulationResult>>();
            simulationTimestamp = DateTime.Now;
        }
        #endregion

        #region Calculation Methods
        /// <summary>
        /// Calculate all derived statistics
        /// </summary>
        public void CalculateStatistics()
        {
            // Basic rates
            successRate = totalRuns > 0 ? (float)successes / totalRuns * 100f : 0f;
            failureRate = totalRuns > 0 ? (float)failures / totalRuns * 100f : 0f;
            
            // Consequence statistics
            if (individualResults.Count > 0)
            {
                var consequences = individualResults
                    .Where(r => r.outcome == SimulationOutcome.Failure)
                    .Select(r => r.consequenceSeverity)
                    .ToList();
                
                if (consequences.Count > 0)
                {
                    averageConsequenceSeverity = consequences.Average();
                    maxConsequenceSeverity = consequences.Max();
                    minConsequenceSeverity = consequences.Min();
                    
                    // Standard deviation
                    float sumSquaredDiffs = consequences.Sum(c => 
                        (c - averageConsequenceSeverity) * (c - averageConsequenceSeverity));
                    consequenceStdDeviation = Mathf.Sqrt(sumSquaredDiffs / consequences.Count);
                }
            }
            
            // Calculate VaR
            CalculateValueAtRisk();
            
            // Calculate overall risk score
            CalculateOverallRiskScore();
        }

        private void CalculateValueAtRisk()
        {
            if (individualResults.Count == 0) return;
            
            var losses = individualResults
                .Select(r => r.monetaryImpact)
                .Where(m => m < 0)
                .OrderBy(m => m)
                .ToList();
            
            if (losses.Count > 0)
            {
                int var95Index = Mathf.FloorToInt(losses.Count * 0.05f);
                int var99Index = Mathf.FloorToInt(losses.Count * 0.01f);
                
                valueAtRisk95 = Mathf.Abs(losses[Mathf.Clamp(var95Index, 0, losses.Count - 1)]);
                valueAtRisk99 = Mathf.Abs(losses[Mathf.Clamp(var99Index, 0, losses.Count - 1)]);
                maximumPotentialLoss = Mathf.Abs(losses.Min());
            }
            
            // Expected monetary value
            expectedMonetaryValue = individualResults.Average(r => r.monetaryImpact);
        }

        private void CalculateOverallRiskScore()
        {
            // Risk score based on multiple factors
            float failureWeight = 0.4f;
            float severityWeight = 0.3f;
            float varWeight = 0.3f;
            
            float normalizedFailureRate = failureRate / 100f;
            float normalizedSeverity = averageConsequenceSeverity;
            float normalizedVaR = Mathf.Clamp01(valueAtRisk95 / 100000f);
            
            overallRiskScore = (normalizedFailureRate * failureWeight +
                               normalizedSeverity * severityWeight +
                               normalizedVaR * varWeight) * 100f;
        }
        #endregion

        #region Reporting
        /// <summary>
        /// Generate a detailed text report
        /// </summary>
        public string GenerateReport()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== RISK SIMULATION REPORT ===");
            sb.AppendLine($"Generated: {simulationTimestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration: {simulationDuration:F2} seconds");
            sb.AppendLine();
            
            sb.AppendLine("--- PARAMETERS ---");
            sb.AppendLine($"Technical Risk: {testedTechnicalRisk}");
            sb.AppendLine($"Financial Risk: {testedFinancialRisk}");
            sb.AppendLine($"Security Risk: {testedSecurityRisk}");
            sb.AppendLine($"Reputation: {testedReputation:F1}");
            sb.AppendLine($"Tech Level: {testedTechLevel:F1}");
            sb.AppendLine($"Budget: {testedBudget:C}");
            sb.AppendLine();
            
            sb.AppendLine("--- RESULTS ---");
            sb.AppendLine($"Total Runs: {totalRuns}");
            sb.AppendLine($"Successes: {successes} ({successRate:F1}%)");
            sb.AppendLine($"Failures: {failures} ({failureRate:F1}%)");
            sb.AppendLine($"Overall Risk Score: {overallRiskScore:F1}/100");
            sb.AppendLine();
            
            sb.AppendLine("--- FAILURE BREAKDOWN ---");
            foreach (var kvp in failureDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / failures * 100f;
                sb.AppendLine($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            sb.AppendLine();
            
            sb.AppendLine("--- CONSEQUENCE ANALYSIS ---");
            sb.AppendLine($"Average Severity: {averageConsequenceSeverity:F2}");
            sb.AppendLine($"Min Severity: {minConsequenceSeverity:F2}");
            sb.AppendLine($"Max Severity: {maxConsequenceSeverity:F2}");
            sb.AppendLine($"Std Deviation: {consequenceStdDeviation:F2}");
            sb.AppendLine();
            
            sb.AppendLine("--- FINANCIAL METRICS ---");
            sb.AppendLine($"Expected Value: {expectedMonetaryValue:C}");
            sb.AppendLine($"VaR (95%): {valueAtRisk95:C}");
            sb.AppendLine($"VaR (99%): {valueAtRisk99:C}");
            sb.AppendLine($"Max Potential Loss: {maximumPotentialLoss:C}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Generate a summary for display
        /// </summary>
        public string GenerateSummary()
        {
            return $"Success: {successRate:F1}% | Risk Score: {overallRiskScore:F0}/100 | VaR95: {valueAtRisk95:C0}";
        }
        #endregion

        #region Data Export
        /// <summary>
        /// Export results to CSV format
        /// </summary>
        public string ExportToCSV()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Header
            sb.AppendLine("Run,Outcome,FailureType,ConsequenceSeverity,MonetaryImpact,TechnicalRisk,FinancialRisk,SecurityRisk");
            
            // Data
            for (int i = 0; i < individualResults.Count; i++)
            {
                var result = individualResults[i];
                sb.AppendLine($"{i + 1},{result.outcome},{result.failureType},{result.consequenceSeverity:F4},{result.monetaryImpact:F2},{result.technicalRisk},{result.financialRisk},{result.securityRisk}");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Export results to JSON format
        /// </summary>
        public string ExportToJSON()
        {
            return JsonUtility.ToJson(this, true);
        }
        #endregion
    }

    #region Supporting Types
    /// <summary>
    /// Risk levels for simulation
    /// </summary>
    public enum RiskLevel
    {
        None = 0,
        VeryLow = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        VeryHigh = 5,
        Critical = 6
    }

    /// <summary>
    /// Types of failures that can occur
    /// </summary>
    public enum FailureType
    {
        None,
        TechnicalFailure,
        BudgetOverrun,
        ScheduleSlip,
        SecurityBreach,
        QualityIssue,
        RegulatoryViolation,
        MarketRejection,
        SupplyChainFailure,
        TeamDeparture,
        EquipmentFailure,
        DataLoss,
        CompetitorInterference,
        NaturalDisaster,
        LegalChallenge
    }

    /// <summary>
    /// Possible simulation outcomes
    /// </summary>
    public enum SimulationOutcome
    {
        Success,
        PartialSuccess,
        Failure,
        CatastrophicFailure
    }

    /// <summary>
    /// Single simulation run result
    /// </summary>
    [System.Serializable]
    public class SingleSimulationResult
    {
        public SimulationOutcome outcome;
        public FailureType failureType;
        public float consequenceSeverity;
        public float monetaryImpact;
        public float timeImpact;
        public float reputationImpact;
        public RiskLevel technicalRisk;
        public RiskLevel financialRisk;
        public RiskLevel securityRisk;
        public float randomRoll;
        public float calculatedSuccessChance;
        public List<string> contributingFactors;

        public SingleSimulationResult()
        {
            contributingFactors = new List<string>();
        }
    }
    #endregion
}
