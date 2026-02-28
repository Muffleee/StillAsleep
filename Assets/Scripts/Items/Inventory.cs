using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    [SerializeField] private byte maxInventorySize;
    [SerializeField] private InventoryUI inventoryUI;

    private List<IItem> inventory = new();

    // Inside Inventory.cs
    private void Start() // Ensure capital 'S' and no typos!
    {
        if (inventoryUI != null)
        {
            Debug.Log("Inventory Start: Initializing UI..."); // Add this to confirm it runs
            inventoryUI.InitializeUI(maxInventorySize);
        }
        else 
        {
            Debug.LogError("Inventory UI reference is missing on the Inventory script!");
        }
    }

    public bool AddItem(IItem item, int slot)
    {
        if (inventory.Count >= maxInventorySize) return false;

        inventory.Insert(slot, item);
        if (inventoryUI != null) inventoryUI?.UpdateSlot(slot, item); 
        return true;
    }

    public bool AddItem(IItem item)
    {
        return AddItem(item, 0);
    }

    public bool RemoveItem(int slot)
    {
        if (inventory.Count <= slot && inventoryUI != null)
        {
            inventory.RemoveAt(slot);
            inventoryUI.ClearSlot(slot);
            return true;
        }
        
        return false;
    }

    public bool RemoveItem()
    {
        return RemoveItem(0);
    }

    public bool UseItem(byte slot)
    {
        if (inventory.Count <= slot)
        {
            inventory[slot].OnUse();
            inventory.RemoveAt(slot);
            return true;
        }
        return false;
    }
    
    public bool UseItem()
    {
        return UseItem(0);
    }
}