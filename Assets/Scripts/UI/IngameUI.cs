using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Class handling the game's user interface, including providing the player with placeable tiles and handling placing selected tiles.
/// </summary>
public class IngameUI : MonoBehaviour
{
    private List<GridObj> gridObjs = new List<GridObj>();
    public Toggle[] toggles;
    private GridObj selectedObj;
    private int selectedIndex = -1;
    private bool DEBUG = false;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Sprite Configuration")]
    public Sprite imgF; 
    public Sprite imgB;
    public Sprite imgL;
    public Sprite imgR;
    public Sprite imgE;
    public Sprite imgFB;
    public Sprite imgFL;
    public Sprite imgFR;
    public Sprite imgBL;
    public Sprite imgBR;
    public Sprite imgLR;
    public Sprite imgDefault; 
[Header("3D Prefab Configuration")]
public GameObject prefabF;
public GameObject prefabB;
public GameObject prefabL;
public GameObject prefabR;
public GameObject prefabE;
public GameObject prefabFB;
public GameObject prefabFL;
public GameObject prefabFR;
public GameObject prefabBL;
public GameObject prefabBR;
public GameObject prefabLR;

    /// <summary>
    /// Add a GridObj to the list of selectable GridObjs.
    /// </summary>
    /// <param name="obj">GridObj to be added.</param>
    public void AddGridObj(GridObj obj)
    {
        this.gridObjs.Add(obj);
        this.UpdateToggles();
    }

    /// <summary>
    /// Remove a GridObj from the list of selectable GridObjs.
    /// </summary>
    /// <param name="obj">GridObj to be removed.</param>
    public void RemoveGridObj(GridObj obj)
    {
        if (this.gridObjs.Contains(obj))
        {
            this.gridObjs.Remove(obj);
            this.UpdateToggles();
        }
    }

    /// <summary>
    /// Remove the selected GridObj from the selectable GridObj list, deselect it, and fill the respective spot with a new selectable GridObj if desired.
    /// </summary>
    /// <param name="regenerate"></param>
    public void RemoveSelected(bool regenerate)
    {
        if (this.selectedIndex >= 0 && this.selectedIndex < this.gridObjs.Count)
        {
            // Remove selected
            this.gridObjs.RemoveAt(this.selectedIndex);

            if(regenerate)
            {
                // Replace it with a new random GridObj so toggle stays filled
                List<GridObj> all = GridObj.GetPossiblePlaceables();
                if (all.Count > 0)
                {
                    GridObj replacement = all[Random.Range(0, all.Count)];
                    this.gridObjs.Insert(this.selectedIndex, replacement);
                }
            }

            // Clear selection
            if (this.selectedIndex < this.toggles.Length)
                this.toggles[this.selectedIndex].isOn = false;

            this.selectedObj = null;
            this.selectedIndex = -1;

            this.UpdateToggles();
            if(this.DEBUG) Debug.Log("Removed and replaced selected GridObj.");
        }
        else
        {
            if(this.DEBUG) Debug.Log("No valid selection to remove.");
        }
    }

    /// <summary>
    /// Fill the list of selectable GridObjs with random GridObjs..
    /// </summary>
    public void FillList()
    {
        List<GridObj> all = GridObj.GetPossiblePlaceables();

        while (this.gridObjs.Count < 3)
        {
            GridObj randomObj = all[Random.Range(0, all.Count)];
            this.gridObjs.Add(randomObj);
        }

        this.UpdateToggles();
    }

    /// <summary>
    /// Helper function to switch images based on Name
    /// </summary>
    private Sprite GetSpriteByName(string nameCode)
    {
        switch (nameCode)
        {
            case "F":  return imgF;
            case "B":  return imgB;
            case "L":  return imgL;
            case "R":  return imgR;
            case "E":  return imgE;
            case "FB": return imgFB;
            case "FL": return imgFL;
            case "FR": return imgFR;
            case "BL": return imgBL;
            case "BR": return imgBR;
            case "LR": return imgLR;
            default:   return imgDefault;
        }
    }

public GameObject GetPrefabByName(string nameCode)
{
    switch (nameCode)
    {
        case "F":  return prefabF;
        case "B":  return prefabB;
        case "L":  return prefabL;
        case "R":  return prefabR;
        case "E":  return prefabE;
        case "FB": return prefabFB;
        case "FL": return prefabFL;
        case "FR": return prefabFR;
        case "BL": return prefabBL;
        case "BR": return prefabBR;
        case "LR": return prefabLR;
        default:   return null;
    }
}
    /// <summary>
    /// Update the toggle buttons on the user interface to reflect any changes in selectable GridObjs.
    /// </summary>
    private void UpdateToggles()
    {
        for (int i = 0; i < this.toggles.Length; i++)
        {
            if (i < this.gridObjs.Count)
            {
                string objName = this.gridObjs[i].GetName();
                this.toggles[i].GetComponentInChildren<Text>().text = objName;

                if (this.toggles[i].targetGraphic != null)
                {
                    Image bg = this.toggles[i].targetGraphic.GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.sprite = GetSpriteByName(objName);
                    }
                }

                this.toggles[i].gameObject.SetActive(true);
            }
            else
            {
                this.toggles[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Handles any presses of the toggle buttons and selects/deselects a GridObj if needed.
    /// </summary>
    /// <param name="changedToggle">The changed toggle button.</param>
    public void OnToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            string name = changedToggle.GetComponentInChildren<Text>().text;
            this.selectedIndex = this.gridObjs.FindIndex(obj => obj.GetName() == name);
            this.selectedObj = this.selectedIndex >= 0 ? this.gridObjs[this.selectedIndex] : null;
            if (this.DEBUG) Debug.Log("Selected: " + name);
        }
        else
        {
            string name = changedToggle.GetComponentInChildren<Text>().text;
            if (this.selectedObj != null && this.selectedObj.GetName() == name)
            {
                this.selectedObj = null;
                this.selectedIndex = -1;
                if (this.DEBUG) Debug.Log("Selected cleared");
            }
        }
    }

    public bool HasSelectedObj() {return this.selectedObj != null;}

    public GridObj GetSelected() {return this.selectedObj;}

    /// <summary>
    /// Setting components and disabling the tutorial canvas
    /// </summary>
    private void Awake()
    {
        tutorialText.enabled = false;
    }
    /// <summary>
    /// At game start, fill the list of selectable GridObjs and add listeners to all toggles.
    /// </summary>
    private void Start()
    {
        
        this.FillList();

        foreach (Toggle t in this.toggles)
        {
            t.onValueChanged.AddListener(delegate { this.OnToggleChanged(t); });
        }
    }
    /// <summary>
    /// Open the tutorial by setting the text and enabling it. Freezes the game
    /// </summary>
    /// <param name="text"> The text it should be set to</param>
    /// <param name="objPosition"> The object worldPos it should be positioned at</param>
    public void OpenTutorialText (Vector3 objPosition, string text)
    {
        if(tutorialText != null)
        {
            Vector2 textPos = Camera.main.WorldToScreenPoint(objPosition);
            textPos.y += 50f;
            tutorialText.rectTransform.position = textPos;
            tutorialText.enabled = true;
            Time.timeScale = 0;
            tutorialText.text = text;
        } else Debug.Log("no tutorial text");

    }
    /// <summary>
    /// Closing tutorial and setting the game to normal speed
    /// </summary>
    public void CloseTutorialText()
    {
        if (tutorialText != null)
        {
            tutorialText.enabled = false;
            Time.timeScale = 1f;
        }
    }
}
