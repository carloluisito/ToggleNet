using System.Collections.Generic;

namespace ToggleNet.Core.Entities
{
    /// <summary>
    /// Represents user context for targeting rule evaluation
    /// </summary>
    public class UserContext
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// User attributes for targeting evaluation
        /// Key-value pairs where key is the attribute name and value is the attribute value
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; } = new();
        
        /// <summary>
        /// Helper method to get an attribute value with type conversion
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="key">The attribute key</param>
        /// <returns>The converted value or default(T) if not found or conversion fails</returns>
        public T? GetAttribute<T>(string key)
        {
            if (!Attributes.TryGetValue(key, out var value))
                return default(T);
                
            try
            {
                if (value is T directValue)
                    return directValue;
                    
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
        
        /// <summary>
        /// Helper method to get an attribute value as string
        /// </summary>
        /// <param name="key">The attribute key</param>
        /// <returns>The attribute value as string or null if not found</returns>
        public string? GetAttributeAsString(string key)
        {
            return Attributes.TryGetValue(key, out var value) ? value?.ToString() : null;
        }
        
        /// <summary>
        /// Helper method to check if an attribute exists
        /// </summary>
        /// <param name="key">The attribute key</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
        public bool HasAttribute(string key)
        {
            return Attributes.ContainsKey(key);
        }
    }
}
