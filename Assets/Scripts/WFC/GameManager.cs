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
    private int remainingTilesToPlace = 0;

    /// <summary>
    /// initializing the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        this.grid = new Grid(this.width, this.height);

        grid.CollapseWorld();
        grid.IncreaseGrid();
        grid.CreateExit(new Vector2Int(4, 4), 1);
        PlayerMovement.currentGridPos = new Vector2Int(PlayerMovement.currentGridPos.x + 1, PlayerMovement.currentGridPos.y + 1);
        grid.InstantiateMissing();
    }

    public void OnMove(Vector2Int from, Vector2Int to, WallPos direction, long step)
    {   

    }

    public void OnDiceRolled(int movementSteps, int tileReward)
    {
        remainingTilesToPlace = tileReward;

        if (gui != null)
        {
            gui.GenerateTiles(tileReward);
        }

        Debug.Log($"You can now place {remainingTilesToPlace} tiles this turn!");
    }

    public void OnTurnStart(int turnNumber)
    {
        grid.CollapseWorld();
        grid.InstantiateMissing();

        // Check if we should expand the grid
        if (turnNumber > 0 && turnNumber % this.generateAfter == 0)
        {
            grid.IncreaseGrid();
            PlayerMovement.currentGridPos = new Vector2Int(
                PlayerMovement.currentGridPos.x + 1,
                PlayerMovement.currentGridPos.y + 1
            );
            grid.InstantiateMissing();
        }

        // Check if we should reposition the exit
        if (turnNumber > 0 && turnNumber % this.replaceExitAfter == 0)
        {
            grid.RepositionExit(WallPos.BACK);
        }
    }

    public void OnClick(GameObject clicked)
    {
        GridObj selected = this.grid.GetGridObjFromGameObj(clicked);
        if (selected == null || selected.GetGridType() != GridType.REPLACEABLE) return;
        if (!this.gui.HasSelectedObj()) return;

        // Check if player has tiles left to place
        if (remainingTilesToPlace <= 0)
        {
            Debug.Log("No tiles left to place this turn!");
            return;
        }

        GridObj virtualObj = this.gui.GetSelected();
        GridObj toPlace = new GridObj(selected.GetGridPos(), virtualObj.GetWallStatus());
        toPlace.SetGridPos(selected.GetGridPos());
        this.grid.PlaceObj(toPlace);
        this.gui.RemoveSelected(false);

        remainingTilesToPlace--;
    }

    public void OnTurnEnd()
    {
        remainingTilesToPlace = 0;
    }

    public Grid GetCurrentGrid() { return this.grid; }
    public int GetRemainingTilesToPlace() { return remainingTilesToPlace; }
}
