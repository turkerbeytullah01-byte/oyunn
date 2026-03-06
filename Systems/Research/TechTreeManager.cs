using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Research
{
    /// <summary>
    /// Data structure for visual tree node representation
    /// </summary>
    public class TechTreeNode
    {
        public ResearchData Research { get; set; }
        public Vector2 Position { get; set; }
        public TechNodeState State { get; set; }
        public List<TechTreeNode> Prerequisites { get; set; } = new List<TechTreeNode>();
        public List<TechTreeNode> Dependents { get; set; } = new List<TechTreeNode>();
        public int Depth { get; set; }
    }

    /// <summary>
    /// States for tech tree nodes
    /// </summary>
    public enum TechNodeState
    {
        Locked,         // Prerequisites not met
        Available,      // Can be researched
        Queued,         // In research queue
        InProgress,     // Currently being researched
        Completed       // Research completed
    }

    /// <summary>
    /// Manager for technology tree navigation, validation, and visualization data.
    /// Works alongside ResearchManager to provide tree-specific functionality.
    /// </summary>
    public class TechTreeManager : MonoBehaviour
    {
        public static TechTreeManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("Technology tree data")]
        public TechnologyTreeData technologyTree;

        [Header("Grid Settings")]
        [Tooltip("Horizontal spacing between nodes")]
        public float nodeSpacingX = 150f;
        
        [Tooltip("Vertical spacing between nodes")]
        public float nodeSpacingY = 100f;
        
        [Tooltip("Starting position for tree layout")]
        public Vector2 startPosition = new Vector2(0, 0);

        // Runtime data
        private Dictionary<string, TechTreeNode> _nodeCache;
        private List<List<TechTreeNode>> _layers;
        private ResearchManager _researchManager;

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _researchManager = ResearchManager.Instance;
            InitializeTree();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the tech tree structure
        /// </summary>
        public void InitializeTree()
        {
            if (technologyTree == null)
            {
                Debug.LogError("[TechTreeManager] No technology tree assigned");
                return;
            }

            technologyTree.Initialize();
            BuildNodeCache();
            CalculateNodePositions();
        }

        /// <summary>
        /// Builds the node cache for fast lookup
        /// </summary>
        private void BuildNodeCache()
        {
            _nodeCache = new Dictionary<string, TechTreeNode>();

            foreach (var research in technologyTree.allResearches)
            {
                if (research == null)
                    continue;

                var node = new TechTreeNode
                {
                    Research = research,
                    State = TechNodeState.Locked
                };

                _nodeCache[research.researchId] = node;
            }

            // Build prerequisite and dependent connections
            foreach (var node in _nodeCache.Values)
            {
                foreach (var prereqId in node.Research.prerequisiteIds)
                {
                    if (_nodeCache.TryGetValue(prereqId, out var prereqNode))
                    {
                        node.Prerequisites.Add(prereqNode);
                        prereqNode.Dependents.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates node positions for visual layout
        /// </summary>
        private void CalculateNodePositions()
        {
            if (_nodeCache == null || _nodeCache.Count == 0)
                return;

            // Find root nodes (no prerequisites)
            var rootNodes = _nodeCache.Values
                .Where(n => n.Prerequisites.Count == 0)
                .ToList();

            // Calculate depth for each node using BFS
            var visited = new HashSet<string>();
            var queue = new Queue<TechTreeNode>();

            foreach (var root in rootNodes)
            {
                root.Depth = 0;
                queue.Enqueue(root);
            }

            int maxDepth = 0;
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Contains(node.Research.researchId))
                    continue;

                visited.Add(node.Research.researchId);
                maxDepth = Mathf.Max(maxDepth, node.Depth);

                foreach (var dependent in node.Dependents)
                {
                    if (!visited.Contains(dependent.Research.researchId))
                    {
                        dependent.Depth = Mathf.Max(dependent.Depth, node.Depth + 1);
                        queue.Enqueue(dependent);
                    }
                }
            }

            // Group nodes by depth (layer)
            _layers = new List<List<TechTreeNode>>();
            for (int i = 0; i <= maxDepth; i++)
            {
                _layers.Add(new List<TechTreeNode>());
            }

            foreach (var node in _nodeCache.Values)
            {
                if (node.Depth < _layers.Count)
                {
                    _layers[node.Depth].Add(node);
                }
            }

            // Sort each layer by prerequisites for consistent ordering
            foreach (var layer in _layers)
            {
                layer.Sort((a, b) =>
                {
                    // Sort by first prerequisite ID for consistency
                    string aPrereq = a.Prerequisites.FirstOrDefault()?.Research.researchId ?? "";
                    string bPrereq = b.Prerequisites.FirstOrDefault()?.Research.researchId ?? "";
                    return aPrereq.CompareTo(bPrereq);
                });
            }

            // Calculate positions
            for (int depth = 0; depth < _layers.Count; depth++)
            {
                var layer = _layers[depth];
                float layerHeight = (layer.Count - 1) * nodeSpacingY;
                float startY = startPosition.y - layerHeight / 2f;

                for (int i = 0; i < layer.Count; i++)
                {
                    layer[i].Position = new Vector2(
                        startPosition.x + depth * nodeSpacingX,
                        startY + i * nodeSpacingY
                    );
                }
            }
        }
        #endregion

        #region Node Queries
        /// <summary>
        /// Gets a tree node by research ID
        /// </summary>
        public TechTreeNode GetNode(string researchId)
        {
            return _nodeCache != null && _nodeCache.TryGetValue(researchId, out var node) 
                ? node 
                : null;
        }

        /// <summary>
        /// Gets all nodes in the tree
        /// </summary>
        public List<TechTreeNode> GetAllNodes()
        {
            return _nodeCache?.Values.ToList() ?? new List<TechTreeNode>();
        }

        /// <summary>
        /// Gets nodes filtered by category
        /// </summary>
        public List<TechTreeNode> GetNodesByCategory(TechCategory category)
        {
            return _nodeCache?.Values
                .Where(n => n.Research.techCategory == category)
                .ToList() ?? new List<TechTreeNode>();
        }

        /// <summary>
        /// Gets nodes by their current state
        /// </summary>
        public List<TechTreeNode> GetNodesByState(TechNodeState state)
        {
            return _nodeCache?.Values
                .Where(n => n.State == state)
                .ToList() ?? new List<TechTreeNode>();
        }
        #endregion

        #region Validation
        /// <summary>
        /// Checks if all prerequisites are met for a research
        /// </summary>
        public bool ArePrerequisitesMet(string researchId)
        {
            if (_researchManager == null)
                return false;

            var node = GetNode(researchId);
            if (node == null)
                return false;

            foreach (var prereq in node.Prerequisites)
            {
                if (!_researchManager.IsResearchCompleted(prereq.Research.researchId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the list of unmet prerequisites for a research
        /// </summary>
        public List<ResearchData> GetUnmetPrerequisites(string researchId)
        {
            var unmet = new List<ResearchData>();
            
            if (_researchManager == null)
                return unmet;

            var node = GetNode(researchId);
            if (node == null)
                return unmet;

            foreach (var prereq in node.Prerequisites)
            {
                if (!_researchManager.IsResearchCompleted(prereq.Research.researchId))
                {
                    unmet.Add(prereq.Research);
                }
            }

            return unmet;
        }

        /// <summary>
        /// Checks if a research is unlocked (category unlocked)
        /// </summary>
        public bool IsCategoryUnlocked(TechCategory category)
        {
            if (_researchManager == null)
                return false;

            var completedIds = _researchManager.GetCompletedResearch();
            return technologyTree?.IsCategoryUnlocked(category, completedIds) ?? true;
        }
        #endregion

        #region State Management
        /// <summary>
        /// Updates all node states based on current research progress
        /// Call this when research state changes
        /// </summary>
        public void UpdateNodeStates()
        {
            if (_nodeCache == null || _researchManager == null)
                return;

            string activeId = _researchManager.GetActiveResearchId();
            var queueIds = _researchManager.GetResearchQueue().Select(r => r.researchId).ToList();
            var completedIds = _researchManager.GetCompletedResearch();

            foreach (var node in _nodeCache.Values)
            {
                string id = node.Research.researchId;

                if (completedIds.Contains(id))
                {
                    node.State = TechNodeState.Completed;
                }
                else if (activeId == id)
                {
                    node.State = TechNodeState.InProgress;
                }
                else if (queueIds.Contains(id))
                {
                    node.State = TechNodeState.Queued;
                }
                else if (ArePrerequisitesMet(id))
                {
                    node.State = TechNodeState.Available;
                }
                else
                {
                    node.State = TechNodeState.Locked;
                }
            }
        }

        /// <summary>
        /// Gets the current state of a research node
        /// </summary>
        public TechNodeState GetNodeState(string researchId)
        {
            var node = GetNode(researchId);
            return node?.State ?? TechNodeState.Locked;
        }
        #endregion

        #region Visual Data Provider
        /// <summary>
        /// Gets the tree layers for visual rendering
        /// </summary>
        public List<List<TechTreeNode>> GetTreeLayers()
        {
            return _layers ?? new List<List<TechTreeNode>>();
        }

        /// <summary>
        /// Gets connections between nodes for line drawing
        /// </summary>
        public List<(Vector2 from, Vector2 to, TechNodeConnection.ConnectionType type)> GetConnections()
        {
            var connections = new List<(Vector2, Vector2, TechNodeConnection.ConnectionType)>();

            if (_nodeCache == null || technologyTree?.connections == null)
                return connections;

            foreach (var conn in technologyTree.connections)
            {
                var fromNode = GetNode(conn.fromResearchId);
                var toNode = GetNode(conn.toResearchId);

                if (fromNode != null && toNode != null)
                {
                    connections.Add((fromNode.Position, toNode.Position, conn.connectionType));
                }
            }

            return connections;
        }

        /// <summary>
        /// Gets all connections from prerequisites for a node
        /// </summary>
        public List<(Vector2 from, Vector2 to)> GetPrerequisiteConnections(string researchId)
        {
            var connections = new List<(Vector2, Vector2)>();
            var node = GetNode(researchId);

            if (node == null)
                return connections;

            foreach (var prereq in node.Prerequisites)
            {
                connections.Add((prereq.Position, node.Position));
            }

            return connections;
        }
        #endregion

        #region Path Finding
        /// <summary>
        /// Gets the research path from a starting node to a target node
        /// </summary>
        public List<string> GetResearchPath(string fromId, string toId)
        {
            var path = new List<string>();
            
            if (_nodeCache == null)
                return path;

            // BFS to find path
            var visited = new HashSet<string>();
            var queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { fromId });

            while (queue.Count > 0)
            {
                var currentPath = queue.Dequeue();
                var currentId = currentPath[currentPath.Count - 1];

                if (currentId == toId)
                    return currentPath;

                if (visited.Contains(currentId))
                    continue;

                visited.Add(currentId);

                var node = GetNode(currentId);
                if (node != null)
                {
                    foreach (var dependent in node.Dependents)
                    {
                        if (!visited.Contains(dependent.Research.researchId))
                        {
                            var newPath = new List<string>(currentPath);
                            newPath.Add(dependent.Research.researchId);
                            queue.Enqueue(newPath);
                        }
                    }
                }
            }

            return path; // Empty if no path found
        }

        /// <summary>
        /// Gets all researches that need to be completed to unlock a target
        /// </summary>
        public List<string> GetRequiredResearchesFor(string targetId)
        {
            var required = new List<string>();
            var node = GetNode(targetId);

            if (node == null)
                return required;

            // DFS to collect all prerequisites
            CollectPrerequisites(node, required, new HashSet<string>());

            return required;
        }

        private void CollectPrerequisites(TechTreeNode node, List<string> required, HashSet<string> visited)
        {
            if (visited.Contains(node.Research.researchId))
                return;

            visited.Add(node.Research.researchId);

            foreach (var prereq in node.Prerequisites)
            {
                if (!required.Contains(prereq.Research.researchId))
                {
                    required.Add(prereq.Research.researchId);
                }
                CollectPrerequisites(prereq, required, visited);
            }
        }
        #endregion

        #region Debug
        /// <summary>
        /// Validates the tech tree for issues
        /// </summary>
        [ContextMenu("Validate Tech Tree")]
        private void DebugValidateTree()
        {
            var errors = technologyTree?.ValidateTree();
            if (errors != null && errors.Count > 0)
            {
                Debug.LogWarning($"[TechTreeManager] Found {errors.Count} issues:");
                foreach (var error in errors)
                {
                    Debug.LogWarning($"  - {error}");
                }
            }
            else
            {
                Debug.Log("[TechTreeManager] Tech tree is valid");
            }
        }
        #endregion
    }
}
