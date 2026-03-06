using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Performance monitor for tracking FPS, memory, and other metrics
    /// Helps identify performance issues during development
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        #region Singleton
        private static PerformanceMonitor _instance;
        public static PerformanceMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PerformanceMonitor>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PerformanceMonitor");
                        _instance = go.AddComponent<PerformanceMonitor>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI memoryText;
        [SerializeField] private TextMeshProUGUI drawCallsText;
        [SerializeField] private TextMeshProUGUI trianglesText;
        [SerializeField] private TextMeshProUGUI verticesText;
        [SerializeField] private TextMeshProUGUI textureMemoryText;
        [SerializeField] private TextMeshProUGUI gcText;
        [SerializeField] private TextMeshProUGUI batteryText;
        [SerializeField] private TextMeshProUGUI thermalText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Toggle showOnScreenToggle;
        [SerializeField] private Toggle detailedModeToggle;
        #endregion

        #region Settings
        [Header("Update Settings")]
        [Tooltip("How often to update the display (seconds)")]
        public float updateInterval = 1f;
        
        [Tooltip("Show monitor on startup")]
        public bool showOnStart = true;
        
        [Tooltip("Enable detailed metrics")]
        public bool detailedMode = false;
        
        [Tooltip("Position on screen")]
        public TextAnchor screenPosition = TextAnchor.UpperRight;
        
        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        
        [Tooltip("Text color")]
        public Color textColor = Color.white;
        #endregion

        #region Thresholds
        [Header("Performance Thresholds")]
        [Tooltip("FPS below this is considered poor")]
        public float poorFpsThreshold = 30f;
        
        [Tooltip("FPS below this is considered critical")]
        public float criticalFpsThreshold = 15f;
        
        [Tooltip("Memory above this (MB) is considered high")]
        public long highMemoryThreshold = 512;
        
        [Tooltip("Memory above this (MB) is considered critical")]
        public long criticalMemoryThreshold = 1024;
        #endregion

        #region Events
        public event Action<float> OnFpsDropped;
        public event Action<long> OnMemoryHigh;
        public event Action OnPerformancePoor;
        public event Action OnPerformanceCritical;
        #endregion

        #region Private Fields
        private float _fps;
        private float _fpsAccumulator;
        private int _fpsFrames;
        private float _lastFpsUpdate;
        
        private long _memoryUsage;
        private long _lastMemoryUsage;
        private long _textureMemory;
        
        private int _drawCalls;
        private int _triangles;
        private int _vertices;
        
        private int _gcCollections;
        private int _lastGcCollections;
        
        private Queue<float> _fpsHistory = new Queue<float>();
        private Queue<long> _memoryHistory = new Queue<long>();
        private const int HISTORY_SIZE = 60;
        
        private bool _isVisible = true;
        private Coroutine _updateCoroutine;
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
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            SetupUI();
            
            _isVisible = showOnStart;
            
            if (_isVisible)
            {
                StartMonitoring();
            }
        }

        private void OnDestroy()
        {
            StopMonitoring();
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            if (showOnScreenToggle != null)
            {
                showOnScreenToggle.isOn = showOnStart;
                showOnScreenToggle.onValueChanged.AddListener(SetVisibility);
            }
            
            if (detailedModeToggle != null)
            {
                detailedModeToggle.isOn = detailedMode;
                detailedModeToggle.onValueChanged.AddListener(SetDetailedMode);
            }
            
            // Apply colors
            if (canvasGroup != null)
            {
                // Background would be set on a panel
            }
            
            UpdateTextColors();
        }

        private void UpdateTextColors()
        {
            TextMeshProUGUI[] texts = { fpsText, memoryText, drawCallsText, trianglesText, 
                                       verticesText, textureMemoryText, gcText, batteryText, thermalText };
            
            foreach (var text in texts)
            {
                if (text != null)
                {
                    text.color = textColor;
                }
            }
        }
        #endregion

        #region Monitoring Control
        /// <summary>
        /// Start performance monitoring
        /// </summary>
        public void StartMonitoring()
        {
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }
            
            _updateCoroutine = StartCoroutine(MonitoringCoroutine());
            _isVisible = true;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Stop performance monitoring
        /// </summary>
        public void StopMonitoring()
        {
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        /// <summary>
        /// Toggle monitoring visibility
        /// </summary>
        public void ToggleVisibility()
        {
            SetVisibility(!_isVisible);
        }

        /// <summary>
        /// Set monitoring visibility
        /// </summary>
        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
            
            if (visible && _updateCoroutine == null)
            {
                StartMonitoring();
            }
            else if (!visible && _updateCoroutine != null)
            {
                StopMonitoring();
            }
        }

        /// <summary>
        /// Set detailed mode
        /// </summary>
        public void SetDetailedMode(bool detailed)
        {
            detailedMode = detailed;
            UpdateDisplay();
        }
        #endregion

        #region Monitoring Coroutine
        private IEnumerator MonitoringCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);
                
                CollectMetrics();
                UpdateDisplay();
                CheckThresholds();
            }
        }

        private void CollectMetrics()
        {
            // FPS calculation
            float currentFps = 1f / Time.unscaledDeltaTime;
            _fpsAccumulator += currentFps;
            _fpsFrames++;
            
            if (Time.unscaledTime - _lastFpsUpdate >= updateInterval)
            {
                _fps = _fpsAccumulator / _fpsFrames;
                _fpsAccumulator = 0f;
                _fpsFrames = 0;
                _lastFpsUpdate = Time.unscaledTime;
                
                // Add to history
                _fpsHistory.Enqueue(_fps);
                while (_fpsHistory.Count > HISTORY_SIZE)
                {
                    _fpsHistory.Dequeue();
                }
            }
            
            // Memory usage
            _lastMemoryUsage = _memoryUsage;
            _memoryUsage = GC.GetTotalMemory(false) / (1024 * 1024);
            
            _memoryHistory.Enqueue(_memoryUsage);
            while (_memoryHistory.Count > HISTORY_SIZE)
            {
                _memoryHistory.Dequeue();
            }
            
            // GC collections
            _lastGcCollections = _gcCollections;
            _gcCollections = GC.CollectionCount(0);
            
            // Note: Draw calls, triangles, vertices, and texture memory
            // require Unity Profiler API which is not available in runtime
            // These are placeholders that would need platform-specific implementations
            #if UNITY_EDITOR
            // In editor, we could use the profiler API
            #endif
        }

        private void UpdateDisplay()
        {
            // FPS
            if (fpsText != null)
            {
                Color fpsColor = GetFpsColor(_fps);
                fpsText.text = $"FPS: {_fps:F0}";
                fpsText.color = fpsColor;
            }
            
            // Memory
            if (memoryText != null)
            {
                Color memColor = GetMemoryColor(_memoryUsage);
                long memDelta = _memoryUsage - _lastMemoryUsage;
                string deltaStr = memDelta > 0 ? $" (+{memDelta})" : memDelta < 0 ? $" ({memDelta})" : "";
                memoryText.text = $"Memory: {_memoryUsage} MB{deltaStr}";
                memoryText.color = memColor;
            }
            
            // Draw calls (placeholder)
            if (drawCallsText != null && detailedMode)
            {
                drawCallsText.text = $"Draw Calls: N/A";
                drawCallsText.gameObject.SetActive(true);
            }
            else if (drawCallsText != null)
            {
                drawCallsText.gameObject.SetActive(false);
            }
            
            // Triangles (placeholder)
            if (trianglesText != null && detailedMode)
            {
                trianglesText.text = $"Triangles: N/A";
                trianglesText.gameObject.SetActive(true);
            }
            else if (trianglesText != null)
            {
                trianglesText.gameObject.SetActive(false);
            }
            
            // Vertices (placeholder)
            if (verticesText != null && detailedMode)
            {
                verticesText.text = $"Vertices: N/A";
                verticesText.gameObject.SetActive(true);
            }
            else if (verticesText != null)
            {
                verticesText.gameObject.SetActive(false);
            }
            
            // Texture memory (placeholder)
            if (textureMemoryText != null && detailedMode)
            {
                textureMemoryText.text = $"Texture: N/A";
                textureMemoryText.gameObject.SetActive(true);
            }
            else if (textureMemoryText != null)
            {
                textureMemoryText.gameObject.SetActive(false);
            }
            
            // GC
            if (gcText != null)
            {
                int gcDelta = _gcCollections - _lastGcCollections;
                string gcStr = gcDelta > 0 ? $" (+{gcDelta})" : "";
                gcText.text = $"GC: Gen0={_gcCollections}{gcStr}";
            }
            
            // Battery (mobile)
            if (batteryText != null && detailedMode)
            {
                #if UNITY_ANDROID || UNITY_IOS
                batteryText.text = $"Battery: {SystemInfo.batteryLevel * 100f:F0}%";
                #else
                batteryText.text = "Battery: N/A";
                #endif
                batteryText.gameObject.SetActive(true);
            }
            else if (batteryText != null)
            {
                batteryText.gameObject.SetActive(false);
            }
            
            // Thermal (mobile)
            if (thermalText != null && detailedMode)
            {
                thermalText.text = "Thermal: N/A";
                thermalText.gameObject.SetActive(true);
            }
            else if (thermalText != null)
            {
                thermalText.gameObject.SetActive(false);
            }
        }

        private void CheckThresholds()
        {
            // FPS thresholds
            if (_fps < criticalFpsThreshold)
            {
                OnPerformanceCritical?.Invoke();
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"CRITICAL: FPS dropped to {_fps:F0}");
                }
            }
            else if (_fps < poorFpsThreshold)
            {
                OnFpsDropped?.Invoke(_fps);
                OnPerformancePoor?.Invoke();
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"WARNING: FPS dropped to {_fps:F0}");
                }
            }
            
            // Memory thresholds
            if (_memoryUsage > criticalMemoryThreshold)
            {
                OnMemoryHigh?.Invoke(_memoryUsage);
                OnPerformanceCritical?.Invoke();
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"CRITICAL: Memory usage at {_memoryUsage} MB");
                }
            }
            else if (_memoryUsage > highMemoryThreshold)
            {
                OnMemoryHigh?.Invoke(_memoryUsage);
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"WARNING: Memory usage at {_memoryUsage} MB");
                }
            }
        }
        #endregion

        #region Color Helpers
        private Color GetFpsColor(float fps)
        {
            if (fps >= 60) return Color.green;
            if (fps >= poorFpsThreshold) return Color.yellow;
            if (fps >= criticalFpsThreshold) return new Color(1f, 0.5f, 0f); // Orange
            return Color.red;
        }

        private Color GetMemoryColor(long memory)
        {
            if (memory < highMemoryThreshold / 2) return Color.green;
            if (memory < highMemoryThreshold) return Color.yellow;
            if (memory < criticalMemoryThreshold) return new Color(1f, 0.5f, 0f); // Orange
            return Color.red;
        }
        #endregion

        #region Public Getters
        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetCurrentFPS()
        {
            return _fps;
        }

        /// <summary>
        /// Get average FPS over history
        /// </summary>
        public float GetAverageFPS()
        {
            if (_fpsHistory.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (float fps in _fpsHistory)
            {
                sum += fps;
            }
            return sum / _fpsHistory.Count;
        }

        /// <summary>
        /// Get minimum FPS in history
        /// </summary>
        public float GetMinFPS()
        {
            if (_fpsHistory.Count == 0) return 0f;
            
            float min = float.MaxValue;
            foreach (float fps in _fpsHistory)
            {
                if (fps < min) min = fps;
            }
            return min;
        }

        /// <summary>
        /// Get current memory usage in MB
        /// </summary>
        public long GetCurrentMemory()
        {
            return _memoryUsage;
        }

        /// <summary>
        /// Get average memory usage
        /// </summary>
        public long GetAverageMemory()
        {
            if (_memoryHistory.Count == 0) return 0;
            
            long sum = 0;
            foreach (long mem in _memoryHistory)
            {
                sum += mem;
            }
            return sum / _memoryHistory.Count;
        }

        /// <summary>
        /// Get FPS history
        /// </summary>
        public float[] GetFPSHistory()
        {
            return _fpsHistory.ToArray();
        }

        /// <summary>
        /// Get memory history
        /// </summary>
        public long[] GetMemoryHistory()
        {
            return _memoryHistory.ToArray();
        }

        /// <summary>
        /// Get performance report
        /// </summary>
        public string GetPerformanceReport()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== PERFORMANCE REPORT ===");
            sb.AppendLine();
            sb.AppendLine($"Current FPS: {_fps:F1}");
            sb.AppendLine($"Average FPS: {GetAverageFPS():F1}");
            sb.AppendLine($"Min FPS: {GetMinFPS():F1}");
            sb.AppendLine();
            sb.AppendLine($"Current Memory: {_memoryUsage} MB");
            sb.AppendLine($"Average Memory: {GetAverageMemory()} MB");
            sb.AppendLine();
            sb.AppendLine($"GC Collections: {_gcCollections}");
            sb.AppendLine();
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"RAM: {SystemInfo.systemMemorySize} MB");
            
            return sb.ToString();
        }
        #endregion

        #region Snapshot
        /// <summary>
        /// Take a performance snapshot
        /// </summary>
        public PerformanceSnapshot TakeSnapshot()
        {
            return new PerformanceSnapshot
            {
                timestamp = DateTime.Now,
                fps = _fps,
                memoryUsage = _memoryUsage,
                gcCollections = _gcCollections,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };
        }
        #endregion
    }

    /// <summary>
    /// Performance snapshot data
    /// </summary>
    [System.Serializable]
    public class PerformanceSnapshot
    {
        public DateTime timestamp;
        public float fps;
        public long memoryUsage;
        public int gcCollections;
        public string sceneName;
    }
}
