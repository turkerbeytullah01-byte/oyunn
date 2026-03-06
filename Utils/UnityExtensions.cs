// ============================================================================
// Project Aegis: Drone Dominion
// UnityExtensions - Extension methods for Unity types
// ============================================================================
// Provides useful extension methods for common Unity operations.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Utils
{
    /// <summary>
    /// Extension methods for Unity types.
    /// </summary>
    public static class UnityExtensions
    {
        #region Transform Extensions
        
        /// <summary>
        /// Resets the transform to identity.
        /// </summary>
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Destroys all children of this transform.
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Gets or adds a component to the GameObject.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        
        /// <summary>
        /// Gets or adds a component to the Transform's GameObject.
        /// </summary>
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.GetOrAddComponent<T>();
        }
        
        #endregion
        
        #region GameObject Extensions
        
        /// <summary>
        /// Sets the layer recursively for all children.
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
        
        /// <summary>
        /// Checks if the GameObject has a specific component.
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }
        
        #endregion
        
        #region Vector Extensions
        
        /// <summary>
        /// Returns a vector with only X and Y components.
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        /// <summary>
        /// Returns a vector with X and Y components, Z set to 0.
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y, 0);
        }
        
        /// <summary>
        /// Returns a vector with a specific Z value.
        /// </summary>
        public static Vector3 WithZ(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        /// <summary>
        /// Returns a vector with a specific X value.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }
        
        /// <summary>
        /// Returns a vector with a specific Y value.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }
        
        /// <summary>
        /// Returns a vector with a specific Z value.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        #endregion
        
        #region Color Extensions
        
        /// <summary>
        /// Returns a color with a specific alpha value.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        /// <summary>
        /// Returns a hex string representation of the color.
        /// </summary>
        public static string ToHex(this Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }
        
        #endregion
        
        #region RectTransform Extensions
        
        /// <summary>
        /// Sets the anchor position while preserving the current position.
        /// </summary>
        public static void SetAnchor(this RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
        
        /// <summary>
        /// Sets all anchors to the same value.
        /// </summary>
        public static void SetAnchor(this RectTransform rectTransform, Vector2 anchor)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
        }
        
        /// <summary>
        /// Sets the size delta while preserving the current position.
        /// </summary>
        public static void SetSize(this RectTransform rectTransform, Vector2 size)
        {
            rectTransform.sizeDelta = size;
        }
        
        /// <summary>
        /// Sets the size delta while preserving the current position.
        /// </summary>
        public static void SetSize(this RectTransform rectTransform, float width, float height)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        
        #endregion
        
        #region String Extensions
        
        /// <summary>
        /// Formats a number with appropriate suffix (K, M, B, T).
        /// </summary>
        public static string FormatNumber(this long number)
        {
            if (number >= 1_000_000_000_000)
                return $"{number / 1_000_000_000_000f:F2}T";
            if (number >= 1_000_000_000)
                return $"{number / 1_000_000_000f:F2}B";
            if (number >= 1_000_000)
                return $"{number / 1_000_000f:F2}M";
            if (number >= 1_000)
                return $"{number / 1_000f:F2}K";
            return number.ToString();
        }
        
        /// <summary>
        /// Formats a number with appropriate suffix (K, M, B, T).
        /// </summary>
        public static string FormatNumber(this int number)
        {
            return FormatNumber((long)number);
        }
        
        /// <summary>
        /// Truncates a string to a maximum length with ellipsis.
        /// </summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + suffix;
        }
        
        #endregion
        
        #region List Extensions
        
        /// <summary>
        /// Shuffles a list in place using Fisher-Yates algorithm.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        #endregion
    }
}
