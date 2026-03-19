using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private byte maxInventorySize = 10; 
    [SerializeField] private InventoryUI inventoryUI;
    
    private int currentSelectedItem = 0; 
    private List<IItem> inventory = new List<IItem>();

    private void Start()
    {
        for (int i = 0; i < maxInventorySize; i++)
        {
            inventory.Add(null);
        }

        if (inventoryUI != null)
        {
            inventoryUI.InitializeUI(maxInventorySize);
            inventoryUI.SelectSlot(currentSelectedItem);
        }
    }

    public bool AddItem(IItem item, int slot)
    {
        if (slot < 0 || slot >= maxInventorySize) return false;
        
        if (inventory[slot] == null)
        {
            inventory[slot] = item;
            if (inventoryUI != null) inventoryUI.UpdateSlot(slot, item); 
            return true;
        }
        return false;
    }

    public bool AddItem(IItem item)
    {
        for (int i = 0; i < maxInventorySize; i++)
        {
            if (inventory[i] == null) return AddItem(item, i);
        }
        return false; 
    }

    public bool RemoveItem(int slot)
    {
        if (slot >= 0 && slot < maxInventorySize && inventory[slot] != null)
        {
            inventory[slot] = null;
            if (inventoryUI != null) inventoryUI.ClearSlot(slot);
            return true;
        }
        return false;
    }

    public bool RemoveItem() => RemoveItem(currentSelectedItem);

    public bool UseItem(int slot)
    {
        if (slot >= 0 && slot < maxInventorySize && inventory[slot] != null)
        {
            IItem itemToUse = inventory[slot];

            if (PlayerMovement.INSTANCE == null) return false;
            PlayerResources playerResources = PlayerMovement.INSTANCE.GetComponent<PlayerResources>();

            if (playerResources != null)
            {
                int cost = itemToUse.GetEnergyCost();

                if (playerResources.CanAfford(cost))
                {
                    playerResources.Spend(cost);
                    itemToUse.OnUse();

                    inventory[slot] = null; 
                    if (inventoryUI != null) inventoryUI.ClearSlot(slot); 
                    
                    return true; 
                }
                else
                {
                    // Not enough energy
                    Debug.LogWarning($"Not enough energy! {itemToUse.GetName()} costs {cost} energy.");
                    return false; 
                }
            }
        }
        return false;
    }
    
    public bool UseItem() => UseItem(currentSelectedItem);

    public int GetSelectedSlot() => currentSelectedItem;

    private void Update()
    {
        // --- 1. SCROLLING LOGIC ---
        float mouseScroll = Input.mouseScrollDelta.y;
        bool controlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (mouseScroll != 0 && !controlDown)
        {
            int previousSelectedItem = currentSelectedItem;
            if (mouseScroll > 0) currentSelectedItem--;
            else currentSelectedItem++;

            currentSelectedItem = Mathf.Clamp(currentSelectedItem, 0, maxInventorySize - 1);

            if (currentSelectedItem != previousSelectedItem && inventoryUI != null)
            {
                inventoryUI.SelectSlot(currentSelectedItem);
            }
        }

        // --- 2. USE SELECTED ITEM (F Key) ---
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseItem(); 
        }

        // --- 3. USE SPECIFIC ITEM (Number Keys 1-9, 0) ---
        CheckNumberKeys();

        // --- 4. DROP SELECTED ITEM (Q Key) ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropSelectedItem();
        }
    }

    public void DropSelectedItem()
    {
        IItem itemToDrop = inventory[currentSelectedItem];

        if (itemToDrop != null && PlayerMovement.INSTANCE != null)
        {
            Vector3 dropPosition = PlayerMovement.INSTANCE.transform.position;
            
            Collider[] colliders = Physics.OverlapSphere(dropPosition, 0.5f);
            foreach (Collider col in colliders)
            {
                if (col.GetComponent<ItemPickup>() != null)
                {
                    Debug.LogWarning("Cannot drop! There is already an item right here.");
                    return; 
                }
            }

            GameObject dropPrefab = itemToDrop.GetPrefab();
            if (dropPrefab != null)
            {
                Instantiate(dropPrefab, dropPosition, Quaternion.identity);
                Debug.Log($"Dropped {itemToDrop.GetIcon().name} at your feet.");
            }

            inventory[currentSelectedItem] = null;
            if (inventoryUI != null) 
            {
                inventoryUI.ClearSlot(currentSelectedItem);
            }
        }
    }

    public void SwapWithSelected(IItem newItem, Vector3 dropPosition)
    {
        IItem oldItem = inventory[currentSelectedItem];

        // 1. Drop the item currently in your hand
        if (oldItem != null)
        {
            GameObject dropPrefab = oldItem.GetPrefab();
            if (dropPrefab != null)
            {
                Instantiate(dropPrefab, dropPosition, Quaternion.identity);
            }
        }

        // 2. Put the new item into that same slot
        inventory[currentSelectedItem] = newItem;
        if (inventoryUI != null) 
        {
            inventoryUI.UpdateSlot(currentSelectedItem, newItem);
        }
    }

    private void CheckNumberKeys()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int slotIndex = (i == 0) ? 9 : i - 1;
                UseItem(slotIndex);
            }
        }
    }

    public List<IItem> GetItems() => inventory;
    public int GetMaxSlots() => maxInventorySize;
}