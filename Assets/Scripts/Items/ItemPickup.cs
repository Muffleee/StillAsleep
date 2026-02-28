using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item itemData;
    [SerializeField] private float pickupRadius = 1.5f;

    private void Update()
    {
        if (PlayerMovement.INSTANCE == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerMovement.INSTANCE.transform.position);

        // 3. If the player is close enough, start listening for the 'E' key
        if (distanceToPlayer <= pickupRadius)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUpItem();
            }
        }
    }

    private void PickUpItem()
    {
        if (itemData == null)
        {
            Debug.LogError("This pickup has no Item Data assigned in the inspector!");
            return;
        }

        Inventory playerInventory = Object.FindAnyObjectByType<Inventory>();
        
        if (playerInventory == null)
        {
            Debug.LogError("Player tried to pick up the item, but couldn't find an Inventory script!");
            return;
        }

        if (playerInventory.AddItem(itemData))
        {
            Debug.Log($"{itemData.itemName} picked up successfully!");
            Destroy(gameObject); 
        }
        else
        {
            Debug.LogWarning("Inventory is full!");
        }
    }

    // Add this so the GameManager can read the weight!
    public IItem GetItemData() 
    { 
        return itemData; 
    }
}