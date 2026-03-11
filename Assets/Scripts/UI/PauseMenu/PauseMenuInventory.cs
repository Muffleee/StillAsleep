using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuInventory : MonoBehaviour
{
    [Header("Item Slot 1")]
    [SerializeField] private GameObject slot1Panel;
    [SerializeField] private TextMeshProUGUI slot1Name;
    [SerializeField] private TextMeshProUGUI slot1Description;
    [SerializeField] private Image slot1Icon;

    [Header("Item Slot 2")]
    [SerializeField] private GameObject slot2Panel;
    [SerializeField] private TextMeshProUGUI slot2Name;
    [SerializeField] private TextMeshProUGUI slot2Description;
    [SerializeField] private Image slot2Icon;

    [SerializeField] private Inventory inventory;

    public void Show()
    {
        if (inventory == null) { Debug.LogError("Inventory not assigned!"); return; }

        List<IItem> items = inventory.GetItems();
        List<IItem> ownedItems = new List<IItem>();
        foreach (var item in items)
            if (item != null) ownedItems.Add(item);

        ShowInSlot(slot1Panel, slot1Name, slot1Description, slot1Icon,
            ownedItems.Count > 0 ? ownedItems[0] : null);

        ShowInSlot(slot2Panel, slot2Name, slot2Description, slot2Icon,
            ownedItems.Count > 1 ? ownedItems[1] : null);
    }

    private void ShowInSlot(GameObject panel, TextMeshProUGUI nameText,
        TextMeshProUGUI descText, Image icon, IItem item)
    {
        if (item == null)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);
        if (nameText != null) nameText.text = item.GetName();
        if (descText != null) descText.text = item.GetDescription();
        if (icon != null)
        {
            icon.sprite = item.GetIcon();
            icon.enabled = item.GetIcon() != null;
        }
    }
}