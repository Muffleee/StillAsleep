using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IItem
{
    [Header("Basic Info")]
    public string itemName;
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

    public virtual void OnUse()
    {
        // This is what happens when the player clicks it in the inventory
        Debug.Log($"Used item: {itemName}");
    }
}