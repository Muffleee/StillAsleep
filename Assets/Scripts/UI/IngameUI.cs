using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IngameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tileButtonPrefab;
    [SerializeField] private Transform tileContainer;
    [SerializeField] private TextMeshProUGUI remainingTilesText;

    [Header("3D Preview Settings")]
    [SerializeField] private GameObject wallPreviewPrefab;
    [SerializeField] private GameObject floorPreviewPrefab;
    [SerializeField] private float previewRotationSpeed = 30f;

    private List<GridObj> gridObjs = new List<GridObj>();
    private List<GameObject> tileButtons = new List<GameObject>();
    private GridObj selectedObj;
    private int selectedIndex = -1;
    private bool DEBUG = false;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        ClearAllTiles();
    }

    private void Update()
    {
        // Update remaining tiles text
        if (remainingTilesText != null && gameManager != null)
        {
            remainingTilesText.text = $"Tiles to Place: {gameManager.GetRemainingTilesToPlace()}";
        }

        // Rotate all 3D previews
        foreach (GameObject button in tileButtons)
        {
            Transform previewContainer = button.transform.Find("PreviewContainer");
            if (previewContainer != null)
            {
                previewContainer.Rotate(Vector3.up, previewRotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Generate tiles based on the number rolled
    /// </summary>
    public void GenerateTiles(int count)
    {
        ClearAllTiles();

        List<GridObj> all = GridObj.GetPossiblePlaceables();

        for (int i = 0; i < count; i++)
        {
            GridObj randomObj = all[Random.Range(0, all.Count)];
            gridObjs.Add(randomObj);
        }

        CreateTileButtons();
    }

    /// <summary>
    /// Create UI buttons with 3D previews for each tile
    /// </summary>
    private void CreateTileButtons()
    {
        for (int i = 0; i < gridObjs.Count; i++)
        {
            GameObject button = Instantiate(tileButtonPrefab, tileContainer);
            tileButtons.Add(button);

            // Set button text
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = gridObjs[i].GetName();
            }

            // Create 3D preview
            Create3DPreview(button, gridObjs[i], i);

            // Add button listener
            int index = i; // Capture for closure
            Button btn = button.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnTileSelected(index));
            }
        }
    }

    /// <summary>
    /// Create a 3D preview of the tile
    /// </summary>
    private void Create3DPreview(GameObject button, GridObj gridObj, int index)
    {
        // Find or create preview container
        Transform previewContainer = button.transform.Find("PreviewContainer");
        if (previewContainer == null)
        {
            GameObject container = new GameObject("PreviewContainer");
            container.transform.SetParent(button.transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localScale = Vector3.one;
            previewContainer = container.transform;
        }

        // Clear existing preview
        foreach (Transform child in previewContainer)
        {
            Destroy(child.gameObject);
        }

        // Create floor preview
        if (floorPreviewPrefab != null)
        {
            GameObject floor = Instantiate(floorPreviewPrefab, previewContainer);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = Vector3.one * 0.3f;
        }

        // Create wall previews based on wallStatus
        if (wallPreviewPrefab != null)
        {
            WallStatus ws = gridObj.GetWallStatus();

            if (ws.HasWallAt(WallPos.FRONT))
            {
                CreateWallPreview(previewContainer, WallPos.FRONT);
            }
            if (ws.HasWallAt(WallPos.BACK))
            {
                CreateWallPreview(previewContainer, WallPos.BACK);
            }
            if (ws.HasWallAt(WallPos.LEFT))
            {
                CreateWallPreview(previewContainer, WallPos.LEFT);
            }
            if (ws.HasWallAt(WallPos.RIGHT))
            {
                CreateWallPreview(previewContainer, WallPos.RIGHT);
            }
        }
    }

    /// <summary>
    /// Create a single wall preview at the given position
    /// </summary>
    private void CreateWallPreview(Transform parent, WallPos wallPos)
    {
        GameObject wall = Instantiate(wallPreviewPrefab, parent);
        wall.transform.localScale = Vector3.one * 0.3f;

        // Position based on wall direction
        switch (wallPos)
        {
            case WallPos.FRONT:
                wall.transform.localPosition = new Vector3(0, 0, -0.3f);
                wall.transform.localRotation = Quaternion.identity;
                break;
            case WallPos.BACK:
                wall.transform.localPosition = new Vector3(0, 0, 0.3f);
                wall.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                break;
            case WallPos.LEFT:
                wall.transform.localPosition = new Vector3(-0.3f, 0, 0);
                wall.transform.localRotation = Quaternion.Euler(0, 90f, 0);
                break;
            case WallPos.RIGHT:
                wall.transform.localPosition = new Vector3(0.3f, 0, 0);
                wall.transform.localRotation = Quaternion.Euler(0, -90f, 0);
                break;
        }
    }

    /// <summary>
    /// Called when a tile button is clicked
    /// </summary>
    private void OnTileSelected(int index)
    {
        if (index < 0 || index >= gridObjs.Count) return;

        selectedIndex = index;
        selectedObj = gridObjs[index];

        // Visual feedback
        UpdateButtonSelection();

        if (DEBUG) Debug.Log($"Selected tile: {selectedObj.GetName()}");
    }

    /// <summary>
    /// Update visual selection state of buttons
    /// </summary>
    private void UpdateButtonSelection()
    {
        for (int i = 0; i < tileButtons.Count; i++)
        {
            Image buttonImage = tileButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (i == selectedIndex) ? Color.yellow : Color.white;
            }
        }
    }

    /// <summary>
    /// Remove the selected tile after placement
    /// </summary>
    public void RemoveSelected(bool regenerate)
    {
        if (selectedIndex >= 0 && selectedIndex < gridObjs.Count)
        {
            // Remove from list
            gridObjs.RemoveAt(selectedIndex);

            // Destroy button
            if (selectedIndex < tileButtons.Count)
            {
                Destroy(tileButtons[selectedIndex]);
                tileButtons.RemoveAt(selectedIndex);
            }

            // Clear selection
            selectedObj = null;
            selectedIndex = -1;

            if (DEBUG) Debug.Log("Removed selected tile.");
        }
    }

    /// <summary>
    /// Clear all tiles and buttons
    /// </summary>
    private void ClearAllTiles()
    {
        gridObjs.Clear();

        foreach (GameObject button in tileButtons)
        {
            if (button != null) Destroy(button);
        }
        tileButtons.Clear();

        selectedObj = null;
        selectedIndex = -1;
    }

    /// <summary>
    /// Legacy method - now calls GenerateTiles with fixed count
    /// </summary>
    public void FillList()
    {
        GenerateTiles(3); // Default fallback
    }

    public bool HasSelectedObj()
    {
        return this.selectedObj != null;
    }

    public GridObj GetSelected()
    {
        return this.selectedObj;
    }
}