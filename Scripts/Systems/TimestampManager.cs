using System;
using UnityEngine;

namespace ProjectAegis.Systems.Idle
{
    /// <summary>
    /// Manages reliable timestamp tracking with anti-cheat detection.
    /// Handles device time changes, detects rollbacks, and provides safe time calculations.
    /// </summary>
    public class TimestampManager : BaseManager<TimestampManager>
    {
        [Header("Anti-Cheat Settings")]
        [SerializeField] private bool enableAntiCheat = true;
        [SerializeField] private float maxTimeJumpMinutes = 5f; // Max reasonable time between checks
        [SerializeField] private float rollbackThresholdSeconds = 10f; // Threshold for rollback detection
        
        [Header("Server Time (Optional)")]
        [SerializeField] private bool useServerTime = false;
        [SerializeField] private string serverTimeEndpoint = "";
        
        // Runtime tracking
        private DateTime _lastCheckedTime;
        private DateTime? _serverTimeOffset;
        private float _sessionTimer;
        private float _lastRealtime;
        
        // Anti-cheat state
        private int _rollbackCount;
        private DateTime? _firstDetectedRollback;
        private bool _isTimeValid = true;
        
        // Constants
        private const string PREFS_LAST_CHECKED_TIME = "PA_LastCheckedTime";
        private const string PREFS_SESSION_COUNT = "PA_SessionCount";
        private const int MAX_ROLLBACKS_BEFORE_FLAG = 3;
        private const float REALTIME_CHECK_INTERVAL = 30f; // Check every 30 seconds

        // Events
        public event Action OnTimeRollbackDetected;
        public event Action OnTimeCheatingFlagged;
        public event Action<DateTime, DateTime> OnInvalidTimeDetected; // expected, actual

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _lastCheckedTime = DateTime.UtcNow;
            _lastRealtime = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            ValidateInitialTime();
        }

        private void Update()
        {
            if (!enableAntiCheat) return;
            
            _sessionTimer += Time.deltaTime;
            
            // Periodic validation using realtimeSinceStartup
            if (_sessionTimer >= REALTIME_CHECK_INTERVAL)
            {
                _sessionTimer = 0;
                ValidateRealtimeProgression();
            }
        }

        #endregion

        #region Time Validation

        /// <summary>
        /// Gets the current validated time
        /// </summary>
        public DateTime GetCurrentTime()
        {
            DateTime now = DateTime.UtcNow;
            
            // Apply server offset if available
            if (_serverTimeOffset.HasValue)
            {
                now = now.Add(_serverTimeOffset.Value - now);
            }
            
            // Validate the time
            if (enableAntiCheat && !ValidateTimestamp(_lastCheckedTime, now))
            {
                HandleInvalidTime(now);
            }
            
            _lastCheckedTime = now;
            return now;
        }

        /// <summary>
        /// Validates that current time is after previous time
        /// </summary>
        public bool ValidateTimestamp(DateTime previous, DateTime current)
        {
            // Check for rollback
            if (current < previous)
            {
                double diff = (previous - current).TotalSeconds;
                if (diff > rollbackThresholdSeconds)
                {
                    Debug.LogWarning($"[TimestampManager] Time rollback detected! Previous: {previous}, Current: {current}, Diff: {diff:F1}s");
                    return false;
                }
            }
            
            // Check for unrealistic forward jump (during session only)
            double forwardDiff = (current - previous).TotalMinutes;
            if (forwardDiff > maxTimeJumpMinutes)
            {
                Debug.LogWarning($"[TimestampManager] Unrealistic time jump detected: {forwardDiff:F1} minutes");
                // This might be legitimate (app was backgrounded), so we don't flag as invalid
                // but we log it for debugging
            }
            
            return true;
        }

        /// <summary>
        /// Validates time using Unity's realtimeSinceStartup (unaffected by system time changes)
        /// </summary>
        private void ValidateRealtimeProgression()
        {
            float currentRealtime = Time.realtimeSinceStartup;
            float elapsedRealtime = currentRealtime - _lastRealtime;
            DateTime expectedTime = _lastCheckedTime.AddSeconds(elapsedRealtime);
            DateTime actualTime = DateTime.UtcNow;
            
            double diff = Math.Abs((actualTime - expectedTime).TotalSeconds);
            
            // If difference is significant, system time was changed
            if (diff > rollbackThresholdSeconds)
            {
                Debug.LogWarning($"[TimestampManager] System time change detected. Expected: {expectedTime}, Actual: {actualTime}, Diff: {diff:F1}s");
                
                if (actualTime < expectedTime)
                {
                    // Time was moved backwards - likely cheating
                    HandleTimeRollback(expectedTime, actualTime);
                }
            }
            
            _lastRealtime = currentRealtime;
        }

        /// <summary>
        /// Validates time on app start
        /// </summary>
        private void ValidateInitialTime()
        {
            if (!enableAntiCheat) return;
            
            // Check saved last time
            if (PlayerPrefs.HasKey(PREFS_LAST_CHECKED_TIME))
            {
                string savedTimeStr = PlayerPrefs.GetString(PREFS_LAST_CHECKED_TIME);
                if (DateTime.TryParse(savedTimeStr, out DateTime savedTime))
                {
                    DateTime now = DateTime.UtcNow;
                    
                    if (now < savedTime)
                    {
                        Debug.LogWarning($"[TimestampManager] Rollback detected on startup! Saved: {savedTime}, Now: {now}");
                        HandleTimeRollback(savedTime, now);
                    }
                }
            }
        }

        #endregion

        #region Anti-Cheat Handling

        /// <summary>
        /// Handles detected time rollback
        /// </summary>
        private void HandleTimeRollback(DateTime expected, DateTime actual)
        {
            _rollbackCount++;
            
            if (_firstDetectedRollback == null)
            {
                _firstDetectedRollback = DateTime.UtcNow;
            }
            
            OnTimeRollbackDetected?.Invoke();
            OnInvalidTimeDetected?.Invoke(expected, actual);
            
            // Flag as cheating if too many rollbacks
            if (_rollbackCount >= MAX_ROLLBACKS_BEFORE_FLAG)
            {
                FlagTimeCheating();
            }
        }

        /// <summary>
        /// Handles general invalid time
        /// </summary>
        private void HandleInvalidTime(DateTime actualTime)
        {
            _isTimeValid = false;
            OnInvalidTimeDetected?.Invoke(_lastCheckedTime, actualTime);
        }

        /// <summary>
        /// Flags the user for time cheating
        /// </summary>
        private void FlagTimeCheating()
        {
            Debug.LogError($"[TimestampManager] User flagged for time cheating! Rollback count: {_rollbackCount}");
            OnTimeCheatingFlagged?.Invoke();
            
            // Could implement: ban, reset progress, or just log
            // For MVP, we'll just log and continue with capped progress
        }

        /// <summary>
        /// Detects if time rollback occurred
        /// </summary>
        public bool DetectTimeRollback()
        {
            return _rollbackCount > 0;
        }

        /// <summary>
        /// Gets the number of detected rollbacks
        /// </summary>
        public int GetRollbackCount()
        {
            return _rollbackCount;
        }

        /// <summary>
        /// Resets the anti-cheat state
        /// </summary>
        public void ResetAntiCheatState()
        {
            _rollbackCount = 0;
            _firstDetectedRollback = null;
            _isTimeValid = true;
        }

        #endregion

        #region Safe Duration Calculation

        /// <summary>
        /// Calculates safe offline duration with anti-cheat validation
        /// </summary>
        public TimeSpan GetSafeOfflineDuration(DateTime lastLogout)
        {
            DateTime now = GetCurrentTime();
            
            // Check for rollback
            if (now < lastLogout)
            {
                Debug.LogWarning($"[TimestampManager] Rollback detected in offline calculation!");
                HandleTimeRollback(lastLogout, now);
                return TimeSpan.Zero; // No offline progress if cheating detected
            }
            
            return now - lastLogout;
        }

        /// <summary>
        /// Gets safe offline duration with maximum cap
        /// </summary>
        public TimeSpan GetSafeOfflineDuration(DateTime lastLogout, TimeSpan maxDuration)
        {
            TimeSpan duration = GetSafeOfflineDuration(lastLogout);
            
            if (duration > maxDuration)
            {
                Debug.Log($"[TimestampManager] Offline duration capped from {duration.TotalHours:F1}h to {maxDuration.TotalHours:F1}h");
                return maxDuration;
            }
            
            return duration;
        }

        /// <summary>
        /// Checks if the given duration is valid (not negative, not excessive)
        /// </summary>
        public bool IsValidOfflineDuration(TimeSpan duration, TimeSpan maxDuration)
        {
            if (duration < TimeSpan.Zero) return false;
            if (duration > maxDuration * 2) return false; // Suspicious if way over max
            return true;
        }

        #endregion

        #region Server Time (Optional)

        /// <summary>
        /// Sets server time offset for validation
        /// </summary>
        public void SetServerTime(DateTime serverTime)
        {
            _serverTimeOffset = serverTime;
            useServerTime = true;
            Debug.Log($"[TimestampManager] Server time set: {serverTime}");
        }

        /// <summary>
        /// Clears server time offset
        /// </summary>
        public void ClearServerTime()
        {
            _serverTimeOffset = null;
            useServerTime = false;
        }

        /// <summary>
        /// Fetches server time from endpoint (async - would need coroutine in real implementation)
        /// </summary>
        public void FetchServerTime()
        {
            if (string.IsNullOrEmpty(serverTimeEndpoint))
            {
                Debug.LogWarning("[TimestampManager] No server time endpoint configured");
                return;
            }
            
            // In real implementation, this would be a coroutine with UnityWebRequest
            // For now, we'll just log that it would happen
            Debug.Log($"[TimestampManager] Would fetch server time from: {serverTimeEndpoint}");
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Saves current timestamp state
        /// </summary>
        public void SaveTimestamp()
        {
            PlayerPrefs.SetString(PREFS_LAST_CHECKED_TIME, DateTime.UtcNow.ToString("O"));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads timestamp state
        /// </summary>
        public void LoadTimestamp()
        {
            // Handled in ValidateInitialTime
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether anti-cheat is enabled
        /// </summary>
        public bool IsAntiCheatEnabled => enableAntiCheat;

        /// <summary>
        /// Whether time is currently considered valid
        /// </summary>
        public bool IsTimeValid => _isTimeValid;

        /// <summary>
        /// Whether user has been flagged for cheating
        /// </summary>
        public bool IsFlaggedForCheating => _rollbackCount >= MAX_ROLLBACKS_BEFORE_FLAG;

        /// <summary>
        /// Last time that was validated
        /// </summary>
        public DateTime LastCheckedTime => _lastCheckedTime;

        #endregion

        #region Debug

        /// <summary>
        /// Gets debug information
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Timestamp Manager ===");
            sb.AppendLine($"Current UTC: {DateTime.UtcNow}");
            sb.AppendLine($"Last Checked: {_lastCheckedTime}");
            sb.AppendLine($"Anti-Cheat Enabled: {enableAntiCheat}");
            sb.AppendLine($"Time Valid: {_isTimeValid}");
            sb.AppendLine($"Rollback Count: {_rollbackCount}");
            sb.AppendLine($"Flagged: {IsFlaggedForCheating}");
            sb.AppendLine($"Server Time: {_serverTimeOffset?.ToString() ?? "Not set"}");
            return sb.ToString();
        }

        #endregion
    }
}
