using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Research
{
    /// <summary>
    /// Save data structure for ResearchManager
    /// </summary>
    [Serializable]
    public class ResearchManagerSaveData
    {
        public List<string> completedResearchIds = new List<string>();
        public ResearchProgressData? activeResearch;
        public List<ResearchProgressData> researchQueue = new List<ResearchProgressData>();
        public string lastSaveTimestamp;
    }

    /// <summary>
    /// Main controller for the research system.
    /// Manages active research, queue, progress tracking, and completion.
    /// </summary>
    public class ResearchManager : MonoBehaviour, ISaveable
    {
        public static ResearchManager Instance { get; private set; }

        [Header("Data")]
        [Tooltip("Technology tree data asset")]
        public TechnologyTreeData technologyTree;

        [Header("Settings")]
        [Tooltip("Maximum number of researches in queue")]
        public int maxQueueSize = 5;
        
        [Tooltip("Update interval for progress in seconds")]
        public float updateInterval = 1f;

        // Runtime state
        private ResearchProgress _activeResearch;
        private Queue<ResearchProgress> _researchQueue = new Queue<ResearchProgress>();
        private HashSet<string> _completedResearchIds = new HashSet<string>();
        private float _updateTimer;

        // Save tracking
        private DateTime _lastSaveTime;

        #region Events
        /// <summary>
        /// Event fired when research starts
        /// Parameters: researchId
        /// </summary>
        public event Action<string> OnResearchStarted;

        /// <summary>
        /// Event fired when research completes
        /// Parameters: researchId
        /// </summary>
        public event Action<string> OnResearchCompleted;

        /// <summary>
        /// Event fired when research progress updates
        /// Parameters: researchId, progress (0-1)
        /// </summary>
        public event Action<string, float> OnResearchProgressUpdated;

        /// <summary>
        /// Event fired when research is cancelled
        /// Parameters: researchId
        /// </summary>
        public event Action<string> OnResearchCancelled;

        /// <summary>
        /// Event fired when a module is unlocked
        /// Parameters: moduleId
        /// </summary>
        public event Action<string> OnModuleUnlocked;

        /// <summary>
        /// Event fired when research becomes available
        /// Parameters: researchId
        /// </summary>
        public event Action<string> OnResearchAvailable;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize tech tree
            technologyTree?.Initialize();
        }

        private void Update()
        {
            if (_activeResearch == null)
                return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateProgress();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Save();
            }
            else
            {
                ApplyOfflineProgress();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }
        #endregion

        #region Research Management
        /// <summary>
        /// Starts a research project
        /// </summary>
        /// <param name="researchId">ID of the research to start</param>
        /// <returns>True if research was started successfully</returns>
        public bool StartResearch(string researchId)
        {
            var researchData = technologyTree?.GetResearch(researchId);
            if (researchData == null)
            {
                Debug.LogError($"[ResearchManager] Research not found: {researchId}");
                return false;
            }

            // Check if already completed
            if (_completedResearchIds.Contains(researchId))
            {
                Debug.LogWarning($"[ResearchManager] Research already completed: {researchId}");
                return false;
            }

            // Check if already active or queued
            if (IsResearchActiveOrQueued(researchId))
            {
                Debug.LogWarning($"[ResearchManager] Research already active or queued: {researchId}");
                return false;
            }

            // Check prerequisites
            if (!CanResearch(researchId))
            {
                Debug.LogWarning($"[ResearchManager] Prerequisites not met for: {researchId}");
                return false;
            }

            // Create progress tracker
            var progress = new ResearchProgress(researchData);
            progress.OnResearchCompleted += () => CompleteResearch(researchId);

            // If no active research, start immediately
            if (_activeResearch == null)
            {
                _activeResearch = progress;
                Debug.Log($"[ResearchManager] Started research: {researchData.displayName}");
                OnResearchStarted?.Invoke(researchId);
            }
            else if (_researchQueue.Count < maxQueueSize)
            {
                // Add to queue
                _researchQueue.Enqueue(progress);
                Debug.Log($"[ResearchManager] Added to queue: {researchData.displayName}");
            }
            else
            {
                Debug.LogWarning($"[ResearchManager] Research queue is full");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cancels the active research
        /// </summary>
        /// <param name="refundPercentage">Percentage of cost to refund (0-1)</param>
        /// <returns>True if research was cancelled</returns>
        public bool CancelResearch(float refundPercentage = 0.5f)
        {
            if (_activeResearch == null)
                return false;

            string researchId = _activeResearch.ResearchId;
            var researchData = technologyTree?.GetResearch(researchId);

            // Calculate refund
            if (researchData != null)
            {
                float refund = researchData.cost * refundPercentage * (1f - _activeResearch.ProgressPercentage);
                // TODO: Add refund to player currency via EconomyManager
                Debug.Log($"[ResearchManager] Refunded {refund} credits for cancelling {researchData.displayName}");
            }

            _activeResearch = null;
            OnResearchCancelled?.Invoke(researchId);

            // Start next in queue
            ProcessQueue();

            return true;
        }

        /// <summary>
        /// Skips the active research (instant completion, typically costs premium currency)
        /// </summary>
        public bool SkipResearch()
        {
            if (_activeResearch == null)
                return false;

            _activeResearch.InstantComplete();
            return true;
        }

        /// <summary>
        /// Completes the active research and processes unlocks
        /// </summary>
        private void CompleteResearch(string researchId)
        {
            var researchData = technologyTree?.GetResearch(researchId);
            if (researchData == null)
                return;

            // Mark as completed
            _completedResearchIds.Add(researchId);

            // Unlock modules
            foreach (var moduleId in researchData.unlocksModules)
            {
                OnModuleUnlocked?.Invoke(moduleId);
                Debug.Log($"[ResearchManager] Unlocked module: {moduleId}");
            }

            // Notify new available researches
            foreach (var unlockedId in researchData.unlocksResearch)
            {
                if (CanResearch(unlockedId))
                {
                    OnResearchAvailable?.Invoke(unlockedId);
                }
            }

            Debug.Log($"[ResearchManager] Completed research: {researchData.displayName}");
            OnResearchCompleted?.Invoke(researchId);

            // Clear active and process queue
            _activeResearch = null;
            ProcessQueue();
        }

        /// <summary>
        /// Processes the research queue
        /// </summary>
        private void ProcessQueue()
        {
            if (_activeResearch != null || _researchQueue.Count == 0)
                return;

            _activeResearch = _researchQueue.Dequeue();
            string researchId = _activeResearch.ResearchId;
            var researchData = technologyTree?.GetResearch(researchId);

            _activeResearch.OnResearchCompleted += () => CompleteResearch(researchId);
            
            Debug.Log($"[ResearchManager] Started queued research: {researchData?.displayName}");
            OnResearchStarted?.Invoke(researchId);
        }

        /// <summary>
        /// Updates the active research progress
        /// </summary>
        private void UpdateProgress()
        {
            if (_activeResearch == null)
                return;

            _activeResearch.Update();
            OnResearchProgressUpdated?.Invoke(_activeResearch.ResearchId, _activeResearch.ProgressPercentage);
        }
        #endregion

        #region Queries
        /// <summary>
        /// Gets the current research progress (0-1)
        /// </summary>
        public float GetProgress()
        {
            return _activeResearch?.ProgressPercentage ?? 0f;
        }

        /// <summary>
        /// Gets the time remaining for current research
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            return _activeResearch?.GetTimeRemaining() ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the formatted time remaining string
        /// </summary>
        public string GetTimeRemainingString()
        {
            return _activeResearch?.GetTimeRemainingString() ?? "--";
        }

        /// <summary>
        /// Checks if research is currently active
        /// </summary>
        public bool IsResearching()
        {
            return _activeResearch != null;
        }

        /// <summary>
        /// Gets the currently active research ID
        /// </summary>
        public string GetActiveResearchId()
        {
            return _activeResearch?.ResearchId;
        }

        /// <summary>
        /// Gets the active research data
        /// </summary>
        public ResearchData GetActiveResearchData()
        {
            return _activeResearch != null 
                ? technologyTree?.GetResearch(_activeResearch.ResearchId) 
                : null;
        }

        /// <summary>
        /// Checks if a research can be started (prerequisites met)
        /// </summary>
        public bool CanResearch(string researchId)
        {
            var researchData = technologyTree?.GetResearch(researchId);
            if (researchData == null)
                return false;

            // Check if already completed
            if (_completedResearchIds.Contains(researchId))
                return false;

            // Check prerequisites
            foreach (var prereqId in researchData.prerequisiteIds)
            {
                if (!_completedResearchIds.Contains(prereqId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all available researches (prerequisites met, not completed)
        /// </summary>
        public List<ResearchData> GetAvailableResearch()
        {
            var available = new List<ResearchData>();
            
            if (technologyTree?.allResearches == null)
                return available;

            foreach (var research in technologyTree.allResearches)
            {
                if (research == null)
                    continue;

                if (CanResearch(research.researchId) && 
                    !IsResearchActiveOrQueued(research.researchId))
                {
                    available.Add(research);
                }
            }

            return available;
        }

        /// <summary>
        /// Gets available researches filtered by category
        /// </summary>
        public List<ResearchData> GetAvailableResearchByCategory(TechCategory category)
        {
            return GetAvailableResearch()
                .Where(r => r.techCategory == category)
                .ToList();
        }

        /// <summary>
        /// Gets all completed research IDs
        /// </summary>
        public List<string> GetCompletedResearch()
        {
            return _completedResearchIds.ToList();
        }

        /// <summary>
        /// Gets completed research data
        /// </summary>
        public List<ResearchData> GetCompletedResearchData()
        {
            var completed = new List<ResearchData>();
            foreach (var id in _completedResearchIds)
            {
                var research = technologyTree?.GetResearch(id);
                if (research != null)
                    completed.Add(research);
            }
            return completed;
        }

        /// <summary>
        /// Checks if a specific research is completed
        /// </summary>
        public bool IsResearchCompleted(string researchId)
        {
            return _completedResearchIds.Contains(researchId);
        }

        /// <summary>
        /// Gets the research queue
        /// </summary>
        public List<ResearchData> GetResearchQueue()
        {
            return _researchQueue
                .Select(p => technologyTree?.GetResearch(p.ResearchId))
                .Where(r => r != null)
                .ToList();
        }

        /// <summary>
        /// Gets the number of items in queue
        /// </summary>
        public int GetQueueCount()
        {
            return _researchQueue.Count;
        }

        /// <summary>
        /// Checks if research is active or queued
        /// </summary>
        private bool IsResearchActiveOrQueued(string researchId)
        {
            if (_activeResearch?.ResearchId == researchId)
                return true;

            return _researchQueue.Any(p => p.ResearchId == researchId);
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Saves the research manager state
        /// </summary>
        public void Save()
        {
            var saveData = new ResearchManagerSaveData
            {
                completedResearchIds = _completedResearchIds.ToList(),
                lastSaveTimestamp = DateTime.UtcNow.ToString("O")
            };

            if (_activeResearch != null)
            {
                saveData.activeResearch = _activeResearch.ToSaveData();
            }

            saveData.researchQueue = _researchQueue.Select(p => p.ToSaveData()).ToList();

            // Save via SaveManager
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("ResearchManager_Save", json);
            PlayerPrefs.Save();

            _lastSaveTime = DateTime.UtcNow;
            Debug.Log("[ResearchManager] Saved state");
        }

        /// <summary>
        /// Loads the research manager state
        /// </summary>
        public void Load()
        {
            string json = PlayerPrefs.GetString("ResearchManager_Save", null);
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("[ResearchManager] No save data found");
                return;
            }

            var saveData = JsonUtility.FromJson<ResearchManagerSaveData>(json);
            
            // Load completed researches
            _completedResearchIds = new HashSet<string>(saveData.completedResearchIds);

            // Load active research
            if (saveData.activeResearch.HasValue)
            {
                var progressData = saveData.activeResearch.Value;
                var researchData = technologyTree?.GetResearch(progressData.researchId);
                if (researchData != null)
                {
                    _activeResearch = new ResearchProgress(progressData, researchData);
                    _activeResearch.OnResearchCompleted += () => CompleteResearch(progressData.researchId);
                }
            }

            // Load queue
            _researchQueue = new Queue<ResearchProgress>();
            foreach (var queueData in saveData.researchQueue)
            {
                var researchData = technologyTree?.GetResearch(queueData.researchId);
                if (researchData != null)
                {
                    var progress = new ResearchProgress(queueData, researchData);
                    _researchQueue.Enqueue(progress);
                }
            }

            // Parse last save time
            if (DateTime.TryParse(saveData.lastSaveTimestamp, out var lastSave))
            {
                _lastSaveTime = lastSave;
            }

            Debug.Log("[ResearchManager] Loaded state");

            // Apply offline progress
            ApplyOfflineProgress();
        }

        /// <summary>
        /// Applies offline progress calculation
        /// </summary>
        private void ApplyOfflineProgress()
        {
            if (_lastSaveTime == default)
                return;

            if (_activeResearch != null)
            {
                _activeResearch.ApplyOfflineProgress(_lastSaveTime, DateTime.UtcNow);
            }
        }
        #endregion

        #region Debug
        /// <summary>
        /// Debug: Instantly completes all active research
        /// </summary>
        [ContextMenu("Debug: Complete Active Research")]
        private void DebugCompleteActive()
        {
            SkipResearch();
        }

        /// <summary>
        /// Debug: Resets all research progress
        /// </summary>
        [ContextMenu("Debug: Reset All Research")]
        private void DebugResetResearch()
        {
            _activeResearch = null;
            _researchQueue.Clear();
            _completedResearchIds.Clear();
            PlayerPrefs.DeleteKey("ResearchManager_Save");
            Debug.Log("[ResearchManager] Reset all research progress");
        }
        #endregion
    }

    /// <summary>
    /// Interface for saveable systems
    /// </summary>
    public interface ISaveable
    {
        void Save();
        void Load();
    }
}
