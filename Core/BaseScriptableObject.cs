// ============================================================================
// Project Aegis: Drone Dominion
// BaseScriptableObject - Foundation for all game data assets
// ============================================================================
// Provides a base class for all ScriptableObjects with automatic ID generation,
// validation, and common functionality for game data assets.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Abstract base class for all game ScriptableObjects.
    /// Provides ID management, validation, and common data functionality.
    /// </summary>
    public abstract class BaseScriptableObject : ScriptableObject, IIdentifiable, IDisplayable
    {
        #region Identification
        
        /// <summary>
        /// Unique identifier for this asset.
        /// Automatically generated if empty.
        /// </summary>
        [SerializeField, Tooltip("Unique identifier. Auto-generated if left empty.")]
        private string _id;
        
        /// <summary>
        /// Gets the unique identifier for this asset.
        /// </summary>
        public string Id 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(_id))
                {
                    GenerateId();
                }
                return _id; 
            }
            protected set => _id = value;
        }
        
        /// <summary>
        /// Generates a new unique ID for this asset.
        /// </summary>
        protected virtual void GenerateId()
        {
            _id = Guid.NewGuid().ToString("N").Substring(0, 16);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        #endregion
        
        #region Display Information
        
        /// <summary>
        /// Display name shown in UI.
        /// </summary>
        [SerializeField, Tooltip("Display name shown in UI")]
        private string _displayName;
        
        /// <summary>
        /// Gets the display name for UI.
        /// </summary>
        public virtual string DisplayName 
        { 
            get => string.IsNullOrEmpty(_displayName) ? name : _displayName;
            protected set => _displayName = value;
        }
        
        /// <summary>
        /// Description shown in tooltips and info panels.
        /// </summary>
        [SerializeField, TextArea(3, 6), Tooltip("Description shown in tooltips")]
        private string _description;
        
        /// <summary>
        /// Gets the description for UI.
        /// </summary>
        public virtual string Description 
        { 
            get => _description ?? string.Empty;
            protected set => _description = value;
        }
        
        /// <summary>
        /// Icon sprite for UI representation.
        /// </summary>
        [SerializeField, Tooltip("Icon sprite for UI")]
        private Sprite _icon;
        
        /// <summary>
        /// Gets the icon sprite for UI.
        /// </summary>
        public virtual Sprite Icon 
        { 
            get => _icon;
            protected set => _icon = value;
        }
        
        #endregion
        
        #region Metadata
        
        /// <summary>
        /// Category for organization and filtering.
        /// </summary>
        [SerializeField, Tooltip("Category for organization")]
        private string _category;
        
        /// <summary>
        /// Gets the category of this asset.
        /// </summary>
        public string Category => _category ?? "Uncategorized";
        
        /// <summary>
        /// Tags for filtering and searching.
        /// </summary>
        [SerializeField, Tooltip("Tags for filtering")]
        private string[] _tags = Array.Empty<string>();
        
        /// <summary>
        /// Gets the tags associated with this asset.
        /// </summary>
        public IReadOnlyList<string> Tags => _tags;
        
        /// <summary>
        /// Creation timestamp for tracking.
        /// </summary>
        [SerializeField, HideInInspector]
        private long _creationTimestamp;
        
        /// <summary>
        /// Gets the creation timestamp.
        /// </summary>
        public DateTime CreationDate => DateTime.FromFileTimeUtc(_creationTimestamp);
        
        /// <summary>
        /// Version number for data migration.
        /// </summary>
        [SerializeField, Tooltip("Version for data migration")]
        private int _version = 1;
        
        /// <summary>
        /// Gets the version of this asset.
        /// </summary>
        public int Version => _version;
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Called when the ScriptableObject is created or loaded.
        /// </summary>
        protected virtual void OnEnable()
        {
            // Generate ID if missing
            if (string.IsNullOrEmpty(_id))
            {
                GenerateId();
            }
            
            // Set creation timestamp if not set
            if (_creationTimestamp == 0)
            {
                _creationTimestamp = DateTime.UtcNow.ToFileTimeUtc();
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
            
            OnValidate();
        }
        
        /// <summary>
        /// Called when values are changed in the inspector.
        /// Override for validation logic.
        /// </summary>
        protected virtual void OnValidate()
        {
            // Ensure ID is set
            if (string.IsNullOrEmpty(_id))
            {
                GenerateId();
            }
            
            // Validate derived class
            Validate();
        }
        
        /// <summary>
        /// Override this to add custom validation logic.
        /// </summary>
        protected virtual void Validate() { }
        
        /// <summary>
        /// Called when the ScriptableObject is reset.
        /// </summary>
        protected virtual void Reset()
        {
            GenerateId();
            _creationTimestamp = DateTime.UtcNow.ToFileTimeUtc();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Checks if this asset has a specific tag.
        /// </summary>
        public bool HasTag(string tag)
        {
            if (_tags == null || string.IsNullOrEmpty(tag))
                return false;
                
            foreach (var t in _tags)
            {
                if (string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a string representation of this asset.
        /// </summary>
        public override string ToString()
        {
            return $"[{GetType().Name}] {DisplayName} (ID: {Id})";
        }
        
        /// <summary>
        /// Creates a runtime copy of this ScriptableObject.
        /// Use this when you need to modify values at runtime.
        /// </summary>
        public virtual BaseScriptableObject CreateRuntimeCopy()
        {
            var copy = Instantiate(this);
            copy._id = Guid.NewGuid().ToString("N").Substring(0, 16);
            copy.name = $"{name}_Runtime";
            return copy;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Generic base class for typed ScriptableObject collections.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public abstract class BaseScriptableObjectCollection<T> : BaseScriptableObject where T : BaseScriptableObject
    {
        /// <summary>
        /// The items in this collection.
        /// </summary>
        [SerializeField]
        private T[] _items = Array.Empty<T>();
        
        /// <summary>
        /// Gets all items in the collection.
        /// </summary>
        public IReadOnlyList<T> Items => _items;
        
        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => _items?.Length ?? 0;
        
        /// <summary>
        /// Gets an item by index.
        /// </summary>
        public T GetItem(int index)
        {
            if (_items == null || index < 0 || index >= _items.Length)
                return null;
            return _items[index];
        }
        
        /// <summary>
        /// Finds an item by ID.
        /// </summary>
        public T FindById(string id)
        {
            if (_items == null || string.IsNullOrEmpty(id))
                return null;
                
            foreach (var item in _items)
            {
                if (item != null && item.Id == id)
                    return item;
            }
            return null;
        }
        
        /// <summary>
        /// Finds items by category.
        /// </summary>
        public T[] FindByCategory(string category)
        {
            if (_items == null || string.IsNullOrEmpty(category))
                return Array.Empty<T>();
                
            var results = new List<T>();
            foreach (var item in _items)
            {
                if (item != null && item.Category == category)
                    results.Add(item);
            }
            return results.ToArray();
        }
        
        /// <summary>
        /// Finds items with a specific tag.
        /// </summary>
        public T[] FindByTag(string tag)
        {
            if (_items == null || string.IsNullOrEmpty(tag))
                return Array.Empty<T>();
                
            var results = new List<T>();
            foreach (var item in _items)
            {
                if (item != null && item.HasTag(tag))
                    results.Add(item);
            }
            return results.ToArray();
        }
        
        protected override void Validate()
        {
            base.Validate();
            
            // Check for duplicate IDs
            if (_items != null)
            {
                var ids = new HashSet<string>();
                foreach (var item in _items)
                {
                    if (item == null) continue;
                    
                    if (!ids.Add(item.Id))
                    {
                        Debug.LogWarning($"[BaseScriptableObjectCollection] Duplicate ID found: {item.Id} in {name}");
                    }
                }
            }
        }
    }
}
