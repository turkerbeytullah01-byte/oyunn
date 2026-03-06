// ============================================================================
// Project Aegis: Drone Dominion
// ResearchProgress - Research system data structures
// ============================================================================
// Contains all data structures related to the research system including
// active research, completed research, and research queue management.
// ============================================================================

using UnityEngine;
using System;
using ProjectAegis.Core;

namespace ProjectAegis.Data
{
    /// <summary>
    /// Represents the current state of an active research project.
    /// </summary>
    [Serializable]
    public class ResearchProgress : IProgressable, ISaveable
    {
        #region Properties
        
        /// <summary>
        /// ID of the research being conducted.
        /// </summary>
        public string ResearchId { get; private set; }
        
        /// <summary>
        /// Display name of the research.
        /// </summary>
        public string ResearchName { get; private set; }
        
        /// <summary>
        /// Current progress in research points.
        /// </summary>
        public float CurrentProgress { get; private set; }
        
        /// <summary>
        /// Total research points required to complete.
        /// </summary>
        public float TotalProgressRequired { get; private set; }
        
        /// <summary>
        /// Whether the research is currently progressing.
        /// </summary>
        public bool IsProgressing { get; private set; }
        
        /// <summary>
        /// When the research was started.
        /// </summary>
        public DateTime StartTime { get; private set; }
        
        /// <summary>
        /// Estimated completion time based on current progress rate.
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; private set; }
        
        /// <summary>
        /// Current research point generation rate per second.
        /// </summary>
        public float ProgressRatePerSecond { get; set; }
        
        /// <summary>
        /// Whether this research has been completed.
        /// </summary>
        public bool IsComplete => CurrentProgress >= TotalProgressRequired;
        
        /// <summary>
        /// Normalized progress (0.0 to 1.0).
        /// </summary>
        public float NormalizedProgress => TotalProgressRequired > 0 
            ? Mathf.Clamp01(CurrentProgress / TotalProgressRequired) 
            : 0f;
        
        /// <summary>
        /// Time remaining in seconds at current rate.
        /// </summary>
        public float TimeRemaining => ProgressRatePerSecond > 0 
            ? (TotalProgressRequired - CurrentProgress) / ProgressRatePerSecond 
            : float.MaxValue;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when research progress changes.
        /// </summary>
        public event Action<float> OnProgressChanged;
        
        /// <summary>
        /// Called when research is completed.
        /// </summary>
        public event Action OnProgressComplete;
        
        /// <summary>
        /// Called when research is cancelled.
        /// </summary>
        public event Action<float> OnCancelled;
        
        #endregion
        
        #region Construction
        
        /// <summary>
        /// Creates a new research progress tracker.
        /// </summary>
        public ResearchProgress(string researchId, string researchName, float totalRequired)
        {
            ResearchId = researchId;
            ResearchName = researchName;
            TotalProgressRequired = totalRequired;
            CurrentProgress = 0f;
            IsProgressing = false;
            StartTime = DateTime.UtcNow;
            ProgressRatePerSecond = 0f;
        }
        
        #endregion
        
        #region Progress Control
        
        /// <summary>
        /// Starts the research progress.
        /// </summary>
        public void Start()
        {
            if (IsComplete) return;
            
            IsProgressing = true;
            UpdateEstimatedCompletion();
            
            EventManager.Instance?.TriggerResearchStarted(ResearchId, ResearchName, TimeRemaining);
        }
        
        /// <summary>
        /// Pauses the research progress.
        /// </summary>
        public void Pause()
        {
            IsProgressing = false;
            EstimatedCompletionTime = null;
        }
        
        /// <summary>
        /// Advances the research progress.
        /// </summary>
        public void AdvanceProgress(float amount)
        {
            if (!IsProgressing || IsComplete) return;
            
            var previousProgress = CurrentProgress;
            CurrentProgress = Mathf.Min(CurrentProgress + amount, TotalProgressRequired);
            
            if (CurrentProgress != previousProgress)
            {
                OnProgressChanged?.Invoke(NormalizedProgress);
                EventManager.Instance?.TriggerResearchProgress(ResearchId, NormalizedProgress, TimeRemaining);
                
                if (IsComplete)
                {
                    Complete();
                }
            }
        }
        
        /// <summary>
        /// Updates progress based on elapsed time and current rate.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsProgressing || IsComplete || ProgressRatePerSecond <= 0) return;
            
            AdvanceProgress(ProgressRatePerSecond * deltaTime);
            UpdateEstimatedCompletion();
        }
        
        /// <summary>
        /// Completes the research.
        /// </summary>
        private void Complete()
        {
            IsProgressing = false;
            CurrentProgress = TotalProgressRequired;
            
            OnProgressComplete?.Invoke();
            EventManager.Instance?.TriggerResearchCompleted(ResearchId, ResearchName);
        }
        
        /// <summary>
        /// Cancels the research.
        /// </summary>
        /// <param name="refundProgress">Whether to refund partial progress</param>
        public void Cancel(bool refundProgress = false)
        {
            var progressLost = refundProgress ? 0f : CurrentProgress;
            
            IsProgressing = false;
            OnCancelled?.Invoke(progressLost);
            EventManager.Instance?.TriggerResearchCancelled(ResearchId, ResearchName, progressLost);
        }
        
        /// <summary>
        /// Updates the estimated completion time.
        /// </summary>
        private void UpdateEstimatedCompletion()
        {
            if (ProgressRatePerSecond > 0 && !IsComplete)
            {
                var remainingSeconds = (TotalProgressRequired - CurrentProgress) / ProgressRatePerSecond;
                EstimatedCompletionTime = DateTime.UtcNow.AddSeconds(remainingSeconds);
            }
            else
            {
                EstimatedCompletionTime = null;
            }
        }
        
        #endregion
        
        #region ISaveable Implementation
        
        public string SaveKey => $"ResearchProgress_{ResearchId}";
        public int SaveVersion => 1;
        
        public object CaptureState()
        {
            return new ResearchProgressSnapshot
            {
                ResearchId = ResearchId,
                ResearchName = ResearchName,
                CurrentProgress = CurrentProgress,
                TotalProgressRequired = TotalProgressRequired,
                IsProgressing = IsProgressing,
                StartTime = StartTime,
                ProgressRatePerSecond = ProgressRatePerSecond,
                Version = SaveVersion
            };
        }
        
        public void RestoreState(object state)
        {
            if (state is ResearchProgressSnapshot snapshot)
            {
                CurrentProgress = snapshot.CurrentProgress;
                IsProgressing = snapshot.IsProgressing;
                ProgressRatePerSecond = snapshot.ProgressRatePerSecond;
            }
        }
        
        #endregion
    }
    
    #region Snapshot for Serialization
    
    /// <summary>
    /// Serializable snapshot of ResearchProgress.
    /// </summary>
    [Serializable]
    public struct ResearchProgressSnapshot
    {
        public string ResearchId;
        public string ResearchName;
        public float CurrentProgress;
        public float TotalProgressRequired;
        public bool IsProgressing;
        public DateTime StartTime;
        public float ProgressRatePerSecond;
        public int Version;
    }
    
    #endregion
    
    #region Research Data
    
    /// <summary>
    /// Static data for a research project.
    /// </summary>
    [CreateAssetMenu(fileName = "NewResearch", menuName = "Project Aegis/Research/Research Data")]
    public class ResearchData : BaseScriptableObject
    {
        #region Properties
        
        /// <summary>
        /// Category of research (e.g., Propulsion, Sensors, Materials).
        /// </summary>
        [SerializeField, Tooltip("Research category")]
        private ResearchCategory _category;
        
        public ResearchCategory Category => _category;
        
        /// <summary>
        /// Tier level of this research (1-5).
        /// </summary>
        [SerializeField, Range(1, 5), Tooltip("Research tier (1-5)")]
        private int _tier = 1;
        
        public int Tier => _tier;
        
        /// <summary>
        /// Research points required to complete.
        /// </summary>
        [SerializeField, Tooltip("Research points required")]
        private float _researchPointsRequired = 100f;
        
        public float ResearchPointsRequired => _researchPointsRequired;
        
        /// <summary>
        /// Base time to complete in seconds (at 1 RP/s).
        /// </summary>
        public float BaseTimeToComplete => _researchPointsRequired;
        
        /// <summary>
        /// Cost to start this research.
        /// </summary>
        [SerializeField, Tooltip("Cost to start research")]
        private Cost _researchCost;
        
        public Cost ResearchCost => _researchCost;
        
        /// <summary>
        /// IDs of prerequisite research projects.
        /// </summary>
        [SerializeField, Tooltip("Prerequisite research IDs")]
        private string[] _prerequisites = Array.Empty<string>();
        
        public string[] Prerequisites => _prerequisites;
        
        /// <summary>
        /// IDs of technologies unlocked by this research.
        /// </summary>
        [SerializeField, Tooltip("Technology IDs unlocked by this research")]
        private string[] _unlockedTechnologies = Array.Empty<string>();
        
        public string[] UnlockedTechnologies => _unlockedTechnologies;
        
        /// <summary>
        /// IDs of drone types unlocked by this research.
        /// </summary>
        [SerializeField, Tooltip("Drone IDs unlocked by this research")]
        private string[] _unlockedDrones = Array.Empty<string>();
        
        public string[] UnlockedDrones => _unlockedDrones;
        
        /// <summary>
        /// Modifiers applied when research is completed.
        /// </summary>
        [SerializeField, Tooltip("Modifiers granted by this research")]
        private ResearchModifier[] _modifiers;
        
        public ResearchModifier[] Modifiers => _modifiers;
        
        /// <summary>
        /// Visual effects or animations for this research.
        /// </summary>
        [SerializeField, Tooltip("Visual effect prefab")]
        private GameObject _completionEffect;
        
        public GameObject CompletionEffect => _completionEffect;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Checks if prerequisites are met.
        /// </summary>
        public bool ArePrerequisitesMet(PlayerData playerData)
        {
            foreach (var prereq in _prerequisites)
            {
                if (!playerData.CompletedResearchIds.Contains(prereq))
                {
                    return false;
                }
            }
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Categories of research.
    /// </summary>
    public enum ResearchCategory
    {
        Propulsion,
        Sensors,
        Materials,
        EnergySystems,
        Communication,
        Navigation,
        PayloadSystems,
        Manufacturing,
        SafetySystems,
        SpecialProjects
    }
    
    /// <summary>
    /// Modifier granted by research completion.
    /// </summary>
    [Serializable]
    public struct ResearchModifier
    {
        /// <summary>
        /// Target property to modify.
        /// </summary>
        public string TargetProperty;
        
        /// <summary>
        /// Type of modification.
        /// </summary>
        public ModifierType Type;
        
        /// <summary>
        /// Value of the modification.
        /// </summary>
        public float Value;
    }
    
    #endregion
}
