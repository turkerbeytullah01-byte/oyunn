using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectAegis.Mobile
{
    /// <summary>
    /// Manages mobile-specific optimizations for performance and battery life
    /// </summary>
    public class MobileOptimizationManager : MonoBehaviour
    {
        public static MobileOptimizationManager Instance { get; private set; }
        
        [Header("Quality Settings")]
        [SerializeField] private MobileQualityLevel defaultQuality = MobileQualityLevel.Medium;
        [SerializeField] private bool autoDetectQuality = true;
        
        [Header("Performance")]
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private bool limitFrameRate = true;
        [SerializeField] private float updateInterval = 0.1f; // For non-critical updates
        
        [Header("Battery Optimization")]
        [SerializeField] private bool reduceFPSWhenBackgrounded = true;
        [SerializeField] private int backgroundFrameRate = 5;
        [SerializeField] private bool disableVSync = true;
        
        [Header("Memory Management")]
        [SerializeField] private long memoryWarningThresholdMB = 512;
        [SerializeField] private bool aggressiveGC = false;
        
        // Events
        public System.Action<MobileQualityLevel> OnQualityChanged;
        public System.Action OnLowMemoryWarning;
        
        // State
        private MobileQualityLevel currentQuality;
        private float lastUpdateTime;
        private bool isInBackground;
        
        public MobileQualityLevel CurrentQuality => currentQuality;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            InitializeOptimizations();
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            isInBackground = !hasFocus;
            
            if (reduceFPSWhenBackgrounded)
            {
                Application.targetFrameRate = isInBackground ? backgroundFrameRate : targetFrameRate;
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            isInBackground = pauseStatus;
            
            if (reduceFPSWhenBackgrounded)
            {
                Application.targetFrameRate = pauseStatus ? backgroundFrameRate : targetFrameRate;
            }
        }
        
        void Update()
        {
            // Check memory periodically
            if (Time.time - lastUpdateTime > updateInterval)
            {
                CheckMemoryUsage();
                lastUpdateTime = Time.time;
            }
        }
        
        private void InitializeOptimizations()
        {
            // Set target frame rate
            if (limitFrameRate)
            {
                Application.targetFrameRate = targetFrameRate;
            }
            
            // Disable VSync for mobile
            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }
            
            // Auto-detect quality
            if (autoDetectQuality)
            {
                DetectOptimalQuality();
            }
            else
            {
                SetQualityLevel(defaultQuality);
            }
            
            // Mobile-specific settings
            SetupMobileSettings();
            
            Debug.Log($"[MobileOptimization] Initialized with quality: {currentQuality}");
        }
        
        private void SetupMobileSettings()
        {
            // Reduce physics updates
            Time.fixedDeltaTime = 0.05f; // 20 physics updates per second
            
            // Disable unnecessary features
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            
            // Optimize shadows based on quality
            switch (currentQuality)
            {
                case MobileQualityLevel.Low:
                    QualitySettings.shadows = ShadowQuality.Disable;
                    break;
                case MobileQualityLevel.Medium:
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    break;
                case MobileQualityLevel.High:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    break;
            }
        }
        
        private void DetectOptimalQuality()
        {
            // Get device specs
            int processorCount = SystemInfo.processorCount;
            int systemMemory = SystemInfo.systemMemorySize;
            GraphicsDeviceType graphicsType = SystemInfo.graphicsDeviceType;
            
            // Determine quality based on specs
            if (systemMemory < 2048 || processorCount < 4)
            {
                SetQualityLevel(MobileQualityLevel.Low);
            }
            else if (systemMemory < 4096 || processorCount < 6)
            {
                SetQualityLevel(MobileQualityLevel.Medium);
            }
            else
            {
                SetQualityLevel(MobileQualityLevel.High);
            }
        }
        
        public void SetQualityLevel(MobileQualityLevel level)
        {
            currentQuality = level;
            
            switch (level)
            {
                case MobileQualityLevel.Low:
                    ApplyLowQuality();
                    break;
                case MobileQualityLevel.Medium:
                    ApplyMediumQuality();
                    break;
                case MobileQualityLevel.High:
                    ApplyHighQuality();
                    break;
            }
            
            OnQualityChanged?.Invoke(level);
            Debug.Log($"[MobileOptimization] Quality set to: {level}");
        }
        
        private void ApplyLowQuality()
        {
            QualitySettings.SetQualityLevel(0, true);
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.textureQuality = 1; // Half resolution
            QualitySettings.particleRaycastBudget = 0;
        }
        
        private void ApplyMediumQuality()
        {
            QualitySettings.SetQualityLevel(2, true);
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.textureQuality = 0; // Full resolution
            QualitySettings.particleRaycastBudget = 64;
        }
        
        private void ApplyHighQuality()
        {
            QualitySettings.SetQualityLevel(4, true);
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.antiAliasing = 2;
            QualitySettings.textureQuality = 0;
            QualitySettings.particleRaycastBudget = 256;
        }
        
        private void CheckMemoryUsage()
        {
            long usedMemory = GC.GetTotalMemory(false) / 1024 / 1024;
            long totalMemory = SystemInfo.systemMemorySize;
            
            if (usedMemory > memoryWarningThresholdMB)
            {
                Debug.LogWarning($"[MobileOptimization] Low memory warning: {usedMemory}MB used");
                OnLowMemoryWarning?.Invoke();
                
                if (aggressiveGC)
                {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                }
            }
        }
        
        /// <summary>
        /// Call this when loading a heavy scene
        /// </summary>
        public void PrepareForHeavyLoad()
        {
            // Reduce quality temporarily
            if (currentQuality > MobileQualityLevel.Low)
            {
                SetQualityLevel(MobileQualityLevel.Low);
            }
            
            // Force garbage collection
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        
        /// <summary>
        /// Call this after heavy load is complete
        /// </summary>
        public void RestoreAfterLoad()
        {
            if (autoDetectQuality)
            {
                DetectOptimalQuality();
            }
        }
        
        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetCurrentFPS()
        {
            return 1.0f / Time.unscaledDeltaTime;
        }
        
        /// <summary>
        /// Get memory usage in MB
        /// </summary>
        public long GetMemoryUsageMB()
        {
            return GC.GetTotalMemory(false) / 1024 / 1024;
        }
    }
    
    public enum MobileQualityLevel
    {
        Low,
        Medium,
        High
    }
}
