using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Item
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite sprite;
    [SerializeField] private IItemBehaviour behaviour;
    public byte weight;

    public bool Use()
    {
        return behaviour.Use();
    }
}

public abstract class IItemBehaviour : MonoBehaviour
{
    public abstract bool Use();
        
}
