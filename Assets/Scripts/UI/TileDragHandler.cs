using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject ghostObject; // This is now a 3D object, not UI
    private Toggle myToggle;
    private IngameUI uiController;

    void Start()
    {
        myToggle = GetComponent<Toggle>();
        uiController = Object.FindAnyObjectByType<IngameUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Get the name of the tile from the UI text
        string tileName = GetComponentInChildren<Text>().text;

        // 2. Get the 3D Prefab from the UI Controller
        GameObject prefab = uiController.GetPrefabByName(tileName);

        if (prefab != null)
        {
            // 3. Spawn the 3D Prefab into the world
            ghostObject = Instantiate(prefab);
            
            // 4. Disable collisions on the ghost so it doesn't bump things
            if (ghostObject.TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        }

        if (myToggle != null) myToggle.isOn = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ghostObject.transform.position = hit.point;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostObject != null) Destroy(ghostObject);

        // Place the real object via GameManager
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (GameManager.INSTANCE != null)
            {
                GameManager.INSTANCE.OnClick(hit.collider.gameObject);
            }
        }
    }
}