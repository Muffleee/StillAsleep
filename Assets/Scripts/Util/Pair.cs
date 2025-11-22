using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<T, U>
{
    public T first;
    public U second;
    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }
}
