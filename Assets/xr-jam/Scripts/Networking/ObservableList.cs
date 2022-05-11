using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper script to define an extensible list datatype.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class ObservedList<T> : List<T>
{
    public event Action<int> Changed = delegate { };
    public event Action<T> OnAddition = delegate { };
    public event Action<T> OnRemoved = delegate { };

    public new void Add(T item)
    {
        base.Add(item);
        OnAddition(item);
    }

    public new void Remove(T item)
    {
        base.Remove(item);
        OnRemoved(item);
    }

    //public new void AddRange(IEnumerable<T> collection)
    //{
    //    base.AddRange(collection);
    //    OnAddition();
    //}
    //public new void RemoveRange(int index, int count)
    //{
    //    base.RemoveRange(index, count);
    //    OnRemoved();
    //}
    //public new void Clear()
    //{
    //    base.Clear();
    //    OnAddition();
    //}
    //public new void Insert(int index, T item)
    //{
    //    base.Insert(index, item);
    //    OnAddition();
    //}
    //public new void InsertRange(int index, IEnumerable<T> collection)
    //{
    //    base.InsertRange(index, collection);
    //    OnAddition();
    //}
    //public new void RemoveAll(Predicate<T> match)
    //{
    //    base.RemoveAll(match);
    //    OnAddition();
    //}


    //public new T this[int index]
    //{
    //    get
    //    {
    //        return base[index];
    //    }
    //    set
    //    {
    //        base[index] = value;
    //        Changed(index);
    //    }
    //}


}