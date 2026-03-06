// ============================================================================
// Project Aegis: Drone Dominion
// TimeManager - Centralized time handling for game systems
// ============================================================================
// Manages game time, real time, and offline progress calculation.
// Provides pause functionality and time scale control for debugging.
// Handles time-based calculations for idle progression.
// ============================================================================

using UnityEngine;
using System;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Central manager for all time-related functionality.
    /// Handles game time, real time, and offline progress.
    /// </summary>
    public class TimeManager : BaseManager<TimeManager>
    {
        #region Configuration
        
        public override int InitializationPriority => 2; // Initialize early
        
        /// <summary>
        /// Maximum offline time to calculate (in seconds).
        /// Prevents excessive offline gains.
        /// </summary>
        [SerializeField, Tooltip("Maximum offline time in hours")]
        private float _maxOfflineHours = 24f;
        
        /// <summary>
        /// Minimum offline time to report (in seconds).
        /// </summary>
        [SerializeField, Tooltip("Minimum offline time to report (seconds)")]
        private float _minOfflineTimeThreshold = 5f;
        
        /// <summary>
        /// Whether to use system time or Unity time.
        /// System time is more accurate for offline calculations.
        /// </summary>
        [SerializeField]
        private bool _useSystemTime = true;
        
        /// <summary>
        /// Time scale multiplier for debugging (1.0 = normal).
        /// </summary>
        [SerializeField, Range(0f, 100f)]
        private float _debugTimeScale = 1f;
        
        #endregion
        
        #region Time State
        
        /// <summary>
        /// Whether the game is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// Current game time scale (affected by pause and debug settings).
        /// </summary>
        public float TimeScale => IsPaused ? 0f : _debugTimeScale * UnityEngine.Time.timeScale;
        
        /// <summary>
        /// Delta time adjusted for game time scale.
        /// </summary>
        public float DeltaTime => IsPaused ? 0f : UnityEngine.Time.deltaTime * TimeScale;
        
        /// <summary>
        /// Unscaled delta time (real time).
        /// </summary>
        public float UnscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;
        
        /// <summary>
        /// Total time since game started (in seconds).
        /// </summary>
        public float TotalGameTime { get; private set; }
        
        /// <summary>
        /// Total real time since game started (in seconds).
        /// </summary>
        public float TotalRealTime { get; private set; }
        
        /// <summary>
        /// Current system timestamp.
        /// </summary>
        public DateTime CurrentSystemTime => DateTime.UtcNow;
        
        /// <summary>
        /// Timestamp when the game session started.
        /// </summary>
        public DateTime SessionStartTime { get; private set; }
        
        /// <summary>
        /// Timestamp of the last save.
        /// </summary>
        public DateTime LastSaveTime { get; private set; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when the game is paused.
        /// </summary>
        public event Action OnPaused;
        
        /// <summary>
        /// Called when the game is resumed.
        /// </summary>
        public event Action OnResumed;
        
        /// <summary>
        /// Called when offline progress is calculated.
        /// Parameters: (float offlineSeconds, float effectiveSeconds)
        /// </summary>
        public event Action<float, float> OnOfflineProgressCalculated;
        
        /// <summary>
        /// Called when time scale changes.
        /// Parameters: (float newTimeScale)
        /// </summary>
        public event Action<float> OnTimeScaleChanged;
        
        #endregion
        
        #region Initialization
        
        protected override void OnAwake()
        {
            SessionStartTime = DateTime.UtcNow;
        }
        
        protected override void OnInitialize()
        {
            // Load last save time from player prefs or save data
            LoadLastSaveTime();
            
            Log($"TimeManager initialized. Session started at {SessionStartTime:yyyy-MM-dd HH:mm:ss}");
        }
        
        #endregion
        
        #region Update
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Update time tracking
            TotalGameTime += DeltaTime;
            TotalRealTime += UnscaledDeltaTime;
        }
        
        #endregion
        
        #region Pause Control
        
        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void Pause()
        {
            if (IsPaused) return;
            
            IsPaused = true;
            OnPaused?.Invoke();
            
            Log("Game paused");
        }
        
        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused) return;
            
            IsPaused = false;
            OnResumed?.Invoke();
            
            Log("Game resumed");
        }
        
        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
        
        #endregion
        
        #region Time Scale Control
        
        /// <summary>
        /// Sets the debug time scale.
        /// </summary>
        /// <param name="scale">Time scale multiplier (1.0 = normal)</param>
        public void SetDebugTimeScale(float scale)
        {
            var previousScale = _debugTimeScale;
            _debugTimeScale = Mathf.Max(0f, scale);
            
            if (!Mathf.Approximately(previousScale, _debugTimeScale))
            {
                OnTimeScaleChanged?.Invoke(TimeScale);
                Log($"Debug time scale set to {_debugTimeScale:F2}x");
            }
        }
        
        /// <summary>
        /// Resets the debug time scale to normal.
        /// </summary>
        public void ResetDebugTimeScale()
        {
            SetDebugTimeScale(1f);
        }
        
        /// <summary>
        /// Gets the current effective time scale.
        /// </summary>
        public float GetEffectiveTimeScale()
        {
            return TimeScale;
        }
        
        #endregion
        
        #region Offline Progress
        
        /// <summary>
        /// Calculates offline progress time since last save.
        /// </summary>
        /// <returns>Offline progress data</returns>
        public OfflineProgressData CalculateOfflineProgress()
        {
            var now = DateTime.UtcNow;
            var offlineDuration = now - LastSaveTime;
            var offlineSeconds = (float)offlineDuration.TotalSeconds;
            
            // Check if offline time is significant
            if (offlineSeconds < _minOfflineTimeThreshold)
            {
                return new OfflineProgressData
                {
                    WasOffline = false,
                    OfflineSeconds = 0f,
                    EffectiveSeconds = 0f,
                    IsCapped = false
                };
            }
            
            // Cap offline time
            var maxOfflineSeconds = _maxOfflineHours * 3600f;
            var isCapped = offlineSeconds > maxOfflineSeconds;
            var effectiveSeconds = isCapped ? maxOfflineSeconds : offlineSeconds;
            
            var data = new OfflineProgressData
            {
                WasOffline = true,
                OfflineSeconds = offlineSeconds,
                EffectiveSeconds = effectiveSeconds,
                IsCapped = isCapped,
                LastSaveTime = LastSaveTime,
                CurrentTime = now
            };
            
            OnOfflineProgressCalculated?.Invoke(offlineSeconds, effectiveSeconds);
            
            Log($"Offline progress: {offlineSeconds:F0}s (effective: {effectiveSeconds:F0}s, capped: {isCapped})");
            
            return data;
        }
        
        /// <summary>
        /// Records the current time as the save time.
        /// Call this when saving the game.
        /// </summary>
        public void RecordSaveTime()
        {
            LastSaveTime = DateTime.UtcNow;
            SaveLastSaveTime();
            
            Log($"Save time recorded: {LastSaveTime:yyyy-MM-dd HH:mm:ss}");
        }
        
        /// <summary>
        /// Sets the last save time manually (e.g., when loading a save).
        /// </summary>
        public void SetLastSaveTime(DateTime saveTime)
        {
            LastSaveTime = saveTime;
            Log($"Last save time set to: {LastSaveTime:yyyy-MM-dd HH:mm:ss}");
        }
        
        #endregion
        
        #region Persistence
        
        private const string LAST_SAVE_TIME_KEY = "ProjectAegis_LastSaveTime";
        
        private void LoadLastSaveTime()
        {
            // Try to load from PlayerPrefs as fallback
            if (PlayerPrefs.HasKey(LAST_SAVE_TIME_KEY))
            {
                var ticks = PlayerPrefs.GetString(LAST_SAVE_TIME_KEY);
                if (long.TryParse(ticks, out var longTicks))
                {
                    LastSaveTime = new DateTime(longTicks, DateTimeKind.Utc);
                }
                else
                {
                    LastSaveTime = DateTime.UtcNow;
                }
            }
            else
            {
                LastSaveTime = DateTime.UtcNow;
            }
        }
        
        private void SaveLastSaveTime()
        {
            PlayerPrefs.SetString(LAST_SAVE_TIME_KEY, LastSaveTime.Ticks.ToString());
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Formats a duration in seconds to a readable string.
        /// </summary>
        public static string FormatDuration(float seconds)
        {
            if (seconds < 60f)
                return $"{seconds:F0}s";
            
            if (seconds < 3600f)
            {
                var minutes = seconds / 60f;
                return $"{minutes:F1}m";
            }
            
            if (seconds < 86400f)
            {
                var hours = seconds / 3600f;
                return $"{hours:F1}h";
            }
            
            var days = seconds / 86400f;
            return $"{days:F1}d";
        }
        
        /// <summary>
        /// Formats a duration in seconds to a detailed string.
        /// </summary>
        public static string FormatDurationDetailed(float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            
            if (timeSpan.TotalDays >= 1)
                return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            
            if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            
            return $"{timeSpan.Seconds}s";
        }
        
        /// <summary>
        /// Converts game seconds to real seconds.
        /// </summary>
        public float GameToRealSeconds(float gameSeconds)
        {
            return gameSeconds / TimeScale;
        }
        
        /// <summary>
        /// Converts real seconds to game seconds.
        /// </summary>
        public float RealToGameSeconds(float realSeconds)
        {
            return realSeconds * TimeScale;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Data structure for offline progress calculation.
    /// </summary>
    [Serializable]
    public struct OfflineProgressData
    {
        /// <summary>
        /// Whether there was any offline time.
        /// </summary>
        public bool WasOffline;
        
        /// <summary>
        /// Total time the player was offline (in seconds).
        /// </summary>
        public float OfflineSeconds;
        
        /// <summary>
        /// Effective time used for calculations (may be capped).
        /// </summary>
        public float EffectiveSeconds;
        
        /// <summary>
        /// Whether the offline time was capped.
        /// </summary>
        public bool IsCapped;
        
        /// <summary>
        /// When the last save occurred.
        /// </summary>
        public DateTime LastSaveTime;
        
        /// <summary>
        /// Current time when calculation was performed.
        /// </summary>
        public DateTime CurrentTime;
        
        /// <summary>
        /// Formatted string for display.
        /// </summary>
        public string FormattedDuration => TimeManager.FormatDurationDetailed(EffectiveSeconds);
    }
    
    #endregion
}
