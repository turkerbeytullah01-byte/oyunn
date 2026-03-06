using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Helper class for handling Unity serialization issues and custom conversions
    /// </summary>
    public static class SerializationHelper
    {
        #region DateTime Serialization
        
        private const string DATETIME_FORMAT = "O"; // ISO 8601 format
        
        /// <summary>
        /// Converts DateTime to string for JSON serialization
        /// </summary>
        public static string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString(DATETIME_FORMAT, CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// Parses DateTime from string
        /// </summary>
        public static DateTime StringToDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return DateTime.MinValue;
                
            if (DateTime.TryParseExact(dateTimeString, DATETIME_FORMAT, 
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result;
            }
            
            // Fallback to standard parsing
            if (DateTime.TryParse(dateTimeString, out result))
            {
                return result;
            }
            
            return DateTime.MinValue;
        }
        
        #endregion
        
        #region Dictionary Serialization
        
        /// <summary>
        /// Serializable key-value pair for dictionary conversion
        /// </summary>
        [Serializable]
        public class SerializableKeyValuePair<TKey, TValue>
        {
            public TKey key;
            public TValue value;
            
            public SerializableKeyValuePair() { }
            
            public SerializableKeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }
        
        /// <summary>
        /// Converts Dictionary to serializable list
        /// </summary>
        public static List<SerializableKeyValuePair<TKey, TValue>> DictionaryToList<TKey, TValue>(
            Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return new List<SerializableKeyValuePair<TKey, TValue>>();
                
            return dictionary.Select(kvp => new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value)).ToList();
        }
        
        /// <summary>
        /// Converts serializable list back to Dictionary
        /// </summary>
        public static Dictionary<TKey, TValue> ListToDictionary<TKey, TValue>(
            List<SerializableKeyValuePair<TKey, TValue>> list)
        {
            if (list == null)
                return new Dictionary<TKey, TValue>();
                
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var kvp in list)
            {
                if (kvp.key != null && !dictionary.ContainsKey(kvp.key))
                {
                    dictionary[kvp.key] = kvp.value;
                }
            }
            return dictionary;
        }
        
        #endregion
        
        #region Vector Serialization
        
        /// <summary>
        /// Serializable Vector3 wrapper
        /// </summary>
        [Serializable]
        public struct SerializableVector3
        {
            public float x;
            public float y;
            public float z;
            
            public SerializableVector3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            
            public SerializableVector3(Vector3 vector)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }
            
            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
            
            public static implicit operator Vector3(SerializableVector3 sv)
            {
                return sv.ToVector3();
            }
            
            public static implicit operator SerializableVector3(Vector3 v)
            {
                return new SerializableVector3(v);
            }
        }
        
        /// <summary>
        /// Serializable Quaternion wrapper
        /// </summary>
        [Serializable]
        public struct SerializableQuaternion
        {
            public float x;
            public float y;
            public float z;
            public float w;
            
            public SerializableQuaternion(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }
            
            public SerializableQuaternion(Quaternion quaternion)
            {
                x = quaternion.x;
                y = quaternion.y;
                z = quaternion.z;
                w = quaternion.w;
            }
            
            public Quaternion ToQuaternion()
            {
                return new Quaternion(x, y, z, w);
            }
            
            public static implicit operator Quaternion(SerializableQuaternion sq)
            {
                return sq.ToQuaternion();
            }
            
            public static implicit operator SerializableQuaternion(Quaternion q)
            {
                return new SerializableQuaternion(q);
            }
        }
        
        /// <summary>
        /// Serializable Color wrapper
        /// </summary>
        [Serializable]
        public struct SerializableColor
        {
            public float r;
            public float g;
            public float b;
            public float a;
            
            public SerializableColor(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
            
            public SerializableColor(Color color)
            {
                r = color.r;
                g = color.g;
                b = color.b;
                a = color.a;
            }
            
            public Color ToColor()
            {
                return new Color(r, g, b, a);
            }
            
            public static implicit operator Color(SerializableColor sc)
            {
                return sc.ToColor();
            }
            
            public static implicit operator SerializableColor(Color c)
            {
                return new SerializableColor(c);
            }
        }
        
        #endregion
        
        #region JSON Serialization
        
        /// <summary>
        /// Serializes object to JSON string with proper formatting
        /// </summary>
        public static string ToJson<T>(T obj, bool prettyPrint = true)
        {
            if (obj == null)
                return "{}";
                
            try
            {
                return JsonUtility.ToJson(obj, prettyPrint);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SerializationHelper] JSON serialization failed: {ex.Message}");
                return "{}";
            }
        }
        
        /// <summary>
        /// Deserializes JSON string to object
        /// </summary>
        public static T FromJson<T>(string json) where T : new()
        {
            if (string.IsNullOrEmpty(json))
                return new T();
                
            try
            {
                return JsonUtility.FromJson<T>(json) ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SerializationHelper] JSON deserialization failed: {ex.Message}");
                return new T();
            }
        }
        
        /// <summary>
        /// Deserializes JSON string to object with type parameter
        /// </summary>
        public static object FromJson(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
                return null;
                
            try
            {
                return JsonUtility.FromJson(json, type);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SerializationHelper] JSON deserialization failed: {ex.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region Integrity Check
        
        /// <summary>
        /// Generates SHA256 hash for data integrity verification
        /// </summary>
        public static string GenerateHash(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;
                
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = sha256.ComputeHash(bytes);
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                
                return builder.ToString();
            }
        }
        
        /// <summary>
        /// Verifies data integrity using hash
        /// </summary>
        public static bool VerifyIntegrity(string data, string expectedHash)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(expectedHash))
                return false;
                
            string actualHash = GenerateHash(data);
            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        
        #endregion
        
        #region Compression
        
        /// <summary>
        /// Compresses string using GZip (if System.IO.Compression is available)
        /// </summary>
        public static string CompressString(string text)
        {
            // Note: In Unity, you may need to add reference to System.IO.Compression
            // For now, return uncompressed
            return text;
        }
        
        /// <summary>
        /// Decompresses GZip compressed string
        /// </summary>
        public static string DecompressString(string compressedText)
        {
            // Note: In Unity, you may need to add reference to System.IO.Compression
            // For now, return as-is
            return compressedText;
        }
        
        #endregion
        
        #region Type Conversion Helpers
        
        /// <summary>
        /// Safely converts float to int with clamping
        /// </summary>
        public static int SafeFloatToInt(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0;
                
            if (value > int.MaxValue)
                return int.MaxValue;
            if (value < int.MinValue)
                return int.MinValue;
                
            return Mathf.RoundToInt(value);
        }
        
        /// <summary>
        /// Safely clamps float value
        /// </summary>
        public static float SafeClamp(float value, float min, float max)
        {
            if (float.IsNaN(value))
                return min;
            if (float.IsInfinity(value))
                return value > 0 ? max : min;
                
            return Mathf.Clamp(value, min, max);
        }
        
        /// <summary>
        /// Validates and cleans string for serialization
        /// </summary>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            // Remove control characters that could break JSON
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (!char.IsControl(c) || c == '\t' || c == '\n' || c == '\r')
                {
                    sb.Append(c);
                }
            }
            
            return sb.ToString().Trim();
        }
        
        #endregion
    }
}
