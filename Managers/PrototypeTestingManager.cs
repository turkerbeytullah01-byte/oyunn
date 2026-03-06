using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Types of prototype tests
    /// </summary>
    public enum PrototypeTestType
    {
        Flight,         // Tests maneuverability and stability
        Signal,         // Tests range and interference resistance
        BatteryStress   // Tests endurance and heat management
    }

    /// <summary>
    /// Status of a prototype test
    /// </summary>
    public enum PrototypeTestStatus
    {
        NotStarted,
        InProgress,
        Passed,
        Failed
    }

    /// <summary>
    /// Represents a single prototype test
    /// </summary>
    [Serializable]
    public class PrototypeTest
    {
        public string testId;
        public string droneId;
        public PrototypeTestType testType;
        public PrototypeTestStatus status;
        public float duration;          // In seconds
        public float baseCost;
        public float basePassChance;    // 0-1
        public float progress;          // 0-1
        public float timeRemaining;
        public DateTime startTime;
        public List<string> appliedTechBonuses;

        public PrototypeTest()
        {
            appliedTechBonuses = new List<string>();
            status = PrototypeTestStatus.NotStarted;
        }
    }

    /// <summary>
    /// Manages drone prototype testing before production
    /// </summary>
    public class PrototypeTestingManager : BaseManager<PrototypeTestingManager>
    {
        #region Constants
        private const float FLIGHT_TEST_DURATION = 300f;      // 5 minutes
        private const float SIGNAL_TEST_DURATION = 600f;      // 10 minutes
        private const float BATTERY_TEST_DURATION = 900f;     // 15 minutes
        
        private const float FLIGHT_TEST_COST = 500f;
        private const float SIGNAL_TEST_COST = 750f;
        private const float BATTERY_TEST_COST = 1000f;
        
        private const float BASE_PASS_CHANCE = 0.70f;
        private const float TECH_BONUS_PER_LEVEL = 0.05f;
        private const float MAX_PASS_CHANCE = 0.95f;
        
        private const float FAILURE_DELAY_MULTIPLIER = 1.5f;
        private const float FAILURE_COST_MULTIPLIER = 0.3f;
        #endregion

        #region Events
        public event Action<PrototypeTest> OnTestStarted;
        public event Action<PrototypeTest> OnTestPassed;
        public event Action<PrototypeTest> OnTestFailed;
        public event Action<PrototypeTest, float> OnTestProgress; // test, progress
        public event Action<PrototypeTest> OnTestCancelled;
        #endregion

        #region Properties
        [SerializeField] private List<PrototypeTest> _activeTests = new List<PrototypeTest>();
        [SerializeField] private List<PrototypeTest> _completedTests = new List<PrototypeTest>();
        
        public IReadOnlyList<PrototypeTest> ActiveTests => _activeTests.AsReadOnly();
        public IReadOnlyList<PrototypeTest> CompletedTests => _completedTests.AsReadOnly();
        
        public bool HasActiveTests => _activeTests.Count > 0;
        public int ActiveTestCount => _activeTests.Count;
        public int MaxConcurrentTests { get; private set; } = 2;
        #endregion

        #region Initialization
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log("[PrototypeTestingManager] Initialized");
        }

        protected override void OnSetupEventSubscriptions()
        {
            base.OnSetupEventSubscriptions();
            
            if (TimeManager.HasInstance)
            {
                TimeManager.Instance.OnTick += HandleGameTick;
            }
            
            if (TechTreeManager.HasInstance)
            {
                TechTreeManager.Instance.OnTechnologyUnlocked += HandleTechUnlocked;
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            
            if (TimeManager.HasInstance)
            {
                TimeManager.Instance.OnTick -= HandleGameTick;
            }
            
            if (TechTreeManager.HasInstance)
            {
                TechTreeManager.Instance.OnTechnologyUnlocked -= HandleTechUnlocked;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start a new prototype test
        /// </summary>
        public bool StartTest(string droneId, PrototypeTestType testType)
        {
            // Check if we can start more tests
            if (_activeTests.Count >= MaxConcurrentTests)
            {
                Debug.LogWarning("[PrototypeTestingManager] Maximum concurrent tests reached");
                return false;
            }
            
            // Check if drone exists
            if (!DroneManager.HasInstance || !DroneManager.Instance.IsDroneUnlocked(droneId))
            {
                Debug.LogWarning($"[PrototypeTestingManager] Drone {droneId} not unlocked");
                return false;
            }
            
            // Check if already testing this drone with same test type
            if (IsTestInProgress(droneId, testType))
            {
                Debug.LogWarning($"[PrototypeTestingManager] Test already in progress for {droneId}");
                return false;
            }
            
            // Get test cost
            float cost = GetTestCost(testType);
            
            // Check funds
            if (PlayerDataManager.HasInstance && !PlayerDataManager.Instance.CanAfford(cost))
            {
                Debug.LogWarning("[PrototypeTestingManager] Insufficient funds for test");
                return false;
            }
            
            // Deduct cost
            if (PlayerDataManager.HasInstance)
            {
                PlayerDataManager.Instance.SpendCurrency(cost);
            }
            
            // Create test
            PrototypeTest test = new PrototypeTest
            {
                testId = Guid.NewGuid().ToString(),
                droneId = droneId,
                testType = testType,
                status = PrototypeTestStatus.InProgress,
                duration = GetTestDuration(testType),
                baseCost = cost,
                basePassChance = GetBasePassChance(testType),
                progress = 0f,
                timeRemaining = GetTestDuration(testType),
                startTime = DateTime.Now,
                appliedTechBonuses = GetAppliedTechBonuses(testType)
            };
            
            _activeTests.Add(test);
            OnTestStarted?.Invoke(test);
            
            Debug.Log($"[PrototypeTestingManager] Started {testType} test for {droneId}");
            return true;
        }

        /// <summary>
        /// Cancel an active test
        /// </summary>
        public bool CancelTest(string testId)
        {
            PrototypeTest test = _activeTests.Find(t => t.testId == testId);
            if (test == null) return false;
            
            _activeTests.Remove(test);
            OnTestCancelled?.Invoke(test);
            
            Debug.Log($"[PrototypeTestingManager] Cancelled test {testId}");
            return true;
        }

        /// <summary>
        /// Check if a test is in progress for a specific drone and type
        /// </summary>
        public bool IsTestInProgress(string droneId, PrototypeTestType testType)
        {
            return _activeTests.Exists(t => t.droneId == droneId && t.testType == testType);
        }

        /// <summary>
        /// Check if all required tests are passed for a drone
        /// </summary>
        public bool AreAllTestsPassed(string droneId)
        {
            foreach (PrototypeTestType testType in Enum.GetValues(typeof(PrototypeTestType)))
            {
                if (!IsTestPassed(droneId, testType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if a specific test type is passed for a drone
        /// </summary>
        public bool IsTestPassed(string droneId, PrototypeTestType testType)
        {
            return _completedTests.Exists(t => 
                t.droneId == droneId && 
                t.testType == testType && 
                t.status == PrototypeTestStatus.Passed);
        }

        /// <summary>
        /// Get test progress for a specific test
        /// </summary>
        public float GetTestProgress(string testId)
        {
            PrototypeTest test = _activeTests.Find(t => t.testId == testId);
            return test?.progress ?? 0f;
        }

        /// <summary>
        /// Get the pass chance for a test type
        /// </summary>
        public float GetPassChance(PrototypeTestType testType)
        {
            float baseChance = GetBasePassChance(testType);
            float techBonus = CalculateTechBonus(testType);
            return Mathf.Min(baseChance + techBonus, MAX_PASS_CHANCE);
        }

        /// <summary>
        /// Get test cost
        /// </summary>
        public float GetTestCost(PrototypeTestType testType)
        {
            return testType switch
            {
                PrototypeTestType.Flight => FLIGHT_TEST_COST,
                PrototypeTestType.Signal => SIGNAL_TEST_COST,
                PrototypeTestType.BatteryStress => BATTERY_TEST_COST,
                _ => FLIGHT_TEST_COST
            };
        }

        /// <summary>
        /// Get test duration in seconds
        /// </summary>
        public float GetTestDuration(PrototypeTestType testType)
        {
            return testType switch
            {
                PrototypeTestType.Flight => FLIGHT_TEST_DURATION,
                PrototypeTestType.Signal => SIGNAL_TEST_DURATION,
                PrototypeTestType.BatteryStress => BATTERY_TEST_DURATION,
                _ => FLIGHT_TEST_DURATION
            };
        }

        /// <summary>
        /// Get test description
        /// </summary>
        public string GetTestDescription(PrototypeTestType testType)
        {
            return testType switch
            {
                PrototypeTestType.Flight => "Tests maneuverability, stability, and control responsiveness",
                PrototypeTestType.Signal => "Tests communication range, signal strength, and interference resistance",
                PrototypeTestType.BatteryStress => "Tests battery endurance, heat management, and power efficiency",
                _ => "Unknown test type"
            };
        }

        /// <summary>
        /// Get test name
        /// </summary>
        public string GetTestName(PrototypeTestType testType)
        {
            return testType switch
            {
                PrototypeTestType.Flight => "Flight Test",
                PrototypeTestType.Signal => "Signal Test",
                PrototypeTestType.BatteryStress => "Battery Stress Test",
                _ => "Unknown Test"
            };
        }

        /// <summary>
        /// Get active test for a drone and type
        /// </summary>
        public PrototypeTest GetActiveTest(string droneId, PrototypeTestType testType)
        {
            return _activeTests.Find(t => t.droneId == droneId && t.testType == testType);
        }

        /// <summary>
        /// Get all passed tests for a drone
        /// </summary>
        public List<PrototypeTestType> GetPassedTests(string droneId)
        {
            List<PrototypeTestType> passed = new List<PrototypeTestType>();
            
            foreach (var test in _completedTests)
            {
                if (test.droneId == droneId && test.status == PrototypeTestStatus.Passed)
                {
                    passed.Add(test.testType);
                }
            }
            
            return passed;
        }
        #endregion

        #region Private Methods
        private float GetBasePassChance(PrototypeTestType testType)
        {
            return testType switch
            {
                PrototypeTestType.Flight => BASE_PASS_CHANCE + 0.05f,
                PrototypeTestType.Signal => BASE_PASS_CHANCE,
                PrototypeTestType.BatteryStress => BASE_PASS_CHANCE - 0.05f,
                _ => BASE_PASS_CHANCE
            };
        }

        private List<string> GetAppliedTechBonuses(PrototypeTestType testType)
        {
            List<string> bonuses = new List<string>();
            
            if (!TechTreeManager.HasInstance) return bonuses;
            
            // Check for relevant technologies
            switch (testType)
            {
                case PrototypeTestType.Flight:
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("flight_stabilization"))
                        bonuses.Add("flight_stabilization");
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("advanced_gyroscopes"))
                        bonuses.Add("advanced_gyroscopes");
                    break;
                    
                case PrototypeTestType.Signal:
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("signal_boosting"))
                        bonuses.Add("signal_boosting");
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("frequency_hopping"))
                        bonuses.Add("frequency_hopping");
                    break;
                    
                case PrototypeTestType.BatteryStress:
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("efficient_cooling"))
                        bonuses.Add("efficient_cooling");
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("thermal_management"))
                        bonuses.Add("thermal_management");
                    break;
            }
            
            return bonuses;
        }

        private float CalculateTechBonus(PrototypeTestType testType)
        {
            var bonuses = GetAppliedTechBonuses(testType);
            return bonuses.Count * TECH_BONUS_PER_LEVEL;
        }

        private void CompleteTest(PrototypeTest test, bool passed)
        {
            _activeTests.Remove(test);
            test.status = passed ? PrototypeTestStatus.Passed : PrototypeTestStatus.Failed;
            test.progress = 1f;
            _completedTests.Add(test);
            
            if (passed)
            {
                OnTestPassed?.Invoke(test);
                Debug.Log($"[PrototypeTestingManager] Test {test.testType} PASSED for {test.droneId}");
                
                // Trigger event
                if (EventManager.HasInstance)
                {
                    EventManager.Instance.TriggerEvent(GameEventType.PrototypeTestComplete, 
                        new EventContext { StringValue = test.droneId, FloatValue = 1f });
                }
            }
            else
            {
                OnTestFailed?.Invoke(test);
                Debug.Log($"[PrototypeTestingManager] Test {test.testType} FAILED for {test.droneId}");
                
                // Apply failure consequences
                ApplyFailureConsequences(test);
            }
        }

        private void ApplyFailureConsequences(PrototypeTest test)
        {
            // Delay: Need to retest
            float delayCost = test.baseCost * FAILURE_COST_MULTIPLIER;
            
            if (PlayerDataManager.HasInstance)
            {
                PlayerDataManager.Instance.SpendCurrency(delayCost);
            }
            
            Debug.Log($"[PrototypeTestingManager] Failure cost: {delayCost:F0}");
        }

        private void HandleGameTick(float deltaTime)
        {
            // Update active tests
            for (int i = _activeTests.Count - 1; i >= 0; i--)
            {
                PrototypeTest test = _activeTests[i];
                test.timeRemaining -= deltaTime;
                test.progress = 1f - (test.timeRemaining / test.duration);
                
                OnTestProgress?.Invoke(test, test.progress);
                
                if (test.timeRemaining <= 0)
                {
                    // Test complete - determine pass/fail
                    float passChance = GetPassChance(test.testType);
                    bool passed = UnityEngine.Random.value < passChance;
                    CompleteTest(test, passed);
                }
            }
        }

        private void HandleTechUnlocked(TechnologyNode tech)
        {
            // Check if tech increases max concurrent tests
            if (tech.nodeId == "parallel_testing")
            {
                MaxConcurrentTests++;
                Debug.Log($"[PrototypeTestingManager] Max concurrent tests increased to {MaxConcurrentTests}");
            }
        }
        #endregion

        #region Save/Load
        public PrototypeTestingSaveData GetSaveData()
        {
            return new PrototypeTestingSaveData
            {
                activeTests = new List<PrototypeTest>(_activeTests),
                completedTests = new List<PrototypeTest>(_completedTests),
                maxConcurrentTests = MaxConcurrentTests
            };
        }

        public void LoadSaveData(PrototypeTestingSaveData data)
        {
            if (data != null)
            {
                _activeTests = data.activeTests ?? new List<PrototypeTest>();
                _completedTests = data.completedTests ?? new List<PrototypeTest>();
                MaxConcurrentTests = data.maxConcurrentTests;
                
                Debug.Log($"[PrototypeTestingManager] Loaded { _activeTests.Count} active tests, {_completedTests.Count} completed tests");
            }
        }
        #endregion
    }

    #region Save Data
    [Serializable]
    public class PrototypeTestingSaveData
    {
        public List<PrototypeTest> activeTests;
        public List<PrototypeTest> completedTests;
        public int maxConcurrentTests;
    }
    #endregion
}
