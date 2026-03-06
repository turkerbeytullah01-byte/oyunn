using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegis.Systems.Research.Examples
{
    /// <summary>
    /// Example implementation showing how to use the Research System.
    /// This can be used as a reference for integrating the system into your game.
    /// </summary>
    public class ResearchSystemExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private UnityEngine.UI.Text researchNameText;
        [SerializeField] private UnityEngine.UI.Text timeRemainingText;
        [SerializeField] private UnityEngine.UI.Text statusText;
        [SerializeField] private Transform availableListContainer;
        [SerializeField] private GameObject researchButtonPrefab;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            InitializeResearchSystem();
            SubscribeToEvents();
            RefreshUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization
        private void InitializeResearchSystem()
        {
            // Load saved research progress
            ResearchManager.Instance.Load();

            // Update tech tree states
            TechTreeManager.Instance.UpdateNodeStates();

            Log("Research system initialized");
        }

        private void SubscribeToEvents()
        {
            var manager = ResearchManager.Instance;
            
            manager.OnResearchStarted += OnResearchStarted;
            manager.OnResearchCompleted += OnResearchCompleted;
            manager.OnResearchProgressUpdated += OnProgressUpdated;
            manager.OnResearchCancelled += OnResearchCancelled;
            manager.OnModuleUnlocked += OnModuleUnlocked;
            manager.OnResearchAvailable += OnResearchAvailable;
        }

        private void UnsubscribeFromEvents()
        {
            var manager = ResearchManager.Instance;
            
            manager.OnResearchStarted -= OnResearchStarted;
            manager.OnResearchCompleted -= OnResearchCompleted;
            manager.OnResearchProgressUpdated -= OnProgressUpdated;
            manager.OnResearchCancelled -= OnResearchCancelled;
            manager.OnModuleUnlocked -= OnModuleUnlocked;
            manager.OnResearchAvailable -= OnResearchAvailable;
        }
        #endregion

        #region Event Handlers
        private void OnResearchStarted(string researchId)
        {
            var research = ResearchManager.Instance.GetActiveResearchData();
            Log($"Started research: {research?.displayName ?? researchId}");
            
            if (researchNameText != null)
                researchNameText.text = research?.displayName ?? "Researching...";
            
            if (statusText != null)
                statusText.text = "In Progress";

            RefreshAvailableList();
        }

        private void OnResearchCompleted(string researchId)
        {
            var techTree = ResearchManager.Instance.technologyTree;
            var research = techTree?.GetResearch(researchId);
            
            Log($"Completed research: {research?.displayName ?? researchId}");
            
            if (statusText != null)
                statusText.text = "Completed!";

            // Show completion notification
            ShowNotification($"Research Complete: {research?.displayName}");

            // Refresh UI
            RefreshUI();
            TechTreeManager.Instance.UpdateNodeStates();
        }

        private void OnProgressUpdated(string researchId, float progress)
        {
            if (progressSlider != null)
                progressSlider.value = progress;

            if (timeRemainingText != null)
                timeRemainingText.text = ResearchManager.Instance.GetTimeRemainingString();
        }

        private void OnResearchCancelled(string researchId)
        {
            Log($"Cancelled research: {researchId}");
            
            if (statusText != null)
                statusText.text = "Cancelled";

            RefreshUI();
        }

        private void OnModuleUnlocked(string moduleId)
        {
            Log($"Module unlocked: {moduleId}");
            ShowNotification($"New Module: {moduleId}");
        }

        private void OnResearchAvailable(string researchId)
        {
            var techTree = ResearchManager.Instance.technologyTree;
            var research = techTree?.GetResearch(researchId);
            
            Log($"New research available: {research?.displayName ?? researchId}");
            
            RefreshAvailableList();
        }
        #endregion

        #region UI Methods
        private void RefreshUI()
        {
            RefreshProgressDisplay();
            RefreshAvailableList();
        }

        private void RefreshProgressDisplay()
        {
            var manager = ResearchManager.Instance;
            
            if (manager.IsResearching())
            {
                var research = manager.GetActiveResearchData();
                
                if (researchNameText != null)
                    researchNameText.text = research?.displayName ?? "Researching...";
                
                if (progressSlider != null)
                    progressSlider.value = manager.GetProgress();
                
                if (timeRemainingText != null)
                    timeRemainingText.text = manager.GetTimeRemainingString();
                
                if (statusText != null)
                    statusText.text = "In Progress";
            }
            else
            {
                if (researchNameText != null)
                    researchNameText.text = "No Active Research";
                
                if (progressSlider != null)
                    progressSlider.value = 0;
                
                if (timeRemainingText != null)
                    timeRemainingText.text = "--";
                
                if (statusText != null)
                    statusText.text = "Idle";
            }
        }

        private void RefreshAvailableList()
        {
            if (availableListContainer == null || researchButtonPrefab == null)
                return;

            // Clear existing buttons
            foreach (Transform child in availableListContainer)
            {
                Destroy(child.gameObject);
            }

            // Get available researches
            var available = ResearchManager.Instance.GetAvailableResearch();

            // Create button for each
            foreach (var research in available)
            {
                var buttonObj = Instantiate(researchButtonPrefab, availableListContainer);
                var button = buttonObj.GetComponent<Button>();
                var texts = buttonObj.GetComponentsInChildren<UnityEngine.UI.Text>();
                
                // Set button text
                if (texts.Length > 0)
                    texts[0].text = research.displayName;
                if (texts.Length > 1)
                    texts[1].text = $"{research.cost} credits | {research.durationMinutes} min";

                // Set button click
                string researchId = research.researchId; // Capture for closure
                button.onClick.AddListener(() => StartResearch(researchId));
            }
        }

        private void ShowNotification(string message)
        {
            Log($"[Notification] {message}");
            // Implement your notification system here
            // Example: UIManager.Instance.ShowNotification(message);
        }
        #endregion

        #region Public Methods (for UI Buttons)
        /// <summary>
        /// Starts a research by ID (can be called from UI button)
        /// </summary>
        public void StartResearch(string researchId)
        {
            // Check if we can afford it (integrate with EconomyManager)
            var research = ResearchManager.Instance.technologyTree?.GetResearch(researchId);
            if (research == null)
            {
                LogError($"Research not found: {researchId}");
                return;
            }

            // TODO: Check economy before starting
            // if (EconomyManager.Instance.GetCredits() < research.cost)
            // {
            //     ShowNotification("Not enough credits!");
            //     return;
            // }
            // EconomyManager.Instance.DeductCredits(research.cost);

            bool started = ResearchManager.Instance.StartResearch(researchId);
            if (!started)
            {
                LogWarning($"Failed to start research: {researchId}");
            }
        }

        /// <summary>
        /// Cancels the active research (can be called from UI button)
        /// </summary>
        public void CancelResearch()
        {
            bool cancelled = ResearchManager.Instance.CancelResearch(refundPercentage: 0.5f);
            if (!cancelled)
            {
                LogWarning("No active research to cancel");
            }
        }

        /// <summary>
        /// Skips the active research (can be called from UI button)
        /// </summary>
        public void SkipResearch()
        {
            // TODO: Check premium currency
            // if (EconomyManager.Instance.GetPremiumCurrency() < skipCost)
            // {
            //     ShowNotification("Not enough premium currency!");
            //     return;
            // }

            bool skipped = ResearchManager.Instance.SkipResearch();
            if (!skipped)
            {
                LogWarning("No active research to skip");
            }
        }

        /// <summary>
        /// Saves research progress (can be called from UI button)
        /// </summary>
        public void SaveResearch()
        {
            ResearchManager.Instance.Save();
            ShowNotification("Research progress saved!");
        }

        /// <summary>
        /// Debug: Completes active research instantly
        /// </summary>
        [ContextMenu("Debug: Complete Active")]
        public void DebugCompleteActive()
        {
            ResearchManager.Instance.SkipResearch();
        }

        /// <summary>
        /// Debug: Resets all research
        /// </summary>
        [ContextMenu("Debug: Reset All")]
        public void DebugResetAll()
        {
            // Access private field via reflection or add public method
            // For now, just clear PlayerPrefs
            PlayerPrefs.DeleteKey("ResearchManager_Save");
            PlayerPrefs.Save();
            
            Log("All research progress reset");
            RefreshUI();
        }
        #endregion

        #region Utility
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[ResearchExample] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ResearchExample] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ResearchExample] {message}");
        }
        #endregion
    }
}
