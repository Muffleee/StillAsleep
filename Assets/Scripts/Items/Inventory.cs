using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Item[] items;
    [SerializeField] private byte maxInventorySize;
    [SerializeField] private InventoryUI inventoryUI;
    private byte selectedInventorySlot = 0;

    /// <summary>
    /// Dictionary which maps the position of an item in the inventory to the item itself.
    /// Example: ItemExample is in the 3rd inventory slot, so inventory[3] = ItemExample
    /// </summary>
    private Dictionary<byte, Item> inventory = new();

    public bool AddItem(Item item, byte slot)
    {
        if (inventory.ContainsKey(slot)) return false;

        inventory.Add(slot, item);
        return true;
    }

    public bool AddItem(Item item)
    {
        if (inventory.Count >= maxInventorySize) return false;

        if (AddItem(item, selectedInventorySlot)) return true;

        for (byte i = 0; i < maxInventorySize; i++)
        {
            if (AddItem(item, i)) return true;
        }

        return false;
    }

    public bool RemoveItem(byte slot)
    {
        return inventory.Remove(slot);
    }

    public bool RemoveItem()
    {
        return RemoveItem(selectedInventorySlot);
    }

    public bool UseItem(byte slot)
    {
        return inventory[slot].Use();
    }
    
    public bool UseItem()
    {
        return UseItem(selectedInventorySlot);
    }
}
