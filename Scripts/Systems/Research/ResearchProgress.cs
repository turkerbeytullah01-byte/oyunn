using System;
using UnityEngine;

namespace ProjectAegis.Systems.Research
{
    /// <summary>
    /// Serializable data for saving/loading research progress
    /// </summary>
    [Serializable]
    public struct ResearchProgressData
    {
        public string researchId;
        public string startTimestamp;
        public float progressPercentage;
        public bool isPaused;
        public string pauseTimestamp;
        public float accumulatedProgress;
    }

    /// <summary>
    /// Runtime progress tracking for active research.
    /// Handles progress calculation with offline support.
    /// </summary>
    public class ResearchProgress
    {
        // Core data
        public string ResearchId { get; private set; }
        public DateTime StartTimestamp { get; private set; }
        public float ProgressPercentage { get; private set; }
        public bool IsPaused { get; private set; }
        public DateTime? PauseTimestamp { get; private set; }
        public float AccumulatedProgress { get; private set; }

        // References
        private ResearchData _researchData;
        private TimeSpan _totalDuration;

        // Events
        public event Action<float> OnProgressUpdated;
        public event Action OnResearchCompleted;

        /// <summary>
        /// Creates a new research progress tracker
        /// </summary>
        public ResearchProgress(ResearchData researchData)
        {
            if (researchData == null)
                throw new ArgumentNullException(nameof(researchData));

            _researchData = researchData;
            ResearchId = researchData.researchId;
            StartTimestamp = DateTime.UtcNow;
            _totalDuration = researchData.GetDuration();
            ProgressPercentage = 0f;
            IsPaused = false;
            PauseTimestamp = null;
            AccumulatedProgress = 0f;
        }

        /// <summary>
        /// Restores research progress from saved data
        /// </summary>
        public ResearchProgress(ResearchProgressData data, ResearchData researchData)
        {
            _researchData = researchData;
            ResearchId = data.researchId;
            
            if (DateTime.TryParse(data.startTimestamp, out var startTime))
                StartTimestamp = startTime;
            else
                StartTimestamp = DateTime.UtcNow;

            ProgressPercentage = data.progressPercentage;
            IsPaused = data.isPaused;
            AccumulatedProgress = data.accumulatedProgress;

            if (!string.IsNullOrEmpty(data.pauseTimestamp) && 
                DateTime.TryParse(data.pauseTimestamp, out var pauseTime))
                PauseTimestamp = pauseTime;

            if (researchData != null)
                _totalDuration = researchData.GetDuration();
        }

        /// <summary>
        /// Calculates current progress based on elapsed time
        /// Handles offline progress calculation
        /// </summary>
        public float CalculateProgress(DateTime currentTime)
        {
            if (_researchData == null)
                return ProgressPercentage;

            if (IsPaused)
            {
                // Don't advance progress while paused
                return ProgressPercentage;
            }

            // Calculate elapsed time since start (minus any pause duration)
            TimeSpan elapsed = currentTime - StartTimestamp;
            
            // Subtract time spent paused
            if (PauseTimestamp.HasValue)
            {
                elapsed -= (currentTime - PauseTimestamp.Value);
            }

            // Add accumulated progress from previous sessions
            float totalProgressSeconds = (float)elapsed.TotalSeconds + AccumulatedProgress;
            float durationSeconds = (float)_totalDuration.TotalSeconds;

            if (durationSeconds <= 0)
                return 1f;

            ProgressPercentage = Mathf.Clamp01(totalProgressSeconds / durationSeconds);
            
            OnProgressUpdated?.Invoke(ProgressPercentage);

            if (ProgressPercentage >= 1f)
            {
                OnResearchCompleted?.Invoke();
            }

            return ProgressPercentage;
        }

        /// <summary>
        /// Updates progress - call this regularly (e.g., every frame or second)
        /// </summary>
        public void Update()
        {
            CalculateProgress(DateTime.UtcNow);
        }

        /// <summary>
        /// Pauses the research progress
        /// </summary>
        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            PauseTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Resumes the research progress
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            PauseTimestamp = null;
        }

        /// <summary>
        /// Gets the time remaining for completion
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            if (_researchData == null)
                return TimeSpan.Zero;

            float remainingPercentage = 1f - ProgressPercentage;
            double remainingSeconds = remainingPercentage * _totalDuration.TotalSeconds;
            
            return TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
        }

        /// <summary>
        /// Gets the formatted time remaining string
        /// </summary>
        public string GetTimeRemainingString()
        {
            var remaining = GetTimeRemaining();
            
            if (remaining.TotalDays >= 1)
                return $"{remaining.Days}d {remaining.Hours}h";
            if (remaining.TotalHours >= 1)
                return $"{remaining.Hours}h {remaining.Minutes}m";
            if (remaining.TotalMinutes >= 1)
                return $"{remaining.Minutes}m {remaining.Seconds}s";
            
            return $"{remaining.Seconds}s";
        }

        /// <summary>
        /// Checks if research is complete
        /// </summary>
        public bool IsComplete()
        {
            return ProgressPercentage >= 1f;
        }

        /// <summary>
        /// Adds progress boost (e.g., from boosters or instant completion)
        /// </summary>
        public void AddProgress(float percentageBoost)
        {
            ProgressPercentage = Mathf.Clamp01(ProgressPercentage + percentageBoost);
            OnProgressUpdated?.Invoke(ProgressPercentage);

            if (ProgressPercentage >= 1f)
            {
                OnResearchCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Instantly completes the research
        /// </summary>
        public void InstantComplete()
        {
            ProgressPercentage = 1f;
            OnProgressUpdated?.Invoke(1f);
            OnResearchCompleted?.Invoke();
        }

        /// <summary>
        /// Converts to serializable data for saving
        /// </summary>
        public ResearchProgressData ToSaveData()
        {
            return new ResearchProgressData
            {
                researchId = ResearchId,
                startTimestamp = StartTimestamp.ToString("O"),
                progressPercentage = ProgressPercentage,
                isPaused = IsPaused,
                pauseTimestamp = PauseTimestamp?.ToString("O"),
                accumulatedProgress = AccumulatedProgress
            };
        }

        /// <summary>
        /// Applies offline progress calculation on game load
        /// </summary>
        public void ApplyOfflineProgress(DateTime lastSaveTime, DateTime currentTime)
        {
            if (IsPaused || _researchData == null)
                return;

            // Calculate how much time passed since last save
            TimeSpan offlineDuration = currentTime - lastSaveTime;
            
            // Convert to progress
            float durationSeconds = (float)_totalDuration.TotalSeconds;
            float offlineProgress = (float)offlineDuration.TotalSeconds / durationSeconds;
            
            // Add to accumulated progress
            AccumulatedProgress += (float)offlineDuration.TotalSeconds;
            
            // Recalculate current progress
            CalculateProgress(currentTime);

            Debug.Log($"[ResearchProgress] Applied {offlineDuration.TotalMinutes:F1} minutes offline progress to {ResearchId}. " +
                      $"Current progress: {ProgressPercentage:P1}");
        }
    }
}
