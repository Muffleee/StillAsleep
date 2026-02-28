using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Tooltip("The UI panel that has the Grid Layout Group attached to it.")]
    [SerializeField] private Transform itemsParent; 
    
    [Tooltip("The Slot Prefab we will spawn for each inventory space.")]
    [SerializeField] private GameObject slotPrefab; 
    
    [SerializeField] private Inventory inventory;

    private InventorySlotUI[] slots;

    // Generates the empty slots when the game starts
    public void InitializeUI(int maxSlots)
    {
        slots = new InventorySlotUI[maxSlots];

        for (byte i = 0; i < maxSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);
            slots[i] = newSlot.GetComponent<InventorySlotUI>();
            slots[i].Init(i, inventory);
        }
    }

    // Tells a specific slot to show an item's icon// Inside InventoryUI.cs
    public void UpdateSlot(int slotIndex, IItem item)
    {
        // If Start() didn't run or InitializeUI failed, 'slots' will be null.
        // This check prevents the NullReferenceException.
        if (slots == null) 
        {
            Debug.LogError("UI Error: 'slots' array is null. InitializeUI was never called!");
            return; 
        }

        if (slotIndex < slots.Length && slots[slotIndex] != null)
        {
            slots[slotIndex].UpdateSlot(item);
        }
    }

    // Tells a specific slot to hide its icon
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < slots.Length)
        {
            slots[slotIndex].ClearSlot();
        }
    }
}