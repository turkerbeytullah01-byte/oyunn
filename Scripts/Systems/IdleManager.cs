using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Idle
{
    /// <summary>
    /// Main controller for idle/offline progression system.
    /// Handles app lifecycle, calculates offline progress, and applies rewards on login.
    /// </summary>
    public class IdleManager : BaseManager<IdleManager>
    {
        [Header("Offline Settings")]
        [SerializeField] private float maxOfflineHours = 4f;
        [SerializeField] private bool enableOfflineProgress = true;
        [SerializeField] private bool showOfflinePopup = true;
        
        [Header("Anti-Cheat")]
        [SerializeField] private bool zeroProgressOnRollback = true;
        [SerializeField] private bool capProgressOnRollback = true;
        
        // Runtime state
        private DateTime _lastLogoutTime;
        private DateTime _sessionStartTime;
        private bool _isCalculatingOffline;
        private OfflineProgressResult _lastOfflineResult;
        private IdleSaveData _saveData;
        
        // Constants
        private const string PREFS_IDLE_DATA = "PA_IdleSaveData";
        private const float SAVE_INTERVAL_SECONDS = 30f;
        private float _saveTimer;
        
        // Dependencies (would be injected in real implementation)
        private ProductionManager _productionManager;
        private TimestampManager _timestampManager;
        // private ResearchManager _researchManager; // Would integrate with research system
        // private EconomyManager _economyManager; // Would integrate with economy
        
        // Events
        public event Action<OfflineProgressResult> OnOfflineProgressCalculated;
        public event Action<OfflineProgressResult> OnOfflineProgressApplied;
        public event Action OnApplicationPaused;
        public event Action OnApplicationResumed;
        public event Action<DateTime> OnLogoutTimeSet;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _sessionStartTime = DateTime.UtcNow;
            LoadSaveData();
        }

        private void Start()
        {
            // Get dependencies
            _productionManager = ProductionManager.Instance;
            _timestampManager = TimestampManager.Instance;
            
            // Calculate offline progress on first start
            if (enableOfflineProgress && _lastLogoutTime != DateTime.MinValue)
            {
                CalculateAndApplyOfflineProgress();
            }
            
            // Increment session counter
            _saveData.totalSessions++;
        }

        private void Update()
        {
            // Periodic auto-save
            _saveTimer += Time.deltaTime;
            if (_saveTimer >= SAVE_INTERVAL_SECONDS)
            {
                _saveTimer = 0;
                SaveCurrentState();
            }
            
            // Track play time
            _saveData.totalPlayTimeMinutes += Time.deltaTime / 60f;
        }

        private void OnApplicationPause(bool pause)
        {
            OnApplicationPauseInternal(pause);
        }

        private void OnApplicationFocus(bool focus)
        {
            OnApplicationFocusInternal(focus);
        }

        private void OnDestroy()
        {
            // Ensure we save on destroy
            SaveCurrentState();
        }

        #endregion

        #region Application Lifecycle

        /// <summary>
        /// Handles application pause (backgrounding)
        /// </summary>
        public void OnApplicationPauseInternal(bool pause)
        {
            if (pause)
            {
                Debug.Log("[IdleManager] Application paused - saving state");
                OnApplicationPaused?.Invoke();
                SaveCurrentState();
                SetLastLogoutTime(DateTime.UtcNow);
            }
            else
            {
                Debug.Log("[IdleManager] Application resumed");
                OnApplicationResumed?.Invoke();
                
                if (enableOfflineProgress)
                {
                    CalculateAndApplyOfflineProgress();
                }
            }
        }

        /// <summary>
        /// Handles application focus changes
        /// </summary>
        public void OnApplicationFocusInternal(bool focus)
        {
            if (focus)
            {
                // App gained focus - check for offline progress
                if (enableOfflineProgress && !_isCalculatingOffline)
                {
                    // Small delay to ensure all systems are ready
                    Invoke(nameof(DelayedOfflineCheck), 0.5f);
                }
            }
        }

        private void DelayedOfflineCheck()
        {
            if (_lastLogoutTime != DateTime.MinValue)
            {
                TimeSpan sinceLastLogout = DateTime.UtcNow - _lastLogoutTime;
                if (sinceLastLogout.TotalMinutes > 1)
                {
                    CalculateAndApplyOfflineProgress();
                }
            }
        }

        #endregion

        #region Offline Progress Calculation

        /// <summary>
        /// Calculates offline progress and applies it
        /// </summary>
        public void CalculateAndApplyOfflineProgress()
        {
            if (_isCalculatingOffline) return;
            
            _isCalculatingOffline = true;
            
            try
            {
                var result = CalculateOfflineProgress();
                _lastOfflineResult = result;
                
                OnOfflineProgressCalculated?.Invoke(result);
                
                if (result.HasProgress() || result.wasCapped)
                {
                    ApplyOfflineProgress(result);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[IdleManager] Error calculating offline progress: {e}");
            }
            finally
            {
                _isCalculatingOffline = false;
            }
        }

        /// <summary>
        /// Calculates what progress was made while offline
        /// </summary>
        public OfflineProgressResult CalculateOfflineProgress()
        {
            if (_lastLogoutTime == DateTime.MinValue)
            {
                Debug.Log("[IdleManager] No logout time recorded, skipping offline calculation");
                return OfflineProgressResult.CreateEmpty();
            }
            
            var result = new OfflineProgressResult
            {
                logoutTime = _lastLogoutTime,
                loginTime = DateTime.UtcNow
            };
            
            // Get safe duration with anti-cheat
            TimeSpan offlineDuration = _timestampManager != null 
                ? _timestampManager.GetSafeOfflineDuration(_lastLogoutTime)
                : DateTime.UtcNow - _lastLogoutTime;
            
            result.offlineDuration = offlineDuration;
            
            // Check for rollback
            if (_timestampManager != null && _timestampManager.DetectTimeRollback())
            {
                result.timeRollbackDetected = true;
                
                if (zeroProgressOnRollback)
                {
                    Debug.LogWarning("[IdleManager] Time rollback detected - zeroing offline progress");
                    return OfflineProgressResult.CreateRollbackDetected(_lastLogoutTime, DateTime.UtcNow);
                }
                
                if (capProgressOnRollback)
                {
                    Debug.LogWarning("[IdleManager] Time rollback detected - capping offline progress");
                    offlineDuration = TimeSpan.FromMinutes(30); // Cap at 30 min on rollback
                }
            }
            
            // Apply max offline cap
            TimeSpan maxOffline = GetMaxOfflineTime();
            if (offlineDuration > maxOffline)
            {
                result.wasCapped = true;
                offlineDuration = maxOffline;
            }
            
            result.cappedDurationMinutes = (float)offlineDuration.TotalMinutes;
            
            // Calculate production earnings
            if (_productionManager != null)
            {
                result.productionContributions = _productionManager.CalculateOfflineProduction(offlineDuration);
                result.moneyEarned = result.productionContributions.Values.Sum();
            }
            
            // Calculate research progress (would integrate with ResearchManager)
            result.researchProgress = CalculateResearchProgress(offlineDuration);
            result.completedResearches = GetCompletedResearches(result.researchProgress);
            
            // Update statistics
            _saveData.totalOfflineTimeMinutes += (float)offlineDuration.TotalMinutes;
            _saveData.lastOfflineCalculation = DateTime.UtcNow;
            _saveData.totalOfflineEarnings += result.moneyEarned;
            _saveData.offlineSessionsCount++;
            
            Debug.Log($"[IdleManager] Offline progress calculated: {result.GetFormattedDuration()}, " +
                      $"Capped: {result.GetFormattedCappedDuration()}, Money: {result.moneyEarned:F0}");
            
            return result;
        }

        /// <summary>
        /// Applies calculated offline progress to the game
        /// </summary>
        public void ApplyOfflineProgress(OfflineProgressResult result)
        {
            if (result == null) return;
            
            // Apply money
            if (result.moneyEarned > 0)
            {
                // Would call EconomyManager.AddMoney(result.moneyEarned);
                Debug.Log($"[IdleManager] Applied offline money: {result.moneyEarned:F0}");
            }
            
            // Apply research progress
            foreach (var kvp in result.researchProgress)
            {
                // Would call ResearchManager.AddProgress(kvp.Key, kvp.Value);
                Debug.Log($"[IdleManager] Applied research progress: {kvp.Key} +{kvp.Value:P1}");
            }
            
            // Handle completed researches
            foreach (var researchId in result.completedResearches)
            {
                // Would call ResearchManager.CompleteResearch(researchId);
                Debug.Log($"[IdleManager] Research completed offline: {researchId}");
            }
            
            // Clear logout time after applying
            _lastLogoutTime = DateTime.MinValue;
            SaveCurrentState();
            
            OnOfflineProgressApplied?.Invoke(result);
            
            // Show popup if enabled
            if (showOfflinePopup && result.HasProgress())
            {
                ShowOfflineProgressPopup(result);
            }
        }

        /// <summary>
        /// Shows the offline progress popup UI
        /// </summary>
        private void ShowOfflineProgressPopup(OfflineProgressResult result)
        {
            // Would trigger UI popup
            Debug.Log($"[IdleManager] Showing offline popup: {result.GetFormattedDuration()} offline, " +
                      $"earned {result.moneyEarned:F0} money");
            
            // Example: UIManager.Instance.ShowOfflineProgressPopup(result);
        }

        #endregion

        #region Research Progress (Placeholder)

        /// <summary>
        /// Calculates research progress during offline time
        /// </summary>
        private Dictionary<string, float> CalculateResearchProgress(TimeSpan duration)
        {
            var progress = new Dictionary<string, float>();
            
            // Would integrate with ResearchManager to get active researches
            // For each active research:
            //   float rate = research.GetProgressPerMinute();
            //   progress[research.id] = rate * (float)duration.TotalMinutes;
            
            // Placeholder implementation
            if (_saveData.researchProgressAtLogout != null)
            {
                foreach (var kvp in _saveData.researchProgressAtLogout)
                {
                    if (!kvp.Value.isPaused)
                    {
                        float addedProgress = kvp.Value.baseResearchRate * (float)duration.TotalMinutes;
                        progress[kvp.Key] = addedProgress;
                    }
                }
            }
            
            return progress;
        }

        /// <summary>
        /// Gets list of researches that were completed during offline time
        /// </summary>
        private List<string> GetCompletedResearches(Dictionary<string, float> progressAdded)
        {
            var completed = new List<string>();
            
            // Would check each research's progress + added progress >= 1
            // If so, add to completed list
            
            return completed;
        }

        /// <summary>
        /// Saves current research states for offline calculation
        /// </summary>
        public void SaveResearchStates()
        {
            // Would be called before logout to snapshot research progress
            // _saveData.researchProgressAtLogout = ResearchManager.Instance.GetAllProgress();
        }

        #endregion

        #region Timestamp Management

        /// <summary>
        /// Gets the last logout time
        /// </summary>
        public DateTime GetLastLogoutTime()
        {
            return _lastLogoutTime;
        }

        /// <summary>
        /// Sets the last logout time
        /// </summary>
        public void SetLastLogoutTime(DateTime time)
        {
            _lastLogoutTime = time;
            _saveData.lastLogoutTime = time;
            SaveCurrentState();
            OnLogoutTimeSet?.Invoke(time);
            
            Debug.Log($"[IdleManager] Logout time set: {time}");
        }

        /// <summary>
        /// Gets the maximum allowed offline time
        /// </summary>
        public TimeSpan GetMaxOfflineTime()
        {
            return TimeSpan.FromHours(maxOfflineHours);
        }

        /// <summary>
        /// Sets the maximum offline time
        /// </summary>
        public void SetMaxOfflineTime(float hours)
        {
            maxOfflineHours = Mathf.Max(0.5f, hours);
            _saveData.maxOfflineHours = maxOfflineHours;
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Saves current idle system state
        /// </summary>
        public void SaveCurrentState()
        {
            _saveData.lastSaveTime = DateTime.UtcNow;
            
            // Save production lines
            if (_productionManager != null)
            {
                _saveData.productionLines = _productionManager.GetSaveData();
                _saveData.globalProductionMultiplier = 1.0f; // Would get from ProductionManager
                _saveData.premiumMultiplier = 1.0f;
            }
            
            // Save anti-cheat data
            if (_timestampManager != null)
            {
                _saveData.detectedRollbackCount = _timestampManager.GetRollbackCount();
                _saveData.wasFlaggedForCheating = _timestampManager.IsFlaggedForCheating;
            }
            
            // Serialize and save
            string json = JsonUtility.ToJson(_saveData);
            PlayerPrefs.SetString(PREFS_IDLE_DATA, json);
            PlayerPrefs.Save();
            
            // Also save timestamp
            if (_timestampManager != null)
            {
                _timestampManager.SaveTimestamp();
            }
            
            Debug.Log("[IdleManager] State saved");
        }

        /// <summary>
        /// Loads idle system state
        /// </summary>
        private void LoadSaveData()
        {
            if (PlayerPrefs.HasKey(PREFS_IDLE_DATA))
            {
                string json = PlayerPrefs.GetString(PREFS_IDLE_DATA);
                try
                {
                    _saveData = JsonUtility.FromJson<IdleSaveData>(json) ?? new IdleSaveData();
                    _lastLogoutTime = _saveData.lastLogoutTime;
                    
                    Debug.Log($"[IdleManager] Save data loaded. Last logout: {_lastLogoutTime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[IdleManager] Failed to load save data: {e}");
                    _saveData = new IdleSaveData();
                }
            }
            else
            {
                _saveData = new IdleSaveData();
                _saveData.firstInstallTime = DateTime.UtcNow;
                Debug.Log("[IdleManager] No save data found, creating new");
            }
        }

        /// <summary>
        /// Gets the save data (for external save systems)
        /// </summary>
        public IdleSaveData GetSaveData()
        {
            return _saveData;
        }

        /// <summary>
        /// Loads from external save data
        /// </summary>
        public void LoadFromSaveData(IdleSaveData data)
        {
            _saveData = data ?? new IdleSaveData();
            _lastLogoutTime = _saveData.lastLogoutTime;
            
            // Load production lines
            if (_productionManager != null && _saveData.productionLines != null)
            {
                _productionManager.LoadFromSaveData(_saveData.productionLines);
            }
            
            Debug.Log("[IdleManager] Loaded from external save data");
        }

        #endregion

        #region Settings

        /// <summary>
        /// Enables or disables offline progress
        /// </summary>
        public void SetOfflineProgressEnabled(bool enabled)
        {
            enableOfflineProgress = enabled;
            _saveData.offlineProgressEnabled = enabled;
        }

        /// <summary>
        /// Whether offline progress is enabled
        /// </summary>
        public bool IsOfflineProgressEnabled => enableOfflineProgress;

        /// <summary>
        /// Gets the last offline calculation result
        /// </summary>
        public OfflineProgressResult GetLastOfflineResult()
        {
            return _lastOfflineResult;
        }

        #endregion

        #region Debug/Testing

        /// <summary>
        /// Simulates an offline period for testing
        /// </summary>
        public void SimulateOfflinePeriod(float hours)
        {
            DateTime simulatedLogout = DateTime.UtcNow.AddHours(-hours);
            _lastLogoutTime = simulatedLogout;
            
            Debug.Log($"[IdleManager] Simulating {hours} hours offline");
            CalculateAndApplyOfflineProgress();
        }

        /// <summary>
        /// Clears all saved data (for testing)
        /// </summary>
        public void ClearAllData()
        {
            PlayerPrefs.DeleteKey(PREFS_IDLE_DATA);
            _saveData = new IdleSaveData();
            _lastLogoutTime = DateTime.MinValue;
            _lastOfflineResult = null;
            
            Debug.Log("[IdleManager] All data cleared");
        }

        /// <summary>
        /// Gets debug information
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Idle Manager ===");
            sb.AppendLine($"Last Logout: {_lastLogoutTime}");
            sb.AppendLine($"Session Start: {_sessionStartTime}");
            sb.AppendLine($"Max Offline: {maxOfflineHours} hours");
            sb.AppendLine($"Offline Enabled: {enableOfflineProgress}");
            sb.AppendLine($"Total Sessions: {_saveData.totalSessions}");
            sb.AppendLine($"Total Play Time: {_saveData.totalPlayTimeMinutes:F1} min");
            sb.AppendLine($"Total Offline Time: {_saveData.totalOfflineTimeMinutes:F1} min");
            sb.AppendLine($"Total Offline Earnings: {_saveData.totalOfflineEarnings:F0}");
            sb.AppendLine($"Offline Sessions: {_saveData.offlineSessionsCount}");
            
            if (_lastOfflineResult != null)
            {
                sb.AppendLine("\nLast Offline Result:");
                sb.AppendLine($"  Duration: {_lastOfflineResult.GetFormattedDuration()}");
                sb.AppendLine($"  Capped: {_lastOfflineResult.wasCapped}");
                sb.AppendLine($"  Money: {_lastOfflineResult.moneyEarned:F0}");
                sb.AppendLine($"  Rollback: {_lastOfflineResult.timeRollbackDetected}");
            }
            
            return sb.ToString();
        }

        #endregion
    }
}
