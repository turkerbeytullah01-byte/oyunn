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
    /// Research debugger for testing research systems
    /// Allows developers to manipulate research progress, unlock techs, and test edge cases
    /// </summary>
    public class ResearchDebugger : MonoBehaviour
    {
        #region Singleton
        private static ResearchDebugger _instance;
        public static ResearchDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ResearchDebugger>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ResearchDebugger");
                        _instance = go.AddComponent<ResearchDebugger>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown researchDropdown;
        [SerializeField] private Button completeCurrentResearchButton;
        [SerializeField] private Button completeAllResearchButton;
        [SerializeField] private Button unlockAllTechnologiesButton;
        [SerializeField] private Button resetResearchProgressButton;
        [SerializeField] private Button failCurrentResearchButton;
        [SerializeField] private Button addResearchToQueueButton;
        [SerializeField] private Button clearQueueButton;
        [SerializeField] private Button instantCompleteButton;
        [SerializeField] private TMP_InputField researchPointsInput;
        [SerializeField] private Button addResearchPointsButton;
        [SerializeField] private Button setResearchPointsButton;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI currentResearchText;
        [SerializeField] private TextMeshProUGUI researchQueueText;
        [SerializeField] private TextMeshProUGUI unlockedTechsText;
        [SerializeField] private ScrollRect scrollRect;
        #endregion

        #region Debug Research Data
        [System.Serializable]
        public class DebugResearchData
        {
            public string researchId;
            public string researchName;
            public string description;
            public ResearchCategory category;
            public float baseDuration; // in hours
            public float baseCost;
            public List<string> prerequisites;
            public List<string> unlocks;
            public List<ResearchBonus> bonuses;
        }

        [System.Serializable]
        public class ResearchBonus
        {
            public BonusType type;
            public float value;
            public string target;
        }

        public enum ResearchCategory
        {
            Propulsion,
            PowerSystems,
            Avionics,
            Payload,
            Materials,
            AI,
            Manufacturing,
            Stealth
        }

        public enum BonusType
        {
            SpeedIncrease,
            EfficiencyIncrease,
            CapacityIncrease,
            CostReduction,
            UnlockFeature,
            QualityIncrease
        }

        [Header("Available Research")]
        public List<DebugResearchData> availableResearch = new List<DebugResearchData>
        {
            new DebugResearchData { 
                researchId = "motor_eff_1", 
                researchName = "Motor Efficiency I", 
                description = "Improve motor efficiency by 10%", 
                category = ResearchCategory.Propulsion,
                baseDuration = 2f,
                baseCost = 5000f,
                prerequisites = new List<string>(),
                unlocks = new List<string> { "motor_eff_2" },
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.EfficiencyIncrease, value = 0.1f, target = "motor" } }
            },
            new DebugResearchData { 
                researchId = "motor_eff_2", 
                researchName = "Motor Efficiency II", 
                description = "Improve motor efficiency by 20%", 
                category = ResearchCategory.Propulsion,
                baseDuration = 8f,
                baseCost = 15000f,
                prerequisites = new List<string> { "motor_eff_1" },
                unlocks = new List<string> { "motor_eff_3" },
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.EfficiencyIncrease, value = 0.2f, target = "motor" } }
            },
            new DebugResearchData { 
                researchId = "battery_density_1", 
                researchName = "Battery Density I", 
                description = "Increase battery capacity by 15%", 
                category = ResearchCategory.PowerSystems,
                baseDuration = 4f,
                baseCost = 8000f,
                prerequisites = new List<string>(),
                unlocks = new List<string> { "battery_density_2" },
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.CapacityIncrease, value = 0.15f, target = "battery" } }
            },
            new DebugResearchData { 
                researchId = "ai_navigation", 
                researchName = "AI Navigation", 
                description = "Enable autonomous navigation", 
                category = ResearchCategory.AI,
                baseDuration = 24f,
                baseCost = 50000f,
                prerequisites = new List<string> { "avionics_basic" },
                unlocks = new List<string> { "ai_swarm" },
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.UnlockFeature, value = 1f, target = "autonomous_nav" } }
            },
            new DebugResearchData { 
                researchId = "stealth_coating", 
                researchName = "Stealth Coating", 
                description = "Reduce radar signature", 
                category = ResearchCategory.Stealth,
                baseDuration = 48f,
                baseCost = 100000f,
                prerequisites = new List<string> { "materials_advanced" },
                unlocks = new List<string>(),
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.EfficiencyIncrease, value = 0.3f, target = "stealth" } }
            },
            new DebugResearchData { 
                researchId = "payload_heavy", 
                researchName = "Heavy Payload", 
                description = "Increase maximum payload capacity", 
                category = ResearchCategory.Payload,
                baseDuration = 12f,
                baseCost = 25000f,
                prerequisites = new List<string> { "materials_basic" },
                unlocks = new List<string>(),
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.CapacityIncrease, value = 0.5f, target = "payload" } }
            },
            new DebugResearchData { 
                researchId = "manufacturing_auto", 
                researchName = "Automated Manufacturing", 
                description = "Reduce production costs by 25%", 
                category = ResearchCategory.Manufacturing,
                baseDuration = 36f,
                baseCost = 75000f,
                prerequisites = new List<string>(),
                unlocks = new List<string>(),
                bonuses = new List<ResearchBonus> { new ResearchBonus { type = BonusType.CostReduction, value = 0.25f, target = "production" } }
            }
        };
        #endregion

        #region Events
        public event Action<string> OnResearchCompleted;
        public event Action<string> OnResearchFailed;
        public event Action OnAllResearchCompleted;
        public event Action OnResearchReset;
        #endregion

        #region Private Fields
        private string _currentResearchId;
        private float _currentResearchProgress;
        private List<string> _researchQueue = new List<string>();
        private HashSet<string> _completedResearch = new HashSet<string>();
        private HashSet<string> _unlockedTechnologies = new HashSet<string>();
        private float _researchPoints = 0f;
        private Coroutine _researchProgressCoroutine;
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
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            SetupUI();
            PopulateResearchDropdown();
            UpdateDisplay();
        }

        private void Update()
        {
            // Auto-progress research if running
            if (!string.IsNullOrEmpty(_currentResearchId) && _researchProgressCoroutine == null)
            {
                ProgressResearch(Time.deltaTime * GetTimeScale());
            }
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            if (completeCurrentResearchButton != null)
                completeCurrentResearchButton.onClick.AddListener(CompleteCurrentResearch);
            
            if (completeAllResearchButton != null)
                completeAllResearchButton.onClick.AddListener(CompleteAllResearch);
            
            if (unlockAllTechnologiesButton != null)
                unlockAllTechnologiesButton.onClick.AddListener(UnlockAllTechnologies);
            
            if (resetResearchProgressButton != null)
                resetResearchProgressButton.onClick.AddListener(ResetResearchProgress);
            
            if (failCurrentResearchButton != null)
                failCurrentResearchButton.onClick.AddListener(FailCurrentResearch);
            
            if (addResearchToQueueButton != null)
                addResearchToQueueButton.onClick.AddListener(() => AddResearchToQueueFromDropdown());
            
            if (clearQueueButton != null)
                clearQueueButton.onClick.AddListener(ClearResearchQueue);
            
            if (instantCompleteButton != null)
                instantCompleteButton.onClick.AddListener(InstantCompleteCurrent);
            
            if (addResearchPointsButton != null)
                addResearchPointsButton.onClick.AddListener(AddResearchPointsFromInput);
            
            if (setResearchPointsButton != null)
                setResearchPointsButton.onClick.AddListener(SetResearchPointsFromInput);
            
            if (progressSlider != null)
                progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);
        }

        private void PopulateResearchDropdown()
        {
            if (researchDropdown == null) return;
            
            researchDropdown.ClearOptions();
            
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData("Select Research..."));
            
            foreach (var research in availableResearch)
            {
                string status = _completedResearch.Contains(research.researchId) ? "[DONE] " : 
                               (_currentResearchId == research.researchId ? "[ACTIVE] " : "");
                options.Add(new TMP_Dropdown.OptionData($"{status}[{research.category}] {research.researchName}"));
            }
            
            researchDropdown.AddOptions(options);
        }
        #endregion

        #region Research Control
        /// <summary>
        /// Complete the current active research
        /// </summary>
        public void CompleteCurrentResearch()
        {
            if (string.IsNullOrEmpty(_currentResearchId))
            {
                UnityEngine.Debug.Log("[ResearchDebugger] No active research to complete");
                return;
            }
            
            CompleteResearch(_currentResearchId);
        }

        /// <summary>
        /// Complete a specific research by ID
        /// </summary>
        public void CompleteResearch(string researchId)
        {
            var research = availableResearch.Find(r => r.researchId == researchId);
            if (research == null)
            {
                UnityEngine.Debug.LogWarning($"[ResearchDebugger] Research not found: {researchId}");
                return;
            }
            
            _completedResearch.Add(researchId);
            _currentResearchId = null;
            _currentResearchProgress = 0f;
            
            // Unlock technologies
            foreach (var unlock in research.unlocks)
            {
                _unlockedTechnologies.Add(unlock);
            }
            
            // Apply bonuses
            ApplyResearchBonuses(research);
            
            OnResearchCompleted?.Invoke(researchId);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Research completed: {research.researchName}");
            }
            
            // Start next research in queue
            ProcessQueue();
            
            UpdateDisplay();
            PopulateResearchDropdown();
        }

        /// <summary>
        /// Complete all research
        /// </summary>
        public void CompleteAllResearch()
        {
            foreach (var research in availableResearch)
            {
                if (!_completedResearch.Contains(research.researchId))
                {
                    _completedResearch.Add(research.researchId);
                    
                    foreach (var unlock in research.unlocks)
                    {
                        _unlockedTechnologies.Add(unlock);
                    }
                    
                    ApplyResearchBonuses(research);
                }
            }
            
            _currentResearchId = null;
            _currentResearchProgress = 0f;
            _researchQueue.Clear();
            
            OnAllResearchCompleted?.Invoke();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("All research completed");
            }
            
            UpdateDisplay();
            PopulateResearchDropdown();
        }

        /// <summary>
        /// Unlock all technologies (without completing research)
        /// </summary>
        public void UnlockAllTechnologies()
        {
            foreach (var research in availableResearch)
            {
                _unlockedTechnologies.Add(research.researchId);
                foreach (var unlock in research.unlocks)
                {
                    _unlockedTechnologies.Add(unlock);
                }
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("All technologies unlocked");
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Reset all research progress
        /// </summary>
        public void ResetResearchProgress()
        {
            _completedResearch.Clear();
            _unlockedTechnologies.Clear();
            _researchQueue.Clear();
            _currentResearchId = null;
            _currentResearchProgress = 0f;
            _researchPoints = 0f;
            
            if (_researchProgressCoroutine != null)
            {
                StopCoroutine(_researchProgressCoroutine);
                _researchProgressCoroutine = null;
            }
            
            OnResearchReset?.Invoke();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Research progress reset");
            }
            
            UpdateDisplay();
            PopulateResearchDropdown();
        }

        /// <summary>
        /// Fail the current research
        /// </summary>
        public void FailCurrentResearch()
        {
            if (string.IsNullOrEmpty(_currentResearchId))
            {
                UnityEngine.Debug.Log("[ResearchDebugger] No active research to fail");
                return;
            }
            
            var research = availableResearch.Find(r => r.researchId == _currentResearchId);
            
            _currentResearchId = null;
            _currentResearchProgress = 0f;
            
            OnResearchFailed?.Invoke(research?.researchId ?? "unknown");
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Research failed: {research?.researchName ?? "unknown"}");
            }
            
            // Process queue
            ProcessQueue();
            UpdateDisplay();
        }

        /// <summary>
        /// Instantly complete current research
        /// </summary>
        public void InstantCompleteCurrent()
        {
            if (!string.IsNullOrEmpty(_currentResearchId))
            {
                _currentResearchProgress = 1f;
                CompleteCurrentResearch();
            }
        }
        #endregion

        #region Queue Management
        /// <summary>
        /// Add research to queue from dropdown selection
        /// </summary>
        public void AddResearchToQueueFromDropdown()
        {
            if (researchDropdown == null || researchDropdown.value <= 0) return;
            
            int index = researchDropdown.value - 1;
            if (index >= 0 && index < availableResearch.Count)
            {
                AddResearchToQueue(availableResearch[index].researchId);
            }
        }

        /// <summary>
        /// Add a specific research to the queue
        /// </summary>
        public void AddResearchToQueue(string researchId)
        {
            if (_completedResearch.Contains(researchId))
            {
                UnityEngine.Debug.Log($"[ResearchDebugger] Research already completed: {researchId}");
                return;
            }
            
            if (_researchQueue.Contains(researchId))
            {
                UnityEngine.Debug.Log($"[ResearchDebugger] Research already in queue: {researchId}");
                return;
            }
            
            if (_currentResearchId == researchId)
            {
                UnityEngine.Debug.Log($"[ResearchDebugger] Research already active: {researchId}");
                return;
            }
            
            // Check prerequisites
            var research = availableResearch.Find(r => r.researchId == researchId);
            if (research != null)
            {
                foreach (var prereq in research.prerequisites)
                {
                    if (!_completedResearch.Contains(prereq))
                    {
                        UnityEngine.Debug.LogWarning($"[ResearchDebugger] Missing prerequisite: {prereq}");
                        return;
                    }
                }
            }
            
            _researchQueue.Add(researchId);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Added to queue: {research?.researchName ?? researchId}");
            }
            
            // Start if no active research
            if (string.IsNullOrEmpty(_currentResearchId))
            {
                ProcessQueue();
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Remove research from queue
        /// </summary>
        public void RemoveFromQueue(string researchId)
        {
            _researchQueue.Remove(researchId);
            UpdateDisplay();
        }

        /// <summary>
        /// Clear the research queue
        /// </summary>
        public void ClearResearchQueue()
        {
            _researchQueue.Clear();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Research queue cleared");
            }
            
            UpdateDisplay();
        }

        private void ProcessQueue()
        {
            if (!string.IsNullOrEmpty(_currentResearchId)) return;
            if (_researchQueue.Count == 0) return;
            
            string nextResearch = _researchQueue[0];
            _researchQueue.RemoveAt(0);
            
            StartResearch(nextResearch);
        }

        private void StartResearch(string researchId)
        {
            _currentResearchId = researchId;
            _currentResearchProgress = 0f;
            
            var research = availableResearch.Find(r => r.researchId == researchId);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Started research: {research?.researchName ?? researchId}");
            }
            
            UpdateDisplay();
        }
        #endregion

        #region Research Points
        /// <summary>
        /// Add research points
        /// </summary>
        public void AddResearchPoints(float amount)
        {
            _researchPoints += amount;
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Added {amount} research points");
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Set research points
        /// </summary>
        public void SetResearchPoints(float amount)
        {
            _researchPoints = Mathf.Max(0, amount);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Set research points to {amount}");
            }
            
            UpdateDisplay();
        }

        private void AddResearchPointsFromInput()
        {
            if (researchPointsInput != null && float.TryParse(researchPointsInput.text, out float amount))
            {
                AddResearchPoints(amount);
            }
        }

        private void SetResearchPointsFromInput()
        {
            if (researchPointsInput != null && float.TryParse(researchPointsInput.text, out float amount))
            {
                SetResearchPoints(amount);
            }
        }
        #endregion

        #region Progress Control
        private void ProgressResearch(float deltaTime)
        {
            if (string.IsNullOrEmpty(_currentResearchId)) return;
            
            var research = availableResearch.Find(r => r.researchId == _currentResearchId);
            if (research == null) return;
            
            float progressRate = 1f / (research.baseDuration * 3600f); // Convert hours to seconds
            _currentResearchProgress += deltaTime * progressRate * GetResearchSpeedMultiplier();
            
            if (_currentResearchProgress >= 1f)
            {
                _currentResearchProgress = 1f;
                CompleteCurrentResearch();
            }
            
            UpdateDisplay();
        }

        private void OnProgressSliderChanged(float value)
        {
            if (!string.IsNullOrEmpty(_currentResearchId))
            {
                _currentResearchProgress = value;
                UpdateDisplay();
            }
        }

        private float GetResearchSpeedMultiplier()
        {
            // Return multiplier based on research points, upgrades, etc.
            return 1f + (_researchPoints / 10000f);
        }

        private float GetTimeScale()
        {
            return DebugManager.Instance?.timeScale ?? 1f;
        }
        #endregion

        #region Bonuses
        private void ApplyResearchBonuses(DebugResearchData research)
        {
            foreach (var bonus in research.bonuses)
            {
                UnityEngine.Debug.Log($"[ResearchDebugger] Applied bonus: {bonus.type} = {bonus.value:F2} on {bonus.target}");
            }
        }

        /// <summary>
        /// Get all active bonuses
        /// </summary>
        public List<ResearchBonus> GetActiveBonuses()
        {
            List<ResearchBonus> bonuses = new List<ResearchBonus>();
            
            foreach (var researchId in _completedResearch)
            {
                var research = availableResearch.Find(r => r.researchId == researchId);
                if (research != null)
                {
                    bonuses.AddRange(research.bonuses);
                }
            }
            
            return bonuses;
        }
        #endregion

        #region Display Update
        private void UpdateDisplay()
        {
            // Current research
            if (currentResearchText != null)
            {
                if (!string.IsNullOrEmpty(_currentResearchId))
                {
                    var research = availableResearch.Find(r => r.researchId == _currentResearchId);
                    currentResearchText.text = $"Current: {research?.researchName ?? _currentResearchId}\n" +
                                              $"Progress: {_currentResearchProgress * 100f:F1}%";
                }
                else
                {
                    currentResearchText.text = "No active research";
                }
            }
            
            // Progress slider
            if (progressSlider != null)
            {
                progressSlider.value = _currentResearchProgress;
                progressSlider.interactable = !string.IsNullOrEmpty(_currentResearchId);
            }
            
            // Research queue
            if (researchQueueText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"Queue ({_researchQueue.Count}):");
                
                for (int i = 0; i < _researchQueue.Count && i < 5; i++)
                {
                    var research = availableResearch.Find(r => r.researchId == _researchQueue[i]);
                    sb.AppendLine($"{i + 1}. {research?.researchName ?? _researchQueue[i]}");
                }
                
                if (_researchQueue.Count > 5)
                {
                    sb.AppendLine($"... and {_researchQueue.Count - 5} more");
                }
                
                researchQueueText.text = sb.ToString();
            }
            
            // Unlocked technologies
            if (unlockedTechsText != null)
            {
                unlockedTechsText.text = $"Completed: {_completedResearch.Count}/{availableResearch.Count}\n" +
                                        $"Unlocked Techs: {_unlockedTechnologies.Count}\n" +
                                        $"Research Points: {_researchPoints:F0}";
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Check if research is completed
        /// </summary>
        public bool IsResearchCompleted(string researchId)
        {
            return _completedResearch.Contains(researchId);
        }

        /// <summary>
        /// Check if technology is unlocked
        /// </summary>
        public bool IsTechnologyUnlocked(string techId)
        {
            return _unlockedTechnologies.Contains(techId);
        }

        /// <summary>
        /// Get current research ID
        /// </summary>
        public string GetCurrentResearchId()
        {
            return _currentResearchId;
        }

        /// <summary>
        /// Get research progress
        /// </summary>
        public float GetCurrentResearchProgress()
        {
            return _currentResearchProgress;
        }

        /// <summary>
        /// Get queue count
        /// </summary>
        public int GetQueueCount()
        {
            return _researchQueue.Count;
        }

        /// <summary>
        /// Get completed research count
        /// </summary>
        public int GetCompletedCount()
        {
            return _completedResearch.Count;
        }

        /// <summary>
        /// Export research data
        /// </summary>
        public string ExportResearchData()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== RESEARCH STATUS ===");
            sb.AppendLine($"Completed: {_completedResearch.Count}/{availableResearch.Count}");
            sb.AppendLine();
            
            sb.AppendLine("Completed Research:");
            foreach (var id in _completedResearch)
            {
                var research = availableResearch.Find(r => r.researchId == id);
                sb.AppendLine($"  - {research?.researchName ?? id}");
            }
            
            sb.AppendLine();
            sb.AppendLine("Active Bonuses:");
            foreach (var bonus in GetActiveBonuses())
            {
                sb.AppendLine($"  - {bonus.type}: {bonus.value:F2} ({bonus.target})");
            }
            
            return sb.ToString();
        }
        #endregion
    }
}
