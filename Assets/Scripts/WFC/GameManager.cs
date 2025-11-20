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
    [SerializeField] int generateAfter = 4;
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
        PlayerMovement.currentGridPos = new Vector2Int(PlayerMovement.currentGridPos.x + 1, PlayerMovement.currentGridPos.y + 1);
        grid.InstantiateMissing();
        gui.FillList();
    }

    public void OnMove(Vector2Int from, Vector2Int to, WallPos direction, long step)
    {
        if (step % this.generateAfter != 0) return;
        grid.CollapseWorld();
        grid.IncreaseGrid();
        PlayerMovement.currentGridPos = new Vector2Int(PlayerMovement.currentGridPos.x + 1, PlayerMovement.currentGridPos.y + 1);
        grid.InstantiateMissing();
        this.gui.FillList();
    }

    public void OnClick(GameObject clicked)
    {
        GridObj selected = this.grid.GetGridObjFromGameObj(clicked);
        if (selected == null || selected.GetGridType() != GridType.REPLACEABLE) return;
        if (!this.gui.HasSelectedObj()) return;

        GridObj virtualObj = this.gui.GetSelected();
        GridObj toPlace = new GridObj(selected.GetGridPos(), virtualObj.GetWallStatus());
        toPlace.SetGridPos(selected.GetGridPos());
        this.grid.PlaceObj(toPlace);
        this.gui.RemoveSelected(false);
    }

    public Grid GetCurrentGrid() { return this.grid; }
}
