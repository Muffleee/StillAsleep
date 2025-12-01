using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private Timer collapseTimer;
    private bool timerStarted = false;
    private bool collapsePhaseActive = false;
    private bool collapseStarted = false;
    private Vector2Int collapseStartPos;
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

        UnityEvent collapseEvent = new UnityEvent();
        collapseEvent.AddListener(StartCollapsePhase);
        collapseTimer.Init(10f, collapseEvent);
    }

    private void StartCollapsePhase()
    {
        Debug.Log("Timer abgelaufen → Erste große Zerstörung!");

        collapseStartPos = new Vector2Int(4, 4); // Beispielstartpunkt oder beliebig
        collapseStarted = true;
        collapsePhaseActive = true;

        CollapseInitialTiles(); // 4–5 Tiles entfernen
    }

    //private void CollapseTilesAfterEveryMove()
    //{
    //    int amount = Random.Range(1, 5); // 1–4
     //   CollapseTiles(amount);
    //}

    private void CollapseTilesAfterEveryMove()
    {
        if (!collapseStarted) return;

        int amount = Random.Range(4, 7); // 4–6 Tiles pro Move
        int width = Random.Range(0, 3);   // 0–2 Tiles seitlich

        Grid currentGrid = GetCurrentGrid();
        Vector2Int playerPos = PlayerMovement.currentGridPos;

        for (int i = 0; i < amount; i++)
        {
            // Maximalhöhe für Zerstörung ist immer unter Spieler
            int targetY = Mathf.Min(collapseStartPos.y + 1, playerPos.y - 1);

            collapseStartPos.y = targetY;

            // Wenn Startpunkt schon zu hoch ist, abbrechen
            if (collapseStartPos.y >= playerPos.y) break;

            if (!currentGrid.IsInsideGrid(collapseStartPos)) break;

            // Zentrale Linie zerstören
            DestroyTileAt(collapseStartPos, currentGrid);

            // Seitliche Breite auf derselben Y-Ebene
            for (int w = 1; w <= width; w++)
            {
                Vector2Int left = new Vector2Int(collapseStartPos.x - w, collapseStartPos.y);
                Vector2Int right = new Vector2Int(collapseStartPos.x + w, collapseStartPos.y);

                DestroyTileAt(left, currentGrid);
                DestroyTileAt(right, currentGrid);
            }
        }

        Debug.Log($"{amount} Tiles + seitliche Erweiterung zerstört (immer unter Spieler).");
    }

    private void DestroyTileAt(Vector2Int pos, Grid currentGrid)
    {
        if (!currentGrid.IsInsideGrid(pos)) return;

        GridObj obj = currentGrid.GetGridArray()[pos.x, pos.y];

        if (obj != null)
        {
            obj.DestroyObj();  // visuals entfernen
        }

        // NEUES HOLE-TILE ANLEGEN
        GridObj hole = GridObj.CreateHole(pos);
        currentGrid.GetGridArray()[pos.x, pos.y] = hole;

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

        if (!timerStarted && step >= 1)
        {
            timerStarted = true;
            collapseTimer.StartTimer();
            Debug.Log("Countdown gestartet! 10 Sekunden bis zum ersten Tile-Collapse.");
        }

        // 2. Wenn Timer abgelaufen → bei jeder Bewegung Tiles entfernen
        if (collapsePhaseActive)
        {
            CollapseTilesAfterEveryMove();
        }

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


    private void CollapseInitialTiles()
    {
        int amount = Random.Range(4, 6); // 4–5
        CollapseTiles(amount);
    }

    private void CollapseTiles(int amount)
    {
        Grid currentGrid = GetCurrentGrid();
        Vector2Int start = PlayerMovement.currentGridPos;

        for (int i = 1; i <= amount; i++)
        {
            Vector2Int pos = new Vector2Int(start.x, start.y - i);

            if (!currentGrid.IsInsideGrid(pos))
                continue;

            GridObj obj = currentGrid.GetGridArray()[pos.x, pos.y];
            if (obj != null)
            {
                obj.DestroyObj();
            }

            GridObj hole = new GridObj(pos, new WallStatus());
            hole.SetGridType(GridType.HOLE);

            currentGrid.GetGridArray()[pos.x, pos.y] = hole;

        }

        Debug.Log($"{amount} Tiles unterhalb des Spielers zerstört.");
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

    private void TriggerTileCollapse()
    {
        Debug.Log("Timer abgelaufen – Tiles unterhalb des Spielers werden entfernt!");

        Vector2Int start = PlayerMovement.currentGridPos;

        int amount = Random.Range(5, 7); // 5–6 Tiles
        Grid currentGrid = GetCurrentGrid();

        for (int i = 1; i <= amount; i++)
        {
            Vector2Int pos = new Vector2Int(start.x, start.y - i);

            if (!currentGrid.IsInsideGrid(pos)) continue;

            GridObj obj = currentGrid.GetGridArray()[pos.x, pos.y];
            if (obj != null)
            {
                obj.DestroyObj();
                GridObj hole = new GridObj(pos, new WallStatus());
                hole.SetGridType(GridType.HOLE);
                currentGrid.GetGridArray()[pos.x, pos.y] = hole;

            }
        }

        Debug.Log($"{amount} Tiles wurden unterhalb der Spielerposition entfernt.");
    }

}
