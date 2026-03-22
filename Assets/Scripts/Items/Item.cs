using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IItem
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("Mechanics")]
    public int energyCost = 0;
    
    [Header("Spawning Setup")]
    [Tooltip("Higher number = more common. Lower number = rarer.")]
    public int spawnWeight = 10;

    // --- IItem Interface Methods ---
    
    public int GetEnergyCost()
    {
        return energyCost;
    }

    public int GetSpawnWeight()
    {
        return spawnWeight;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }

    public virtual bool OnUse()
    {
        // This is what happens when the player clicks it in the inventory
        // Return true if it was used successfully, or false if the action failed.
        return true; 
    }

    public string GetName()
    {
        return itemName;
    }

    public string GetDescription()
    {
        return description;
    }
}