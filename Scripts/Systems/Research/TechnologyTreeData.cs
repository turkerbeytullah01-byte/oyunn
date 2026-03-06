using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Research
{
    /// <summary>
    /// Represents a connection between two research nodes in the tech tree
    /// </summary>
    [Serializable]
    public class TechNodeConnection
    {
        [Tooltip("Source research ID (prerequisite)")]
        public string fromResearchId;
        
        [Tooltip("Target research ID (dependent)")]
        public string toResearchId;
        
        [Tooltip("Connection type for visual styling")]
        public ConnectionType connectionType = ConnectionType.Required;

        public enum ConnectionType
        {
            Required,       // Must complete prerequisite
            Optional,       // Optional prerequisite (reduces cost/time)
            Alternative     // Alternative path
        }
    }

    /// <summary>
    /// Category unlock requirements for tech tree progression
    /// </summary>
    [Serializable]
    public class CategoryUnlockRequirement
    {
        [Tooltip("Technology category")]
        public TechCategory category;
        
        [Tooltip("Number of researches required in prerequisite category")]
        public int requiredCount;
        
        [Tooltip("Prerequisite category")]
        public TechCategory prerequisiteCategory;
        
        [Tooltip("Specific research IDs that must be completed")]
        public List<string> requiredResearchIds = new List<string>();
    }

    /// <summary>
    /// Container for the entire technology tree.
    /// Manages all research definitions, connections, and category requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "TechnologyTreeData", menuName = "Project Aegis/Research/Technology Tree Data")]
    public class TechnologyTreeData : ScriptableObject
    {
        [Header("Research Definitions")]
        [Tooltip("All research projects in this tech tree")]
        public List<ResearchData> allResearches = new List<ResearchData>();

        [Header("Node Connections")]
        [Tooltip("Visual and logical connections between research nodes")]
        public List<TechNodeConnection> connections = new List<TechNodeConnection>();

        [Header("Category Requirements")]
        [Tooltip("Requirements to unlock each category")]
        public List<CategoryUnlockRequirement> categoryUnlockRequirements = new List<CategoryUnlockRequirement>();

        [Header("Starting Research")]
        [Tooltip("Research IDs available at game start")]
        public List<string> startingResearchIds = new List<string>();

        // Runtime lookup cache
        private Dictionary<string, ResearchData> _researchLookup;
        private Dictionary<TechCategory, List<ResearchData>> _categoryLookup;
        private Dictionary<string, List<string>> _dependencyGraph;

        /// <summary>
        /// Initializes lookup dictionaries for fast access
        /// </summary>
        public void Initialize()
        {
            _researchLookup = new Dictionary<string, ResearchData>();
            _categoryLookup = new Dictionary<TechCategory, List<ResearchData>>();
            _dependencyGraph = new Dictionary<string, List<string>>();

            foreach (var research in allResearches)
            {
                if (research == null || string.IsNullOrEmpty(research.researchId))
                    continue;

                // Build research lookup
                _researchLookup[research.researchId] = research;

                // Build category lookup
                if (!_categoryLookup.ContainsKey(research.techCategory))
                    _categoryLookup[research.techCategory] = new List<ResearchData>();
                _categoryLookup[research.techCategory].Add(research);

                // Build dependency graph (reverse: who depends on this)
                foreach (var prereqId in research.prerequisiteIds)
                {
                    if (!_dependencyGraph.ContainsKey(prereqId))
                        _dependencyGraph[prereqId] = new List<string>();
                    if (!_dependencyGraph[prereqId].Contains(research.researchId))
                        _dependencyGraph[prereqId].Add(research.researchId);
                }
            }
        }

        /// <summary>
        /// Gets a research by its ID
        /// </summary>
        public ResearchData GetResearch(string researchId)
        {
            if (_researchLookup == null)
                Initialize();
            
            return _researchLookup != null && _researchLookup.TryGetValue(researchId, out var research) 
                ? research 
                : null;
        }

        /// <summary>
        /// Gets all researches in a category
        /// </summary>
        public List<ResearchData> GetResearchesByCategory(TechCategory category)
        {
            if (_categoryLookup == null)
                Initialize();
            
            return _categoryLookup != null && _categoryLookup.TryGetValue(category, out var researches) 
                ? researches 
                : new List<ResearchData>();
        }

        /// <summary>
        /// Gets all researches that depend on the given research
        /// </summary>
        public List<string> GetDependentResearches(string researchId)
        {
            if (_dependencyGraph == null)
                Initialize();
            
            return _dependencyGraph != null && _dependencyGraph.TryGetValue(researchId, out var dependents) 
                ? dependents 
                : new List<string>();
        }

        /// <summary>
        /// Gets all prerequisite researches for a given research
        /// </summary>
        public List<ResearchData> GetPrerequisites(string researchId)
        {
            var research = GetResearch(researchId);
            if (research == null)
                return new List<ResearchData>();

            var prerequisites = new List<ResearchData>();
            foreach (var prereqId in research.prerequisiteIds)
            {
                var prereq = GetResearch(prereqId);
                if (prereq != null)
                    prerequisites.Add(prereq);
            }
            return prerequisites;
        }

        /// <summary>
        /// Checks if a category is unlocked based on completed research
        /// </summary>
        public bool IsCategoryUnlocked(TechCategory category, List<string> completedResearchIds)
        {
            var requirement = categoryUnlockRequirements.FirstOrDefault(r => r.category == category);
            
            // No requirement means always unlocked
            if (requirement == null)
                return true;

            // Check specific required researches
            foreach (var requiredId in requirement.requiredResearchIds)
            {
                if (!completedResearchIds.Contains(requiredId))
                    return false;
            }

            // Check count requirement in prerequisite category
            if (requirement.requiredCount > 0)
            {
                var prereqCategoryResearches = GetResearchesByCategory(requirement.prerequisiteCategory);
                var completedInCategory = prereqCategoryResearches.Count(r => completedResearchIds.Contains(r.researchId));
                if (completedInCategory < requirement.requiredCount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all starting researches available at game begin
        /// </summary>
        public List<ResearchData> GetStartingResearches()
        {
            var starting = new List<ResearchData>();
            foreach (var id in startingResearchIds)
            {
                var research = GetResearch(id);
                if (research != null)
                    starting.Add(research);
            }
            return starting;
        }

        /// <summary>
        /// Validates the entire tech tree for consistency
        /// </summary>
        public List<string> ValidateTree()
        {
            var errors = new List<string>();

            // Check for duplicate IDs
            var idCounts = allResearches
                .Where(r => r != null && !string.IsNullOrEmpty(r.researchId))
                .GroupBy(r => r.researchId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            
            foreach (var duplicateId in idCounts)
            {
                errors.Add($"Duplicate research ID: {duplicateId}");
            }

            // Check for missing prerequisites
            foreach (var research in allResearches)
            {
                if (research == null) continue;

                foreach (var prereqId in research.prerequisiteIds)
                {
                    if (GetResearch(prereqId) == null)
                    {
                        errors.Add($"Research '{research.researchId}' has missing prerequisite: {prereqId}");
                    }
                }

                // Check for circular dependencies would require more complex analysis
            }

            return errors;
        }

        /// <summary>
        /// Gets all connections for a research node
        /// </summary>
        public List<TechNodeConnection> GetConnectionsForNode(string researchId)
        {
            return connections.Where(c => 
                c.fromResearchId == researchId || 
                c.toResearchId == researchId).ToList();
        }

        private void OnValidate()
        {
            // Auto-initialize on validate in editor
            if (_researchLookup == null)
                Initialize();
        }
    }
}
