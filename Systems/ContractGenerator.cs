using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Procedural contract generation system
    /// </summary>
    public class ContractGenerator
    {
        #region Configuration

        // Scaling factors
        private const float REWARD_SCALING_FACTOR = 0.15f;
        private const float REQUIREMENT_SCALING_FACTOR = 0.1f;
        private const float REPUTATION_REQUIREMENT_SCALING = 0.08f;

        // Generation parameters
        private const int MAX_CONTRACTS_PER_BATCH = 10;
        private const float LEGENDARY_CHANCE_BASE = 0.02f;
        private const float ELITE_CHANCE_BASE = 0.08f;

        #endregion

        #region Fictional Client Database

        /// <summary>
        /// Predefined fictional clients for contracts
        /// </summary>
        public static readonly ClientInfo[] FictionalClients = new ClientInfo[]
        {
            new ClientInfo { name = "Aether Dynamics", category = ContractCategory.Industrial, budgetModifier = 1.2f, reputationRequirement = 20f },
            new ClientInfo { name = "Nexus Security Solutions", category = ContractCategory.Security, budgetModifier = 1.3f, reputationRequirement = 30f },
            new ClientInfo { name = "Stellar Surveillance Corp", category = ContractCategory.Surveillance, budgetModifier = 1.0f, reputationRequirement = 10f },
            new ClientInfo { name = "Titan Defense Systems", category = ContractCategory.Military, budgetModifier = 1.5f, reputationRequirement = 50f },
            new ClientInfo { name = "Quantum Reconnaissance Ltd", category = ContractCategory.Surveillance, budgetModifier = 1.4f, reputationRequirement = 40f },
            new ClientInfo { name = "OmniTech Industries", category = ContractCategory.Industrial, budgetModifier = 1.1f, reputationRequirement = 15f },
            new ClientInfo { name = "CyberGuard Solutions", category = ContractCategory.Security, budgetModifier = 1.2f, reputationRequirement = 25f },
            new ClientInfo { name = "Nova Agricultural Systems", category = ContractCategory.Agriculture, budgetModifier = 0.9f, reputationRequirement = 5f },
            new ClientInfo { name = "Fusion Logistics", category = ContractCategory.Logistics, budgetModifier = 1.0f, reputationRequirement = 10f },
            new ClientInfo { name = "Prime Research Labs", category = ContractCategory.Research, budgetModifier = 1.3f, reputationRequirement = 35f },
            new ClientInfo { name = "Apex Emergency Response", category = ContractCategory.SearchAndRescue, budgetModifier = 1.1f, reputationRequirement = 20f },
            new ClientInfo { name = "Zenith Aviation", category = ContractCategory.Logistics, budgetModifier = 1.2f, reputationRequirement = 30f },
            new ClientInfo { name = "Vertex Mining Corp", category = ContractCategory.Industrial, budgetModifier = 1.4f, reputationRequirement = 45f },
            new ClientInfo { name = "Helix Biotech", category = ContractCategory.Research, budgetModifier = 1.2f, reputationRequirement = 40f },
            new ClientInfo { name = "Orbit Communications", category = ContractCategory.Surveillance, budgetModifier = 1.1f, reputationRequirement = 25f },
            new ClientInfo { name = "Pulse Energy", category = ContractCategory.Industrial, budgetModifier = 1.3f, reputationRequirement = 35f },
            new ClientInfo { name = "Vortex Shipping", category = ContractCategory.Logistics, budgetModifier = 0.95f, reputationRequirement = 15f },
            new ClientInfo { name = "Echo Detection Systems", category = ContractCategory.Surveillance, budgetModifier = 1.0f, reputationRequirement = 20f },
            new ClientInfo { name = "Horizon Exploration", category = ContractCategory.SearchAndRescue, budgetModifier = 1.2f, reputationRequirement = 30f },
            new ClientInfo { name = "Nebula Cloud Services", category = ContractCategory.Research, budgetModifier = 1.1f, reputationRequirement = 25f }
        };

        /// <summary>
        /// Contract type descriptors
        /// </summary>
        public static readonly ContractTypeDescriptor[] ContractTypes = new ContractTypeDescriptor[]
        {
            new ContractTypeDescriptor { name = "Surveillance Drone Fleet", category = ContractCategory.Surveillance, baseReward = 5000, deadlineDays = 14 },
            new ContractTypeDescriptor { name = "Border Patrol Units", category = ContractCategory.Security, baseReward = 8000, deadlineDays = 21 },
            new ContractTypeDescriptor { name = "Industrial Inspection Drones", category = ContractCategory.Industrial, baseReward = 6000, deadlineDays = 18 },
            new ContractTypeDescriptor { name = "Search & Rescue Swarm", category = ContractCategory.SearchAndRescue, baseReward = 7000, deadlineDays = 10 },
            new ContractTypeDescriptor { name = "Agricultural Monitoring", category = ContractCategory.Agriculture, baseReward = 4000, deadlineDays = 30 },
            new ContractTypeDescriptor { name = "Security Perimeter Drones", category = ContractCategory.Security, baseReward = 7500, deadlineDays = 16 },
            new ContractTypeDescriptor { name = "Logistics Delivery Fleet", category = ContractCategory.Logistics, baseReward = 5500, deadlineDays = 20 },
            new ContractTypeDescriptor { name = "Research Data Collection", category = ContractCategory.Research, baseReward = 9000, deadlineDays = 25 },
            new ContractTypeDescriptor { name = "Tactical Reconnaissance", category = ContractCategory.Military, baseReward = 12000, deadlineDays = 12 },
            new ContractTypeDescriptor { name = "Emergency Response Units", category = ContractCategory.SearchAndRescue, baseReward = 8500, deadlineDays = 8 }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Generate a single contract
        /// </summary>
        public ContractData GenerateContract(
            float difficulty, 
            float playerTechLevel, 
            float playerReputation,
            ContractCategory? preferredCategory = null)
        {
            var contract = ScriptableObject.CreateInstance<ContractData>();

            // Determine difficulty tier
            var contractDifficulty = DetermineDifficulty(difficulty, playerTechLevel);

            // Select client
            var client = SelectClient(playerReputation, preferredCategory);

            // Select contract type
            var contractType = SelectContractType(client.category);

            // Generate basic info
            contract.contractId = System.Guid.NewGuid().ToString("N")[..8];
            contract.displayName = GenerateContractName(contractDifficulty, contractType.name);
            contract.description = GenerateDescription(client.name, contractType.name, contractDifficulty);
            contract.clientName = client.name;
            contract.category = client.category;
            contract.difficulty = contractDifficulty;

            // Generate requirements
            GenerateRequirements(contract, contractDifficulty, playerTechLevel);

            // Generate rewards
            GenerateRewards(contract, contractDifficulty, playerTechLevel, client.budgetModifier, contractType.baseReward);

            // Generate terms
            GenerateTerms(contract, contractDifficulty, contractType.deadlineDays);

            // Generate risk profile
            GenerateRiskProfile(contract, contractDifficulty);

            // Set competition
            contract.competitorCount = DetermineCompetitorCount(contractDifficulty);

            return contract;
        }

        /// <summary>
        /// Generate a batch of contracts
        /// </summary>
        public List<ContractData> GenerateContractBatch(
            int count, 
            float playerReputation,
            float playerTechLevel,
            List<ContractCategory> preferredCategories = null)
        {
            var contracts = new List<ContractData>();
            var usedClients = new HashSet<string>();

            for (int i = 0; i < count && i < MAX_CONTRACTS_PER_BATCH; i++)
            {
                // Vary difficulty slightly
                float difficultyVariance = UnityEngine.Random.Range(-0.2f, 0.3f);
                float difficulty = 1f + difficultyVariance;

                // Occasionally prefer specific categories
                ContractCategory? preferredCategory = null;
                if (preferredCategories != null && preferredCategories.Count > 0 && UnityEngine.Random.value < 0.3f)
                {
                    preferredCategory = preferredCategories[UnityEngine.Random.Range(0, preferredCategories.Count)];
                }

                var contract = GenerateContract(difficulty, playerTechLevel, playerReputation, preferredCategory);
                
                // Avoid duplicate clients in same batch
                if (!usedClients.Contains(contract.clientName))
                {
                    contracts.Add(contract);
                    usedClients.Add(contract.clientName);
                }
                else
                {
                    // Try once more with different client
                    contract = GenerateContract(difficulty, playerTechLevel, playerReputation, null);
                    contracts.Add(contract);
                }
            }

            return contracts;
        }

        /// <summary>
        /// Generate contracts appropriate for player's current progress
        /// </summary>
        public List<ContractData> GenerateProgressAppropriateContracts(
            float playerReputation,
            float playerTechLevel,
            int playerCompanyLevel,
            int count = 5)
        {
            var contracts = new List<ContractData>();

            // Generate contracts at various difficulty levels
            int easyCount = Mathf.Max(1, count / 3);
            int normalCount = Mathf.Max(1, count / 3);
            int hardCount = count - easyCount - normalCount;

            // Easy contracts (below player level)
            for (int i = 0; i < easyCount; i++)
            {
                float difficulty = 0.7f;
                contracts.Add(GenerateContract(difficulty, playerTechLevel, playerReputation));
            }

            // Normal contracts (at player level)
            for (int i = 0; i < normalCount; i++)
            {
                float difficulty = 1.0f;
                contracts.Add(GenerateContract(difficulty, playerTechLevel, playerReputation));
            }

            // Hard contracts (above player level)
            for (int i = 0; i < hardCount; i++)
            {
                float difficulty = 1.3f;
                contracts.Add(GenerateContract(difficulty, playerTechLevel, playerReputation));
            }

            // Shuffle
            return contracts.OrderBy(x => UnityEngine.Random.value).ToList();
        }

        #endregion

        #region Generation Helpers

        /// <summary>
        /// Determine contract difficulty based on player progress
        /// </summary>
        private ContractDifficulty DetermineDifficulty(float targetDifficulty, float playerTechLevel)
        {
            float roll = UnityEngine.Random.value;

            // Chance for legendary/elite increases with tech level
            float legendaryChance = LEGENDARY_CHANCE_BASE + (playerTechLevel * 0.002f);
            float eliteChance = ELITE_CHANCE_BASE + (playerTechLevel * 0.005f);

            if (roll < legendaryChance && targetDifficulty > 1.5f)
                return ContractDifficulty.Legendary;
            if (roll < legendaryChance + eliteChance && targetDifficulty > 1.2f)
                return ContractDifficulty.Elite;
            if (targetDifficulty > 1.1f)
                return ContractDifficulty.Hard;
            if (targetDifficulty > 0.85f)
                return ContractDifficulty.Normal;
            
            return ContractDifficulty.Easy;
        }

        /// <summary>
        /// Select a client based on player reputation
        /// </summary>
        private ClientInfo SelectClient(float playerReputation, ContractCategory? preferredCategory)
        {
            var availableClients = FictionalClients.Where(c => c.reputationRequirement <= playerReputation).ToList();
            
            if (preferredCategory.HasValue)
            {
                var categoryClients = availableClients.Where(c => c.category == preferredCategory.Value).ToList();
                if (categoryClients.Count > 0)
                    availableClients = categoryClients;
            }

            if (availableClients.Count == 0)
                return FictionalClients[0]; // Fallback

            // Weight by budget modifier (higher budget clients more likely at higher rep)
            float totalWeight = availableClients.Sum(c => c.budgetModifier);
            float roll = UnityEngine.Random.Range(0f, totalWeight);

            float currentWeight = 0f;
            foreach (var client in availableClients)
            {
                currentWeight += client.budgetModifier;
                if (roll <= currentWeight)
                    return client;
            }

            return availableClients[availableClients.Count - 1];
        }

        /// <summary>
        /// Select a contract type matching the category
        /// </summary>
        private ContractTypeDescriptor SelectContractType(ContractCategory category)
        {
            var matchingTypes = ContractTypes.Where(t => t.category == category).ToList();
            
            if (matchingTypes.Count == 0)
                matchingTypes = ContractTypes.ToList();

            return matchingTypes[UnityEngine.Random.Range(0, matchingTypes.Count)];
        }

        /// <summary>
        /// Generate contract name
        /// </summary>
        private string GenerateContractName(ContractDifficulty difficulty, string contractType)
        {
            string[] prefixes = new string[] { "Urgent", "Standard", "Premium", "Experimental", "Classified" };
            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];

            string difficultyPrefix = difficulty switch
            {
                ContractDifficulty.Easy => "Basic ",
                ContractDifficulty.Normal => "",
                ContractDifficulty.Hard => "Advanced ",
                ContractDifficulty.Elite => "Elite ",
                ContractDifficulty.Legendary => "Legendary ",
                _ => ""
            };

            return $"{difficultyPrefix}{prefix} {contractType}";
        }

        /// <summary>
        /// Generate contract description
        /// </summary>
        private string GenerateDescription(string clientName, string contractType, ContractDifficulty difficulty)
        {
            string[] templates = new string[]
            {
                $"{clientName} requires a {difficulty.ToString().ToLower()} {contractType.ToLower()} solution for their operations.",
                $"A {difficulty.ToString().ToLower()} priority contract from {clientName} for {contractType.ToLower()} services.",
                $"{clientName} is seeking a reliable partner to deliver {contractType.ToLower()} capabilities.",
                $"High-priority {contractType.ToLower()} project from {clientName} with strict requirements.",
                $"{clientName} needs specialized drones for their {contractType.ToLower()} division."
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        /// <summary>
        /// Generate requirements for the contract
        /// </summary>
        private void GenerateRequirements(ContractData contract, ContractDifficulty difficulty, float playerTechLevel)
        {
            // Base tech level requirement
            float baseTechReq = 5f;
            float difficultyMult = difficulty switch
            {
                ContractDifficulty.Easy => 0.5f,
                ContractDifficulty.Normal => 1f,
                ContractDifficulty.Hard => 1.5f,
                ContractDifficulty.Elite => 2.5f,
                ContractDifficulty.Legendary => 4f,
                _ => 1f
            };

            contract.requiredTechLevel = Mathf.Max(1, baseTechReq * difficultyMult * (1 + playerTechLevel * REQUIREMENT_SCALING_FACTOR * 0.1f));

            // Drone tier requirement
            contract.requiredDroneTier = difficulty switch
            {
                ContractDifficulty.Easy => 1,
                ContractDifficulty.Normal => UnityEngine.Random.Range(1, 3),
                ContractDifficulty.Hard => UnityEngine.Random.Range(2, 4),
                ContractDifficulty.Elite => UnityEngine.Random.Range(3, 5),
                ContractDifficulty.Legendary => 5,
                _ => 1
            };

            // Minimum reputation
            contract.minimumReputationRequired = difficulty switch
            {
                ContractDifficulty.Easy => 0f,
                ContractDifficulty.Normal => 10f,
                ContractDifficulty.Hard => 30f,
                ContractDifficulty.Elite => 60f,
                ContractDifficulty.Legendary => 100f,
                _ => 0f
            };

            // Required technologies (random chance based on difficulty)
            contract.requiredTechnologies = new List<string>();
            if (difficulty >= ContractDifficulty.Hard && UnityEngine.Random.value < 0.5f)
            {
                contract.requiredTechnologies.Add("AdvancedAI");
            }
            if (difficulty >= ContractDifficulty.Elite && UnityEngine.Random.value < 0.4f)
            {
                contract.requiredTechnologies.Add("StealthSystems");
            }
        }

        /// <summary>
        /// Generate rewards for the contract
        /// </summary>
        private void GenerateRewards(ContractData contract, ContractDifficulty difficulty, float playerTechLevel, float budgetModifier, float baseTypeReward)
        {
            float difficultyMult = ContractData.GetDifficultyMultiplier(difficulty);
            float scalingMult = 1f + (playerTechLevel * REWARD_SCALING_FACTOR);

            // Base reward
            contract.baseReward = baseTypeReward * difficultyMult * scalingMult * budgetModifier;
            contract.baseReward *= UnityEngine.Random.Range(0.9f, 1.1f); // Variance

            // Reputation reward
            contract.reputationReward = difficulty switch
            {
                ContractDifficulty.Easy => UnityEngine.Random.Range(3f, 8f),
                ContractDifficulty.Normal => UnityEngine.Random.Range(8f, 15f),
                ContractDifficulty.Hard => UnityEngine.Random.Range(15f, 25f),
                ContractDifficulty.Elite => UnityEngine.Random.Range(25f, 40f),
                ContractDifficulty.Legendary => UnityEngine.Random.Range(50f, 80f),
                _ => 5f
            };

            // Early delivery bonus
            contract.bonusForEarlyDelivery = UnityEngine.Random.Range(0.05f, 0.15f);
            contract.maxEarlyDeliveryBonus = contract.baseReward * 0.2f;

            // Quality bonus
            contract.qualityBonusMultiplier = UnityEngine.Random.Range(0.1f, 0.3f);
        }

        /// <summary>
        /// Generate terms for the contract
        /// </summary>
        private void GenerateTerms(ContractData contract, ContractDifficulty difficulty, float baseDeadlineDays)
        {
            float difficultyMult = difficulty switch
            {
                ContractDifficulty.Easy => 1.2f,
                ContractDifficulty.Normal => 1f,
                ContractDifficulty.Hard => 0.85f,
                ContractDifficulty.Elite => 0.7f,
                ContractDifficulty.Legendary => 0.6f,
                _ => 1f
            };

            contract.deadlineDays = baseDeadlineDays * difficultyMult * UnityEngine.Random.Range(0.9f, 1.1f);
            contract.penaltyPerDayLate = UnityEngine.Random.Range(0.03f, 0.08f);
            contract.maxPenalty = UnityEngine.Random.Range(0.3f, 0.6f);
            contract.upfrontPaymentPercent = UnityEngine.Random.Range(0.15f, 0.3f);
        }

        /// <summary>
        /// Generate risk profile
        /// </summary>
        private void GenerateRiskProfile(ContractData contract, ContractDifficulty difficulty)
        {
            contract.riskProfile = difficulty switch
            {
                ContractDifficulty.Easy => RiskProfile.Low,
                ContractDifficulty.Normal => UnityEngine.Random.value < 0.7f ? RiskProfile.Low : RiskProfile.Medium,
                ContractDifficulty.Hard => UnityEngine.Random.value < 0.6f ? RiskProfile.Medium : RiskProfile.High,
                ContractDifficulty.Elite => UnityEngine.Random.value < 0.5f ? RiskProfile.High : RiskProfile.Critical,
                ContractDifficulty.Legendary => RiskProfile.Critical,
                _ => RiskProfile.Medium
            };

            contract.baseFailureChance = contract.riskProfile switch
            {
                RiskProfile.None => 0f,
                RiskProfile.Low => 0.02f,
                RiskProfile.Medium => 0.05f,
                RiskProfile.High => 0.1f,
                RiskProfile.Critical => 0.2f,
                _ => 0.05f
            };

            contract.reputationPenaltyOnFail = contract.reputationReward * 0.8f;
        }

        /// <summary>
        /// Determine number of competitors
        /// </summary>
        private int DetermineCompetitorCount(ContractDifficulty difficulty)
        {
            return difficulty switch
            {
                ContractDifficulty.Easy => UnityEngine.Random.Range(2, 4),
                ContractDifficulty.Normal => UnityEngine.Random.Range(3, 5),
                ContractDifficulty.Hard => UnityEngine.Random.Range(4, 6),
                ContractDifficulty.Elite => UnityEngine.Random.Range(5, 8),
                ContractDifficulty.Legendary => UnityEngine.Random.Range(7, 11),
                _ => 3
            };
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Client information structure
        /// </summary>
        public class ClientInfo
        {
            public string name;
            public ContractCategory category;
            public float budgetModifier;
            public float reputationRequirement;
        }

        /// <summary>
        /// Contract type descriptor
        /// </summary>
        public class ContractTypeDescriptor
        {
            public string name;
            public ContractCategory category;
            public float baseReward;
            public float deadlineDays;
        }

        #endregion
    }
}
