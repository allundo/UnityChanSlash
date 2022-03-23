using System;
using System.Collections.Generic;

static public class CollectionsExtensions
{
    // List
    static public T GetRandom<T>(this List<T> list)
        => list[UnityEngine.Random.Range(0, list.Count)];

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
}
