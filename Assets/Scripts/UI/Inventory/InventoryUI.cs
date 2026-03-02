using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform itemsParent; 
    [SerializeField] private GameObject slotPrefab; 

    private InventorySlotUI[] slots;

    public void InitializeUI(int maxSlots)
    {
        slots = new InventorySlotUI[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);
            slots[i] = newSlot.GetComponent<InventorySlotUI>();
            
            slots[i].Init(); 
        }
    }

    public void UpdateSlot(int slotIndex, IItem item)
    {
        if (slots != null && slotIndex >= 0 && slotIndex < slots.Length && slots[slotIndex] != null)
        {
            slots[slotIndex].UpdateSlot(item);
        }
    }

    public void ClearSlot(int slotIndex)
    {
        if (slots != null && slotIndex >= 0 && slotIndex < slots.Length && slots[slotIndex] != null)
        {
            slots[slotIndex].ClearSlot();
        }
    }

    public void SelectSlot(int selectedIndex)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetHighlight(i == selectedIndex); 
            }
        }
    }
}