using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject ghostObject; 
    private Toggle myToggle;
    private IngameUI uiController;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject highlightFrame; 

    void Start()
    {
        myToggle = GetComponent<Toggle>();
        uiController = Object.FindAnyObjectByType<IngameUI>();
        
        // Ensure the highlight starts turned off
        if (highlightFrame != null) 
        {
            highlightFrame.SetActive(myToggle.isOn);
        }

        if (myToggle != null)
        {
            // Listen for toggle clicks
            myToggle.onValueChanged.AddListener(UpdateHighlight);
        }
    }

    private void UpdateHighlight(bool isOn)
    {
        if (highlightFrame != null)
        {
            highlightFrame.SetActive(isOn);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        string tileName = GetComponentInChildren<Text>().text;
        GameObject prefab = uiController.GetPrefabByName(tileName);

        if (prefab != null)
        {
            ghostObject = Instantiate(prefab);
            if (ghostObject.TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        }

        if (myToggle != null) myToggle.isOn = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] allHits = Physics.RaycastAll(ray);
            foreach (var hit in allHits)
            {
                if (hit.collider.transform.IsChildOf(ghostObject.transform)) continue;
                Vector3 newPos = hit.point;
                ghostObject.transform.position = newPos;
                return;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostObject != null) Destroy(ghostObject);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] allHits = Physics.RaycastAll(ray);
        foreach (var hit in allHits)
        {
            if (GameManager.INSTANCE != null)
            {
                GridObj selected = GameManager.INSTANCE.GetCurrentGrid().GetGridObjFromGameObj(hit.collider.transform.root.gameObject);
                if (selected == null || (selected.GetGridType() != GridType.REPLACEABLE && selected.GetGridType() != GridType.MANUAL_REPLACEABLE)) continue;
                GameManager.INSTANCE.OnClick(hit.collider.gameObject);
            }
        }
    }
}