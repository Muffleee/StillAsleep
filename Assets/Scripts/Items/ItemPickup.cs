using UnityEngine;

public enum ItemType 
{ 
    None, 
    TimeReversal, 
    WallBreaker, 
    Sludge, 
    Scanner
}

public class ItemPickup : MonoBehaviour
{
    [Header("Item Setup")]
    [Tooltip("Select which item this 3D object represents")]
    public ItemType itemType; 
    
    [SerializeField] private float pickupRadius = 1.5f;

    private void Update()
    {
        if (PlayerMovement.INSTANCE == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerMovement.INSTANCE.transform.position);

        if (distanceToPlayer <= pickupRadius && Input.GetKeyDown(KeyCode.E))
        {
            PickUpItem();
        }
    }

    private void PickUpItem()
    {
        Inventory playerInventory = Object.FindAnyObjectByType<Inventory>();
        if (playerInventory == null) return;

        IItem itemData = GenerateItemData();
        if (itemData == null) return;

        if (playerInventory.AddItem(itemData))
        {
            Debug.Log($"{itemData.GetName()} picked up successfully!");
            Destroy(gameObject);
        }
        else
        {
            Vector3 dropLocation = transform.position + new Vector3(0, 0.5f, 0); 
            
            playerInventory.SwapWithSelected(itemData, dropLocation);
            Destroy(gameObject); 
        }
    }

    private IItem GenerateItemData()
    {
        switch (itemType)
        {
            case ItemType.TimeReversal: return new TimeReversalItem();
            case ItemType.WallBreaker:  return new WallBreakerItem();
            case ItemType.Sludge:       return new SludgeItem();
            case ItemType.Scanner:      return new ScannerItem();
            default: return null;
        }
    }

    public IItem GetItemData() 
    { 
        return GenerateItemData(); 
    }
}