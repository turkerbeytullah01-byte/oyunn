using System.Collections;
using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Automatic save system
    /// Handles periodic auto-saves and save triggers
    /// </summary>
    public class AutoSave : MonoBehaviour
    {
        #region Settings
        
        [Header("Auto-Save Settings")]
        [Tooltip("Interval between auto-saves in minutes")]
        [SerializeField] private float _autoSaveIntervalMinutes = SaveConstants.DEFAULT_AUTO_SAVE_INTERVAL_MINUTES;
        
        [Tooltip("Save when application is paused (goes to background)")]
        [SerializeField] private bool _saveOnPause = true;
        
        [Tooltip("Save when application loses focus")]
        [SerializeField] private bool _saveOnFocusLost = true;
        
        [Tooltip("Save when scene changes")]
        [SerializeField] private bool _saveOnSceneChange = true;
        
        [Tooltip("Show save indicator")]
        [SerializeField] private bool _showSaveIndicator = true;
        
        [Tooltip("Minimum time between saves (prevents spam)")]
        [SerializeField] private float _minimumSaveIntervalSeconds = 5f;
        
        [Header("Trigger Settings")]
        [Tooltip("Save when money changes by this amount")]
        [SerializeField] private float _moneyChangeThreshold = 1000f;
        
        [Tooltip("Save when reputation changes by this amount")]
        [SerializeField] private float _reputationChangeThreshold = 10f;
        
        [Tooltip("Save when research completes")]
        [SerializeField] private bool _saveOnResearchComplete = true;
        
        [Tooltip("Save when contract state changes")]
        [SerializeField] private bool _saveOnContractChange = true;
        
        #endregion
        
        #region Fields
        
        private float _timeSinceLastAutoSave;
        private float _timeSinceLastSave;
        private bool _isAutoSaveEnabled = true;
        
        // Tracked values for change detection
        private float _lastMoney;
        private float _lastReputation;
        private int _lastCompletedResearchCount;
        private int _lastActiveContractCount;
        
        // Coroutine reference
        private Coroutine _autoSaveCoroutine;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets or sets whether auto-save is enabled
        /// </summary>
        public bool IsAutoSaveEnabled
        {
            get => _isAutoSaveEnabled;
            set => _isAutoSaveEnabled = value;
        }
        
        /// <summary>
        /// Gets or sets the auto-save interval in minutes
        /// </summary>
        public float AutoSaveIntervalMinutes
        {
            get => _autoSaveIntervalMinutes;
            set => _autoSaveIntervalMinutes = Mathf.Clamp(value, 
                SaveConstants.MIN_AUTO_SAVE_INTERVAL_MINUTES, 
                SaveConstants.MAX_AUTO_SAVE_INTERVAL_MINUTES);
        }
        
        /// <summary>
        /// Gets time until next auto-save in seconds
        /// </summary>
        public float TimeUntilNextAutoSave => 
            (_autoSaveIntervalMinutes * 60f) - _timeSinceLastAutoSave;
        
        /// <summary>
        /// Gets progress to next auto-save (0-1)
        /// </summary>
        public float AutoSaveProgress => 
            _timeSinceLastAutoSave / (_autoSaveIntervalMinutes * 60f);
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Validate settings
            _autoSaveIntervalMinutes = Mathf.Clamp(_autoSaveIntervalMinutes,
                SaveConstants.MIN_AUTO_SAVE_INTERVAL_MINUTES,
                SaveConstants.MAX_AUTO_SAVE_INTERVAL_MINUTES);
            
            // Initialize tracked values
            InitializeTrackedValues();
            
            // Start auto-save coroutine
            _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            
            // Subscribe to save manager events
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnGameSaved += OnGameSaved;
            }
            
            Debug.Log($"[AutoSave] Initialized with interval: {_autoSaveIntervalMinutes} minutes");
        }
        
        private void OnDestroy()
        {
            // Stop coroutine
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }
            
            // Unsubscribe from events
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnGameSaved -= OnGameSaved;
            }
        }
        
        private void Update()
        {
            if (!_isAutoSaveEnabled)
                return;
            
            // Update timers
            _timeSinceLastAutoSave += Time.unscaledDeltaTime;
            _timeSinceLastSave += Time.unscaledDeltaTime;
            
            // Check for value changes that should trigger save
            CheckValueChanges();
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause && _saveOnPause)
            {
                TriggerSave("Application Paused");
            }
        }
        
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && _saveOnFocusLost)
            {
                TriggerSave("Focus Lost");
            }
        }
        
        #endregion
        
        #region Auto-Save Coroutine
        
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return null;
                
                if (!_isAutoSaveEnabled)
                    continue;
                
                // Check if it's time to auto-save
                if (_timeSinceLastAutoSave >= _autoSaveIntervalMinutes * 60f)
                {
                    TriggerSave("Auto-Save Interval");
                    _timeSinceLastAutoSave = 0f;
                }
            }
        }
        
        #endregion
        
        #region Value Change Detection
        
        private void InitializeTrackedValues()
        {
            var saveData = SaveManager.Instance?.CurrentSaveData;
            if (saveData != null)
            {
                _lastMoney = saveData.playerData.money;
                _lastReputation = saveData.playerData.reputation;
                _lastCompletedResearchCount = saveData.researchData.completedResearchIds?.Count ?? 0;
                _lastActiveContractCount = saveData.contractsData.activeContracts?.Count ?? 0;
            }
        }
        
        private void CheckValueChanges()
        {
            var saveData = SaveManager.Instance?.CurrentSaveData;
            if (saveData == null)
                return;
            
            // Check money change
            float moneyDiff = Mathf.Abs(saveData.playerData.money - _lastMoney);
            if (moneyDiff >= _moneyChangeThreshold)
            {
                TriggerSave("Money Change");
                _lastMoney = saveData.playerData.money;
                return;
            }
            
            // Check reputation change
            float repDiff = Mathf.Abs(saveData.playerData.reputation - _lastReputation);
            if (repDiff >= _reputationChangeThreshold)
            {
                TriggerSave("Reputation Change");
                _lastReputation = saveData.playerData.reputation;
                return;
            }
            
            // Check research completion
            if (_saveOnResearchComplete)
            {
                int researchCount = saveData.researchData.completedResearchIds?.Count ?? 0;
                if (researchCount > _lastCompletedResearchCount)
                {
                    TriggerSave("Research Complete");
                    _lastCompletedResearchCount = researchCount;
                    return;
                }
            }
            
            // Check contract changes
            if (_saveOnContractChange)
            {
                int contractCount = saveData.contractsData.activeContracts?.Count ?? 0;
                if (contractCount != _lastActiveContractCount)
                {
                    TriggerSave("Contract Change");
                    _lastActiveContractCount = contractCount;
                    return;
                }
            }
        }
        
        #endregion
        
        #region Save Triggers
        
        /// <summary>
        /// Triggers a save with the specified reason
        /// </summary>
        public void TriggerSave(string reason)
        {
            // Check minimum interval
            if (_timeSinceLastSave < _minimumSaveIntervalSeconds)
            {
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[AutoSave] Save skipped ({reason}) - too soon since last save");
                }
                return;
            }
            
            if (SaveManager.HasInstance && !SaveManager.Instance.IsSaving)
            {
                if (SaveConstants.DEBUG_SAVE_OPERATIONS)
                {
                    Debug.Log($"[AutoSave] Triggering save: {reason}");
                }
                
                SaveManager.Instance.SaveGame();
            }
        }
        
        /// <summary>
        /// Forces an immediate save regardless of timing
        /// </summary>
        public void ForceSave(string reason)
        {
            if (SaveManager.HasInstance && !SaveManager.Instance.IsSaving)
            {
                Debug.Log($"[AutoSave] Force saving: {reason}");
                SaveManager.Instance.SaveGame();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnGameSaved()
        {
            _timeSinceLastAutoSave = 0f;
            _timeSinceLastSave = 0f;
            
            // Update tracked values
            InitializeTrackedValues();
            
            if (_showSaveIndicator)
            {
                ShowSaveIndicator();
            }
        }
        
        #endregion
        
        #region Save Indicator
        
        private void ShowSaveIndicator()
        {
            // This can be hooked up to a UI element
            // For now, just log
            if (SaveConstants.DEBUG_SAVE_OPERATIONS)
            {
                Debug.Log("[AutoSave] Game Saved");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Resets the auto-save timer
        /// </summary>
        public void ResetTimer()
        {
            _timeSinceLastAutoSave = 0f;
        }
        
        /// <summary>
        /// Pauses auto-save temporarily
        /// </summary>
        public void PauseAutoSave()
        {
            _isAutoSaveEnabled = false;
            Debug.Log("[AutoSave] Auto-save paused");
        }
        
        /// <summary>
        /// Resumes auto-save
        /// </summary>
        public void ResumeAutoSave()
        {
            _isAutoSaveEnabled = true;
            ResetTimer();
            Debug.Log("[AutoSave] Auto-save resumed");
        }
        
        /// <summary>
        /// Performs an immediate save and resets timer
        /// </summary>
        public void SaveNow()
        {
            ForceSave("Manual Save");
        }
        
        #endregion
    }
}
