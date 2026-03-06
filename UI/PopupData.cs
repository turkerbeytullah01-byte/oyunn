using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Represents a choice option in a popup
    /// </summary>
    [Serializable]
    public class PopupChoice
    {
        /// <summary>
        /// Display label for the choice button
        /// </summary>
        public string label;
        
        /// <summary>
        /// Description shown as tooltip
        /// </summary>
        public string description;
        
        /// <summary>
        /// Action to execute when selected
        /// </summary>
        public Action onSelected;
        
        /// <summary>
        /// Button color override (optional)
        /// </summary>
        public Color buttonColor;
        
        /// <summary>
        /// Icon to display on the button
        /// </summary>
        public Sprite icon;
        
        /// <summary>
        /// Whether this choice is available/interactable
        /// </summary>
        public bool isAvailable = true;
        
        /// <summary>
        /// Reason why unavailable (shown as tooltip)
        /// </summary>
        public string unavailableReason;
        
        /// <summary>
        /// Cost associated with this choice
        /// </summary>
        public Dictionary<string, float> costs;
        
        /// <summary>
        /// Rewards from selecting this choice
        /// </summary>
        public Dictionary<string, float> rewards;
        
        public PopupChoice()
        {
            buttonColor = Color.white;
            costs = new Dictionary<string, float>();
            rewards = new Dictionary<string, float>();
        }
        
        public PopupChoice(string label, Action onSelected, Color? buttonColor = null)
        {
            this.label = label;
            this.onSelected = onSelected;
            this.buttonColor = buttonColor ?? Color.white;
            this.costs = new Dictionary<string, float>();
            this.rewards = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// Data structure for popup configuration
    /// </summary>
    [Serializable]
    public class PopupData
    {
        /// <summary>
        /// Popup title
        /// </summary>
        public string title;
        
        /// <summary>
        /// Main description text
        /// </summary>
        public string description;
        
        /// <summary>
        /// Additional details (shown in expandable section)
        /// </summary>
        public string details;
        
        /// <summary>
        /// Icon to display
        /// </summary>
        public Sprite icon;
        
        /// <summary>
        /// Background image
        /// </summary>
        public Sprite background;
        
        /// <summary>
        /// Available choices for the player
        /// </summary>
        public List<PopupChoice> choices;
        
        /// <summary>
        /// Whether the popup can be dismissed without choosing
        /// </summary>
        public bool canDismiss = true;
        
        /// <summary>
        /// Dismiss button text
        /// </summary>
        public string dismissText = "Close";
        
        /// <summary>
        /// Whether to show a countdown timer
        /// </summary>
        public bool showTimer;
        
        /// <summary>
        /// Time limit for making a choice (seconds)
        /// </summary>
        public float timeLimit;
        
        /// <summary>
        /// Action to call when time expires
        /// </summary>
        public Action onTimeExpired;
        
        /// <summary>
        /// Default choice index when time expires (-1 for none)
        /// </summary>
        public int defaultChoiceOnTimeout = -1;
        
        /// <summary>
        /// Popup type for styling
        /// </summary>
        public PopupType type = PopupType.Default;
        
        /// <summary>
        /// Priority level (higher = more important)
        /// </summary>
        public int priority;
        
        /// <summary>
        /// Whether this popup pauses the game
        /// </summary>
        public bool pauseGame;
        
        /// <summary>
        /// Sound effect to play on show
        /// </summary>
        public AudioClip showSound;
        
        /// <summary>
        /// Unique identifier for this popup
        /// </summary>
        public string popupId;
        
        public PopupData()
        {
            choices = new List<PopupChoice>();
            popupId = System.Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// Creates a simple info popup
        /// </summary>
        public static PopupData CreateInfo(string title, string message, string dismissText = "OK")
        {
            return new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                dismissText = dismissText,
                type = PopupType.Info
            };
        }
        
        /// <summary>
        /// Creates a warning popup
        /// </summary>
        public static PopupData CreateWarning(string title, string message, Action onAcknowledge = null)
        {
            var data = new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                dismissText = "Acknowledge",
                type = PopupType.Warning
            };
            
            if (onAcknowledge != null)
            {
                data.choices.Add(new PopupChoice("Acknowledge", onAcknowledge));
            }
            
            return data;
        }
        
        /// <summary>
        /// Creates a confirmation popup
        /// </summary>
        public static PopupData CreateConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            return new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                type = PopupType.Confirm,
                choices = new List<PopupChoice>
                {
                    new PopupChoice("Confirm", onConfirm, Color.green),
                    new PopupChoice("Cancel", onCancel, Color.gray)
                }
            };
        }
        
        /// <summary>
        /// Creates a timed decision popup
        /// </summary>
        public static PopupData CreateTimedDecision(string title, string message, float timeLimit, 
            List<PopupChoice> choices, int defaultChoice = -1)
        {
            return new PopupData
            {
                title = title,
                description = message,
                showTimer = true,
                timeLimit = timeLimit,
                choices = choices,
                defaultChoiceOnTimeout = defaultChoice,
                canDismiss = false,
                type = PopupType.Timed
            };
        }
    }
    
    /// <summary>
    /// Popup type enumeration for styling
    /// </summary>
    public enum PopupType
    {
        Default,
        Info,
        Warning,
        Error,
        Confirm,
        Event,
        Timed,
        Achievement,
        Research,
        Contract
    }
    
    /// <summary>
    /// Game event data for event popups
    /// </summary>
    [Serializable]
    public class GameEventData
    {
        public string eventId;
        public string eventName;
        public string description;
        public string detailedDescription;
        public Sprite eventImage;
        public AudioClip eventSound;
        public List<PopupChoice> choices;
        public float probability;
        public bool isRepeatable;
        public int minReputationRequired;
        public int maxReputationAllowed;
        
        public GameEventData()
        {
            choices = new List<PopupChoice>();
            eventId = System.Guid.NewGuid().ToString();
        }
    }
}
