using UnityEngine;
using UnityEngine.UI; 

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [Tooltip("Drag the new Highlight child object here")]
    [SerializeField] private GameObject highlightObject; 
    
    public void Init()
    {
        ClearSlot();
        SetHighlight(false);
    }

    public void UpdateSlot(IItem item)
    {
        if (item != null && item.GetIcon() != null)
        {
            iconImage.sprite = item.GetIcon();
            iconImage.enabled = true;
            iconImage.preserveAspect = true; 
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
    }
    public void SetHighlight(bool isHighlighted)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isHighlighted);
        }
    }
}