using System;
using System.Collections.Generic;

static public class LinkedListExtensions
{
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
}
