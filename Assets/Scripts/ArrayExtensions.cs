using System;

static public class ArrayExtensions
{
    static public void ForEach<T>(this T[] array, Action<T> action)
        => Array.ForEach(array, action);
}
