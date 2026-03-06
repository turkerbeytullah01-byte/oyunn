// ============================================================================
// Project Aegis: Drone Dominion
// GameLogger - Centralized logging utility
// ============================================================================
// Provides categorized logging with context information.
// Supports log levels and conditional compilation for builds.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Utils
{
    /// <summary>
    /// Log levels for categorized logging.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Centralized logging utility for the game.
    /// </summary>
    public static class GameLogger
    {
        #region Configuration
        
        /// <summary>
        /// Minimum log level to display.
        /// </summary>
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
        
        /// <summary>
        /// Whether to include timestamps in logs.
        /// </summary>
        public static bool IncludeTimestamp { get; set; } = true;
        
        /// <summary>
        /// Whether to include category in logs.
        /// </summary>
        public static bool IncludeCategory { get; set; } = true;
        
        /// <summary>
        /// Categories to suppress.
        /// </summary>
        private static HashSet<string> _suppressedCategories = new HashSet<string>();
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        public static void Debug(string message, string category = "")
        {
            Log(LogLevel.Debug, message, category);
        }
        
        /// <summary>
        /// Logs an info message.
        /// </summary>
        public static void Info(string message, string category = "")
        {
            Log(LogLevel.Info, message, category);
        }
        
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warning(string message, string category = "")
        {
            Log(LogLevel.Warning, message, category);
        }
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void Error(string message, string category = "")
        {
            Log(LogLevel.Error, message, category);
        }
        
        /// <summary>
        /// Logs a critical error message.
        /// </summary>
        public static void Critical(string message, string category = "")
        {
            Log(LogLevel.Critical, message, category);
        }
        
        /// <summary>
        /// Logs a message with the specified level.
        /// </summary>
        public static void Log(LogLevel level, string message, string category = "")
        {
            #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (level < LogLevel.Warning)
                return;
            #endif
            
            if (level < MinimumLogLevel)
                return;
            
            if (!string.IsNullOrEmpty(category) && _suppressedCategories.Contains(category))
                return;
            
            string formattedMessage = FormatMessage(level, message, category);
            
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }
        
        #endregion
        
        #region Category Management
        
        /// <summary>
        /// Suppresses logs from a specific category.
        /// </summary>
        public static void SuppressCategory(string category)
        {
            _suppressedCategories.Add(category);
        }
        
        /// <summary>
        /// Unsuppresses logs from a specific category.
        /// </summary>
        public static void UnsuppressCategory(string category)
        {
            _suppressedCategories.Remove(category);
        }
        
        #endregion
        
        #region Formatting
        
        private static string FormatMessage(LogLevel level, string message, string category)
        {
            var parts = new List<string>();
            
            if (IncludeTimestamp)
                parts.Add($"[{DateTime.Now:HH:mm:ss.fff}]");
            
            parts.Add($"[{level}]");
            
            if (IncludeCategory && !string.IsNullOrEmpty(category))
                parts.Add($"[{category}]");
            
            parts.Add(message);
            
            return string.Join(" ", parts);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Attribute to mark methods that should be logged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LogMethodAttribute : Attribute
    {
        public LogLevel Level { get; }
        public string Category { get; }
        
        public LogMethodAttribute(LogLevel level = LogLevel.Debug, string category = "")
        {
            Level = level;
            Category = category;
        }
    }
}
