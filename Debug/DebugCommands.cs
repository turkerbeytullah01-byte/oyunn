using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Debug commands system for Project Aegis: Drone Dominion
    /// Provides console-like command interface for debugging
    /// </summary>
    public class DebugCommands : MonoBehaviour
    {
        #region Singleton
        private static DebugCommands _instance;
        public static DebugCommands Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DebugCommands>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DebugCommands");
                        _instance = go.AddComponent<DebugCommands>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Command Registry
        private Dictionary<string, DebugCommandInfo> _commands = new Dictionary<string, DebugCommandInfo>();
        private List<string> _commandHistory = new List<string>();
        private const int MAX_HISTORY = 50;
        
        public delegate object DebugCommandDelegate(params object[] args);
        
        public class DebugCommandInfo
        {
            public string name;
            public string description;
            public string usage;
            public DebugCommandDelegate execute;
            public Type[] parameterTypes;
            public bool requiresConfirmation;
        }
        #endregion

        #region Events
        public event Action<string> OnCommandExecuted;
        public event Action<string> OnCommandError;
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
            
            RegisterDefaultCommands();
        }
        #endregion

        #region Command Registration
        /// <summary>
        /// Register a debug command
        /// </summary>
        public void RegisterCommand(string name, string description, string usage, DebugCommandDelegate execute, Type[] parameterTypes = null, bool requiresConfirmation = false)
        {
            _commands[name.ToLower()] = new DebugCommandInfo
            {
                name = name,
                description = description,
                usage = usage,
                execute = execute,
                parameterTypes = parameterTypes ?? new Type[0],
                requiresConfirmation = requiresConfirmation
            };
        }

        /// <summary>
        /// Unregister a command
        /// </summary>
        public void UnregisterCommand(string name)
        {
            _commands.Remove(name.ToLower());
        }

        private void RegisterDefaultCommands()
        {
            // Money commands
            RegisterCommand("money.add", "Add money to player account", "money.add <amount>", 
                args => AddMoney(args), new Type[] { typeof(float) });
            
            RegisterCommand("money.set", "Set player money to specific amount", "money.set <amount>", 
                args => SetMoney(args), new Type[] { typeof(float) });
            
            RegisterCommand("money.get", "Get current money amount", "money.get", 
                args => GetMoney());
            
            // Reputation commands
            RegisterCommand("reputation.add", "Add reputation points", "reputation.add <amount>", 
                args => AddReputation(args), new Type[] { typeof(float) });
            
            RegisterCommand("reputation.set", "Set reputation to specific value", "reputation.set <amount>", 
                args => SetReputation(args), new Type[] { typeof(float) });
            
            // Research commands
            RegisterCommand("research.complete", "Complete a specific research", "research.complete <researchId>", 
                args => CompleteResearch(args), new Type[] { typeof(string) });
            
            RegisterCommand("research.completeall", "Complete all research", "research.completeall", 
                args => CompleteAllResearch());
            
            RegisterCommand("research.unlockall", "Unlock all technologies", "research.unlockall", 
                args => UnlockAllTechnologies());
            
            RegisterCommand("research.reset", "Reset all research progress", "research.reset", 
                args => ResetResearch(), new Type[0], true);
            
            // Time commands
            RegisterCommand("time.skip", "Skip forward in time", "time.skip <hours>", 
                args => SkipTime(args), new Type[] { typeof(float) });
            
            RegisterCommand("time.skipdays", "Skip forward by days", "time.skipdays <days>", 
                args => SkipDays(args), new Type[] { typeof(float) });
            
            RegisterCommand("time.scale", "Set time scale", "time.scale <multiplier>", 
                args => SetTimeScale(args), new Type[] { typeof(float) });
            
            RegisterCommand("time.reset", "Reset time scale to 1x", "time.reset", 
                args => ResetTimeScale());
            
            // Event commands
            RegisterCommand("event.trigger", "Trigger a specific event", "event.trigger <eventId>", 
                args => TriggerEvent(args), new Type[] { typeof(string) });
            
            RegisterCommand("event.random", "Trigger a random event", "event.random", 
                args => TriggerRandomEvent());
            
            RegisterCommand("event.clear", "Clear all active event effects", "event.clear", 
                args => ClearEventEffects());
            
            // Contract commands
            RegisterCommand("contract.generate", "Generate test contracts", "contract.generate <count>", 
                args => GenerateContracts(args), new Type[] { typeof(int) });
            
            RegisterCommand("contract.completeall", "Complete all active contracts", "contract.completeall", 
                args => CompleteAllContracts());
            
            RegisterCommand("contract.winall", "Win all pending bids", "contract.winall", 
                args => WinAllBids());
            
            // Save commands
            RegisterCommand("save.create", "Create test save", "save.create", 
                args => CreateTestSave());
            
            RegisterCommand("save.delete", "Delete save file", "save.delete", 
                args => DeleteSave(), new Type[0], true);
            
            RegisterCommand("save.export", "Export save to clipboard", "save.export", 
                args => ExportSave());
            
            // System commands
            RegisterCommand("system.fps", "Show current FPS", "system.fps", 
                args => GetFPS());
            
            RegisterCommand("system.memory", "Show memory usage", "system.memory", 
                args => GetMemory());
            
            RegisterCommand("system.info", "Show system information", "system.info", 
                args => GetSystemInfo());
            
            // Debug commands
            RegisterCommand("debug.help", "Show available commands", "debug.help", 
                args => ShowHelp());
            
            RegisterCommand("debug.log", "Add message to debug log", "debug.log <message>", 
                args => AddLog(args), new Type[] { typeof(string) });
            
            RegisterCommand("debug.clear", "Clear debug log", "debug.clear", 
                args => ClearLog());
            
            RegisterCommand("debug.reset", "Reset all progress", "debug.reset", 
                args => ResetAllProgress(), new Type[0], true);
        }
        #endregion

        #region Command Execution
        /// <summary>
        /// Execute a command from string input
        /// </summary>
        public string ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "Error: Empty command";
            }
            
            // Parse command and arguments
            string[] parts = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "Error: Empty command";
            }
            
            string commandName = parts[0].ToLower();
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, parts.Length - 1);
            
            // Add to history
            _commandHistory.Add(input);
            while (_commandHistory.Count > MAX_HISTORY)
            {
                _commandHistory.RemoveAt(0);
            }
            
            // Find and execute command
            if (!_commands.TryGetValue(commandName, out DebugCommandInfo command))
            {
                OnCommandError?.Invoke($"Unknown command: {commandName}");
                return $"Error: Unknown command '{commandName}'. Type 'debug.help' for available commands.";
            }
            
            try
            {
                // Parse arguments
                object[] parsedArgs = ParseArguments(args, command.parameterTypes);
                
                // Execute
                object result = command.execute(parsedArgs);
                
                string resultStr = result?.ToString() ?? "OK";
                OnCommandExecuted?.Invoke($"{commandName}: {resultStr}");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"Command executed: {commandName}");
                }
                
                return resultStr;
            }
            catch (Exception e)
            {
                OnCommandError?.Invoke($"{commandName}: {e.Message}");
                return $"Error executing '{commandName}': {e.Message}";
            }
        }

        private object[] ParseArguments(string[] args, Type[] expectedTypes)
        {
            if (expectedTypes == null || expectedTypes.Length == 0)
            {
                return new object[0];
            }
            
            if (args.Length < expectedTypes.Length)
            {
                throw new ArgumentException($"Expected {expectedTypes.Length} arguments, got {args.Length}");
            }
            
            object[] result = new object[expectedTypes.Length];
            
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                result[i] = ConvertArgument(args[i], expectedTypes[i]);
            }
            
            return result;
        }

        private object ConvertArgument(string arg, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return arg;
            }
            else if (targetType == typeof(int))
            {
                if (int.TryParse(arg, out int result))
                {
                    return result;
                }
                throw new ArgumentException($"Cannot parse '{arg}' as integer");
            }
            else if (targetType == typeof(float))
            {
                if (float.TryParse(arg, out float result))
                {
                    return result;
                }
                throw new ArgumentException($"Cannot parse '{arg}' as float");
            }
            else if (targetType == typeof(bool))
            {
                if (bool.TryParse(arg, out bool result))
                {
                    return result;
                }
                throw new ArgumentException($"Cannot parse '{arg}' as boolean");
            }
            
            throw new ArgumentException($"Unsupported parameter type: {targetType.Name}");
        }
        #endregion

        #region Command Implementations
        // Money commands
        private object AddMoney(object[] args)
        {
            float amount = (float)args[0];
            DebugManager.Instance?.AddMoney(amount);
            return $"Added {amount:C} money";
        }

        private object SetMoney(object[] args)
        {
            float amount = (float)args[0];
            DebugManager.Instance?.SetMoney(amount);
            return $"Set money to {amount:C}";
        }

        private object GetMoney()
        {
            // Would get from GameManager
            return "Current money: N/A";
        }

        // Reputation commands
        private object AddReputation(object[] args)
        {
            float amount = (float)args[0];
            DebugManager.Instance?.AddReputation(amount);
            return $"Added {amount} reputation";
        }

        private object SetReputation(object[] args)
        {
            float amount = (float)args[0];
            DebugManager.Instance?.SetReputation(amount);
            return $"Set reputation to {amount}";
        }

        // Research commands
        private object CompleteResearch(object[] args)
        {
            string researchId = (string)args[0];
            ResearchDebugger.Instance?.CompleteResearch(researchId);
            return $"Completed research: {researchId}";
        }

        private object CompleteAllResearch()
        {
            ResearchDebugger.Instance?.CompleteAllResearch();
            return "All research completed";
        }

        private object UnlockAllTechnologies()
        {
            ResearchDebugger.Instance?.UnlockAllTechnologies();
            return "All technologies unlocked";
        }

        private object ResetResearch()
        {
            ResearchDebugger.Instance?.ResetResearchProgress();
            return "Research progress reset";
        }

        // Time commands
        private object SkipTime(object[] args)
        {
            float hours = (float)args[0];
            TimeManipulator.Instance?.SkipHours(hours);
            return $"Skipped {hours} hours";
        }

        private object SkipDays(object[] args)
        {
            float days = (float)args[0];
            TimeManipulator.Instance?.SkipDays(days);
            return $"Skipped {days} days";
        }

        private object SetTimeScale(object[] args)
        {
            float scale = (float)args[0];
            TimeManipulator.Instance?.SetTimeScale(scale);
            return $"Time scale set to {scale}x";
        }

        private object ResetTimeScale()
        {
            TimeManipulator.Instance?.ResetTimeScale();
            return "Time scale reset to 1x";
        }

        // Event commands
        private object TriggerEvent(object[] args)
        {
            string eventId = (string)args[0];
            EventDebugger.Instance?.TriggerEvent(eventId);
            return $"Triggered event: {eventId}";
        }

        private object TriggerRandomEvent()
        {
            EventDebugger.Instance?.TriggerRandomEvent();
            return "Triggered random event";
        }

        private object ClearEventEffects()
        {
            EventDebugger.Instance?.ClearActiveEffects();
            return "All event effects cleared";
        }

        // Contract commands
        private object GenerateContracts(object[] args)
        {
            int count = (int)args[0];
            ContractDebugger.Instance?.GenerateTestContracts(count);
            return $"Generated {count} test contracts";
        }

        private object CompleteAllContracts()
        {
            ContractDebugger.Instance?.CompleteAllContracts();
            return "All contracts completed";
        }

        private object WinAllBids()
        {
            ContractDebugger.Instance?.WinAllBids();
            return "Won all pending bids";
        }

        // Save commands
        private object CreateTestSave()
        {
            SaveDebugger.Instance?.CreateTestSave();
            return "Test save created";
        }

        private object DeleteSave()
        {
            SaveDebugger.Instance?.DeleteSave();
            return "Save deleted";
        }

        private object ExportSave()
        {
            SaveDebugger.Instance?.ExportSaveToClipboard();
            return "Save exported to clipboard";
        }

        // System commands
        private object GetFPS()
        {
            float fps = PerformanceMonitor.Instance?.GetCurrentFPS() ?? 0f;
            return $"Current FPS: {fps:F1}";
        }

        private object GetMemory()
        {
            long memory = PerformanceMonitor.Instance?.GetCurrentMemory() ?? 0;
            return $"Memory usage: {memory} MB";
        }

        private object GetSystemInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("System Information:");
            sb.AppendLine($"  Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"  OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"  CPU: {SystemInfo.processorType}");
            sb.AppendLine($"  Cores: {SystemInfo.processorCount}");
            sb.AppendLine($"  RAM: {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"  GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"  VRAM: {SystemInfo.graphicsMemorySize} MB");
            sb.AppendLine($"  Resolution: {Screen.width}x{Screen.height}");
            sb.AppendLine($"  DPI: {Screen.dpi:F0}");
            return sb.ToString();
        }

        // Debug commands
        private object ShowHelp()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== AVAILABLE COMMANDS ===");
            sb.AppendLine();
            
            foreach (var kvp in _commands.OrderBy(x => x.Key))
            {
                sb.AppendLine($"{kvp.Key}");
                sb.AppendLine($"  {kvp.Value.description}");
                sb.AppendLine($"  Usage: {kvp.Value.usage}");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private object AddLog(object[] args)
        {
            string message = (string)args[0];
            DebugManager.Instance?.LogDebugAction(message);
            return "Log entry added";
        }

        private object ClearLog()
        {
            DebugManager.Instance?.ClearDebugLog();
            return "Debug log cleared";
        }

        private object ResetAllProgress()
        {
            DebugManager.Instance?.ResetAllProgress();
            return "All progress reset";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of available commands
        /// </summary>
        public List<string> GetAvailableCommands()
        {
            return new List<string>(_commands.Keys);
        }

        /// <summary>
        /// Get command info
        /// </summary>
        public DebugCommandInfo GetCommandInfo(string commandName)
        {
            _commands.TryGetValue(commandName.ToLower(), out DebugCommandInfo info);
            return info;
        }

        /// <summary>
        /// Get command history
        /// </summary>
        public List<string> GetCommandHistory()
        {
            return new List<string>(_commandHistory);
        }

        /// <summary>
        /// Clear command history
        /// </summary>
        public void ClearCommandHistory()
        {
            _commandHistory.Clear();
        }
        #endregion
    }

    /// <summary>
    /// Attribute for marking debug command methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugCommandAttribute : Attribute
    {
        public string CommandName { get; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public bool RequiresConfirmation { get; set; }

        public DebugCommandAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
