using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

static public class ForEachExtensions
{
    static public void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (T elem in sequence) action(elem);
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Action<T> action, T exceptFor)
    {
        Filter(sequence, exceptFor).ForEach(action);
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Action<T> action, T exceptFor, params T[] filters)
    {
        sequence = Filter(sequence, exceptFor);

        foreach (T e in filters)
        {
            sequence = Filter(sequence, e);
        }

        sequence.ForEach(action);
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func)
    {
        foreach (T elem in sequence) if (!func(elem)) break;
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func, T exceptFor)
    {
        Filter(sequence, exceptFor).ForEach(func);
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func, T exceptFor, params T[] filters)
    {
        sequence = Filter(sequence, exceptFor);

        foreach (T e in filters)
        {
            sequence = Filter(sequence, e);
        }

        sequence.ForEach(func);
    }

    static private IEnumerable<T> Filter<T>(IEnumerable<T> sequence, T exceptFor)
        => sequence.Where(elem => !EqualityComparer<T>.Default.Equals(elem, exceptFor));

    /// <summary>
    /// Iterate children of specified transform
    /// </summary>
    /// <param name="transform">Parent transform of iteration target</param>
    /// <param name="action">Applies for each children</param>
    static public void ForEach(this Transform transform, Action<Transform> action)
    {
        foreach (Transform tf in transform)
        {
            action(tf);
        }
    }

    /// <summary>
    /// Returns the first child that satisfies a condition in the children of specified transform, or a null if no child is found.
    /// </summary>
    /// <param name="transform">Parent transform of iteration target</param>
    /// <param name="predicate">A function to test each element for a condition</param>
    static public Transform FirstOrDefault(this Transform transform, Func<Transform, bool> predicate)
    {
        foreach (Transform tf in transform)
        {
            if (predicate(tf)) return tf;
        }

        return null;
    }
}
