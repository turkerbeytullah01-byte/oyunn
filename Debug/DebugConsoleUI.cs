using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// In-game debug console UI for Project Aegis: Drone Dominion
    /// Provides visual interface for all debug functionality
    /// </summary>
    public class DebugConsoleUI : MonoBehaviour
    {
        #region UI References
        [Header("Panel References")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Log Display")]
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private ScrollRect logScrollRect;
        [SerializeField] private int maxLogLines = 50;
        
        [Header("Section Panels")]
        [SerializeField] private GameObject timeControlSection;
        [SerializeField] private GameObject cheatsSection;
        [SerializeField] private GameObject researchSection;
        [SerializeField] private GameObject eventsSection;
        [SerializeField] private GameObject contractsSection;
        [SerializeField] private GameObject riskSection;
        [SerializeField] private GameObject saveSection;
        [SerializeField] private GameObject performanceSection;
        
        [Header("Time Control UI")]
        [SerializeField] private Slider timeScaleSlider;
        [SerializeField] private TextMeshProUGUI timeScaleText;
        [SerializeField] private Button[] timePresetButtons;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button skipMinuteButton;
        [SerializeField] private Button skipHourButton;
        [SerializeField] private Button skipDayButton;
        
        [Header("Cheats UI")]
        [SerializeField] private TMP_InputField moneyInput;
        [SerializeField] private Button addMoneyButton;
        [SerializeField] private Button setMoneyButton;
        [SerializeField] private TMP_InputField reputationInput;
        [SerializeField] private Button addReputationButton;
        [SerializeField] private Button setReputationButton;
        [SerializeField] private TMP_InputField researchPointsInput;
        [SerializeField] private Button addResearchPointsButton;
        
        [Header("Research UI")]
        [SerializeField] private Button completeCurrentResearchButton;
        [SerializeField] private Button completeAllResearchButton;
        [SerializeField] private Button unlockAllTechButton;
        [SerializeField] private Button resetResearchButton;
        [SerializeField] private TMP_Dropdown researchDropdown;
        
        [Header("Events UI")]
        [SerializeField] private TMP_Dropdown eventDropdown;
        [SerializeField] private Button triggerEventButton;
        [SerializeField] private Button triggerRandomEventButton;
        [SerializeField] private Button clearEffectsButton;
        [SerializeField] private Button triggerAllEventsButton;
        
        [Header("Contracts UI")]
        [SerializeField] private TMP_InputField contractCountInput;
        [SerializeField] private Button generateContractsButton;
        [SerializeField] private Button completeAllContractsButton;
        [SerializeField] private Button winAllBidsButton;
        [SerializeField] private Button failAllContractsButton;
        [SerializeField] private Button resetContractsButton;
        
        [Header("Risk UI")]
        [SerializeField] private TMP_Dropdown riskTypeDropdown;
        [SerializeField] private TMP_InputField simulationRunsInput;
        [SerializeField] private Button runSimulationButton;
        [SerializeField] private Button runSingleTestButton;
        [SerializeField] private TextMeshProUGUI riskResultsText;
        
        [Header("Save UI")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button resetAllButton;
        [SerializeField] private Button exportSaveButton;
        [SerializeField] private Button showSaveDataButton;
        
        [Header("Performance UI")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI memoryText;
        [SerializeField] private TextMeshProUGUI drawCallsText;
        
        [Header("Settings")]
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        [SerializeField] private bool autoScrollLog = true;
        #endregion

        #region Private Fields
        private List<string> _logEntries = new List<string>();
        private float _fpsAccumulator = 0f;
        private int _fpsFrames = 0;
        private float _fpsCurrent = 0f;
        private float _lastFpsUpdate = 0f;
        private Dictionary<string, Action> _commandRegistry = new Dictionary<string, Action>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupUIReferences();
            RegisterCommands();
        }

        private void OnEnable()
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.OnDebugActionLogged += OnDebugLogReceived;
                DebugManager.Instance.OnTimeScaleChanged += OnTimeScaleChanged;
            }
        }

        private void OnDisable()
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.OnDebugActionLogged -= OnDebugLogReceived;
                DebugManager.Instance.OnTimeScaleChanged -= OnTimeScaleChanged;
            }
        }

        private void Start()
        {
            InitializeUI();
            PopulateDropdowns();
        }

        private void Update()
        {
            UpdatePerformanceDisplay();
        }
        #endregion

        #region Initialization
        private void SetupUIReferences()
        {
            // Auto-find references if not set
            if (consolePanel == null)
                consolePanel = transform.Find("ConsolePanel")?.gameObject;
            
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (logScrollRect == null)
                logScrollRect = GetComponentInChildren<ScrollRect>();
        }

        private void InitializeUI()
        {
            // Set initial visibility
            if (consolePanel != null)
            {
                consolePanel.SetActive(DebugManager.Instance?.showDebugPanelOnStart ?? false);
            }
            
            // Setup time slider
            if (timeScaleSlider != null)
            {
                timeScaleSlider.minValue = 0f;
                timeScaleSlider.maxValue = 100f;
                timeScaleSlider.value = 1f;
                timeScaleSlider.onValueChanged.AddListener(OnTimeScaleSliderChanged);
            }
            
            // Setup input fields with default values
            if (moneyInput != null) moneyInput.text = "10000";
            if (reputationInput != null) reputationInput.text = "10";
            if (researchPointsInput != null) researchPointsInput.text = "1000";
            if (contractCountInput != null) contractCountInput.text = "5";
            if (simulationRunsInput != null) simulationRunsInput.text = "1000";
        }

        private void PopulateDropdowns()
        {
            PopulateEventDropdown();
            PopulateResearchDropdown();
            PopulateRiskDropdown();
        }

        private void PopulateEventDropdown()
        {
            if (eventDropdown == null) return;
            
            eventDropdown.ClearOptions();
            var options = new List<string>
            {
                "Select Event...",
                "Market Crash",
                "Tech Breakthrough",
                "Competitor Launch",
                "Regulatory Change",
                "Supply Shortage",
                "Reputation Boost",
                "Security Breach",
                "Natural Disaster",
                "Economic Boom"
            };
            eventDropdown.AddOptions(options);
        }

        private void PopulateResearchDropdown()
        {
            if (researchDropdown == null) return;
            
            researchDropdown.ClearOptions();
            var options = new List<string>
            {
                "Select Research...",
                "Battery Efficiency I",
                "Battery Efficiency II",
                "Motor Performance I",
                "Motor Performance II",
                "AI Navigation",
                "Payload Capacity",
                "Stealth Tech",
                "Long Range Comms",
                "Auto Landing",
                "Swarm Coordination"
            };
            researchDropdown.AddOptions(options);
        }

        private void PopulateRiskDropdown()
        {
            if (riskTypeDropdown == null) return;
            
            riskTypeDropdown.ClearOptions();
            var options = new List<string>
            {
                "All Risk Types",
                "Technical Risk",
                "Financial Risk",
                "Security Risk",
                "Market Risk"
            };
            riskTypeDropdown.AddOptions(options);
        }
        #endregion

        #region Button Registration
        private void RegisterCommands()
        {
            // Time Control
            RegisterButton(pauseButton, OnPauseButtonClicked);
            RegisterButton(skipMinuteButton, () => OnSkipTimeClicked(1f/60f));
            RegisterButton(skipHourButton, () => OnSkipTimeClicked(1f));
            RegisterButton(skipDayButton, () => OnSkipTimeClicked(24f));
            
            // Cheats
            RegisterButton(addMoneyButton, OnAddMoneyClicked);
            RegisterButton(setMoneyButton, OnSetMoneyClicked);
            RegisterButton(addReputationButton, OnAddReputationClicked);
            RegisterButton(setReputationButton, OnSetReputationClicked);
            RegisterButton(addResearchPointsButton, OnAddResearchPointsClicked);
            
            // Research
            RegisterButton(completeCurrentResearchButton, OnCompleteCurrentResearchClicked);
            RegisterButton(completeAllResearchButton, OnCompleteAllResearchClicked);
            RegisterButton(unlockAllTechButton, OnUnlockAllTechClicked);
            RegisterButton(resetResearchButton, OnResetResearchClicked);
            
            // Events
            RegisterButton(triggerEventButton, OnTriggerEventClicked);
            RegisterButton(triggerRandomEventButton, OnTriggerRandomEventClicked);
            RegisterButton(clearEffectsButton, OnClearEffectsClicked);
            RegisterButton(triggerAllEventsButton, OnTriggerAllEventsClicked);
            
            // Contracts
            RegisterButton(generateContractsButton, OnGenerateContractsClicked);
            RegisterButton(completeAllContractsButton, OnCompleteAllContractsClicked);
            RegisterButton(winAllBidsButton, OnWinAllBidsClicked);
            RegisterButton(failAllContractsButton, OnFailAllContractsClicked);
            RegisterButton(resetContractsButton, OnResetContractsClicked);
            
            // Risk
            RegisterButton(runSimulationButton, OnRunSimulationClicked);
            RegisterButton(runSingleTestButton, OnRunSingleTestClicked);
            
            // Save
            RegisterButton(saveButton, OnSaveClicked);
            RegisterButton(loadButton, OnLoadClicked);
            RegisterButton(deleteSaveButton, OnDeleteSaveClicked);
            RegisterButton(resetAllButton, OnResetAllClicked);
            RegisterButton(exportSaveButton, OnExportSaveClicked);
            RegisterButton(showSaveDataButton, OnShowSaveDataClicked);
        }

        private void RegisterButton(Button button, Action action)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => action?.Invoke());
            }
        }
        #endregion

        #region Time Control Handlers
        private void OnTimeScaleSliderChanged(float value)
        {
            DebugManager.Instance?.SetTimeScale(value);
            UpdateTimeScaleText(value);
        }

        private void OnTimeScaleChanged(float scale)
        {
            if (timeScaleSlider != null && !Mathf.Approximately(timeScaleSlider.value, scale))
            {
                timeScaleSlider.value = scale;
            }
            UpdateTimeScaleText(scale);
        }

        private void UpdateTimeScaleText(float scale)
        {
            if (timeScaleText != null)
            {
                timeScaleText.text = $"{scale:F1}x";
            }
        }

        private void OnPauseButtonClicked()
        {
            DebugManager.Instance?.TogglePause();
            UpdatePauseButtonText();
        }

        private void UpdatePauseButtonText()
        {
            if (pauseButton != null)
            {
                var text = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = DebugManager.Instance?.isPaused ?? false ? "Resume" : "Pause";
                }
            }
        }

        private void OnSkipTimeClicked(float hours)
        {
            DebugManager.Instance?.SimulateOfflineTime(hours);
            AddLog($"Skipped {hours:F2} hours");
        }
        #endregion

        #region Cheat Handlers
        private void OnAddMoneyClicked()
        {
            if (float.TryParse(moneyInput?.text, out float amount))
            {
                DebugManager.Instance?.AddMoney(amount);
            }
        }

        private void OnSetMoneyClicked()
        {
            if (float.TryParse(moneyInput?.text, out float amount))
            {
                DebugManager.Instance?.SetMoney(amount);
            }
        }

        private void OnAddReputationClicked()
        {
            if (float.TryParse(reputationInput?.text, out float amount))
            {
                DebugManager.Instance?.AddReputation(amount);
            }
        }

        private void OnSetReputationClicked()
        {
            if (float.TryParse(reputationInput?.text, out float amount))
            {
                DebugManager.Instance?.SetReputation(amount);
            }
        }

        private void OnAddResearchPointsClicked()
        {
            if (float.TryParse(researchPointsInput?.text, out float amount))
            {
                DebugManager.Instance?.AddResearchPoints(amount);
            }
        }
        #endregion

        #region Research Handlers
        private void OnCompleteCurrentResearchClicked()
        {
            DebugManager.Instance?.CompleteCurrentResearch();
        }

        private void OnCompleteAllResearchClicked()
        {
            DebugManager.Instance?.CompleteAllResearch();
        }

        private void OnUnlockAllTechClicked()
        {
            DebugManager.Instance?.UnlockAllTechnologies();
        }

        private void OnResetResearchClicked()
        {
            DebugManager.Instance?.ResetResearchProgress();
        }
        #endregion

        #region Event Handlers
        private void OnTriggerEventClicked()
        {
            if (eventDropdown != null && eventDropdown.value > 0)
            {
                string eventName = eventDropdown.options[eventDropdown.value].text;
                DebugManager.Instance?.TriggerEvent(eventName);
            }
        }

        private void OnTriggerRandomEventClicked()
        {
            DebugManager.Instance?.TriggerRandomEvent();
        }

        private void OnClearEffectsClicked()
        {
            DebugManager.Instance?.ClearActiveEffects();
        }

        private void OnTriggerAllEventsClicked()
        {
            AddLog("Triggering all events sequentially...");
            // This would trigger all events with delays
        }
        #endregion

        #region Contract Handlers
        private void OnGenerateContractsClicked()
        {
            if (int.TryParse(contractCountInput?.text, out int count))
            {
                DebugManager.Instance?.GenerateTestContracts(count);
            }
        }

        private void OnCompleteAllContractsClicked()
        {
            DebugManager.Instance?.CompleteAllContracts();
        }

        private void OnWinAllBidsClicked()
        {
            DebugManager.Instance?.WinAllBids();
        }

        private void OnFailAllContractsClicked()
        {
            AddLog("Failing all contracts...");
            // ContractManager.Instance?.FailAllContracts();
        }

        private void OnResetContractsClicked()
        {
            AddLog("Resetting all contracts...");
            // ContractManager.Instance?.ResetContracts();
        }
        #endregion

        #region Risk Handlers
        private void OnRunSimulationClicked()
        {
            if (int.TryParse(simulationRunsInput?.text, out int runs))
            {
                AddLog($"Running Monte Carlo simulation with {runs} iterations...");
                // RiskSimulator.Instance?.RunMonteCarloSimulation(runs);
            }
        }

        private void OnRunSingleTestClicked()
        {
            AddLog("Running single risk test...");
            // RiskSimulator.Instance?.RunSingleTest();
        }
        #endregion

        #region Save Handlers
        private void OnSaveClicked()
        {
            DebugManager.Instance?.SaveGame();
        }

        private void OnLoadClicked()
        {
            DebugManager.Instance?.LoadGame();
        }

        private void OnDeleteSaveClicked()
        {
            DebugManager.Instance?.DeleteSave();
        }

        private void OnResetAllClicked()
        {
            DebugManager.Instance?.ResetAllProgress();
        }

        private void OnExportSaveClicked()
        {
            AddLog("Exporting save data to clipboard...");
            // SaveManager.Instance?.ExportSaveToClipboard();
        }

        private void OnShowSaveDataClicked()
        {
            AddLog("Displaying save data...");
            // SaveManager.Instance?.ShowSaveData();
        }
        #endregion

        #region Performance Display
        private void UpdatePerformanceDisplay()
        {
            // Calculate FPS
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrames++;
            
            if (Time.unscaledTime - _lastFpsUpdate >= fpsUpdateInterval)
            {
                _fpsCurrent = _fpsFrames / _fpsAccumulator;
                _fpsFrames = 0;
                _fpsAccumulator = 0f;
                _lastFpsUpdate = Time.unscaledTime;
                
                UpdatePerformanceTexts();
            }
        }

        private void UpdatePerformanceTexts()
        {
            if (fpsText != null)
            {
                Color fpsColor = _fpsCurrent >= 60 ? Color.green : 
                                 _fpsCurrent >= 30 ? Color.yellow : Color.red;
                fpsText.text = $"FPS: {_fpsCurrent:F0}";
                fpsText.color = fpsColor;
            }
            
            if (memoryText != null)
            {
                long memoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
                memoryText.text = $"Memory: {memoryMB} MB";
            }
            
            if (drawCallsText != null)
            {
                // Note: Unity doesn't expose draw calls easily in runtime
                // This would require Unity Profiler API
                drawCallsText.text = "Draw Calls: N/A";
            }
        }
        #endregion

        #region Logging
        /// <summary>
        /// Add a message to the debug log display
        /// </summary>
        public void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";
            
            _logEntries.Add(logEntry);
            
            // Trim log if too long
            while (_logEntries.Count > maxLogLines)
            {
                _logEntries.RemoveAt(0);
            }
            
            UpdateLogDisplay();
        }

        private void OnDebugLogReceived(string message)
        {
            AddLog(message);
        }

        private void UpdateLogDisplay()
        {
            if (logText != null)
            {
                logText.text = string.Join("\n", _logEntries);
                
                if (autoScrollLog && logScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    logScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        /// <summary>
        /// Clear the log display
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
            if (logText != null)
            {
                logText.text = "";
            }
        }
        #endregion

        #region Section Toggles
        /// <summary>
        /// Toggle visibility of a specific section
        /// </summary>
        public void ToggleSection(string sectionName)
        {
            GameObject section = GetSection(sectionName);
            if (section != null)
            {
                section.SetActive(!section.activeSelf);
            }
        }

        /// <summary>
        /// Show a specific section
        /// </summary>
        public void ShowSection(string sectionName)
        {
            GameObject section = GetSection(sectionName);
            if (section != null)
            {
                section.SetActive(true);
            }
        }

        /// <summary>
        /// Hide a specific section
        /// </summary>
        public void HideSection(string sectionName)
        {
            GameObject section = GetSection(sectionName);
            if (section != null)
            {
                section.SetActive(false);
            }
        }

        private GameObject GetSection(string sectionName)
        {
            switch (sectionName.ToLower())
            {
                case "time": return timeControlSection;
                case "cheats": return cheatsSection;
                case "research": return researchSection;
                case "events": return eventsSection;
                case "contracts": return contractsSection;
                case "risk": return riskSection;
                case "save": return saveSection;
                case "performance": return performanceSection;
                default: return null;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Set the alpha/visibility of the console
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        /// <summary>
        /// Check if console is visible
        /// </summary>
        public bool IsVisible()
        {
            return consolePanel != null && consolePanel.activeSelf;
        }
        #endregion
    }
}
