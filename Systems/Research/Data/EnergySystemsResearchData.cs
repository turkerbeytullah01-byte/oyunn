using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Research.Data
{
    /// <summary>
    /// Factory class for creating Energy Systems research ScriptableObjects.
    /// These are the MVP research nodes for the Energy Systems category.
    /// 
    /// To create these assets in Unity:
    /// 1. Right-click in Project window
    /// 2. Select "Project Aegis/Research/Research Data"
    /// 3. Configure using the values below
    /// </summary>
    public static class EnergySystemsResearchFactory
    {
        // ============================================================
        // RESEARCH 1: Basic Power Cell
        // ============================================================
        public static ResearchData CreateBasicPowerCell()
        {
            var research = ScriptableObject.CreateInstance<ResearchData>();
            research.researchId = "energy_basic_power_cell";
            research.displayName = "Basic Power Cell";
            research.description = "Fundamental energy storage technology for drone power systems. " +
                                  "Unlocks basic power modules and enables further energy research.";
            research.cost = 100f;
            research.durationMinutes = 5f;
            research.riskLevel = RiskLevel.Low;
            research.prerequisiteIds = new List<string>(); // No prerequisites - starter research
            research.techCategory = TechCategory.EnergySystems;
            research.unlocksModules = new List<string>
            {
                "module_basic_battery",
                "module_standard_power_cell"
            };
            research.unlocksResearch = new List<string>
            {
                "energy_efficient_cooling"
            };
            // research.icon = [Assign in Unity Editor]
            
            return research;
        }

        // ============================================================
        // RESEARCH 2: Efficient Cooling
        // ============================================================
        public static ResearchData CreateEfficientCooling()
        {
            var research = ScriptableObject.CreateInstance<ResearchData>();
            research.researchId = "energy_efficient_cooling";
            research.displayName = "Efficient Cooling";
            research.description = "Advanced thermal management systems that reduce power cell heat generation " +
                                  "by 25%. Improves power efficiency and extends operational lifetime.";
            research.cost = 250f;
            research.durationMinutes = 10f;
            research.riskLevel = RiskLevel.Low;
            research.prerequisiteIds = new List<string>
            {
                "energy_basic_power_cell"
            };
            research.techCategory = TechCategory.EnergySystems;
            research.unlocksModules = new List<string>
            {
                "module_cooling_system_mk1",
                "module_heat_dissipator"
            };
            research.unlocksResearch = new List<string>
            {
                "energy_advanced_capacitors"
            };
            // research.icon = [Assign in Unity Editor]
            
            return research;
        }

        // ============================================================
        // RESEARCH 3: Advanced Capacitors
        // ============================================================
        public static ResearchData CreateAdvancedCapacitors()
        {
            var research = ScriptableObject.CreateInstance<ResearchData>();
            research.researchId = "energy_advanced_capacitors";
            research.displayName = "Advanced Capacitors";
            research.description = "High-density capacitor technology enabling rapid charge/discharge cycles. " +
                                  "Increases power output by 50% and reduces recharge time significantly.";
            research.cost = 600f;
            research.durationMinutes = 20f;
            research.riskLevel = RiskLevel.Medium;
            research.prerequisiteIds = new List<string>
            {
                "energy_efficient_cooling"
            };
            research.techCategory = TechCategory.EnergySystems;
            research.unlocksModules = new List<string>
            {
                "module_advanced_capacitor_bank",
                "module_rapid_charge_unit"
            };
            research.unlocksResearch = new List<string>
            {
                "energy_fusion_micro_cell"
            };
            // research.icon = [Assign in Unity Editor]
            
            return research;
        }

        // ============================================================
        // RESEARCH 4: Fusion Micro-Cell
        // ============================================================
        public static ResearchData CreateFusionMicroCell()
        {
            var research = ScriptableObject.CreateInstance<ResearchData>();
            research.researchId = "energy_fusion_micro_cell";
            research.displayName = "Fusion Micro-Cell";
            research.description = "Miniaturized fusion reactor technology for drones. Provides near-limitless " +
                                  "energy with a 200% power output increase. Requires precise calibration.";
            research.cost = 1500f;
            research.durationMinutes = 45f;
            research.riskLevel = RiskLevel.High;
            research.prerequisiteIds = new List<string>
            {
                "energy_advanced_capacitors"
            };
            research.techCategory = TechCategory.EnergySystems;
            research.unlocksModules = new List<string>
            {
                "module_fusion_reactor_mini",
                "module_plasma_containment"
            };
            research.unlocksResearch = new List<string>
            {
                "energy_quantum_energy_core"
            };
            // research.icon = [Assign in Unity Editor]
            
            return research;
        }

        // ============================================================
        // RESEARCH 5: Quantum Energy Core
        // ============================================================
        public static ResearchData CreateQuantumEnergyCore()
        {
            var research = ScriptableObject.CreateInstance<ResearchData>();
            research.researchId = "energy_quantum_energy_core";
            research.displayName = "Quantum Energy Core";
            research.description = "Breakthrough quantum energy extraction technology. Harnesses zero-point " +
                                  "energy for virtually unlimited power. The pinnacle of energy research.";
            research.cost = 5000f;
            research.durationMinutes = 90f;
            research.riskLevel = RiskLevel.VeryHigh;
            research.prerequisiteIds = new List<string>
            {
                "energy_fusion_micro_cell"
            };
            research.techCategory = TechCategory.EnergySystems;
            research.unlocksModules = new List<string>
            {
                "module_quantum_core",
                "module_zero_point_extractor",
                "module_infinite_power_cell"
            };
            research.unlocksResearch = new List<string>
            {
                // Unlocks cross-category researches
                "ai_quantum_processor",
                "materials_exotic_alloys"
            };
            // research.icon = [Assign in Unity Editor]
            
            return research;
        }

        // ============================================================
        // CREATE ALL ENERGY SYSTEMS RESEARCHES
        // ============================================================
        /// <summary>
        /// Creates all Energy Systems research data objects
        /// </summary>
        public static List<ResearchData> CreateAllEnergySystemsResearch()
        {
            return new List<ResearchData>
            {
                CreateBasicPowerCell(),
                CreateEfficientCooling(),
                CreateAdvancedCapacitors(),
                CreateFusionMicroCell(),
                CreateQuantumEnergyCore()
            };
        }
    }

    // ============================================================
    // EDITOR UTILITY FOR CREATING ASSETS
    // ============================================================
#if UNITY_EDITOR
    using UnityEditor;

    public static class EnergySystemsResearchAssetCreator
    {
        private const string ASSET_PATH = "Assets/ProjectAegis/Data/Research/EnergySystems/";

        [MenuItem("Project Aegis/Research/Create Energy Systems Research Data")]
        public static void CreateAllAssets()
        {
            // Ensure directory exists
            if (!System.IO.Directory.Exists(ASSET_PATH))
            {
                System.IO.Directory.CreateDirectory(ASSET_PATH);
            }

            var researches = EnergySystemsResearchFactory.CreateAllEnergySystemsResearch();

            foreach (var research in researches)
            {
                string fileName = $"{research.researchId}.asset";
                string fullPath = ASSET_PATH + fileName;

                // Check if asset already exists
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchData>(fullPath) != null)
                {
                    Debug.Log($"Asset already exists: {fileName}");
                    continue;
                }

                UnityEditor.AssetDatabase.CreateAsset(research, fullPath);
                Debug.Log($"Created research asset: {fileName}");
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log("[EnergySystemsResearchAssetCreator] All Energy Systems research assets created!");
        }

        [MenuItem("Project Aegis/Research/Create Individual/Basic Power Cell")]
        public static void CreateBasicPowerCellAsset()
        {
            CreateAsset(EnergySystemsResearchFactory.CreateBasicPowerCell(), "energy_basic_power_cell");
        }

        [MenuItem("Project Aegis/Research/Create Individual/Efficient Cooling")]
        public static void CreateEfficientCoolingAsset()
        {
            CreateAsset(EnergySystemsResearchFactory.CreateEfficientCooling(), "energy_efficient_cooling");
        }

        [MenuItem("Project Aegis/Research/Create Individual/Advanced Capacitors")]
        public static void CreateAdvancedCapacitorsAsset()
        {
            CreateAsset(EnergySystemsResearchFactory.CreateAdvancedCapacitors(), "energy_advanced_capacitors");
        }

        [MenuItem("Project Aegis/Research/Create Individual/Fusion Micro-Cell")]
        public static void CreateFusionMicroCellAsset()
        {
            CreateAsset(EnergySystemsResearchFactory.CreateFusionMicroCell(), "energy_fusion_micro_cell");
        }

        [MenuItem("Project Aegis/Research/Create Individual/Quantum Energy Core")]
        public static void CreateQuantumEnergyCoreAsset()
        {
            CreateAsset(EnergySystemsResearchFactory.CreateQuantumEnergyCore(), "energy_quantum_energy_core");
        }

        private static void CreateAsset(ResearchData research, string fileName)
        {
            if (!System.IO.Directory.Exists(ASSET_PATH))
            {
                System.IO.Directory.CreateDirectory(ASSET_PATH);
            }

            string fullPath = ASSET_PATH + $"{fileName}.asset";

            if (UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchData>(fullPath) != null)
            {
                Debug.LogWarning($"Asset already exists: {fileName}");
                return;
            }

            UnityEditor.AssetDatabase.CreateAsset(research, fullPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[EnergySystemsResearchAssetCreator] Created: {fileName}");
        }
    }
#endif
}
