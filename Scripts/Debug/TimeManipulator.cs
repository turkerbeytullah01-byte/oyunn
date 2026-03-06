using System;
using System.Collections;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Time manipulation tool for Project Aegis: Drone Dominion
    /// Allows developers to fast-forward time, skip periods, and test time-based systems
    /// </summary>
    public class TimeManipulator : MonoBehaviour
    {
        #region Singleton
        private static TimeManipulator _instance;
        public static TimeManipulator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TimeManipulator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TimeManipulator");
                        _instance = go.AddComponent<TimeManipulator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Settings
        [Header("Time Skip Settings")]
        [Tooltip("Default minutes to skip")]
        public float skipMinutes = 5f;
        
        [Tooltip("Default hours to skip")]
        public float skipHours = 1f;
        
        [Tooltip("Default days to skip")]
        public float skipDays = 1f;
        
        [Tooltip("Maximum hours that can be skipped at once")]
        public float maxSkipHours = 168f; // 1 week
        
        [Tooltip("Show confirmation for large time skips")]
        public bool confirmLargeSkips = true;
        
        [Tooltip("Threshold for large skip confirmation (hours)")]
        public float largeSkipThreshold = 24f;
        #endregion

        #region Time Scale Settings
        [Header("Time Scale Settings")]
        [Tooltip("Use custom time scale instead of default")]
        public bool useCustomTimeScale = false;
        
        [Tooltip("Custom time scale multiplier")]
        [Range(0f, 1000f)]
        public float customTimeScale = 10f;
        
        [Tooltip("Smoothly transition between time scales")]
        public bool smoothTransitions = true;
        
        [Tooltip("Duration of time scale transition (seconds)")]
        public float transitionDuration = 0.5f;
        
        [Tooltip("Available time scale presets")]
        public float[] timeScalePresets = { 0.5f, 1f, 2f, 5f, 10f, 50f, 100f, 500f, 1000f };
        #endregion

        #region Simulation Settings
        [Header("Simulation Settings")]
        [Tooltip("Simulate resource generation during time skip")]
        public bool simulateResources = true;
        
        [Tooltip("Simulate events during time skip")]
        public bool simulateEvents = true;
        
        [Tooltip("Simulate contract progress during time skip")]
        public bool simulateContracts = true;
        
        [Tooltip("Simulate research progress during time skip")]
        public bool simulateResearch = true;
        
        [Tooltip("Show progress bar during long simulations")]
        public bool showProgressBar = true;
        #endregion

        #region Events
        public event Action<float> OnTimeSkipped;
        public event Action<float> OnTimeScaleChanged;
        public event Action OnSimulationStarted;
        public event Action OnSimulationCompleted;
        public event Action<float> OnSimulationProgress;
        #endregion

        #region Private Fields
        private float _originalTimeScale = 1f;
        private float _targetTimeScale = 1f;
        private Coroutine _timeScaleTransitionCoroutine;
        private bool _isSimulating = false;
        private DateTime _simulatedCurrentTime;
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
            _targetTimeScale = _originalTimeScale;
        }

        private void OnDestroy()
        {
            // Reset time scale on destroy
            Time.timeScale = _originalTimeScale;
        }
        #endregion

        #region Time Skip Methods
        /// <summary>
        /// Skip forward by minutes
        /// </summary>
        public void SkipMinutes(float minutes)
        {
            SkipHours(minutes / 60f);
        }

        /// <summary>
        /// Skip forward by hours
        /// </summary>
        public void SkipHours(float hours)
        {
            if (hours <= 0) return;
            
            // Check for large skip confirmation
            if (confirmLargeSkips && hours >= largeSkipThreshold)
            {
                UnityEngine.Debug.LogWarning($"[TimeManipulator] Large time skip requested: {hours:F1} hours. Use SkipHoursUnsafe to bypass confirmation.");
            }
            
            SkipHoursUnsafe(hours);
        }

        /// <summary>
        /// Skip forward by hours without confirmation
        /// </summary>
        public void SkipHoursUnsafe(float hours)
        {
            if (hours <= 0) return;
            
            hours = Mathf.Min(hours, maxSkipHours);
            
            StartCoroutine(SimulateTimeSkip(hours));
        }

        /// <summary>
        /// Skip forward by days
        /// </summary>
        public void SkipDays(float days)
        {
            SkipHours(days * 24f);
        }

        /// <summary>
        /// Skip to a specific time of day
        /// </summary>
        public void SkipToTimeOfDay(int hour, int minute = 0)
        {
            // This would integrate with a game time system
            UnityEngine.Debug.Log($"[TimeManipulator] Skip to time: {hour:D2}:{minute:D2}");
        }

        /// <summary>
        /// Skip to next day
        /// </summary>
        public void SkipToNextDay()
        {
            SkipDays(1);
        }

        /// <summary>
        /// Skip to next week
        /// </summary>
        public void SkipToNextWeek()
        {
            SkipDays(7);
        }

        /// <summary>
        /// Skip to next month
        /// </summary>
        public void SkipToNextMonth()
        {
            SkipDays(30);
        }
        #endregion

        #region Time Scale Methods
        /// <summary>
        /// Set the time scale multiplier
        /// </summary>
        public void SetTimeScale(float scale)
        {
            scale = Mathf.Clamp(scale, 0f, 1000f);
            _targetTimeScale = scale;
            
            if (smoothTransitions)
            {
                StartTimeScaleTransition(scale);
            }
            else
            {
                Time.timeScale = scale;
            }
            
            OnTimeScaleChanged?.Invoke(scale);
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Time scale set to {scale:F1}x");
            }
        }

        /// <summary>
        /// Reset time scale to normal (1x)
        /// </summary>
        public void ResetTimeScale()
        {
            SetTimeScale(1f);
        }

        /// <summary>
        /// Set time scale to a preset value
        /// </summary>
        public void SetTimeScalePreset(int presetIndex)
        {
            if (presetIndex >= 0 && presetIndex < timeScalePresets.Length)
            {
                SetTimeScale(timeScalePresets[presetIndex]);
            }
        }

        /// <summary>
        /// Cycle through time scale presets
        /// </summary>
        public void CycleTimeScale()
        {
            float currentScale = Time.timeScale;
            int currentIndex = -1;
            
            for (int i = 0; i < timeScalePresets.Length; i++)
            {
                if (Mathf.Approximately(currentScale, timeScalePresets[i]))
                {
                    currentIndex = i;
                    break;
                }
            }
            
            int nextIndex = (currentIndex + 1) % timeScalePresets.Length;
            SetTimeScale(timeScalePresets[nextIndex]);
        }

        /// <summary>
        /// Get current time scale
        /// </summary>
        public float GetCurrentTimeScale()
        {
            return Time.timeScale;
        }

        /// <summary>
        /// Enable custom time scale
        /// </summary>
        public void EnableCustomTimeScale()
        {
            useCustomTimeScale = true;
            SetTimeScale(customTimeScale);
        }

        /// <summary>
        /// Disable custom time scale
        /// </summary>
        public void DisableCustomTimeScale()
        {
            useCustomTimeScale = false;
            ResetTimeScale();
        }

        private void StartTimeScaleTransition(float targetScale)
        {
            if (_timeScaleTransitionCoroutine != null)
            {
                StopCoroutine(_timeScaleTransitionCoroutine);
            }
            
            _timeScaleTransitionCoroutine = StartCoroutine(TransitionTimeScale(targetScale));
        }

        private IEnumerator TransitionTimeScale(float targetScale)
        {
            float startScale = Time.timeScale;
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionDuration;
                Time.timeScale = Mathf.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            Time.timeScale = targetScale;
        }
        #endregion

        #region Simulation
        /// <summary>
        /// Simulate offline time progression
        /// </summary>
        public void SimulateOfflineTime(float hours)
        {
            if (_isSimulating) return;
            
            StartCoroutine(SimulateTimeSkip(hours));
        }

        private IEnumerator SimulateTimeSkip(float hours)
        {
            _isSimulating = true;
            OnSimulationStarted?.Invoke();
            
            float simulatedHours = 0f;
            float simulationStep = Mathf.Min(1f, hours / 100f); // Simulate in steps
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Starting time simulation for {hours:F1} hours...");
            }
            
            while (simulatedHours < hours)
            {
                float step = Mathf.Min(simulationStep, hours - simulatedHours);
                
                // Simulate systems
                if (simulateResources) SimulateResources(step);
                if (simulateContracts) SimulateContracts(step);
                if (simulateResearch) SimulateResearch(step);
                if (simulateEvents) SimulateEvents(step);
                
                simulatedHours += step;
                
                // Report progress
                float progress = simulatedHours / hours;
                OnSimulationProgress?.Invoke(progress);
                
                yield return null;
            }
            
            OnTimeSkipped?.Invoke(hours);
            OnSimulationCompleted?.Invoke();
            
            _isSimulating = false;
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Time simulation completed: {hours:F1} hours");
            }
        }

        private void SimulateResources(float hours)
        {
            // Simulate resource generation over time
            // This would integrate with ResourceManager
            // Example: ResourceManager.Instance?.Simulate(hours);
        }

        private void SimulateContracts(float hours)
        {
            // Simulate contract progress
            // This would integrate with ContractManager
            // Example: ContractManager.Instance?.Simulate(hours);
        }

        private void SimulateResearch(float hours)
        {
            // Simulate research progress
            // This would integrate with ResearchManager
            // Example: ResearchManager.Instance?.Simulate(hours);
        }

        private void SimulateEvents(float hours)
        {
            // Simulate random events
            // This would integrate with EventManager
            // Example: EventManager.Instance?.Simulate(hours);
        }
        #endregion

        #region Pause/Resume
        /// <summary>
        /// Pause game time
        /// </summary>
        public void Pause()
        {
            Time.timeScale = 0f;
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Game paused");
            }
        }

        /// <summary>
        /// Resume game time
        /// </summary>
        public void Resume()
        {
            Time.timeScale = _targetTimeScale;
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Game resumed");
            }
        }

        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsPaused()
        {
            return Time.timeScale == 0f;
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused()) Resume();
            else Pause();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Check if simulation is in progress
        /// </summary>
        public bool IsSimulating()
        {
            return _isSimulating;
        }

        /// <summary>
        /// Get estimated real time for a time skip
        /// </summary>
        public float GetEstimatedRealTime(float gameHours)
        {
            // Estimate how long the simulation will take in real seconds
            float steps = gameHours / Mathf.Min(1f, gameHours / 100f);
            return steps * 0.016f; // Approximate frame time
        }

        /// <summary>
        /// Format hours into readable string
        /// </summary>
        public string FormatTime(float hours)
        {
            if (hours < 1f)
            {
                return $"{hours * 60f:F0}m";
            }
            else if (hours < 24f)
            {
                return $"{hours:F1}h";
            }
            else
            {
                float days = hours / 24f;
                return $"{days:F1}d";
            }
        }

        /// <summary>
        /// Cancel current simulation
        /// </summary>
        public void CancelSimulation()
        {
            if (_isSimulating)
            {
                StopAllCoroutines();
                _isSimulating = false;
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Time simulation cancelled");
                }
            }
        }
        #endregion
    }
}
