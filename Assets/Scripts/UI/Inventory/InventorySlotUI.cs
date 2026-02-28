using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image and Button

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button slotButton; 
    
    private byte slotIndex;
    private Inventory inventory;

    public void Init(byte index, Inventory inv)
    {
        slotIndex = index;
        inventory = inv;
        ClearSlot();
        
        // Listen for clicks
        slotButton.onClick.AddListener(OnSlotClicked);
    }

    public void UpdateSlot(IItem item)
    {
        if (item != null && item.GetIcon() != null)
        {
            iconImage.sprite = item.GetIcon();
            iconImage.enabled = true;
            slotButton.interactable = true;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        slotButton.interactable = false;
    }

    private void OnSlotClicked()
    {
        inventory.UseItem(slotIndex);
    }
}