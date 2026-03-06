namespace ProjectAegis.UI
{
    /// <summary>
    /// Centralized screen identifiers for Project Aegis: Drone Dominion
    /// Use these constants when navigating between screens
    /// </summary>
    public static class ScreenIds
    {
        // Main Screens
        public const string MAIN_MENU = "MainMenu";
        public const string GAME_HUD = "GameHUD";
        
        // Research & Development
        public const string RESEARCH_TREE = "ResearchTree";
        public const string RESEARCH_DETAIL = "ResearchDetail";
        public const string PROTOTYPE_TEST = "PrototypeTest";
        public const string RND_DASHBOARD = "RndDashboard";
        
        // Business & Contracts
        public const string CONTRACTS = "Contracts";
        public const string CONTRACT_DETAIL = "ContractDetail";
        public const string BIDDING = "Bidding";
        public const string PRODUCTION = "Production";
        public const string FINANCE = "Finance";
        
        // Intelligence
        public const string INTELLIGENCE = "Intelligence";
        public const string MARKET_INTEL = "MarketIntel";
        public const string RIVAL_INTEL = "RivalIntel";
        
        // System
        public const string SETTINGS = "Settings";
        public const string PAUSE_MENU = "PauseMenu";
        public const string SAVE_LOAD = "SaveLoad";
        public const string TUTORIAL = "Tutorial";
        public const string CREDITS = "Credits";
        
        // Meta
        public const string ACHIEVEMENTS = "Achievements";
        public const string STATISTICS = "Statistics";
        public const string EVENT_LOG = "EventLog";
    }
    
    /// <summary>
    /// Screen categories for organization
    /// </summary>
    public enum ScreenCategory
    {
        Main,
        Research,
        Business,
        Intelligence,
        System,
        Meta
    }
    
    /// <summary>
    /// Extension methods for screen IDs
    /// </summary>
    public static class ScreenIdExtensions
    {
        /// <summary>
        /// Gets the category for a screen ID
        /// </summary>
        public static ScreenCategory GetCategory(this string screenId)
        {
            return screenId switch
            {
                ScreenIds.MAIN_MENU or ScreenIds.GAME_HUD => ScreenCategory.Main,
                
                ScreenIds.RESEARCH_TREE or ScreenIds.RESEARCH_DETAIL or 
                ScreenIds.PROTOTYPE_TEST or ScreenIds.RND_DASHBOARD => ScreenCategory.Research,
                
                ScreenIds.CONTRACTS or ScreenIds.CONTRACT_DETAIL or 
                ScreenIds.BIDDING or ScreenIds.PRODUCTION or ScreenIds.FINANCE => ScreenCategory.Business,
                
                ScreenIds.INTELLIGENCE or ScreenIds.MARKET_INTEL or 
                ScreenIds.RIVAL_INTEL => ScreenCategory.Intelligence,
                
                ScreenIds.SETTINGS or ScreenIds.PAUSE_MENU or 
                ScreenIds.SAVE_LOAD or ScreenIds.TUTORIAL or ScreenIds.CREDITS => ScreenCategory.System,
                
                ScreenIds.ACHIEVEMENTS or ScreenIds.STATISTICS or 
                ScreenIds.EVENT_LOG => ScreenCategory.Meta,
                
                _ => ScreenCategory.Main
            };
        }
        
        /// <summary>
        /// Gets the display name for a screen ID
        /// </summary>
        public static string GetDisplayName(this string screenId)
        {
            return screenId switch
            {
                ScreenIds.MAIN_MENU => "Main Menu",
                ScreenIds.GAME_HUD => "Game HUD",
                ScreenIds.RESEARCH_TREE => "Research Tree",
                ScreenIds.RESEARCH_DETAIL => "Research Details",
                ScreenIds.PROTOTYPE_TEST => "Prototype Testing",
                ScreenIds.RND_DASHBOARD => "R&D Dashboard",
                ScreenIds.CONTRACTS => "Contracts",
                ScreenIds.CONTRACT_DETAIL => "Contract Details",
                ScreenIds.BIDDING => "Bid Submission",
                ScreenIds.PRODUCTION => "Production",
                ScreenIds.FINANCE => "Finance",
                ScreenIds.INTELLIGENCE => "Intelligence",
                ScreenIds.MARKET_INTEL => "Market Intelligence",
                ScreenIds.RIVAL_INTEL => "Rival Analysis",
                ScreenIds.SETTINGS => "Settings",
                ScreenIds.PAUSE_MENU => "Pause Menu",
                ScreenIds.SAVE_LOAD => "Save / Load",
                ScreenIds.TUTORIAL => "Tutorial",
                ScreenIds.CREDITS => "Credits",
                ScreenIds.ACHIEVEMENTS => "Achievements",
                ScreenIds.STATISTICS => "Statistics",
                ScreenIds.EVENT_LOG => "Event Log",
                _ => screenId
            };
        }
        
        /// <summary>
        /// Gets the icon path for a screen ID
        /// </summary>
        public static string GetIconPath(this string screenId)
        {
            return $"UI/Icons/Screens/{screenId}";
        }
    }
}
