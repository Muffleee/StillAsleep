using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Dynamic;

public class IngameUI : MonoBehaviour
{
    private List<GridObj> gridObjs = new List<GridObj>();
    public Toggle[] toggles;
    private GridObj selectedObj;
    private int selectedIndex = -1;
    private const bool DEBUG = false;

    public void AddGridObj(GridObj obj)
    {
        gridObjs.Add(obj);
        UpdateToggles();
    }

    public void RemoveGridObj(GridObj obj)
    {
        if (gridObjs.Contains(obj))
        {
            gridObjs.Remove(obj);
            UpdateToggles();
        }
    }

    public void RemoveSelected()
    {
        if (selectedIndex >= 0 && selectedIndex < gridObjs.Count)
        {
            // Remove selected
            gridObjs.RemoveAt(selectedIndex);

            // Replace it with a new random GridObj so toggle stays filled
            List<GridObj> all = GridObj.GetPossiblePlaceables();
            if (all.Count > 0)
            {
                GridObj replacement = all[Random.Range(0, all.Count)];
                gridObjs.Insert(selectedIndex, replacement);
            }

            // Clear selection
            if (selectedIndex < toggles.Length)
                toggles[selectedIndex].isOn = false;

            selectedObj = null;
            selectedIndex = -1;

            UpdateToggles();
            if(DEBUG) Debug.Log("Removed and replaced selected GridObj.");
        }
        else
        {
            if(DEBUG) Debug.Log("No valid selection to remove.");
        }
    }

    public void FillList()
    {
        List<GridObj> all = GridObj.GetPossiblePlaceables();

        while (gridObjs.Count < 3)
        {
            GridObj randomObj = all[Random.Range(0, all.Count)];
            gridObjs.Add(randomObj);
        }

        UpdateToggles();
    }

    private void UpdateToggles()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (i < gridObjs.Count)
            {
                toggles[i].GetComponentInChildren<Text>().text = gridObjs[i].GetName();
                toggles[i].gameObject.SetActive(true);
            }
            else
            {
                toggles[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            string name = changedToggle.GetComponentInChildren<Text>().text;
            selectedIndex = gridObjs.FindIndex(obj => obj.GetName() == name);
            selectedObj = selectedIndex >= 0 ? gridObjs[selectedIndex] : null;
            if (DEBUG) Debug.Log("Selected: " + name);
        }
        else
        {
            string name = changedToggle.GetComponentInChildren<Text>().text;
            if (selectedObj != null && selectedObj.GetName() == name)
            {
                selectedObj = null;
                selectedIndex = -1;
                if (DEBUG) Debug.Log("Selected cleared");
            }
        }
    }

    public bool HasSelectedObj()
    {
        return this.selectedObj != null;
    }

    public GridObj GetSelected()
    {
        return this.selectedObj;
    }

    private void Start()
    {
        FillList();

        foreach (Toggle t in toggles)
        {
            t.onValueChanged.AddListener(delegate { OnToggleChanged(t); });
        }
    }
}
