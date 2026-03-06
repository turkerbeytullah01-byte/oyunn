using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Research.Data
{
    /// <summary>
    /// Factory for creating the Energy Systems Technology Tree.
    /// This tree contains the 5 MVP research nodes with their connections.
    /// </summary>
    public static class EnergySystemsTechTreeFactory
    {
        /// <summary>
        /// Creates the complete Energy Systems technology tree
        /// </summary>
        public static TechnologyTreeData CreateTechTree()
        {
            var techTree = ScriptableObject.CreateInstance<TechnologyTreeData>();

            // Create all research nodes
            techTree.allResearches = EnergySystemsResearchFactory.CreateAllEnergySystemsResearch();

            // Define connections (visual and logical)
            techTree.connections = new List<TechNodeConnection>
            {
                // Basic Power Cell -> Efficient Cooling
                new TechNodeConnection
                {
                    fromResearchId = "energy_basic_power_cell",
                    toResearchId = "energy_efficient_cooling",
                    connectionType = TechNodeConnection.ConnectionType.Required
                },

                // Efficient Cooling -> Advanced Capacitors
                new TechNodeConnection
                {
                    fromResearchId = "energy_efficient_cooling",
                    toResearchId = "energy_advanced_capacitors",
                    connectionType = TechNodeConnection.ConnectionType.Required
                },

                // Advanced Capacitors -> Fusion Micro-Cell
                new TechNodeConnection
                {
                    fromResearchId = "energy_advanced_capacitors",
                    toResearchId = "energy_fusion_micro_cell",
                    connectionType = TechNodeConnection.ConnectionType.Required
                },

                // Fusion Micro-Cell -> Quantum Energy Core
                new TechNodeConnection
                {
                    fromResearchId = "energy_fusion_micro_cell",
                    toResearchId = "energy_quantum_energy_core",
                    connectionType = TechNodeConnection.ConnectionType.Required
                }
            };

            // Define category unlock requirements
            // Energy Systems is available from start, but other categories may require Energy progress
            techTree.categoryUnlockRequirements = new List<CategoryUnlockRequirement>
            {
                // Example: Unlocking AI Systems requires 2 Energy Systems researches
                new CategoryUnlockRequirement
                {
                    category = TechCategory.AISystems,
                    requiredCount = 2,
                    prerequisiteCategory = TechCategory.EnergySystems,
                    requiredResearchIds = new List<string>()
                },

                // Example: Unlocking Materials requires Basic Power Cell
                new CategoryUnlockRequirement
                {
                    category = TechCategory.Materials,
                    requiredCount = 0,
                    prerequisiteCategory = TechCategory.EnergySystems,
                    requiredResearchIds = new List<string> { "energy_basic_power_cell" }
                }
            };

            // Starting researches (available at game start)
            techTree.startingResearchIds = new List<string>
            {
                "energy_basic_power_cell"
            };

            return techTree;
        }

        /// <summary>
        /// Gets the research progression summary for Energy Systems
        /// </summary>
        public static string GetProgressionSummary()
        {
            return @"
ENERGY SYSTEMS RESEARCH PROGRESSION
====================================

Tier 1: Basic Power Cell (5 min, Low Risk)
  - Cost: 100 credits
  - Unlocks: Basic Battery, Standard Power Cell modules
  - Leads to: Efficient Cooling

Tier 2: Efficient Cooling (10 min, Low Risk)
  - Cost: 250 credits
  - Requires: Basic Power Cell
  - Unlocks: Cooling System MK1, Heat Dissipator modules
  - Leads to: Advanced Capacitors

Tier 3: Advanced Capacitors (20 min, Medium Risk)
  - Cost: 600 credits
  - Requires: Efficient Cooling
  - Unlocks: Advanced Capacitor Bank, Rapid Charge Unit modules
  - Leads to: Fusion Micro-Cell

Tier 4: Fusion Micro-Cell (45 min, High Risk)
  - Cost: 1,500 credits
  - Requires: Advanced Capacitors
  - Unlocks: Mini Fusion Reactor, Plasma Containment modules
  - Leads to: Quantum Energy Core

Tier 5: Quantum Energy Core (90 min, Very High Risk)
  - Cost: 5,000 credits
  - Requires: Fusion Micro-Cell
  - Unlocks: Quantum Core, Zero-Point Extractor, Infinite Power Cell modules
  - Unlocks cross-category: AI Quantum Processor, Exotic Alloys

Total Investment: 7,450 credits
Total Time: 170 minutes (2h 50m)
";
        }
    }

    // ============================================================
    // EDITOR UTILITY FOR CREATING TECH TREE ASSET
    // ============================================================
#if UNITY_EDITOR
    using UnityEditor;

    public static class EnergySystemsTechTreeAssetCreator
    {
        private const string ASSET_PATH = "Assets/ProjectAegis/Data/Research/";
        private const string FILE_NAME = "EnergySystemsTechTree.asset";

        [MenuItem("Project Aegis/Research/Create Energy Systems Tech Tree")]
        public static void CreateTechTreeAsset()
        {
            // Ensure directory exists
            if (!System.IO.Directory.Exists(ASSET_PATH))
            {
                System.IO.Directory.CreateDirectory(ASSET_PATH);
            }

            string fullPath = ASSET_PATH + FILE_NAME;

            // Check if asset already exists
            if (UnityEditor.AssetDatabase.LoadAssetAtPath<TechnologyTreeData>(fullPath) != null)
            {
                Debug.LogWarning($"Tech Tree asset already exists: {FILE_NAME}");
                return;
            }

            var techTree = EnergySystemsTechTreeFactory.CreateTechTree();
            UnityEditor.AssetDatabase.CreateAsset(techTree, fullPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[EnergySystemsTechTreeAssetCreator] Created: {FILE_NAME}");
            Debug.Log(EnergySystemsTechTreeFactory.GetProgressionSummary());
        }

        [MenuItem("Project Aegis/Research/Print Energy Systems Progression")]
        public static void PrintProgression()
        {
            Debug.Log(EnergySystemsTechTreeFactory.GetProgressionSummary());
        }
    }
#endif
}
