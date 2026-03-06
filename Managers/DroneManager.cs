using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Represents a drone in the production queue
    /// </summary>
    [Serializable]
    public class ProductionOrder
    {
        public string orderId;
        public string droneId;
        public int quantity;
        public float progress;      // 0-1
        public float timeRemaining;
        public float totalTime;
        public DateTime startTime;
        public bool isPaused;
        public float totalCost;
    }

    /// <summary>
    /// Represents a drone in inventory
    /// </summary>
    [Serializable]
    public class DroneInventoryEntry
    {
        public string droneId;
        public int count;
        public int deployedCount;
        public int maintenanceCount;
        
        public int AvailableCount => count - deployedCount - maintenanceCount;
    }

    /// <summary>
    /// Manages drone unlocks, production, and inventory
    /// </summary>
    public class DroneManager : BaseManager<DroneManager>
    {
        #region Constants
        private const int MAX_PRODUCTION_QUEUE = 5;
        private const float PRODUCTION_SPEED_BASE = 1f;
        #endregion

        #region Events
        public event Action<DroneData> OnDroneUnlocked;
        public event Action<ProductionOrder> OnProductionStarted;
        public event Action<ProductionOrder> OnProductionCompleted;
        public event Action<ProductionOrder, float> OnProductionProgress;
        public event Action<ProductionOrder> OnProductionCancelled;
        public event Action<string, int> OnInventoryChanged; // droneId, newCount
        public event Action<string> OnDroneDeployed;
        public event Action<string> OnDroneReturned;
        #endregion

        #region Properties
        [Header("Drone Database")]
        [SerializeField] private List<DroneData> _allDrones = new List<DroneData>();
        
        [Header("Unlocked Drones")]
        [SerializeField] private List<string> _unlockedDroneIds = new List<string>();
        
        [Header("Production")]
        [SerializeField] private List<ProductionOrder> _productionQueue = new List<ProductionOrder>();
        
        [Header("Inventory")]
        [SerializeField] private List<DroneInventoryEntry> _inventory = new List<DroneInventoryEntry>();
        
        public IReadOnlyList<DroneData> AllDrones => _allDrones.AsReadOnly();
        public IReadOnlyList<ProductionOrder> ProductionQueue => _productionQueue.AsReadOnly();
        public IReadOnlyList<DroneInventoryEntry> Inventory => _inventory.AsReadOnly();
        
        public int ProductionQueueCount => _productionQueue.Count;
        public bool CanAddToQueue => _productionQueue.Count < MAX_PRODUCTION_QUEUE;
        public bool IsProducing => _productionQueue.Count > 0 && !_productionQueue[0].isPaused;
        
        public ProductionOrder CurrentProduction => _productionQueue.Count > 0 ? _productionQueue[0] : null;
        #endregion

        #region Initialization
        protected override void OnInitialize()
        {
            base.OnInitialize();
            LoadDroneDatabase();
            Debug.Log($"[DroneManager] Initialized with {_allDrones.Count} drone types");
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

        #region Public Methods - Unlocking
        /// <summary>
        /// Unlock a drone for production
        /// </summary>
        public bool UnlockDrone(string droneId)
        {
            if (IsDroneUnlocked(droneId))
            {
                Debug.LogWarning($"[DroneManager] Drone {droneId} already unlocked");
                return false;
            }
            
            DroneData drone = GetDroneData(droneId);
            if (drone == null)
            {
                Debug.LogError($"[DroneManager] Drone {droneId} not found");
                return false;
            }
            
            // Check requirements
            if (!drone.CanUnlock())
            {
                Debug.LogWarning($"[DroneManager] Requirements not met for {droneId}");
                return false;
            }
            
            // Check unlock cost
            if (drone.unlockCost > 0 && PlayerDataManager.HasInstance)
            {
                if (!PlayerDataManager.Instance.CanAfford(drone.unlockCost))
                {
                    Debug.LogWarning($"[DroneManager] Insufficient funds to unlock {droneId}");
                    return false;
                }
                PlayerDataManager.Instance.SpendCurrency(drone.unlockCost);
            }
            
            _unlockedDroneIds.Add(droneId);
            OnDroneUnlocked?.Invoke(drone);
            
            Debug.Log($"[DroneManager] Unlocked drone: {drone.displayName}");
            
            // Trigger event
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEventType.DroneUnlocked, 
                    new EventContext { StringValue = droneId });
            }
            
            return true;
        }

        /// <summary>
        /// Check if a drone is unlocked
        /// </summary>
        public bool IsDroneUnlocked(string droneId)
        {
            return _unlockedDroneIds.Contains(droneId);
        }

        /// <summary>
        /// Get all unlocked drones
        /// </summary>
        public List<DroneData> GetUnlockedDrones()
        {
            return _allDrones.Where(d => _unlockedDroneIds.Contains(d.droneId)).ToList();
        }

        /// <summary>
        /// Get all locked drones that can potentially be unlocked
        /// </summary>
        public List<DroneData> GetLockedDrones()
        {
            return _allDrones.Where(d => !_unlockedDroneIds.Contains(d.droneId)).ToList();
        }

        /// <summary>
        /// Get drones that meet unlock requirements but aren't unlocked yet
        /// </summary>
        public List<DroneData> GetUnlockableDrones()
        {
            return _allDrones.Where(d => 
                !_unlockedDroneIds.Contains(d.droneId) && 
                d.CanUnlock()).ToList();
        }
        #endregion

        #region Public Methods - Production
        /// <summary>
        /// Add a drone to the production queue
        /// </summary>
        public bool QueueProduction(string droneId, int quantity = 1)
        {
            if (!CanAddToQueue)
            {
                Debug.LogWarning("[DroneManager] Production queue is full");
                return false;
            }
            
            if (!IsDroneUnlocked(droneId))
            {
                Debug.LogWarning($"[DroneManager] Drone {droneId} not unlocked");
                return false;
            }
            
            DroneData drone = GetDroneData(droneId);
            if (drone == null) return false;
            
            float totalCost = drone.productionCost * quantity;
            
            // Check funds
            if (PlayerDataManager.HasInstance && !PlayerDataManager.Instance.CanAfford(totalCost))
            {
                Debug.LogWarning("[DroneManager] Insufficient funds for production");
                return false;
            }
            
            // Deduct cost
            if (PlayerDataManager.HasInstance)
            {
                PlayerDataManager.Instance.SpendCurrency(totalCost);
            }
            
            ProductionOrder order = new ProductionOrder
            {
                orderId = Guid.NewGuid().ToString(),
                droneId = droneId,
                quantity = quantity,
                progress = 0f,
                timeRemaining = drone.GetProductionTime() * quantity,
                totalTime = drone.GetProductionTime() * quantity,
                startTime = DateTime.Now,
                isPaused = false,
                totalCost = totalCost
            };
            
            _productionQueue.Add(order);
            OnProductionStarted?.Invoke(order);
            
            Debug.Log($"[DroneManager] Queued production of {quantity}x {drone.displayName}");
            return true;
        }

        /// <summary>
        /// Cancel a production order
        /// </summary>
        public bool CancelProduction(string orderId)
        {
            ProductionOrder order = _productionQueue.Find(o => o.orderId == orderId);
            if (order == null) return false;
            
            // Refund partial cost based on progress
            float refundRatio = 1f - (order.progress * 0.5f); // Lose 50% of spent progress
            float refund = order.totalCost * refundRatio;
            
            if (PlayerDataManager.HasInstance)
            {
                PlayerDataManager.Instance.AddCurrency(refund);
            }
            
            _productionQueue.Remove(order);
            OnProductionCancelled?.Invoke(order);
            
            Debug.Log($"[DroneManager] Cancelled production order. Refunded: {refund:F0}");
            return true;
        }

        /// <summary>
        /// Pause/resume production
        /// </summary>
        public void SetProductionPaused(bool paused)
        {
            if (_productionQueue.Count > 0)
            {
                _productionQueue[0].isPaused = paused;
            }
        }

        /// <summary>
        /// Get estimated completion time for a queue position
        /// </summary>
        public float GetEstimatedCompletionTime(int queuePosition)
        {
            float totalTime = 0f;
            for (int i = 0; i <= queuePosition && i < _productionQueue.Count; i++)
            {
                totalTime += _productionQueue[i].timeRemaining;
            }
            return totalTime;
        }
        #endregion

        #region Public Methods - Inventory
        /// <summary>
        /// Get inventory count for a drone
        /// </summary>
        public int GetInventoryCount(string droneId)
        {
            var entry = _inventory.Find(e => e.droneId == droneId);
            return entry?.count ?? 0;
        }

        /// <summary>
        /// Get available (non-deployed) count for a drone
        /// </summary>
        public int GetAvailableCount(string droneId)
        {
            var entry = _inventory.Find(e => e.droneId == droneId);
            return entry?.AvailableCount ?? 0;
        }

        /// <summary>
        /// Deploy a drone (mark as in use)
        /// </summary>
        public bool DeployDrone(string droneId)
        {
            var entry = _inventory.Find(e => e.droneId == droneId);
            if (entry == null || entry.AvailableCount <= 0)
            {
                Debug.LogWarning($"[DroneManager] No available drones of type {droneId}");
                return false;
            }
            
            entry.deployedCount++;
            OnDroneDeployed?.Invoke(droneId);
            
            return true;
        }

        /// <summary>
        /// Return a deployed drone
        /// </summary>
        public bool ReturnDrone(string droneId)
        {
            var entry = _inventory.Find(e => e.droneId == droneId);
            if (entry == null || entry.deployedCount <= 0)
            {
                Debug.LogWarning($"[DroneManager] No deployed drones of type {droneId}");
                return false;
            }
            
            entry.deployedCount--;
            OnDroneReturned?.Invoke(droneId);
            
            return true;
        }

        /// <summary>
        /// Get total drone count across all types
        /// </summary>
        public int GetTotalDroneCount()
        {
            return _inventory.Sum(e => e.count);
        }

        /// <summary>
        /// Get total deployed drone count
        /// </summary>
        public int GetTotalDeployedCount()
        {
            return _inventory.Sum(e => e.deployedCount);
        }
        #endregion

        #region Public Methods - Queries
        /// <summary>
        /// Get drone data by ID
        /// </summary>
        public DroneData GetDroneData(string droneId)
        {
            return _allDrones.Find(d => d.droneId == droneId);
        }

        /// <summary>
        /// Get drones by class
        /// </summary>
        public List<DroneData> GetDronesByClass(DroneClass droneClass)
        {
            return _allDrones.Where(d => d.droneClass == droneClass).ToList();
        }

        /// <summary>
        /// Search drones by name
        /// </summary>
        public List<DroneData> SearchDrones(string searchTerm)
        {
            string term = searchTerm.ToLower();
            return _allDrones.Where(d => 
                d.displayName.ToLower().Contains(term) ||
                d.description.ToLower().Contains(term)).ToList();
        }

        /// <summary>
        /// Get production order by ID
        /// </summary>
        public ProductionOrder GetProductionOrder(string orderId)
        {
            return _productionQueue.Find(o => o.orderId == orderId);
        }

        /// <summary>
        /// Get inventory entry for a drone
        /// </summary>
        public DroneInventoryEntry GetInventoryEntry(string droneId)
        {
            return _inventory.Find(e => e.droneId == droneId);
        }
        #endregion

        #region Private Methods
        private void LoadDroneDatabase()
        {
            // Load all DroneData assets from Resources
            DroneData[] drones = Resources.LoadAll<DroneData>("Drones");
            _allDrones = new List<DroneData>(drones);
            
            // Auto-unlock drones marked as starting unlocked
            foreach (var drone in _allDrones)
            {
                if (drone.startsUnlocked && !_unlockedDroneIds.Contains(drone.droneId))
                {
                    _unlockedDroneIds.Add(drone.droneId);
                }
            }
        }

        private void CompleteProduction(ProductionOrder order)
        {
            _productionQueue.Remove(order);
            order.progress = 1f;
            OnProductionCompleted?.Invoke(order);
            
            // Add to inventory
            AddToInventory(order.droneId, order.quantity);
            
            Debug.Log($"[DroneManager] Production complete: {order.quantity}x {GetDroneData(order.droneId)?.displayName}");
            
            // Trigger event
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEventType.ProductionComplete, 
                    new EventContext { StringValue = order.droneId, IntValue = order.quantity });
            }
        }

        private void AddToInventory(string droneId, int quantity)
        {
            var entry = _inventory.Find(e => e.droneId == droneId);
            if (entry == null)
            {
                entry = new DroneInventoryEntry
                {
                    droneId = droneId,
                    count = 0,
                    deployedCount = 0,
                    maintenanceCount = 0
                };
                _inventory.Add(entry);
            }
            
            entry.count += quantity;
            OnInventoryChanged?.Invoke(droneId, entry.count);
        }

        private float GetProductionSpeed()
        {
            float speed = PRODUCTION_SPEED_BASE;
            
            // Apply tech bonuses
            if (TechTreeManager.HasInstance)
            {
                if (TechTreeManager.Instance.IsTechnologyUnlocked("assembly_automation"))
                    speed *= 1.2f;
                if (TechTreeManager.Instance.IsTechnologyUnlocked("advanced_manufacturing"))
                    speed *= 1.3f;
            }
            
            return speed;
        }
        #endregion

        #region Event Handlers
        private void HandleGameTick(float deltaTime)
        {
            if (_productionQueue.Count == 0) return;
            
            ProductionOrder current = _productionQueue[0];
            if (current.isPaused) return;
            
            float speed = GetProductionSpeed();
            current.timeRemaining -= deltaTime * speed;
            current.progress = 1f - (current.timeRemaining / current.totalTime);
            
            OnProductionProgress?.Invoke(current, current.progress);
            
            if (current.timeRemaining <= 0)
            {
                CompleteProduction(current);
            }
        }

        private void HandleTechUnlocked(TechnologyNode tech)
        {
            // Check if any drones can now be unlocked
            foreach (var drone in GetLockedDrones())
            {
                if (drone.CanUnlock() && drone.requiredTechnologies.Contains(tech.nodeId))
                {
                    Debug.Log($"[DroneManager] Drone {drone.displayName} is now unlockable!");
                    
                    // Notify UI
                    if (EventManager.HasInstance)
                    {
                        EventManager.Instance.TriggerEvent(GameEventType.Notification, 
                            new EventContext { StringValue = $"New drone available: {drone.displayName}" });
                    }
                }
            }
        }
        #endregion

        #region Save/Load
        public DroneManagerSaveData GetSaveData()
        {
            return new DroneManagerSaveData
            {
                unlockedDroneIds = new List<string>(_unlockedDroneIds),
                productionQueue = new List<ProductionOrder>(_productionQueue),
                inventory = new List<DroneInventoryEntry>(_inventory)
            };
        }

        public void LoadSaveData(DroneManagerSaveData data)
        {
            if (data != null)
            {
                _unlockedDroneIds = data.unlockedDroneIds ?? new List<string>();
                _productionQueue = data.productionQueue ?? new List<ProductionOrder>();
                _inventory = data.inventory ?? new List<DroneInventoryEntry>();
                
                Debug.Log($"[DroneManager] Loaded {_unlockedDroneIds.Count} unlocked drones, {_productionQueue.Count} production orders, {_inventory.Count} inventory entries");
            }
        }
        #endregion
    }

    #region Save Data
    [Serializable]
    public class DroneManagerSaveData
    {
        public List<string> unlockedDroneIds;
        public List<ProductionOrder> productionQueue;
        public List<DroneInventoryEntry> inventory;
    }
    #endregion
}
