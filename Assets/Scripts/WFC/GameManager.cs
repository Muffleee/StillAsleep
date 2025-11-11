using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private IngameUI gui;
    public static List<GridObj> AllGridObjs = new List<GridObj>();

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject destructibleWallPrefab;
    public GameObject exitPrefab;

    Grid grid;

    /// <summary>
    /// initializing the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        this.grid = new Grid(this.width, this.height);

        grid.CollapseWorld();
        grid.IncreaseGrid();
        grid.InstantiateMissing();

        gui.FillList();

        if (true) return;
        grid.PlaceObj(new GridObj(new Vector2Int(0, 0), new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.REGULAR, WallType.NONE)), new Vector3(-2, 0, -2));
        grid.CollapseWorld();
        grid.IncreaseGrid();
        grid.InstantiateMissing();
        grid.CollapseWorld();
        grid.IncreaseGrid();
        grid.InstantiateMissing();

    }
    
    public void OnClick(GameObject clicked)
    {
        GridObj selected = this.grid.GetGridObjFromGameObj(clicked);
        if (selected == null || selected.GetGridType() != GridType.REPLACEABLE) return;
        if (!this.gui.HasSelectedObj()) return;

        GridObj toPlace = this.gui.GetSelected();
        toPlace.SetGridPos(selected.GetGridPos());
        this.grid.PlaceObj(toPlace);
    }
}
