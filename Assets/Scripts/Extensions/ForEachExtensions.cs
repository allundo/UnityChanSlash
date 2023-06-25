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

    static public void ForEach<T>(this IEnumerable<T> sequence, Action<T> action, params T[] exceptFor)
    {
        sequence.Filter(exceptFor).ForEach(action);
    }

    static public void ForEach<T>(this IEnumerable<T> sequence, Action<T> action, IEnumerable<T> filters)
    {
        sequence.Filter(filters).ForEach(action);
    }

    /// <summary>
    /// Short hand foreach statement. Loop is breakable with false return value of loop function.
    /// </summary>
    /// <param name="func">Function applies to enumerable element. The loop breaks when this function returned false.</param>
    /// <returns>TRUE if the loop is completed.</returns>
    static public bool ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func)
    {
        foreach (T elem in sequence) if (!func(elem)) return false;
        return true;
    }

    static public bool ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func, params T[] exceptFor)
        => sequence.Filter(exceptFor).ForEach(func);

    static public bool ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> func, IEnumerable<T> filter)
        => sequence.Filter(filter).ForEach(func);

    static public IEnumerable<T> Filter<T>(this IEnumerable<T> sequence, IEnumerable<T> filters)
    {
        if (filters == null) return sequence.Where(elem => elem != null);
        return sequence.Where(elem => !filters.Contains(elem));
    }

    static private bool Equals<T>(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);

    static private bool Contains<T>(IEnumerable<T> sequence, T searchFor)
        => !sequence.ForEach(elem => !Equals(elem, searchFor));

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
