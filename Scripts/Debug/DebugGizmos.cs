using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Debug gizmos and visualizations for Project Aegis: Drone Dominion
    /// Provides visual debugging aids in Scene and Game views
    /// </summary>
    public class DebugGizmos : MonoBehaviour
    {
        #region Singleton
        private static DebugGizmos _instance;
        public static DebugGizmos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DebugGizmos>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DebugGizmos");
                        _instance = go.AddComponent<DebugGizmos>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Research Visualization
        [Header("Research")]
        [Tooltip("Show research progress visualization")]
        public bool showResearchProgress = true;
        
        [Tooltip("Color for research in progress")]
        public Color researchInProgressColor = Color.yellow;
        
        [Tooltip("Color for completed research")]
        public Color researchCompleteColor = Color.green;
        
        [Tooltip("Color for locked research")]
        public Color researchLockedColor = Color.gray;
        
        [Tooltip("Show research tree connections")]
        public bool showResearchConnections = true;
        
        [Tooltip("Connection line color")]
        public Color researchConnectionColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        #endregion

        #region Contract Visualization
        [Header("Contracts")]
        [Tooltip("Show contract deadline indicators")]
        public bool showContractDeadlines = true;
        
        [Tooltip("Color for urgent deadlines (< 1 hour)")]
        public Color deadlineUrgentColor = Color.red;
        
        [Tooltip("Color for warning deadlines (< 6 hours)")]
        public Color deadlineWarningColor = new Color(1f, 0.5f, 0f);
        
        [Tooltip("Color for normal deadlines")]
        public Color deadlineNormalColor = Color.white;
        
        [Tooltip("Show contract difficulty indicators")]
        public bool showContractDifficulty = true;
        
        [Tooltip("Show contract reward visualization")]
        public bool showContractRewards = true;
        #endregion

        #region Drone Visualization
        [Header("Drones")]
        [Tooltip("Show drone patrol paths")]
        public bool showDronePaths = true;
        
        [Tooltip("Color for active drone paths")]
        public Color dronePathColor = new Color(0f, 0.5f, 1f, 0.5f);
        
        [Tooltip("Show drone sensor ranges")]
        public bool showDroneSensors = true;
        
        [Tooltip("Color for sensor range")]
        public Color droneSensorColor = new Color(0f, 1f, 0f, 0.2f);
        
        [Tooltip("Show drone status indicators")]
        public bool showDroneStatus = true;
        
        [Tooltip("Show drone communication links")]
        public bool showDroneLinks = true;
        #endregion

        #region Facility Visualization
        [Header("Facilities")]
        [Tooltip("Show facility influence zones")]
        public bool showFacilityZones = true;
        
        [Tooltip("Color for facility zones")]
        public Color facilityZoneColor = new Color(1f, 1f, 0f, 0.2f);
        
        [Tooltip("Show facility production rates")]
        public bool showProductionRates = true;
        
        [Tooltip("Show facility connections")]
        public bool showFacilityConnections = true;
        #endregion

        #region Risk Visualization
        [Header("Risk")]
        [Tooltip("Show risk heat map")]
        public bool showRiskHeatMap = false;
        
        [Tooltip("Color for low risk areas")]
        public Color riskLowColor = Color.green;
        
        [Tooltip("Color for medium risk areas")]
        public Color riskMediumColor = Color.yellow;
        
        [Tooltip("Color for high risk areas")]
        public Color riskHighColor = Color.red;
        
        [Tooltip("Show risk probability indicators")]
        public bool showRiskProbabilities = true;
        #endregion

        #region Event Visualization
        [Header("Events")]
        [Tooltip("Show active event areas")]
        public bool showEventAreas = true;
        
        [Tooltip("Color for positive events")]
        public Color eventPositiveColor = new Color(0f, 1f, 0f, 0.3f);
        
        [Tooltip("Color for negative events")]
        public Color eventNegativeColor = new Color(1f, 0f, 0f, 0.3f);
        
        [Tooltip("Show event effect radius")]
        public bool showEventRadius = true;
        #endregion

        #region Grid Visualization
        [Header("Grid")]
        [Tooltip("Show world grid")]
        public bool showWorldGrid = false;
        
        [Tooltip("Grid size")]
        public float gridSize = 10f;
        
        [Tooltip("Grid color")]
        public Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        
        [Tooltip("Show coordinate labels")]
        public bool showCoordinates = false;
        #endregion

        #region Performance Visualization
        [Header("Performance")]
        [Tooltip("Show performance overlay")]
        public bool showPerformanceOverlay = false;
        
        [Tooltip("Show object count")]
        public bool showObjectCount = false;
        
        [Tooltip("Show draw call hotspots")]
        public bool showDrawCallHotspots = false;
        #endregion

        #region Debug Data
        private List<ResearchNodeData> _researchNodes = new List<ResearchNodeData>();
        private List<ContractData> _contractData = new List<ContractData>();
        private List<DroneData> _droneData = new List<DroneData>();
        private List<FacilityData> _facilityData = new List<FacilityData>();
        private List<EventData> _eventData = new List<EventData>();
        
        private class ResearchNodeData
        {
            public string id;
            public Vector3 position;
            public float progress;
            public bool isCompleted;
            public bool isLocked;
            public List<string> prerequisites;
        }
        
        private class ContractData
        {
            public string id;
            public Vector3 position;
            public float remainingTime;
            public float totalTime;
            public int difficulty;
            public float reward;
        }
        
        private class DroneData
        {
            public string id;
            public Vector3 position;
            public float sensorRange;
            public List<Vector3> patrolPath;
            public bool isActive;
        }
        
        private class FacilityData
        {
            public string id;
            public Vector3 position;
            public float influenceRadius;
            public float productionRate;
            public List<string> connectedFacilities;
        }
        
        private class EventData
        {
            public string id;
            public Vector3 position;
            public float radius;
            public bool isPositive;
            public float intensity;
        }
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
        #endregion

        #region Gizmo Drawing
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            DrawResearchGizmos();
            DrawContractGizmos();
            DrawDroneGizmos();
            DrawFacilityGizmos();
            DrawRiskGizmos();
            DrawEventGizmos();
            DrawGridGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw selected object specific gizmos
        }

        private void DrawResearchGizmos()
        {
            if (!showResearchProgress) return;
            
            foreach (var node in _researchNodes)
            {
                // Determine color based on state
                Color color;
                if (node.isCompleted)
                {
                    color = researchCompleteColor;
                }
                else if (node.isLocked)
                {
                    color = researchLockedColor;
                }
                else
                {
                    color = Color.Lerp(researchInProgressColor, researchCompleteColor, node.progress);
                }
                
                // Draw node
                Gizmos.color = color;
                Gizmos.DrawSphere(node.position, 0.5f);
                
                // Draw progress ring for in-progress research
                if (!node.isCompleted && !node.isLocked && node.progress > 0)
                {
                    DrawProgressRing(node.position, node.progress, 0.7f, color);
                }
            }
            
            // Draw connections
            if (showResearchConnections)
            {
                Gizmos.color = researchConnectionColor;
                foreach (var node in _researchNodes)
                {
                    foreach (var prereqId in node.prerequisites)
                    {
                        var prereq = _researchNodes.Find(n => n.id == prereqId);
                        if (prereq != null)
                        {
                            Gizmos.DrawLine(prereq.position, node.position);
                        }
                    }
                }
            }
        }

        private void DrawContractGizmos()
        {
            if (!showContractDeadlines) return;
            
            foreach (var contract in _contractData)
            {
                float timeRatio = contract.remainingTime / contract.totalTime;
                
                Color color;
                if (timeRatio < 0.1f)
                {
                    color = deadlineUrgentColor;
                }
                else if (timeRatio < 0.3f)
                {
                    color = deadlineWarningColor;
                }
                else
                {
                    color = deadlineNormalColor;
                }
                
                Gizmos.color = color;
                Gizmos.DrawWireCube(contract.position, Vector3.one * (1f + contract.difficulty * 0.2f));
                
                // Draw time indicator
                DrawProgressBar(contract.position + Vector3.up * 1.5f, timeRatio, 1f, 0.2f, color);
            }
        }

        private void DrawDroneGizmos()
        {
            if (showDronePaths)
            {
                Gizmos.color = dronePathColor;
                foreach (var drone in _droneData)
                {
                    if (drone.patrolPath != null && drone.patrolPath.Count > 1)
                    {
                        for (int i = 0; i < drone.patrolPath.Count - 1; i++)
                        {
                            Gizmos.DrawLine(drone.patrolPath[i], drone.patrolPath[i + 1]);
                        }
                        // Close the loop
                        Gizmos.DrawLine(drone.patrolPath[drone.patrolPath.Count - 1], drone.patrolPath[0]);
                    }
                }
            }
            
            if (showDroneSensors)
            {
                Gizmos.color = droneSensorColor;
                foreach (var drone in _droneData)
                {
                    Gizmos.DrawWireSphere(drone.position, drone.sensorRange);
                }
            }
            
            if (showDroneStatus)
            {
                foreach (var drone in _droneData)
                {
                    Gizmos.color = drone.isActive ? Color.green : Color.red;
                    Gizmos.DrawCube(drone.position + Vector3.up * 0.5f, new Vector3(0.2f, 0.2f, 0.2f));
                }
            }
        }

        private void DrawFacilityGizmos()
        {
            if (showFacilityZones)
            {
                Gizmos.color = facilityZoneColor;
                foreach (var facility in _facilityData)
                {
                    Gizmos.DrawWireSphere(facility.position, facility.influenceRadius);
                }
            }
            
            if (showFacilityConnections)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                foreach (var facility in _facilityData)
                {
                    foreach (var connectedId in facility.connectedFacilities)
                    {
                        var connected = _facilityData.Find(f => f.id == connectedId);
                        if (connected != null)
                        {
                            Gizmos.DrawLine(facility.position, connected.position);
                        }
                    }
                }
            }
        }

        private void DrawRiskGizmos()
        {
            if (!showRiskHeatMap) return;
            
            // Draw risk zones as colored areas
            // This would be populated from actual risk data
        }

        private void DrawEventGizmos()
        {
            if (!showEventAreas) return;
            
            foreach (var evt in _eventData)
            {
                Color color = evt.isPositive ? eventPositiveColor : eventNegativeColor;
                color.a *= evt.intensity;
                
                Gizmos.color = color;
                Gizmos.DrawSphere(evt.position, evt.radius);
                
                if (showEventRadius)
                {
                    Gizmos.color = new Color(color.r, color.g, color.b, 1f);
                    Gizmos.DrawWireSphere(evt.position, evt.radius);
                }
            }
        }

        private void DrawGridGizmos()
        {
            if (!showWorldGrid) return;
            
            Gizmos.color = gridColor;
            
            float gridExtent = 100f;
            int gridLines = Mathf.FloorToInt(gridExtent / gridSize);
            
            for (int i = -gridLines; i <= gridLines; i++)
            {
                float pos = i * gridSize;
                Gizmos.DrawLine(new Vector3(pos, 0, -gridExtent), new Vector3(pos, 0, gridExtent));
                Gizmos.DrawLine(new Vector3(-gridExtent, 0, pos), new Vector3(gridExtent, 0, pos));
            }
        }
        #endregion

        #region Helper Methods
        private void DrawProgressRing(Vector3 center, float progress, float radius, Color color)
        {
            int segments = 32;
            float angle = progress * 360f;
            
            Vector3 prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.up * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = (angle / segments) * i;
                Vector3 point = center + Quaternion.Euler(0, 0, -currentAngle) * Vector3.up * radius;
                
                if (currentAngle <= angle)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }
                
                prevPoint = point;
            }
        }

        private void DrawProgressBar(Vector3 position, float progress, float width, float height, Color color)
        {
            // Background
            Gizmos.color = Color.gray;
            Gizmos.DrawCube(position, new Vector3(width, height, 0.1f));
            
            // Progress
            Gizmos.color = color;
            float progressWidth = width * progress;
            Vector3 progressPos = position - new Vector3((width - progressWidth) / 2, 0, 0);
            Gizmos.DrawCube(progressPos, new Vector3(progressWidth, height, 0.15f));
        }
        #endregion

        #region Data Registration
        /// <summary>
        /// Register a research node for visualization
        /// </summary>
        public void RegisterResearchNode(string id, Vector3 position, List<string> prerequisites)
        {
            var existing = _researchNodes.Find(n => n.id == id);
            if (existing != null)
            {
                existing.position = position;
                existing.prerequisites = prerequisites;
            }
            else
            {
                _researchNodes.Add(new ResearchNodeData
                {
                    id = id,
                    position = position,
                    prerequisites = prerequisites,
                    progress = 0f,
                    isCompleted = false,
                    isLocked = true
                });
            }
        }

        /// <summary>
        /// Update research progress
        /// </summary>
        public void UpdateResearchProgress(string id, float progress)
        {
            var node = _researchNodes.Find(n => n.id == id);
            if (node != null)
            {
                node.progress = progress;
            }
        }

        /// <summary>
        /// Mark research as completed
        /// </summary>
        public void CompleteResearch(string id)
        {
            var node = _researchNodes.Find(n => n.id == id);
            if (node != null)
            {
                node.isCompleted = true;
                node.progress = 1f;
            }
        }

        /// <summary>
        /// Register a contract for visualization
        /// </summary>
        public void RegisterContract(string id, Vector3 position, float totalTime, int difficulty, float reward)
        {
            _contractData.Add(new ContractData
            {
                id = id,
                position = position,
                totalTime = totalTime,
                remainingTime = totalTime,
                difficulty = difficulty,
                reward = reward
            });
        }

        /// <summary>
        /// Register a drone for visualization
        /// </summary>
        public void RegisterDrone(string id, Vector3 position, float sensorRange)
        {
            var existing = _droneData.Find(d => d.id == id);
            if (existing != null)
            {
                existing.position = position;
                existing.sensorRange = sensorRange;
            }
            else
            {
                _droneData.Add(new DroneData
                {
                    id = id,
                    position = position,
                    sensorRange = sensorRange,
                    patrolPath = new List<Vector3>(),
                    isActive = true
                });
            }
        }

        /// <summary>
        /// Register a facility for visualization
        /// </summary>
        public void RegisterFacility(string id, Vector3 position, float influenceRadius)
        {
            var existing = _facilityData.Find(f => f.id == id);
            if (existing != null)
            {
                existing.position = position;
                existing.influenceRadius = influenceRadius;
            }
            else
            {
                _facilityData.Add(new FacilityData
                {
                    id = id,
                    position = position,
                    influenceRadius = influenceRadius,
                    connectedFacilities = new List<string>()
                });
            }
        }

        /// <summary>
        /// Register an event for visualization
        /// </summary>
        public void RegisterEvent(string id, Vector3 position, float radius, bool isPositive, float intensity)
        {
            _eventData.Add(new EventData
            {
                id = id,
                position = position,
                radius = radius,
                isPositive = isPositive,
                intensity = intensity
            });
        }

        /// <summary>
        /// Clear all event visualizations
        /// </summary>
        public void ClearEvents()
        {
            _eventData.Clear();
        }
        #endregion

        #region Toggle Methods
        /// <summary>
        /// Toggle all gizmos
        /// </summary>
        public void ToggleAllGizmos(bool enabled)
        {
            showResearchProgress = enabled;
            showContractDeadlines = enabled;
            showDronePaths = enabled;
            showDroneSensors = enabled;
            showFacilityZones = enabled;
            showEventAreas = enabled;
            showWorldGrid = enabled;
        }

        /// <summary>
        /// Clear all registered data
        /// </summary>
        public void ClearAllData()
        {
            _researchNodes.Clear();
            _contractData.Clear();
            _droneData.Clear();
            _facilityData.Clear();
            _eventData.Clear();
        }
        #endregion
    }
}
