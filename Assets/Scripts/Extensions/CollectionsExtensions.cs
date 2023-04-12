using System;
using System.Linq;
using System.Collections.Generic;

static public class CollectionsExtensions
{
    // List
    static public T GetRandom<T>(this List<T> list)
        => list[UnityEngine.Random.Range(0, list.Count)];
    static public K GetRandomKey<K, V>(this Dictionary<K, V> dictionary)
    {
        return dictionary.Select(kv => kv.Key).ToList().GetRandom();
    }

    static public KeyValuePair<K, V> GetRandom<K, V>(this Dictionary<K, V> dictionary)
    {
        K key = dictionary.GetRandomKey();
        return new KeyValuePair<K, V>(key, dictionary[key]);
    }

    // LinkedList
    static public void Enqueue<T>(this LinkedList<T> linkedList, T value)
    {
        linkedList.AddLast(value);
    }

    static public T Dequeue<T>(this LinkedList<T> linkedList)
    {
        T value = linkedList.First.Value;
        linkedList.RemoveFirst();
        return value;
    }

    static public void ReplaceFirstWith<T>(this LinkedList<T> linkedList, T value)
    {
        if (linkedList.First == null) throw new InvalidOperationException("LinkedList is empty.");
        linkedList.First.Value = value;
    }

    static public T Peek<T>(this LinkedList<T> linkedList)
    {
        if (linkedList.First == null) throw new InvalidOperationException("LinkedList is empty.");
        return linkedList.First.Value;
    }

    // Dictionary
    static public V LazyLoad<K, V>(this Dictionary<K, V> dictionary, K key, Func<K, V> loader)
    {
        V value;

        if (!dictionary.TryGetValue(key, out value))
        {
            value = loader(key);
            dictionary.Add(key, value);
        }

        return value;
    }

    static public Stack<T> ToStack<T>(this IEnumerable<T> collection) => new Stack<T>(collection);

    static public bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        foreach (T elem in collection)
        {
            if (predicate(elem)) return true;
        }
        return false;
    }

    static public Dictionary<K, V> Shuffle<K, V>(this Dictionary<K, V> dictionary)
    {
        return dictionary.OrderBy(elem => Guid.NewGuid()).ToDictionary(x => x.Key, x => x.Value);
    }
}
