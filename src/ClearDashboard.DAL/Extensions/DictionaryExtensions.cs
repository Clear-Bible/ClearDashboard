using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClearDashboard.DAL.Extensions
{
    public static class DictionaryExtensions
    {
        public static T? DeserializeValue<T>(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var element))
            {
                if (element is JsonElement)
                {
                    var jsonElement = (JsonElement)dictionary[key];
                    return jsonElement.Deserialize<T>();
                }

                throw new InvalidCastException($"Element at key {key} is not a JsonElement and thus cannot be deserialized");
            }

            throw new KeyNotFoundException($"Key {key} not found");
        }
    }
} 
