using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    [SerializeField] private GameManager gameManager;
    [SerializeField] private byte maxInventorySize;
    [SerializeField] private InventoryUI inventoryUI;
    private int currentSelectedItem = 0;

    private List<IItem> inventory = new();

    private void Start()
    {
        if (inventoryUI != null)
        {
            Debug.Log("Inventory Start: Initializing UI...");
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
        if (inventoryUI != null) inventoryUI.UpdateSlot(slot, item); 
        return true;
    }

    public bool AddItem(IItem item)
    {
        return AddItem(item, currentSelectedItem);
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
        return RemoveItem(currentSelectedItem);
    }

    public bool UseItem(int slot)
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
        return UseItem(currentSelectedItem);
    }

    public int GetSelectedItem()
    {
        return currentSelectedItem;
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            currentSelectedItem = math.max(0, currentSelectedItem - 1);
            Debug.Log(currentSelectedItem);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            currentSelectedItem = math.min(inventory.Count, currentSelectedItem + 1);
            Debug.Log(currentSelectedItem);
        }
    }
}