using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Research tree categories
    /// </summary>
    public enum TechCategory
    {
        Propulsion,
        Sensors,
        AI,
        Stealth,
        Weapons,
        Defense,
        All
    }
    
    /// <summary>
    /// Main research tree screen - displays scrollable tech tree
    /// </summary>
    public class ResearchTreeUI : BaseScreen
    {
        #region Fields
        
        [Header("Scroll Components")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform treeContainer;
        [SerializeField] private RectTransform contentRect;
        
        [Header("Node Prefab")]
        [SerializeField] private ResearchNodeUI nodePrefab;
        
        [Header("Detail Panel")]
        [SerializeField] private ResearchDetailPanel detailPanel;
        
        [Header("Category Tabs")]
        [SerializeField] private ToggleGroup categoryToggleGroup;
        [SerializeField] private List<Toggle> categoryToggles;
        [SerializeField] private List<TextMeshProUGUI> categoryLabels;
        
        [Header("Connection Lines")]
        [SerializeField] private RectTransform lineContainer;
        [SerializeField] private Image linePrefab;
        
        [Header("Controls")]
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button resetViewButton;
        [SerializeField] private Button searchButton;
        [SerializeField] private TMP_InputField searchInput;
        
        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI availablePointsText;
        [SerializeField] private TextMeshProUGUI activeResearchText;
        [SerializeField] private Button researchQueueButton;
        
        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 1.5f;
        [SerializeField] private float zoomStep = 0.1f;
        
        // Runtime state
        private Dictionary<string, ResearchNodeUI> _nodeMap = new Dictionary<string, ResearchNodeUI>();
        private List<Image> _connectionLines = new List<Image>();
        private TechCategory _currentCategory = TechCategory.All;
        private float _currentZoom = 1f;
        private ResearchData _selectedResearch;
        
        #endregion
        
        #region Properties
        
        public ResearchData SelectedResearch => _selectedResearch;
        
        #endregion
        
        #region BaseScreen Implementation
        
        protected override void Initialize()
        {
            SetupButtons();
            SetupCategoryToggles();
            
            if (detailPanel != null)
            {
                detailPanel.OnStartResearch += OnStartResearch;
                detailPanel.OnCancelResearch += OnCancelResearch;
                detailPanel.OnQueueResearch += OnQueueResearch;
            }
        }
        
        public override void OnShow()
        {
            // Build the tree
            BuildTree(_currentCategory);
            
            // Update displays
            UpdateInfoPanel();
            
            // Show with animation
            AnimateIn();
            
            // Subscribe to research events
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted += OnResearchStarted;
                ResearchManager.Instance.OnResearchCompleted += OnResearchCompleted;
                ResearchManager.Instance.OnResearchProgress += OnResearchProgress;
            }
        }
        
        public override void OnHide()
        {
            // Unsubscribe from events
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted -= OnResearchStarted;
                ResearchManager.Instance.OnResearchCompleted -= OnResearchCompleted;
                ResearchManager.Instance.OnResearchProgress -= OnResearchProgress;
            }
            
            // Hide detail panel
            detailPanel?.Hide();
        }
        
        public override void OnRefresh()
        {
            UpdateNodeStates();
            UpdateInfoPanel();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (zoomInButton != null)
                zoomInButton.onClick.AddListener(() => SetZoom(_currentZoom + zoomStep));
            
            if (zoomOutButton != null)
                zoomOutButton.onClick.AddListener(() => SetZoom(_currentZoom - zoomStep));
            
            if (resetViewButton != null)
                resetViewButton.onClick.AddListener(ResetView);
            
            if (searchButton != null)
                searchButton.onClick.AddListener(OnSearch);
            
            if (researchQueueButton != null)
                researchQueueButton.onClick.AddListener(OnShowResearchQueue);
        }
        
        private void SetupCategoryToggles()
        {
            for (int i = 0; i < categoryToggles.Count && i < System.Enum.GetValues(typeof(TechCategory)).Length - 1; i++)
            {
                int index = i; // Capture for closure
                var toggle = categoryToggles[i];
                
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnCategoryChanged((TechCategory)index);
                    }
                });
            }
        }
        
        #endregion
        
        #region Tree Building
        
        /// <summary>
        /// Builds the research tree for the specified category
        /// </summary>
        public void BuildTree(TechCategory category)
        {
            _currentCategory = category;
            ClearTree();
            
            if (ResearchManager.Instance == null) return;
            
            // Get research data
            var researches = ResearchManager.Instance.GetResearchesByCategory(category);
            
            // Create nodes
            foreach (var research in researches)
            {
                CreateNode(research);
            }
            
            // Create connections
            CreateConnections();
            
            // Update node states
            UpdateNodeStates();
            
            // Adjust content size
            AdjustContentSize();
        }
        
        private void CreateNode(ResearchData data)
        {
            if (_nodeMap.ContainsKey(data.id)) return;
            
            var node = Instantiate(nodePrefab, treeContainer);
            node.name = $"Node_{data.id}";
            node.SetData(data);
            
            // Position the node
            RectTransform rect = node.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(data.treePosition.x * 200, -data.treePosition.y * 150);
            
            // Subscribe to click event
            node.OnClicked += OnNodeClicked;
            
            _nodeMap[data.id] = node;
        }
        
        private void CreateConnections()
        {
            foreach (var kvp in _nodeMap)
            {
                var node = kvp.Value;
                var data = node.Data;
                
                if (data.prerequisites != null)
                {
                    foreach (var prereqId in data.prerequisites)
                    {
                        if (_nodeMap.TryGetValue(prereqId, out ResearchNodeUI prereqNode))
                        {
                            CreateConnectionLine(prereqNode, node);
                        }
                    }
                }
            }
        }
        
        private void CreateConnectionLine(ResearchNodeUI from, ResearchNodeUI to)
        {
            if (linePrefab == null || lineContainer == null) return;
            
            var line = Instantiate(linePrefab, lineContainer);
            
            Vector2 fromPos = from.GetComponent<RectTransform>().anchoredPosition;
            Vector2 toPos = to.GetComponent<RectTransform>().anchoredPosition;
            
            // Calculate line position and size
            Vector2 center = (fromPos + toPos) / 2f;
            float distance = Vector2.Distance(fromPos, toPos);
            float angle = Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg;
            
            RectTransform lineRect = line.GetComponent<RectTransform>();
            lineRect.anchoredPosition = center;
            lineRect.sizeDelta = new Vector2(distance, 4f);
            lineRect.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set color based on connection state
            bool isActive = from.CurrentState == NodeState.Completed;
            line.color = isActive ? UIManager.Instance.Theme.primaryColor : 
                new Color(0.3f, 0.3f, 0.3f, 0.5f);
            
            _connectionLines.Add(line);
        }
        
        private void ClearTree()
        {
            // Clear nodes
            foreach (var kvp in _nodeMap)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.OnClicked -= OnNodeClicked;
                    Destroy(kvp.Value.gameObject);
                }
            }
            _nodeMap.Clear();
            
            // Clear lines
            foreach (var line in _connectionLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            _connectionLines.Clear();
        }
        
        private void AdjustContentSize()
        {
            if (contentRect == null) return;
            
            Vector2 minPos = Vector2.zero;
            Vector2 maxPos = Vector2.zero;
            
            foreach (var kvp in _nodeMap)
            {
                Vector2 pos = kvp.Value.GetComponent<RectTransform>().anchoredPosition;
                minPos = Vector2.Min(minPos, pos);
                maxPos = Vector2.Max(maxPos, pos);
            }
            
            // Add padding
            float padding = 100f;
            contentRect.sizeDelta = new Vector2(
                maxPos.x - minPos.x + padding * 2,
                Mathf.Abs(minPos.y) + padding * 2
            );
        }
        
        #endregion
        
        #region Node Management
        
        /// <summary>
        /// Updates all node states based on research progress
        /// </summary>
        public void UpdateNodeStates()
        {
            foreach (var kvp in _nodeMap)
            {
                var node = kvp.Value;
                var state = GetNodeState(node.Data);
                node.SetState(state);
            }
        }
        
        private NodeState GetNodeState(ResearchData data)
        {
            if (ResearchManager.Instance == null) return NodeState.Locked;
            
            if (ResearchManager.Instance.IsResearchCompleted(data.id))
                return NodeState.Completed;
            
            if (ResearchManager.Instance.IsResearchInProgress(data.id))
                return NodeState.InProgress;
            
            if (ResearchManager.Instance.CanResearch(data.id))
                return NodeState.Available;
            
            return NodeState.Locked;
        }
        
        /// <summary>
        /// Called when a node is clicked
        /// </summary>
        public void OnNodeClicked(ResearchNodeUI node)
        {
            _selectedResearch = node.Data;
            
            // Show detail panel
            if (detailPanel != null)
            {
                detailPanel.Show(node.Data, node.CurrentState);
            }
            
            // Highlight the node
            foreach (var kvp in _nodeMap)
            {
                kvp.Value.SetHighlighted(kvp.Value == node);
            }
            
            Debug.Log($"[ResearchTreeUI] Node clicked: {node.Data.name}");
        }
        
        /// <summary>
        /// Centers the view on a specific research node
        /// </summary>
        public void CenterOnNode(string researchId)
        {
            if (!_nodeMap.TryGetValue(researchId, out ResearchNodeUI node))
                return;
            
            Vector2 nodePos = node.GetComponent<RectTransform>().anchoredPosition;
            Vector2 contentSize = contentRect.sizeDelta;
            Vector2 viewportSize = ((RectTransform)scrollRect.viewport).sizeDelta;
            
            // Calculate normalized position
            float normalizedX = (nodePos.x + contentSize.x / 2) / contentSize.x;
            float normalizedY = 1f - (Mathf.Abs(nodePos.y) + contentSize.y / 2) / contentSize.y;
            
            scrollRect.normalizedPosition = new Vector2(normalizedX, normalizedY);
            
            // Highlight the node
            OnNodeClicked(node);
        }
        
        #endregion
        
        #region Research Actions
        
        private void OnStartResearch(ResearchData data)
        {
            if (ResearchManager.Instance == null) return;
            
            if (ResearchManager.Instance.StartResearch(data.id))
            {
                UpdateNodeStates();
                UpdateInfoPanel();
                
                UIManager.Instance?.ShowNotification(
                    $"Started research: {data.name}", 
                    NotificationType.Info
                );
            }
            else
            {
                UIManager.Instance?.ShowNotification(
                    "Cannot start research - check prerequisites and resources",
                    NotificationType.Warning
                );
            }
        }
        
        private void OnCancelResearch(ResearchData data)
        {
            if (ResearchManager.Instance == null) return;
            
            ResearchManager.Instance.CancelResearch(data.id);
            UpdateNodeStates();
            UpdateInfoPanel();
        }
        
        private void OnQueueResearch(ResearchData data)
        {
            if (ResearchManager.Instance == null) return;
            
            ResearchManager.Instance.QueueResearch(data.id);
            UIManager.Instance?.ShowNotification(
                $"Added to queue: {data.name}",
                NotificationType.Info
            );
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnResearchStarted(ResearchData data)
        {
            UpdateNodeStates();
            UpdateInfoPanel();
        }
        
        private void OnResearchCompleted(ResearchData data)
        {
            UpdateNodeStates();
            UpdateInfoPanel();
            
            // Show completion effect on the node
            if (_nodeMap.TryGetValue(data.id, out ResearchNodeUI node))
            {
                node.PlayCompletionEffect();
            }
            
            UIManager.Instance?.ShowResearchComplete(data.name);
        }
        
        private void OnResearchProgress(ResearchData data, float progress)
        {
            if (_nodeMap.TryGetValue(data.id, out ResearchNodeUI node))
            {
                node.UpdateProgress(progress);
            }
        }
        
        private void OnCategoryChanged(TechCategory category)
        {
            BuildTree(category);
        }
        
        #endregion
        
        #region UI Controls
        
        private void SetZoom(float zoom)
        {
            _currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            
            if (treeContainer != null)
            {
                treeContainer.localScale = Vector3.one * _currentZoom;
            }
        }
        
        private void ResetView()
        {
            _currentZoom = 1f;
            SetZoom(1f);
            
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            }
        }
        
        private void OnSearch()
        {
            if (searchInput == null) return;
            
            string query = searchInput.text.ToLower();
            if (string.IsNullOrEmpty(query)) return;
            
            foreach (var kvp in _nodeMap)
            {
                if (kvp.Value.Data.name.ToLower().Contains(query))
                {
                    CenterOnNode(kvp.Key);
                    return;
                }
            }
            
            UIManager.Instance?.ShowNotification("Research not found", NotificationType.Warning);
        }
        
        private void OnShowResearchQueue()
        {
            // TODO: Show research queue popup
            UIManager.Instance?.ShowScreen(ScreenIds.RND_DASHBOARD);
        }
        
        private void UpdateInfoPanel()
        {
            if (ResearchManager.Instance == null) return;
            
            if (availablePointsText != null)
            {
                availablePointsText.text = $"RP: {ResearchManager.Instance.AvailablePoints}";
            }
            
            if (activeResearchText != null)
            {
                int activeCount = ResearchManager.Instance.ActiveResearchCount;
                activeResearchText.text = $"Active: {activeCount}";
            }
        }
        
        #endregion
        
        #region Utility
        
        public override bool OnBackPressed()
        {
            if (detailPanel != null && detailPanel.IsVisible)
            {
                detailPanel.Hide();
                return true;
            }
            
            return false;
        }
        
        #endregion
    }
}
