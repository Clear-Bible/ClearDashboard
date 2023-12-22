using System;

namespace ClearDashboard.Collaboration.Merge;

public class MergeCache
{

    public List<(Type EntityType, Guid DatabaseId)> IdsToDenormalize = new();

    private readonly Dictionary<(Type EntityType, string EntityId, string ItemName), Dictionary<string, object?>> _cache = new();
    //private readonly Dictionary<(Type EntityType, (string key1, string key2)), Dictionary<string, object?>> _cache = new();

    public void AddCacheEntry((Type EntityType, string EntityId, string ItemName) key, string name, object? value)
    {
        if (_cache.ContainsKey(key))
        {
            if (_cache[key].ContainsKey(name))
            {
                _cache[key][name] = value;
            }
            else
            {
                _cache[key].Add(name, value);
            }
        }
        else
        {
            _cache.Add(key, new Dictionary<string, object?>() { { name, value } });
        }
    }

    public void AddCacheEntrySet((Type EntityType, string EntityId, string ItemName) key, Dictionary<string, object?> value)
    {
        if (_cache.ContainsKey(key))
        {
            _cache[key] = value;
        }
        else
        {
            _cache.Add(key, value);
        }
    }

    public bool TryLookupCacheEntrySet((Type EntityType, string EntityId, string ItemName) key, out Dictionary<string, object?>? value)
    {
        if (_cache.TryGetValue(key, out value))
        {
            return true;
        }

        value = null;
        return false;
    }

    public bool TryLookupCacheEntry((Type EntityType, string EntityId, string ItemName) key, string name, out object? value)
    {
        value = null;
        if (_cache.TryGetValue(key, out var nvp))
        {
            return _cache[key].TryGetValue(name, out value);
        }

        return false;
    }

    public IEnumerable<string> GetCacheEntrySetEntityIds(Type entityType, string itemName)
    {
        return _cache.Keys
            .Where(e => e.EntityType == entityType)
            .Where(e => e.ItemName == itemName)
            .Select(e => e.EntityId);
    }
    
    public bool ContainsCacheKey((Type EntityType, string EntityId, string ItemName) key)
    {
        return _cache.ContainsKey(key);
    }
}

