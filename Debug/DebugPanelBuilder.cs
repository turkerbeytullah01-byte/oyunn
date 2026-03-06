using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Helper script to build the debug panel UI programmatically
    /// Attach this to a Canvas in your scene to create the debug panel
    /// </summary>
    public class DebugPanelBuilder : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Create debug panel on start")]
        public bool createOnStart = true;
        
        [Tooltip("Panel width")]
        public float panelWidth = 500f;
        
        [Tooltip("Panel height")]
        public float panelHeight = 700f;
        
        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        [Tooltip("Header color")]
        public Color headerColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        [Tooltip("Section header color")]
        public Color sectionHeaderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        [Tooltip("Text color")]
        public Color textColor = Color.white;
        
        [Tooltip("Button color")]
        public Color buttonColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        
        [Tooltip("Accent color")]
        public Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);

        private GameObject _debugPanel;
        private DebugConsoleUI _consoleUI;

        private void Start()
        {
            if (createOnStart)
            {
                BuildDebugPanel();
            }
        }

        /// <summary>
        /// Build the complete debug panel
        /// </summary>
        public void BuildDebugPanel()
        {
            // Create main panel
            _debugPanel = CreatePanel("DebugPanel", transform as RectTransform);
            _debugPanel.SetActive(false); // Hidden by default
            
            // Add DebugConsoleUI component
            _consoleUI = _debugPanel.AddComponent<DebugConsoleUI>();
            
            // Create layout
            CreateHeader();
            CreateScrollableContent();
            CreateLogSection();
            
            // Setup sections
            SetupTimeControlSection();
            SetupCheatsSection();
            SetupResearchSection();
            SetupEventsSection();
            SetupContractsSection();
            SetupRiskSection();
            SetupSaveSection();
            SetupPerformanceSection();
            
            UnityEngine.Debug.Log("[DebugPanelBuilder] Debug panel created successfully");
        }

        private GameObject CreatePanel(string name, RectTransform parent)
        {
            GameObject panel = new GameObject(name);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-10, 0);
            rect.sizeDelta = new Vector2(panelWidth, panelHeight);
            
            Image image = panel.AddComponent<Image>();
            image.color = backgroundColor;
            
            // Add shadow effect
            Shadow shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(5, -5);
            
            return panel;
        }

        private void CreateHeader()
        {
            GameObject header = new GameObject("Header");
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.SetParent(_debugPanel.transform);
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, 0);
            headerRect.sizeDelta = new Vector2(0, 40);
            
            Image headerImage = header.AddComponent<Image>();
            headerImage.color = headerColor;
            
            // Title
            GameObject title = new GameObject("Title");
            RectTransform titleRect = title.AddComponent<RectTransform>();
            titleRect.SetParent(header.transform);
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 5);
            titleRect.offsetMax = new Vector2(-10, -5);
            
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "DEBUG PANEL";
            titleText.color = accentColor;
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            
            // Close button
            GameObject closeBtn = CreateButton("X", header.transform, new Vector2(-30, -20), new Vector2(30, 30));
            Button btn = closeBtn.GetComponent<Button>();
            btn.onClick.AddListener(() => _debugPanel.SetActive(false));
        }

        private void CreateScrollableContent()
        {
            // Create scroll view
            GameObject scrollView = new GameObject("ScrollView");
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.SetParent(_debugPanel.transform);
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0, -90);
            scrollRect.sizeDelta = new Vector2(-20, -190);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.SetParent(scrollView.transform);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            
            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = new Color(1, 1, 1, 0.1f);
            
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // Content
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(viewport.transform);
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.vertical = true;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            
            // Store reference
            _consoleUI.GetType().GetField("contentContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_consoleUI, content.transform);
        }

        private void CreateLogSection()
        {
            // Log display at bottom
            GameObject logPanel = new GameObject("LogPanel");
            RectTransform logRect = logPanel.AddComponent<RectTransform>();
            logRect.SetParent(_debugPanel.transform);
            logRect.anchorMin = new Vector2(0, 0);
            logRect.anchorMax = new Vector2(1, 0);
            logRect.pivot = new Vector2(0.5f, 0);
            logRect.anchoredPosition = new Vector2(0, 10);
            logRect.sizeDelta = new Vector2(-20, 80);
            
            Image logBg = logPanel.AddComponent<Image>();
            logBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            
            // Log text
            GameObject logTextObj = new GameObject("LogText");
            RectTransform logTextRect = logTextObj.AddComponent<RectTransform>();
            logTextRect.SetParent(logPanel.transform);
            logTextRect.anchorMin = Vector2.zero;
            logTextRect.anchorMax = Vector2.one;
            logTextRect.offsetMin = new Vector2(5, 5);
            logTextRect.offsetMax = new Vector2(-5, -5);
            
            TextMeshProUGUI logText = logTextObj.AddComponent<TextMeshProUGUI>();
            logText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            logText.fontSize = 10;
            logText.alignment = TextAlignmentOptions.TopLeft;
            logText.text = "Debug log ready...";
            
            // Store reference
            _consoleUI.GetType().GetField("logText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_consoleUI, logText);
        }

        private void SetupTimeControlSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("TIME CONTROL", content);
            
            // Time scale slider
            GameObject sliderObj = CreateSlider("Time Scale", content, 0, 100, 1);
            
            // Preset buttons
            GameObject presetRow = CreateHorizontalLayout(content);
            CreateButton("1x", presetRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("5x", presetRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("10x", presetRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("50x", presetRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("100x", presetRow.transform, Vector2.zero, new Vector2(50, 30));
            
            // Skip buttons
            GameObject skipRow = CreateHorizontalLayout(content);
            CreateButton("+1m", skipRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("+1h", skipRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("+1d", skipRow.transform, Vector2.zero, new Vector2(50, 30));
            CreateButton("Pause", skipRow.transform, Vector2.zero, new Vector2(60, 30));
        }

        private void SetupCheatsSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("CHEATS", content);
            
            // Money
            GameObject moneyRow = CreateHorizontalLayout(content);
            CreateInputField("Amount", moneyRow.transform, new Vector2(100, 30));
            CreateButton("Add Money", moneyRow.transform, Vector2.zero, new Vector2(90, 30));
            CreateButton("Set Money", moneyRow.transform, Vector2.zero, new Vector2(90, 30));
            
            // Reputation
            GameObject repRow = CreateHorizontalLayout(content);
            CreateInputField("Amount", repRow.transform, new Vector2(100, 30));
            CreateButton("Add Rep", repRow.transform, Vector2.zero, new Vector2(90, 30));
            CreateButton("Set Rep", repRow.transform, Vector2.zero, new Vector2(90, 30));
            
            // Research Points
            GameObject rpRow = CreateHorizontalLayout(content);
            CreateInputField("Amount", rpRow.transform, new Vector2(100, 30));
            CreateButton("Add RP", rpRow.transform, Vector2.zero, new Vector2(90, 30));
        }

        private void SetupResearchSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("RESEARCH", content);
            
            // Dropdown
            GameObject dropdownObj = CreateDropdown(content, new Vector2(400, 30));
            
            // Buttons
            GameObject btnRow1 = CreateHorizontalLayout(content);
            CreateButton("Complete Current", btnRow1.transform, Vector2.zero, new Vector2(120, 30));
            CreateButton("Complete All", btnRow1.transform, Vector2.zero, new Vector2(100, 30));
            
            GameObject btnRow2 = CreateHorizontalLayout(content);
            CreateButton("Unlock All Tech", btnRow2.transform, Vector2.zero, new Vector2(120, 30));
            CreateButton("Reset", btnRow2.transform, Vector2.zero, new Vector2(80, 30));
        }

        private void SetupEventsSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("EVENTS", content);
            
            // Dropdown
            GameObject dropdownObj = CreateDropdown(content, new Vector2(400, 30));
            
            // Buttons
            GameObject btnRow = CreateHorizontalLayout(content);
            CreateButton("Trigger", btnRow.transform, Vector2.zero, new Vector2(80, 30));
            CreateButton("Random", btnRow.transform, Vector2.zero, new Vector2(80, 30));
            CreateButton("Clear Effects", btnRow.transform, Vector2.zero, new Vector2(100, 30));
        }

        private void SetupContractsSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("CONTRACTS", content);
            
            // Generate row
            GameObject genRow = CreateHorizontalLayout(content);
            CreateInputField("Count", genRow.transform, new Vector2(80, 30));
            CreateButton("Generate", genRow.transform, Vector2.zero, new Vector2(80, 30));
            
            // Action buttons
            GameObject btnRow1 = CreateHorizontalLayout(content);
            CreateButton("Complete All", btnRow1.transform, Vector2.zero, new Vector2(100, 30));
            CreateButton("Win All Bids", btnRow1.transform, Vector2.zero, new Vector2(100, 30));
            
            GameObject btnRow2 = CreateHorizontalLayout(content);
            CreateButton("Fail All", btnRow2.transform, Vector2.zero, new Vector2(80, 30));
            CreateButton("Reset", btnRow2.transform, Vector2.zero, new Vector2(80, 30));
        }

        private void SetupRiskSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("RISK SIMULATION", content);
            
            // Risk type dropdown
            GameObject dropdownObj = CreateDropdown(content, new Vector2(400, 30));
            
            // Simulation runs
            GameObject runsRow = CreateHorizontalLayout(content);
            CreateInputField("Runs", runsRow.transform, new Vector2(100, 30));
            CreateButton("Run Simulation", runsRow.transform, Vector2.zero, new Vector2(110, 30));
            CreateButton("Single Test", runsRow.transform, Vector2.zero, new Vector2(90, 30));
        }

        private void SetupSaveSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("SAVE/LOAD", content);
            
            // Buttons
            GameObject btnRow1 = CreateHorizontalLayout(content);
            CreateButton("Save", btnRow1.transform, Vector2.zero, new Vector2(70, 30));
            CreateButton("Load", btnRow1.transform, Vector2.zero, new Vector2(70, 30));
            CreateButton("Delete", btnRow1.transform, Vector2.zero, new Vector2(70, 30));
            
            GameObject btnRow2 = CreateHorizontalLayout(content);
            CreateButton("Test Save", btnRow2.transform, Vector2.zero, new Vector2(80, 30));
            CreateButton("Export", btnRow2.transform, Vector2.zero, new Vector2(70, 30));
            CreateButton("Show Data", btnRow2.transform, Vector2.zero, new Vector2(90, 30));
        }

        private void SetupPerformanceSection()
        {
            Transform content = _debugPanel.transform.Find("ScrollView/Viewport/Content");
            if (content == null) return;
            
            CreateSectionHeader("PERFORMANCE", content);
            
            // Stats display
            GameObject statsObj = new GameObject("Stats");
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.SetParent(content);
            statsRect.sizeDelta = new Vector2(400, 60);
            
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.color = textColor;
            statsText.fontSize = 12;
            statsText.text = "FPS: --\nMemory: -- MB";
            
            // Toggle buttons
            GameObject btnRow = CreateHorizontalLayout(content);
            CreateButton("Toggle Monitor", btnRow.transform, Vector2.zero, new Vector2(110, 30));
            CreateButton("Take Snapshot", btnRow.transform, Vector2.zero, new Vector2(110, 30));
        }

        #region Helper Methods
        private void CreateSectionHeader(string text, Transform parent)
        {
            GameObject header = new GameObject($"Header_{text}");
            RectTransform rect = header.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.sizeDelta = new Vector2(400, 25);
            
            Image image = header.AddComponent<Image>();
            image.color = sectionHeaderColor;
            
            GameObject textObj = new GameObject("Text");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(header.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = accentColor;
            tmp.fontSize = 12;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Left;
        }

        private GameObject CreateHorizontalLayout(Transform parent)
        {
            GameObject row = new GameObject("Row");
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.sizeDelta = new Vector2(400, 35);
            
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            
            return row;
        }

        private GameObject CreateButton(string text, Transform parent, Vector2 position, Vector2 size)
        {
            GameObject button = new GameObject($"Button_{text}");
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            
            Image image = button.AddComponent<Image>();
            image.color = buttonColor;
            
            Button btn = button.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = accentColor;
            colors.pressedColor = accentColor * 0.8f;
            btn.colors = colors;
            
            GameObject textObj = new GameObject("Text");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(button.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = textColor;
            tmp.fontSize = 11;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return button;
        }

        private GameObject CreateInputField(string placeholder, Transform parent, Vector2 size)
        {
            GameObject input = new GameObject($"Input_{placeholder}");
            RectTransform rect = input.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.sizeDelta = size;
            
            Image image = input.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            TMP_InputField inputField = input.AddComponent<TMP_InputField>();
            
            GameObject textObj = new GameObject("Text");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(input.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.color = textColor;
            tmp.fontSize = 11;
            tmp.alignment = TextAlignmentOptions.Left;
            
            inputField.textComponent = tmp;
            inputField.placeholder = tmp;
            
            return input;
        }

        private GameObject CreateSlider(string label, Transform parent, float min, float max, float value)
        {
            GameObject sliderObj = new GameObject($"Slider_{label}");
            RectTransform rect = sliderObj.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.sizeDelta = new Vector2(400, 30);
            
            // Label
            GameObject labelObj = new GameObject("Label");
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.SetParent(sliderObj.transform);
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0, 0.5f);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.anchoredPosition = new Vector2(0, 0);
            labelRect.sizeDelta = new Vector2(80, 20);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.color = textColor;
            labelText.fontSize = 11;
            labelText.alignment = TextAlignmentOptions.Left;
            
            // Slider
            GameObject slider = new GameObject("Slider");
            RectTransform sliderRect = slider.AddComponent<RectTransform>();
            sliderRect.SetParent(sliderObj.transform);
            sliderRect.anchorMin = new Vector2(0, 0.5f);
            sliderRect.anchorMax = new Vector2(1, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(40, 0);
            sliderRect.sizeDelta = new Vector2(-90, 20);
            
            Slider s = slider.AddComponent<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.value = value;
            
            // Background
            GameObject bg = new GameObject("Background");
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.SetParent(slider.transform);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Fill area
            GameObject fillArea = new GameObject("Fill Area");
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.SetParent(slider.transform);
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;
            
            // Fill
            GameObject fill = new GameObject("Fill");
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.SetParent(fillArea.transform);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = accentColor;
            
            s.fillRect = fillRect;
            
            return sliderObj;
        }

        private GameObject CreateDropdown(Transform parent, Vector2 size)
        {
            GameObject dropdown = new GameObject("Dropdown");
            RectTransform rect = dropdown.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.sizeDelta = size;
            
            Image image = dropdown.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            TMP_Dropdown dd = dropdown.AddComponent<TMP_Dropdown>();
            
            GameObject labelObj = new GameObject("Label");
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.SetParent(dropdown.transform);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-30, -2);
            
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.color = textColor;
            label.fontSize = 11;
            label.alignment = TextAlignmentOptions.Left;
            
            dd.captionText = label;
            
            return dropdown;
        }
        #endregion
    }
}
