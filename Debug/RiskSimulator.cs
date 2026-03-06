using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Risk simulator for testing risk calculations and probabilities
    /// Performs Monte Carlo simulations to analyze risk scenarios
    /// </summary>
    public class RiskSimulator : MonoBehaviour
    {
        #region Singleton
        private static RiskSimulator _instance;
        public static RiskSimulator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RiskSimulator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RiskSimulator");
                        _instance = go.AddComponent<RiskSimulator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown technicalRiskDropdown;
        [SerializeField] private TMP_Dropdown financialRiskDropdown;
        [SerializeField] private TMP_Dropdown securityRiskDropdown;
        [SerializeField] private TMP_InputField reputationInput;
        [SerializeField] private TMP_InputField techLevelInput;
        [SerializeField] private TMP_InputField budgetInput;
        [SerializeField] private TMP_InputField simulationRunsInput;
        [SerializeField] private Button runSimulationButton;
        [SerializeField] private Button runSingleTestButton;
        [SerializeField] private Button exportResultsButton;
        [SerializeField] private Button clearResultsButton;
        [SerializeField] private TextMeshProUGUI resultsText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private ScrollRect resultsScrollRect;
        #endregion

        #region Test Parameters
        [Header("Test Parameters")]
        [Tooltip("Technical risk level for simulation")]
        public RiskLevel testTechnicalRisk = RiskLevel.Medium;
        
        [Tooltip("Financial risk level for simulation")]
        public RiskLevel testFinancialRisk = RiskLevel.Medium;
        
        [Tooltip("Security risk level for simulation")]
        public RiskLevel testSecurityRisk = RiskLevel.Medium;
        
        [Tooltip("Reputation value (0-100)")]
        [Range(0f, 100f)]
        public float testReputation = 50f;
        
        [Tooltip("Technology level (1-10)")]
        [Range(1f, 10f)]
        public float testTechLevel = 5f;
        
        [Tooltip("Budget for the project")]
        public float testBudget = 100000f;
        
        [Tooltip("Base success chance without modifiers")]
        [Range(0f, 1f)]
        public float baseSuccessChance = 0.7f;
        #endregion

        #region Simulation Settings
        [Header("Simulation Settings")]
        [Tooltip("Number of Monte Carlo runs")]
        public int simulationRuns = 1000;
        
        [Tooltip("Maximum runs allowed")]
        public int maxSimulationRuns = 10000;
        
        [Tooltip("Show progress bar during simulation")]
        public bool showProgressBar = true;
        
        [Tooltip("Update UI every N runs")]
        public int uiUpdateInterval = 10;
        
        [Tooltip("Auto-export results after simulation")]
        public bool autoExportResults = false;
        #endregion

        #region Risk Modifiers
        [Header("Risk Modifiers")]
        [Tooltip("Impact of technical risk on success chance")]
        public AnimationCurve technicalRiskCurve = AnimationCurve.Linear(0, 0, 6, -0.3f);
        
        [Tooltip("Impact of financial risk on success chance")]
        public AnimationCurve financialRiskCurve = AnimationCurve.Linear(0, 0, 6, -0.25f);
        
        [Tooltip("Impact of security risk on success chance")]
        public AnimationCurve securityRiskCurve = AnimationCurve.Linear(0, 0, 6, -0.2f);
        
        [Tooltip("Reputation impact multiplier")]
        public float reputationMultiplier = 0.002f;
        
        [Tooltip("Technology level impact multiplier")]
        public float techLevelMultiplier = 0.03f;
        #endregion

        #region Events
        public event Action<RiskSimulationResult> OnSimulationCompleted;
        public event Action<float> OnSimulationProgress;
        public event Action OnSimulationStarted;
        #endregion

        #region Private Fields
        private RiskSimulationResult _lastResult;
        private Coroutine _simulationCoroutine;
        private bool _isSimulating = false;
        private List<RiskSimulationResult> _simulationHistory = new List<RiskSimulationResult>();
        private const int MAX_HISTORY = 10;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupUI();
            PopulateDropdowns();
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            if (runSimulationButton != null)
                runSimulationButton.onClick.AddListener(() => RunMonteCarloSimulation());
            
            if (runSingleTestButton != null)
                runSingleTestButton.onClick.AddListener(RunSingleTest);
            
            if (exportResultsButton != null)
                exportResultsButton.onClick.AddListener(ExportLastResults);
            
            if (clearResultsButton != null)
                clearResultsButton.onClick.AddListener(ClearResults);
            
            // Set default values
            if (reputationInput != null) reputationInput.text = testReputation.ToString();
            if (techLevelInput != null) techLevelInput.text = testTechLevel.ToString();
            if (budgetInput != null) budgetInput.text = testBudget.ToString();
            if (simulationRunsInput != null) simulationRunsInput.text = simulationRuns.ToString();
        }

        private void PopulateDropdowns()
        {
            string[] riskLevels = System.Enum.GetNames(typeof(RiskLevel));
            
            PopulateDropdown(technicalRiskDropdown, riskLevels, (int)testTechnicalRisk);
            PopulateDropdown(financialRiskDropdown, riskLevels, (int)testFinancialRisk);
            PopulateDropdown(securityRiskDropdown, riskLevels, (int)testSecurityRisk);
        }

        private void PopulateDropdown(TMP_Dropdown dropdown, string[] options, int defaultIndex)
        {
            if (dropdown == null) return;
            
            dropdown.ClearOptions();
            dropdown.AddOptions(options.ToList());
            dropdown.value = defaultIndex;
        }
        #endregion

        #region Parameter Reading
        private void ReadParametersFromUI()
        {
            if (technicalRiskDropdown != null)
                testTechnicalRisk = (RiskLevel)technicalRiskDropdown.value;
            
            if (financialRiskDropdown != null)
                testFinancialRisk = (RiskLevel)financialRiskDropdown.value;
            
            if (securityRiskDropdown != null)
                testSecurityRisk = (RiskLevel)securityRiskDropdown.value;
            
            if (reputationInput != null && float.TryParse(reputationInput.text, out float rep))
                testReputation = Mathf.Clamp(rep, 0f, 100f);
            
            if (techLevelInput != null && float.TryParse(techLevelInput.text, out float tech))
                testTechLevel = Mathf.Clamp(tech, 1f, 10f);
            
            if (budgetInput != null && float.TryParse(budgetInput.text, out float budget))
                testBudget = budget;
            
            if (simulationRunsInput != null && int.TryParse(simulationRunsInput.text, out int runs))
                simulationRuns = Mathf.Clamp(runs, 1, maxSimulationRuns);
        }
        #endregion

        #region Single Test
        /// <summary>
        /// Run a single risk test and display result
        /// </summary>
        public void RunSingleTest()
        {
            ReadParametersFromUI();
            
            SingleSimulationResult result = SimulateSingleRun(
                testTechnicalRisk,
                testFinancialRisk,
                testSecurityRisk,
                testReputation,
                testTechLevel,
                testBudget
            );
            
            DisplaySingleResult(result);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Single risk test: {result.outcome}");
            }
        }

        private void DisplaySingleResult(SingleSimulationResult result)
        {
            if (resultsText == null) return;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== SINGLE TEST RESULT ===");
            sb.AppendLine($"Outcome: {result.outcome}");
            sb.AppendLine($"Success Chance: {result.calculatedSuccessChance:P1}");
            sb.AppendLine($"Random Roll: {result.randomRoll:F4}");
            
            if (result.outcome != SimulationOutcome.Success)
            {
                sb.AppendLine($"Failure Type: {result.failureType}");
                sb.AppendLine($"Consequence Severity: {result.consequenceSeverity:F2}");
                sb.AppendLine($"Monetary Impact: {result.monetaryImpact:C}");
                sb.AppendLine($"Reputation Impact: {result.reputationImpact:F1}");
            }
            
            if (result.contributingFactors.Count > 0)
            {
                sb.AppendLine("Contributing Factors:");
                foreach (var factor in result.contributingFactors)
                {
                    sb.AppendLine($"  - {factor}");
                }
            }
            
            resultsText.text = sb.ToString();
        }
        #endregion

        #region Monte Carlo Simulation
        /// <summary>
        /// Run a Monte Carlo simulation with current parameters
        /// </summary>
        public void RunMonteCarloSimulation(int? runs = null)
        {
            if (_isSimulating)
            {
                UnityEngine.Debug.LogWarning("[RiskSimulator] Simulation already in progress");
                return;
            }
            
            ReadParametersFromUI();
            int runCount = runs ?? simulationRuns;
            
            _simulationCoroutine = StartCoroutine(RunSimulationCoroutine(runCount));
        }

        private IEnumerator RunSimulationCoroutine(int runs)
        {
            _isSimulating = true;
            OnSimulationStarted?.Invoke();
            
            float startTime = Time.time;
            
            // Initialize result
            RiskSimulationResult result = new RiskSimulationResult
            {
                totalRuns = runs,
                testedTechnicalRisk = testTechnicalRisk,
                testedFinancialRisk = testFinancialRisk,
                testedSecurityRisk = testSecurityRisk,
                testedReputation = testReputation,
                testedTechLevel = testTechLevel,
                testedBudget = testBudget
            };
            
            // Initialize failure distribution
            foreach (FailureType type in System.Enum.GetValues(typeof(FailureType)))
            {
                result.failureDistribution[type] = 0;
            }
            
            // Run simulations
            for (int i = 0; i < runs; i++)
            {
                SingleSimulationResult singleResult = SimulateSingleRun(
                    testTechnicalRisk,
                    testFinancialRisk,
                    testSecurityRisk,
                    testReputation,
                    testTechLevel,
                    testBudget
                );
                
                result.individualResults.Add(singleResult);
                
                // Update statistics
                if (singleResult.outcome == SimulationOutcome.Success || 
                    singleResult.outcome == SimulationOutcome.PartialSuccess)
                {
                    result.successes++;
                }
                else
                {
                    result.failures++;
                    result.failureDistribution[singleResult.failureType]++;
                }
                
                // Update progress
                float progress = (float)(i + 1) / runs;
                
                if (i % uiUpdateInterval == 0)
                {
                    UpdateProgressUI(progress);
                    OnSimulationProgress?.Invoke(progress);
                    yield return null;
                }
            }
            
            // Finalize results
            result.simulationDuration = Time.time - startTime;
            result.CalculateStatistics();
            
            _lastResult = result;
            _simulationHistory.Add(result);
            
            while (_simulationHistory.Count > MAX_HISTORY)
            {
                _simulationHistory.RemoveAt(0);
            }
            
            // Update UI
            UpdateProgressUI(1f);
            DisplayResults(result);
            
            OnSimulationCompleted?.Invoke(result);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Risk simulation completed: {result.successRate:F1}% success rate");
            }
            
            if (autoExportResults)
            {
                ExportLastResults();
            }
            
            _isSimulating = false;
        }

        private SingleSimulationResult SimulateSingleRun(
            RiskLevel techRisk,
            RiskLevel finRisk,
            RiskLevel secRisk,
            float reputation,
            float techLevel,
            float budget)
        {
            SingleSimulationResult result = new SingleSimulationResult();
            
            // Calculate success chance
            float successChance = baseSuccessChance;
            
            // Apply risk modifiers
            successChance += technicalRiskCurve.Evaluate((int)techRisk);
            successChance += financialRiskCurve.Evaluate((int)finRisk);
            successChance += securityRiskCurve.Evaluate((int)secRisk);
            
            // Apply positive modifiers
            successChance += reputation * reputationMultiplier;
            successChance += (techLevel - 1) * techLevelMultiplier;
            
            // Clamp success chance
            successChance = Mathf.Clamp01(successChance);
            
            result.calculatedSuccessChance = successChance;
            result.technicalRisk = techRisk;
            result.financialRisk = finRisk;
            result.securityRisk = secRisk;
            
            // Roll for outcome
            result.randomRoll = UnityEngine.Random.value;
            
            if (result.randomRoll < successChance)
            {
                result.outcome = SimulationOutcome.Success;
                result.failureType = FailureType.None;
                result.consequenceSeverity = 0f;
                result.monetaryImpact = CalculateSuccessValue(budget);
            }
            else if (result.randomRoll < successChance + 0.1f)
            {
                result.outcome = SimulationOutcome.PartialSuccess;
                result.failureType = DetermineFailureType(techRisk, finRisk, secRisk);
                result.consequenceSeverity = UnityEngine.Random.Range(0.1f, 0.4f);
                result.monetaryImpact = CalculatePartialSuccessValue(budget, result.consequenceSeverity);
            }
            else
            {
                result.outcome = SimulationOutcome.Failure;
                result.failureType = DetermineFailureType(techRisk, finRisk, secRisk);
                result.consequenceSeverity = CalculateConsequenceSeverity(techRisk, finRisk, secRisk);
                result.monetaryImpact = CalculateFailureCost(budget, result.consequenceSeverity, result.failureType);
            }
            
            result.reputationImpact = CalculateReputationImpact(result);
            result.contributingFactors = DetermineContributingFactors(techRisk, finRisk, secRisk);
            
            return result;
        }
        #endregion

        #region Calculation Methods
        private FailureType DetermineFailureType(RiskLevel techRisk, RiskLevel finRisk, RiskLevel secRisk)
        {
            // Weight failure types based on risk levels
            List<(FailureType type, float weight)> failureTypes = new List<(FailureType, float)>
            {
                (FailureType.TechnicalFailure, (int)techRisk * 1.5f),
                (FailureType.BudgetOverrun, (int)finRisk * 2f),
                (FailureType.SecurityBreach, (int)secRisk * 2f),
                (FailureType.ScheduleSlip, (int)techRisk + (int)finRisk),
                (FailureType.QualityIssue, (int)techRisk),
                (FailureType.EquipmentFailure, (int)techRisk * 0.8f),
                (FailureType.SupplyChainFailure, UnityEngine.Random.Range(0.5f, 1.5f)),
                (FailureType.TeamDeparture, UnityEngine.Random.Range(0.3f, 1f)),
                (FailureType.DataLoss, (int)secRisk * 0.7f)
            };
            
            // Select based on weights
            float totalWeight = failureTypes.Sum(x => x.weight);
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            
            float cumulative = 0f;
            foreach (var ft in failureTypes)
            {
                cumulative += ft.weight;
                if (roll <= cumulative)
                {
                    return ft.type;
                }
            }
            
            return FailureType.TechnicalFailure;
        }

        private float CalculateConsequenceSeverity(RiskLevel techRisk, RiskLevel finRisk, RiskLevel secRisk)
        {
            float baseSeverity = UnityEngine.Random.Range(0.3f, 0.7f);
            float riskFactor = ((int)techRisk + (int)finRisk + (int)secRisk) / 18f;
            float randomFactor = UnityEngine.Random.Range(-0.1f, 0.3f);
            
            return Mathf.Clamp01(baseSeverity + riskFactor + randomFactor);
        }

        private float CalculateFailureCost(float budget, float severity, FailureType failureType)
        {
            float baseCost = budget * severity;
            
            // Type-specific multipliers
            float typeMultiplier = failureType switch
            {
                FailureType.SecurityBreach => 1.5f,
                FailureType.DataLoss => 1.3f,
                FailureType.RegulatoryViolation => 2f,
                FailureType.BudgetOverrun => 1.2f,
                _ => 1f
            };
            
            return -baseCost * typeMultiplier * UnityEngine.Random.Range(0.8f, 1.2f);
        }

        private float CalculateSuccessValue(float budget)
        {
            return budget * UnityEngine.Random.Range(0.1f, 0.3f);
        }

        private float CalculatePartialSuccessValue(float budget, float severity)
        {
            return budget * UnityEngine.Random.Range(0.02f, 0.1f) * (1f - severity);
        }

        private float CalculateReputationImpact(SingleSimulationResult result)
        {
            if (result.outcome == SimulationOutcome.Success)
            {
                return UnityEngine.Random.Range(1f, 5f);
            }
            else if (result.outcome == SimulationOutcome.PartialSuccess)
            {
                return -result.consequenceSeverity * UnityEngine.Random.Range(2f, 5f);
            }
            else
            {
                return -result.consequenceSeverity * UnityEngine.Random.Range(5f, 15f);
            }
        }

        private List<string> DetermineContributingFactors(RiskLevel techRisk, RiskLevel finRisk, RiskLevel secRisk)
        {
            List<string> factors = new List<string>();
            
            if ((int)techRisk >= 4) factors.Add("High technical complexity");
            if ((int)finRisk >= 4) factors.Add("Budget constraints");
            if ((int)secRisk >= 4) factors.Add("Security vulnerabilities");
            if (testReputation < 30) factors.Add("Low company reputation");
            if (testTechLevel < 3) factors.Add("Insufficient technology");
            
            return factors;
        }
        #endregion

        #region UI Updates
        private void UpdateProgressUI(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{progress * 100f:F0}%";
            }
        }

        private void DisplayResults(RiskSimulationResult result)
        {
            if (resultsText != null)
            {
                resultsText.text = result.GenerateReport();
            }
            
            // Auto-scroll to top
            if (resultsScrollRect != null)
            {
                resultsScrollRect.verticalNormalizedPosition = 1f;
            }
        }
        #endregion

        #region Export Methods
        /// <summary>
        /// Export the last simulation results
        /// </summary>
        public void ExportLastResults()
        {
            if (_lastResult == null)
            {
                UnityEngine.Debug.LogWarning("[RiskSimulator] No results to export");
                return;
            }
            
            string csv = _lastResult.ExportToCSV();
            GUIUtility.systemCopyBuffer = csv;
            
            UnityEngine.Debug.Log("[RiskSimulator] Results exported to clipboard (CSV format)");
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Risk results exported to clipboard");
            }
        }

        /// <summary>
        /// Export results to JSON
        /// </summary>
        public string ExportToJSON()
        {
            if (_lastResult == null) return null;
            return _lastResult.ExportToJSON();
        }

        /// <summary>
        /// Get the last simulation result
        /// </summary>
        public RiskSimulationResult GetLastResult()
        {
            return _lastResult;
        }

        /// <summary>
        /// Get simulation history
        /// </summary>
        public List<RiskSimulationResult> GetSimulationHistory()
        {
            return new List<RiskSimulationResult>(_simulationHistory);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Clear all results
        /// </summary>
        public void ClearResults()
        {
            _lastResult = null;
            _simulationHistory.Clear();
            
            if (resultsText != null)
            {
                resultsText.text = "No results. Run a simulation to see results here.";
            }
            
            UpdateProgressUI(0f);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Risk simulation results cleared");
            }
        }

        /// <summary>
        /// Check if simulation is running
        /// </summary>
        public bool IsSimulating()
        {
            return _isSimulating;
        }

        /// <summary>
        /// Cancel current simulation
        /// </summary>
        public void CancelSimulation()
        {
            if (_simulationCoroutine != null)
            {
                StopCoroutine(_simulationCoroutine);
                _isSimulating = false;
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Risk simulation cancelled");
                }
            }
        }

        /// <summary>
        /// Compare multiple simulation results
        /// </summary>
        public string CompareResults(List<RiskSimulationResult> results)
        {
            if (results == null || results.Count < 2)
            {
                return "Need at least 2 results to compare";
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== RISK COMPARISON ===");
            sb.AppendLine();
            
            sb.AppendLine("| Config | Success% | Risk Score | VaR95 | EMV |");
            sb.AppendLine("|--------|----------|------------|-------|-----|");
            
            foreach (var result in results)
            {
                string config = $"T{(int)result.testedTechnicalRisk}F{(int)result.testedFinancialRisk}S{(int)result.testedSecurityRisk}";
                sb.AppendLine($"| {config} | {result.successRate:F1}% | {result.overallRiskScore:F0} | {result.valueAtRisk95:C0} | {result.expectedMonetaryValue:C0} |");
            }
            
            return sb.ToString();
        }
        #endregion
    }
}
