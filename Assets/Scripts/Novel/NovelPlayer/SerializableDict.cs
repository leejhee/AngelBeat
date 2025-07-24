using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[Serializable]
public class SerializableDict<TKey, TValue>
{
    public List<SerializableKeyValuePair<TKey, TValue>> pairs = new();
    public List<TKey> keys = new();
    public List<TValue> values = new();
    public void Add(TKey key, TValue value)
    {
        pairs.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
        keys.Add(key);
        values.Add(value);
    }
    public TValue GetValue(TKey key)
    {
        foreach (var pair in pairs)
        {
            if (EqualityComparer<TKey>.Default.Equals(pair.key, key))
                return pair.value;
        }
        return default;
    }
    public bool ContainsKey(TKey key)
    {
        foreach (var pair in pairs)
        {
            if (EqualityComparer<TKey>.Default.Equals(pair.key, key))
                return true;
        }
        return false;
    }
    public void Clear()
    {
        pairs.Clear();
    }
}

[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}