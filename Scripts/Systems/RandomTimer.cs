using System;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// A configurable timer that triggers after a random interval
    /// </summary>
    public class RandomTimer
    {
        #region Fields
        
        private float _minInterval;
        private float _maxInterval;
        private float _currentInterval;
        private float _elapsedTime;
        private bool _isRunning;
        private bool _isPaused;
        private float _timeScale = 1f;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Minimum interval in seconds
        /// </summary>
        public float MinIntervalSeconds => _minInterval;
        
        /// <summary>
        /// Maximum interval in seconds
        /// </summary>
        public float MaxIntervalSeconds => _maxInterval;
        
        /// <summary>
        /// Current target interval in seconds
        /// </summary>
        public float CurrentIntervalSeconds => _currentInterval;
        
        /// <summary>
        /// Elapsed time in seconds
        /// </summary>
        public float ElapsedTimeSeconds => _elapsedTime;
        
        /// <summary>
        /// Remaining time in seconds
        /// </summary>
        public float RemainingTimeSeconds => Mathf.Max(0, _currentInterval - _elapsedTime);
        
        /// <summary>
        /// Progress from 0 to 1
        /// </summary>
        public float Progress => _currentInterval > 0 ? Mathf.Clamp01(_elapsedTime / _currentInterval) : 0f;
        
        /// <summary>
        /// Whether the timer is currently running
        /// </summary>
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// Whether the timer is paused
        /// </summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>
        /// Whether the timer has completed
        /// </summary>
        public bool IsComplete => _isRunning && _elapsedTime >= _currentInterval;
        
        /// <summary>
        /// Time scale multiplier (1.0 = normal, 0.5 = half speed, 2.0 = double speed)
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Max(0f, value);
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Called when the timer completes
        /// </summary>
        public event Action OnTimerComplete;
        
        /// <summary>
        /// Called when the timer starts
        /// </summary>
        public event Action OnTimerStart;
        
        /// <summary>
        /// Called when the timer is paused
        /// </summary>
        public event Action OnTimerPaused;
        
        /// <summary>
        /// Called when the timer is resumed
        /// </summary>
        public event Action OnTimerResumed;
        
        /// <summary>
        /// Called when the timer is reset
        /// </summary>
        public event Action OnTimerReset;
        
        /// <summary>
        /// Called every update with current progress
        /// </summary>
        public event Action<float> OnTimerProgress;

        #endregion

        #region Constructors
        
        /// <summary>
        /// Creates a new RandomTimer with specified interval range
        /// </summary>
        public RandomTimer(float minIntervalSeconds, float maxIntervalSeconds)
        {
            SetIntervalRange(minIntervalSeconds, maxIntervalSeconds);
            _elapsedTime = 0f;
            _isRunning = false;
            _isPaused = false;
        }
        
        /// <summary>
        /// Creates a new RandomTimer with fixed interval
        /// </summary>
        public RandomTimer(float fixedIntervalSeconds) : this(fixedIntervalSeconds, fixedIntervalSeconds) { }
        
        /// <summary>
        /// Creates a new RandomTimer with default 15-20 minute range
        /// </summary>
        public RandomTimer() : this(900f, 1200f) { } // 15-20 minutes in seconds

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Sets a new interval range and regenerates the current interval
        /// </summary>
        public void SetIntervalRange(float minSeconds, float maxSeconds)
        {
            _minInterval = Mathf.Max(0.1f, minSeconds);
            _maxInterval = Mathf.Max(_minInterval, maxSeconds);
            
            if (!_isRunning)
            {
                GenerateNewInterval();
            }
        }
        
        /// <summary>
        /// Starts the timer with a new random interval
        /// </summary>
        public void Start()
        {
            GenerateNewInterval();
            _elapsedTime = 0f;
            _isRunning = true;
            _isPaused = false;
            OnTimerStart?.Invoke();
        }
        
        /// <summary>
        /// Starts the timer with a specific interval
        /// </summary>
        public void Start(float specificIntervalSeconds)
        {
            _currentInterval = Mathf.Max(0.1f, specificIntervalSeconds);
            _elapsedTime = 0f;
            _isRunning = true;
            _isPaused = false;
            OnTimerStart?.Invoke();
        }
        
        /// <summary>
        /// Stops the timer without triggering completion
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }
        
        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void Pause()
        {
            if (_isRunning && !_isPaused)
            {
                _isPaused = true;
                OnTimerPaused?.Invoke();
            }
        }
        
        /// <summary>
        /// Resumes a paused timer
        /// </summary>
        public void Resume()
        {
            if (_isRunning && _isPaused)
            {
                _isPaused = false;
                OnTimerResumed?.Invoke();
            }
        }
        
        /// <summary>
        /// Resets the timer to zero and stops it
        /// </summary>
        public void Reset()
        {
            _elapsedTime = 0f;
            _isRunning = false;
            _isPaused = false;
            OnTimerReset?.Invoke();
        }
        
        /// <summary>
        /// Resets and restarts the timer with a new interval
        /// </summary>
        public void Restart()
        {
            Reset();
            Start();
        }
        
        /// <summary>
        /// Updates the timer. Call this from Update() or a coroutine.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isRunning || _isPaused)
                return;

            // Apply time scale
            float scaledDeltaTime = deltaTime * _timeScale;
            
            // Accumulate elapsed time
            _elapsedTime += scaledDeltaTime;
            
            // Report progress
            OnTimerProgress?.Invoke(Progress);
            
            // Check for completion
            if (_elapsedTime >= _currentInterval)
            {
                CompleteTimer();
            }
        }
        
        /// <summary>
        /// Generates a new random interval within the configured range
        /// </summary>
        public void GenerateNewInterval()
        {
            _currentInterval = UnityEngine.Random.Range(_minInterval, _maxInterval);
        }
        
        /// <summary>
        /// Forces the timer to complete immediately
        /// </summary>
        public void ForceComplete()
        {
            if (_isRunning)
            {
                _elapsedTime = _currentInterval;
                CompleteTimer();
            }
        }
        
        /// <summary>
        /// Adds time to the current interval (extends timer)
        /// </summary>
        public void AddTime(float seconds)
        {
            _currentInterval += seconds;
        }
        
        /// <summary>
        /// Removes time from the current interval (shortens timer)
        /// </summary>
        public void RemoveTime(float seconds)
        {
            _currentInterval = Mathf.Max(_elapsedTime + 0.1f, _currentInterval - seconds);
        }
        
        /// <summary>
        /// Sets elapsed time directly (for save/load)
        /// </summary>
        public void SetElapsedTime(float seconds)
        {
            _elapsedTime = Mathf.Clamp(seconds, 0, _currentInterval);
        }
        
        /// <summary>
        /// Sets the current interval directly (for save/load)
        /// </summary>
        public void SetCurrentInterval(float seconds)
        {
            _currentInterval = Mathf.Max(0.1f, seconds);
        }

        #endregion

        #region Private Methods
        
        private void CompleteTimer()
        {
            _isRunning = false;
            OnTimerComplete?.Invoke();
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Gets a formatted string of remaining time
        /// </summary>
        public string GetFormattedRemainingTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(RemainingTimeSeconds);
            
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours:D1}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            else
                return $"{timeSpan.Minutes:D1}:{timeSpan.Seconds:D2}";
        }
        
        /// <summary>
        /// Gets a formatted string of elapsed time
        /// </summary>
        public string GetFormattedElapsedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_elapsedTime);
            
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours:D1}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            else
                return $"{timeSpan.Minutes:D1}:{timeSpan.Seconds:D2}";
        }
        
        /// <summary>
        /// Returns timer state for serialization
        /// </summary>
        public TimerSaveData GetSaveData()
        {
            return new TimerSaveData
            {
                MinInterval = _minInterval,
                MaxInterval = _maxInterval,
                CurrentInterval = _currentInterval,
                ElapsedTime = _elapsedTime,
                IsRunning = _isRunning,
                IsPaused = _isPaused,
                TimeScale = _timeScale
            };
        }
        
        /// <summary>
        /// Restores timer state from serialization
        /// </summary>
        public void LoadSaveData(TimerSaveData data)
        {
            _minInterval = data.MinInterval;
            _maxInterval = data.MaxInterval;
            _currentInterval = data.CurrentInterval;
            _elapsedTime = data.ElapsedTime;
            _isRunning = data.IsRunning;
            _isPaused = data.IsPaused;
            _timeScale = data.TimeScale;
        }

        #endregion
    }

    /// <summary>
    /// Serializable data for timer state persistence
    /// </summary>
    [Serializable]
    public struct TimerSaveData
    {
        public float MinInterval;
        public float MaxInterval;
        public float CurrentInterval;
        public float ElapsedTime;
        public bool IsRunning;
        public bool IsPaused;
        public float TimeScale;
    }
}
