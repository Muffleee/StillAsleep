using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEditor.Progress;

/// <summary>
/// Main game manager class, handles game initialization, world generation, and move and click events
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] int generateAfter = 4;
    [SerializeField] int replaceExitAfter = 2;
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
    /// Initializes the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        grid = new Grid(width, height);

        grid.CollapseWorld();
        grid.IncreaseGrid();

        grid.CreateExit(new Vector2Int(4, 4), 1);
        grid.InstantiateMissing();

        gui.FillList();
    }

    /// <summary>
    /// Function to be called on player movement, handles dynamic map generation and movement of the exit
    /// </summary>
    /// <param name="from">Coordinate *from* which the player is moving</param>
    /// <param name="to">Coordinate *to* which the player is moving</param>
    /// <param name="direction">Direction of movement</param>
    /// <param name="step">Count of all movement steps taken by the player</param>
    public void OnMove(Vector2Int from, Vector2Int to, WallPos direction, long step)
    {   
        if (step % generateAfter != 0)
        {
            if(step % replaceExitAfter == 0)
            {
                grid.RepositionExit(WallPos.BACK);
            }
            return;
        }
        grid.CollapseWorld();
        grid.IncreaseGrid();
        grid.InstantiateMissing();

        if(step % replaceExitAfter == 0)
        {
            grid.RepositionExit(WallPos.BACK);
        }

        gui.FillList();
    }

    /// <summary>
    /// Function to be called whenever the player clicks in the world, handles placing player-selected tiles
    /// </summary>
    /// <param name="clicked">Clicked game object</param>
    public void OnClick(GameObject clicked)
    {
        GridObj selectedTile = grid.GetGridObjFromGameObj(clicked);
        if (selectedTile == null || selectedTile.GetGridType() != GridType.REPLACEABLE) return;
        if (!gui.HasSelectedObj()) return;

        GridObj virtualObj = gui.GetSelected();
        GridObj toPlace = new GridObj(selectedTile.GetGridPos(), virtualObj.GetWallStatus());
        grid.PlaceObj(toPlace);

        gui.RemoveSelected(false);
    }
    
    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    public Grid GetCurrentGrid() { return grid; }
}
