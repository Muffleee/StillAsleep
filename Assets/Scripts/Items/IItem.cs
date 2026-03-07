using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    public int GetEnergyCost();
    public int GetSpawnWeight();
    public Sprite GetIcon();
    public GameObject GetPrefab();
    public void OnUse();
    public string GetName();
    public string GetDescription();
}