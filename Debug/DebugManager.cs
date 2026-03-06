using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Main debug controller for Project Aegis: Drone Dominion
    /// Provides centralized access to all debug functionality
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        #region Singleton
        private static DebugManager _instance;
        public static DebugManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DebugManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DebugManager");
                        _instance = go.AddComponent<DebugManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Settings
        [Header("Settings")]
        [Tooltip("Enable or disable debug mode entirely")]
        public bool enableDebugMode = true;
        
        [Tooltip("Key to toggle debug panel visibility")]
        public KeyCode debugToggleKey = KeyCode.BackQuote;
        
        [Tooltip("Show debug UI on startup")]
        public bool showDebugPanelOnStart = false;
        
        [Tooltip("Log all debug actions to console")]
        public bool logDebugActions = true;
        #endregion

        #region Time Control
        [Header("Time Control")]
        [Tooltip("Current time scale multiplier")]
        [Range(0f, 100f)]
        public float timeScale = 1f;
        
        [Tooltip("Preset time scale values for quick selection")]
        public List<float> timeScalePresets = new List<float> { 0.5f, 1f, 2f, 5f, 10f, 50f, 100f };
        
        [Tooltip("Maximum allowed time scale")]
        public float maxTimeScale = 1000f;
        
        [Tooltip("Whether to pause game time")]
        public bool isPaused = false;
        #endregion

        #region Cheats
        [Header("Cheats")]
        [Tooltip("Amount of money to add with cheat")]
        public float moneyCheatAmount = 10000f;
        
        [Tooltip("Amount of reputation to add with cheat")]
        public float reputationCheatAmount = 10f;
        
        [Tooltip("Amount of research points to add with cheat")]
        public float researchPointsCheatAmount = 1000f;
        
        [Tooltip("Amount of drone parts to add with cheat")]
        public int dronePartsCheatAmount = 100;
        #endregion

        #region Events
        [Header("Events")]
        public event Action<bool> OnDebugModeChanged;
        public event Action<float> OnTimeScaleChanged;
        public event Action<string> OnDebugActionLogged;
        #endregion

        #region Private Fields
        private GameObject _debugPanel;
        private bool _isDebugPanelVisible = false;
        private float _originalTimeScale = 1f;
        private List<string> _debugLog = new List<string>();
        private const int MAX_LOG_ENTRIES = 100;
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
            
            _originalTimeScale = Time.timeScale;
        }

        private void Update()
        {
            if (!enableDebugMode) return;
            
            HandleInput();
            UpdateTimeScale();
        }

        private void OnDestroy()
        {
            // Reset time scale on destroy
            Time.timeScale = _originalTimeScale;
        }
        #endregion

        #region Input Handling
        private void HandleInput()
        {
            // Toggle debug panel
            if (Input.GetKeyDown(debugToggleKey))
            {
                ToggleDebugPanel();
            }
            
            // Quick shortcuts (only when debug mode enabled)
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.F1)) CycleTimeScale();
                if (Input.GetKeyDown(KeyCode.F2)) AddMoney(moneyCheatAmount);
                if (Input.GetKeyDown(KeyCode.F3)) CompleteCurrentResearch();
                if (Input.GetKeyDown(KeyCode.F4)) TriggerRandomEvent();
                if (Input.GetKeyDown(KeyCode.F5)) SaveGame();
                if (Input.GetKeyDown(KeyCode.F9)) ResetAllProgress();
            }
        }
        #endregion

        #region Debug Panel
        /// <summary>
        /// Toggle the debug panel visibility
        /// </summary>
        public void ToggleDebugPanel()
        {
            if (_debugPanel == null)
            {
                CreateDebugPanel();
            }
            
            _isDebugPanelVisible = !_isDebugPanelVisible;
            _debugPanel.SetActive(_isDebugPanelVisible);
            
            LogDebugAction($"Debug panel {(_isDebugPanelVisible ? "shown" : "hidden")}");
            OnDebugModeChanged?.Invoke(_isDebugPanelVisible);
        }

        /// <summary>
        /// Show the debug panel
        /// </summary>
        public void ShowDebugPanel()
        {
            if (_debugPanel == null) CreateDebugPanel();
            _isDebugPanelVisible = true;
            _debugPanel.SetActive(true);
        }

        /// <summary>
        /// Hide the debug panel
        /// </summary>
        public void HideDebugPanel()
        {
            if (_debugPanel != null)
            {
                _isDebugPanelVisible = false;
                _debugPanel.SetActive(false);
            }
        }

        private void CreateDebugPanel()
        {
            // This will be set up by DebugConsoleUI
            // For now, just find or create the panel reference
            _debugPanel = GameObject.Find("DebugPanel");
            if (_debugPanel == null)
            {
                // Panel will be created by DebugConsoleUI
                UnityEngine.Debug.LogWarning("DebugPanel not found. Ensure DebugConsoleUI is set up in the scene.");
            }
        }
        #endregion

        #region Time Control
        /// <summary>
        /// Set the time scale for fast-forwarding
        /// </summary>
        public void SetTimeScale(float scale)
        {
            timeScale = Mathf.Clamp(scale, 0f, maxTimeScale);
            
            if (!isPaused)
            {
                Time.timeScale = timeScale;
            }
            
            LogDebugAction($"Time scale set to {timeScale:F2}x");
            OnTimeScaleChanged?.Invoke(timeScale);
        }

        /// <summary>
        /// Cycle through preset time scales
        /// </summary>
        public void CycleTimeScale()
        {
            int currentIndex = timeScalePresets.FindIndex(x => Mathf.Approximately(x, timeScale));
            int nextIndex = (currentIndex + 1) % timeScalePresets.Count;
            SetTimeScale(timeScalePresets[nextIndex]);
        }

        /// <summary>
        /// Reset time scale to normal (1x)
        /// </summary>
        public void ResetTimeScale()
        {
            SetTimeScale(1f);
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            LogDebugAction("Game paused");
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = timeScale;
            LogDebugAction("Game resumed");
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        private void UpdateTimeScale()
        {
            if (!isPaused && Time.timeScale != timeScale)
            {
                Time.timeScale = timeScale;
            }
        }
        #endregion

        #region Money Cheats
        /// <summary>
        /// Add money to the player's account
        /// </summary>
        public void AddMoney(float amount)
        {
            // Call GameManager to add money
            // GameManager.Instance?.AddMoney(amount);
            LogDebugAction($"Added {amount:C} money");
            
            #if UNITY_EDITOR
            UnityEngine.Debug.Log($"[DEBUG] Added {amount:C} money");
            #endif
        }

        /// <summary>
        /// Set money to a specific amount
        /// </summary>
        public void SetMoney(float amount)
        {
            // GameManager.Instance?.SetMoney(amount);
            LogDebugAction($"Set money to {amount:C}");
        }

        /// <summary>
        /// Remove all money
        /// </summary>
        public void ClearMoney()
        {
            SetMoney(0);
        }
        #endregion

        #region Reputation Cheats
        /// <summary>
        /// Add reputation points
        /// </summary>
        public void AddReputation(float amount)
        {
            // GameManager.Instance?.AddReputation(amount);
            LogDebugAction($"Added {amount} reputation");
        }

        /// <summary>
        /// Set reputation to a specific value
        /// </summary>
        public void SetReputation(float amount)
        {
            amount = Mathf.Clamp(amount, 0f, 100f);
            // GameManager.Instance?.SetReputation(amount);
            LogDebugAction($"Set reputation to {amount}");
        }
        #endregion

        #region Research Cheats
        /// <summary>
        /// Complete the current active research
        /// </summary>
        public void CompleteCurrentResearch()
        {
            // ResearchManager.Instance?.CompleteCurrentResearch();
            LogDebugAction("Completed current research");
        }

        /// <summary>
        /// Complete all research
        /// </summary>
        public void CompleteAllResearch()
        {
            // ResearchManager.Instance?.CompleteAllResearch();
            LogDebugAction("Completed all research");
        }

        /// <summary>
        /// Unlock all technologies
        /// </summary>
        public void UnlockAllTechnologies()
        {
            // ResearchManager.Instance?.UnlockAllTechnologies();
            LogDebugAction("Unlocked all technologies");
        }

        /// <summary>
        /// Reset all research progress
        /// </summary>
        public void ResetResearchProgress()
        {
            // ResearchManager.Instance?.ResetAllProgress();
            LogDebugAction("Reset all research progress");
        }

        /// <summary>
        /// Add research points
        /// </summary>
        public void AddResearchPoints(float amount)
        {
            // ResearchManager.Instance?.AddResearchPoints(amount);
            LogDebugAction($"Added {amount} research points");
        }
        #endregion

        #region Event Cheats
        /// <summary>
        /// Trigger a random event
        /// </summary>
        public void TriggerRandomEvent()
        {
            // DynamicEventManager.Instance?.TriggerRandomEvent();
            LogDebugAction("Triggered random event");
        }

        /// <summary>
        /// Trigger a specific event by ID
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            // DynamicEventManager.Instance?.TriggerEvent(eventId);
            LogDebugAction($"Triggered event: {eventId}");
        }

        /// <summary>
        /// Clear all active event effects
        /// </summary>
        public void ClearActiveEffects()
        {
            // DynamicEventManager.Instance?.ClearAllEffects();
            LogDebugAction("Cleared all active effects");
        }
        #endregion

        #region Contract Cheats
        /// <summary>
        /// Generate test contracts
        /// </summary>
        public void GenerateTestContracts(int count)
        {
            // ContractManager.Instance?.GenerateTestContracts(count);
            LogDebugAction($"Generated {count} test contracts");
        }

        /// <summary>
        /// Complete all active contracts
        /// </summary>
        public void CompleteAllContracts()
        {
            // ContractManager.Instance?.CompleteAllContracts();
            LogDebugAction("Completed all contracts");
        }

        /// <summary>
        /// Win all pending bids
        /// </summary>
        public void WinAllBids()
        {
            // ContractManager.Instance?.WinAllBids();
            LogDebugAction("Won all pending bids");
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Save the game
        /// </summary>
        public void SaveGame()
        {
            // SaveManager.Instance?.SaveGame();
            LogDebugAction("Game saved");
        }

        /// <summary>
        /// Load the game
        /// </summary>
        public void LoadGame()
        {
            // SaveManager.Instance?.LoadGame();
            LogDebugAction("Game loaded");
        }

        /// <summary>
        /// Delete all save data
        /// </summary>
        public void DeleteSave()
        {
            // SaveManager.Instance?.DeleteSave();
            LogDebugAction("Save data deleted");
        }

        /// <summary>
        /// Reset all progress
        /// </summary>
        public void ResetAllProgress()
        {
            // SaveManager.Instance?.ResetAllProgress();
            LogDebugAction("All progress reset");
        }
        #endregion

        #region Offline Simulation
        /// <summary>
        /// Simulate offline time progression
        /// </summary>
        public void SimulateOfflineTime(float hours)
        {
            // TimeManager.Instance?.SimulateOfflineTime(hours);
            LogDebugAction($"Simulated {hours} hours of offline time");
        }

        /// <summary>
        /// Simulate offline time in minutes
        /// </summary>
        public void SimulateOfflineMinutes(float minutes)
        {
            SimulateOfflineTime(minutes / 60f);
        }

        /// <summary>
        /// Simulate offline time in days
        /// </summary>
        public void SimulateOfflineDays(float days)
        {
            SimulateOfflineTime(days * 24f);
        }
        #endregion

        #region Logging
        /// <summary>
        /// Log a debug action
        /// </summary>
        public void LogDebugAction(string message)
        {
            if (!logDebugActions) return;
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";
            
            _debugLog.Add(logEntry);
            
            // Keep log size manageable
            if (_debugLog.Count > MAX_LOG_ENTRIES)
            {
                _debugLog.RemoveAt(0);
            }
            
            OnDebugActionLogged?.Invoke(logEntry);
            
            #if UNITY_EDITOR
            UnityEngine.Debug.Log($"[DEBUG] {message}");
            #endif
        }

        /// <summary>
        /// Get all log entries
        /// </summary>
        public List<string> GetDebugLog()
        {
            return new List<string>(_debugLog);
        }

        /// <summary>
        /// Clear the debug log
        /// </summary>
        public void ClearDebugLog()
        {
            _debugLog.Clear();
            LogDebugAction("Debug log cleared");
        }
        #endregion

        #region Utility
        /// <summary>
        /// Check if debug mode is enabled
        /// </summary>
        public bool IsDebugEnabled()
        {
            return enableDebugMode;
        }

        /// <summary>
        /// Enable or disable debug mode
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            enableDebugMode = enabled;
            LogDebugAction($"Debug mode {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetCurrentFPS()
        {
            return 1f / Time.unscaledDeltaTime;
        }

        /// <summary>
        /// Get current memory usage in MB
        /// </summary>
        public long GetMemoryUsageMB()
        {
            return GC.GetTotalMemory(false) / (1024 * 1024);
        }
        #endregion
    }
}
