using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject dragIcon;
    private Canvas parentCanvas;
    private Toggle myToggle;

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        myToggle = GetComponent<Toggle>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Create the visual "Ghost" icon
        dragIcon = Instantiate(gameObject, parentCanvas.transform);
        dragIcon.name = "DragIcon";

        // 2. Clean up the Ghost (Remove logic so it's just a picture)
        Destroy(dragIcon.GetComponent<TileDragHandler>()); // Don't let the ghost drag itself!
        
        Toggle ghostToggle = dragIcon.GetComponent<Toggle>();
        if (ghostToggle != null)
        {
            ghostToggle.group = null; // Detach from group just in case
            Destroy(ghostToggle);     // Remove the component
        }

        // 3. Visual adjustments (Transparency & Click-through)
        CanvasGroup cg = dragIcon.GetComponent<CanvasGroup>();
        if (cg == null) cg = dragIcon.AddComponent<CanvasGroup>();
        
        cg.alpha = 0.6f;
        cg.blocksRaycasts = false; // CRITICAL: Allows the mouse to "see" the 3D world behind the icon

        // 4. CRITICAL FIX: Re-select the original toggle
        // Creating the clone above confused the ToggleGroup and turned this button OFF.
        // We force it back ON here so the Game Logic knows we have a building selected.
        if (myToggle != null)
        {
            myToggle.isOn = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);

        // Raycast into the 3D world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Now that we fixed the selection in OnBeginDrag, this should work!
            if (GameManager.INSTANCE != null)
            {
                GameManager.INSTANCE.OnClick(hit.collider.gameObject);
            }
        }
    }
}