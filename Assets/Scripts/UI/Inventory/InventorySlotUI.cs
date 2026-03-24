using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [Tooltip("Drag the new Highlight child object here")]
    [SerializeField] private GameObject highlightObject; 
    [SerializeField] private GameObject costDiamond;
    [SerializeField] private TMP_Text costText;
    
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

            if (costDiamond != null && costText != null)
            {
                costText.text = item.GetEnergyCost().ToString();
                costDiamond.SetActive(true);
            }
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
        costDiamond.SetActive(false);
    }
    public void SetHighlight(bool isHighlighted)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isHighlighted);
        }
    }
}