using System;
using System.Linq;
using System.Collections.Generic;

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

    static private IEnumerable<T> Filter<T>(IEnumerable<T> sequence, T exceptFor)
        => sequence.Where(elem => !EqualityComparer<T>.Default.Equals(elem, exceptFor));
}
