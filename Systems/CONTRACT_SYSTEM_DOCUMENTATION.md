# Project Aegis: Contract & Bidding System Documentation

## Overview

The Contract/Bidding System is the primary money-making mechanic in Project Aegis: Drone Dominion. Players bid against AI competitors for contracts, with winning chances based on reputation, technology, pricing, and delivery timelines.

---

## Core Components

### 1. ContractData.cs
ScriptableObject defining contract specifications.

**Key Properties:**
- `contractId`, `displayName`, `description`, `clientName`
- `requiredTechLevel`, `requiredTechnologies`, `requiredDroneTier`
- `baseReward`, `reputationReward`, `bonusForEarlyDelivery`
- `deadlineDays`, `penaltyPerDayLate`, `maxPenalty`
- `riskProfile`, `competitorCount`

**Methods:**
- `CalculateTotalReward()` - Includes early delivery and quality bonuses
- `CalculateLatePenalty()` - Penalty calculation for late delivery
- `CanPlayerBid()` - Validates player meets requirements

### 2. ContractManager.cs
Main singleton controller for the contract system.

**Key Methods:**
```csharp
// Generate contracts
List<ContractData> GenerateContracts(int count)
void RefreshAvailableContracts()

// Get contracts
List<ContractData> GetAvailableContracts()
List<ContractData> GetBidEligibleContracts()
List<ActiveContract> GetActiveContracts()

// Bidding
BidResult SubmitBid(ContractData contract, BidParameters bid)
float GetWinningChance(ContractData contract, BidParameters bid)
(float, BidScoreBreakdown) PreviewBid(ContractData contract, BidParameters bid)

// Contract lifecycle
void AcceptContract(ContractData contract)
void StartContract(ActiveContract contract)
void CompleteContract(ActiveContract contract, float quality)
void FailContract(ActiveContract contract, FailureType failure)
```

### 3. BidParameters.cs
Player's bid configuration.

**Properties:**
- `proposedPrice` - Bid amount (can be above/below base)
- `proposedDeadlineDays` - Delivery timeline
- `qualityInvestment` - Extra spending for quality
- `promisedTech` - Technologies to use
- `allocatedDroneCount` - Resources assigned
- `resourcePriority` - Speed/Quality/Cost/Balanced

**Factory Methods:**
- `CreateDefault()` - Base values
- `CreateAggressiveBid()` - Lower price, faster
- `CreatePremiumBid()` - Higher price, better quality
- `CreateBalancedBid()` - Moderate approach

### 4. BidCalculator.cs
Static class for calculating winning chances.

**Scoring Formula:**
```
Total Score = (Reputation × 0.30) +
              (TechMatch × 0.25) +
              (PriceCompetitiveness × 0.25) +
              (DeadlineAdvantage × 0.20)

PriceCompetitiveness = (BasePrice / ProposedPrice) × 100
DeadlineAdvantage = (BaseDeadline / ProposedDeadline) × 100
```

**Key Methods:**
- `CalculateWinningChance()` - Monte Carlo simulation
- `CalculateScoreBreakdown()` - Detailed component scores
- `GetChanceAssessment()` - Qualitative rating
- `RecommendBid()` - Suggested bid parameters

### 5. ActiveContract.cs
Runtime state of an active contract.

**States:**
- `Bidding` → `Accepted` → `InProgress` → `Completed`/`Failed`

**Key Methods:**
- `Start()` - Begin work
- `UpdateProgress()` - Daily progress update
- `Complete()` - Successful finish
- `Fail()` - Unsuccessful finish

### 6. ContractGenerator.cs
Procedural contract generation.

**Features:**
- 20 fictional clients (Aether Dynamics, Nexus Security, etc.)
- 10 contract types (Surveillance, Security, Industrial, etc.)
- Difficulty scaling with player progress
- Category-based generation

**Key Methods:**
- `GenerateContract()` - Single contract
- `GenerateContractBatch()` - Multiple contracts
- `GenerateProgressAppropriateContracts()` - Scaled to player

---

## Bid Calculation Formula (Detailed)

### Score Components

#### 1. Reputation Score (0-100)
```csharp
baseScore = min(playerReputation, 100)
if (playerReputation > minRequired):
    bonus = (excess / minRequired) × 10
```

#### 2. Tech Match Score (0-100)
```csharp
score = (techLevelRatio × 40) +           // Tech level (40 pts)
        (matchedTechPercent × 30) +       // Required tech (30 pts)
        (droneTierRatio × 20) +           // Drone tier (20 pts)
        excessBonus                       // Exceeding reqs (10 pts)
```

#### 3. Price Score (0-200+)
```csharp
score = (basePrice / proposedPrice) × 100

// Diminishing returns
if (score > 150): score = 150 + (score - 150) × 0.3

// Suspicious price penalty
if (proposedPrice < base × 0.5): score ×= 0.7
```

#### 4. Deadline Score (0-200+)
```csharp
score = (baseDeadline / proposedDeadline) × 100

// Diminishing returns
if (score > 150): score = 150 + (score - 150) × 0.3

// Unrealistic deadline penalty
if (proposed < base × 0.3): score ×= 0.6
```

### Probability Calculation (Bradley-Terry Model)
```csharp
// Scale scores to prevent overflow
scaleFactor = maxScore > 50 ? 50 / maxScore : 1

// Calculate exponentials
playerExp = exp(playerScore × scaleFactor / 10)
totalExp = playerExp + sum(competitorExps)

// Probability
winningChance = playerExp / totalExp
```

---

## Code Example: Submitting a Bid

```csharp
using ProjectAegis.Systems.Contracts;

public class BidExample : MonoBehaviour
{
    void SubmitAContractBid()
    {
        // Get the contract manager
        var contractManager = ContractManager.Instance;
        
        // Get available contracts
        var available = contractManager.GetAvailableContracts();
        var contract = available[0];
        
        // Preview winning chance with default bid
        var defaultBid = BidParameters.CreateDefault(contract);
        var (winChance, breakdown) = contractManager.PreviewBid(contract, defaultBid);
        
        Debug.Log($"Default bid win chance: {winChance:P0}");
        
        // Try an aggressive bid for better chance
        var aggressiveBid = BidParameters.CreateAggressiveBid(contract, 0.15f, 0.2f);
        var (aggWinChance, aggBreakdown) = contractManager.PreviewBid(contract, aggressiveBid);
        
        Debug.Log($"Aggressive bid win chance: {aggWinChance:P0}");
        
        // Submit the bid
        var result = contractManager.SubmitBid(contract, aggressiveBid);
        
        if (result.isWinner)
        {
            Debug.Log($"Won contract! {result.winReason}");
            Debug.Log($"Upfront payment: ${result.upfrontPayment:N0}");
            Debug.Log($"Expected reward: ${result.expectedReward:N0}");
        }
        else
        {
            Debug.Log($"Lost contract. {result.loseReason}");
            Debug.Log($"Your rank: {result.playerRank} of {result.totalBidders}");
            
            foreach (var suggestion in result.improvementSuggestions)
            {
                Debug.Log($"- {suggestion}");
            }
        }
    }
}
```

---

## Contract Generation Scaling

### Difficulty Tiers

| Difficulty | Tech Level Mult | Reward Mult | Competitors | Risk |
|------------|-----------------|-------------|-------------|------|
| Easy | 0.5x | 0.6x | 2-3 | Low |
| Normal | 1.0x | 1.0x | 3-4 | Low-Med |
| Hard | 1.5x | 1.5x | 4-6 | Medium |
| Elite | 2.5x | 2.5x | 5-8 | High |
| Legendary | 4.0x | 5.0x | 7-10 | Critical |

### Scaling Formula
```csharp
// Reward scales with player tech level
reward = baseReward × difficultyMult × (1 + playerTech × 0.15)

// Requirements scale with player progress
requiredTech = baseTech × difficultyMult × (1 + playerTech × 0.1)

// Reputation requirements increase with difficulty
minReputation = difficulty switch {
    Easy => 0, Normal => 10, Hard => 30,
    Elite => 60, Legendary => 100
}
```

### Generation Strategy
```csharp
// Generate mix of difficulties
1/3 Easy contracts (below player level)
1/3 Normal contracts (at player level)
1/3 Hard contracts (above player level)

// Occasionally include Elite/Legendary at higher levels
eliteChance = 0.08 + (playerTech × 0.005)
legendaryChance = 0.02 + (playerTech × 0.002)
```

---

## MVP Contracts

### 1. Basic Surveillance Contract
- **Client:** Stellar Surveillance Corp
- **Difficulty:** Easy
- **Requirements:** Tech Level 1, Drone Tier 1
- **Reward:** $2,500 base, +5 reputation
- **Deadline:** 7 days
- **Competitors:** 2

### 2. Advanced Reconnaissance
- **Client:** Quantum Reconnaissance Ltd
- **Difficulty:** Normal
- **Requirements:** Tech Level 5, Drone Tier 2, LongRangeSensors
- **Reward:** $8,000 base, +15 reputation
- **Deadline:** 14 days
- **Competitors:** 4

### 3. Elite Defense Contract
- **Client:** Titan Defense Systems
- **Difficulty:** Hard
- **Requirements:** Tech Level 12, Drone Tier 4, AdvancedAI, StealthSystems
- **Reward:** $25,000 base, +35 reputation
- **Deadline:** 21 days
- **Competitors:** 6

---

## Fictional Clients

| Client Name | Category | Budget Mod | Min Rep |
|-------------|----------|------------|---------|
| Aether Dynamics | Industrial | 1.2x | 20 |
| Nexus Security Solutions | Security | 1.3x | 30 |
| Stellar Surveillance Corp | Surveillance | 1.0x | 10 |
| Titan Defense Systems | Military | 1.5x | 50 |
| Quantum Reconnaissance Ltd | Surveillance | 1.4x | 40 |
| OmniTech Industries | Industrial | 1.1x | 15 |
| CyberGuard Solutions | Security | 1.2x | 25 |
| Nova Agricultural Systems | Agriculture | 0.9x | 5 |
| Fusion Logistics | Logistics | 1.0x | 10 |
| Prime Research Labs | Research | 1.3x | 35 |

---

## Events & Integration

### ContractManager Events
```csharp
OnContractsRefreshed - New contracts available
OnContractAvailable - Single contract added
OnBidSubmitted - Bid result available
OnContractStarted - Work begins
OnContractCompleted - Successful finish
OnContractFailed - Unsuccessful finish
OnContractProgressUpdated - Progress tick
```

### ActiveContract Events
```csharp
OnProgressUpdated - Daily progress
OnContractCompleted - Finished successfully
OnContractFailed - Failed with reason
OnPenaltyApplied - Late penalty incurred
```

---

## File Structure

```
/ProjectAegis/Scripts/Systems/
├── ContractData.cs          # Contract ScriptableObject
├── ContractTemplate.cs      # Procedural generation template
├── BidParameters.cs         # Player bid data
├── BidResult.cs             # Bid outcome
├── CompetitorBid.cs         # AI competitor
├── BidCalculator.cs         # Winning chance algorithm
├── ActiveContract.cs        # Runtime contract state
├── ContractGenerator.cs     # Procedural generation
├── ContractManager.cs       # Main controller
├── MVPContracts.cs          # Sample contracts
└── CONTRACT_SYSTEM_DOCUMENTATION.md  # This file
```

---

## Integration Tips

1. **Initialize in GameManager:**
   ```csharp
   ContractManager.Instance.UpdatePlayerStats(reputation, techLevel, level, droneTier);
   ```

2. **Daily Update:**
   ```csharp
   // ContractManager.Update() handles daily progress automatically
   ```

3. **Save/Load:**
   ```csharp
   string saveData = ContractManager.Instance.SaveToJson();
   ContractManager.Instance.LoadFromJson(saveData);
   ```

4. **UI Binding:**
   ```csharp
   var contracts = ContractManager.Instance.GetBidEligibleContracts();
   foreach (var contract in contracts)
   {
       float winChance = ContractManager.Instance.GetWinningChance(contract, bid);
       // Display contract with win chance
   }
   ```
