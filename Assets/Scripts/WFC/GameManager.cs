using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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
    [SerializeField] private int corridor = 0;
    [SerializeField] private int corner = 0;
    [SerializeField] private int oneWall = 0;
    [SerializeField] private int empty = 0;
    [SerializeField] private PrefabLibrary prefabLibrary;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemyMovement enemyMovement;

    public static int emptyWeight;
    public static int corridorWeight;
    public static int cornerWeight;
    public static int oneWallWeight;
    public static GameManager INSTANCE;

    [SerializeField] private GameObject player;
    private PlayerResources playerResources;

    public static List<GridObj> AllGridObjs = new List<GridObj>();

    /*
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject destructibleWallPrefab;
    public GameObject exitPrefab;

    public GameObject energyCrystalPrefab;
    */

    Grid grid;

    /// <summary>
    /// Initializes the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        INSTANCE = this;
        corridorWeight = this.corridor;
        cornerWeight = this.corner;
        oneWallWeight = this.oneWall;
        emptyWeight = this.empty;
        this.grid = new Grid(this.width, this.height);

        this.playerResources = this.player.GetComponent<PlayerResources>();

        this.grid.CollapseWorld();
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos));

       // this.grid.CreateExit(new Vector2Int(4, 4), 0, 1);
        this.grid.InstantiateMissing();
        this.gui.FillList();
        // EnemyMovement.INSTANCE.SetEnemyGridPos();
        EnemyMovement.INSTANCE.InstantiateEnemy(new Vector2Int(1,1));
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
        enemyMovement.MoveEnemy();
        if(step % this.replaceExitAfter == 0)
        {
            //this.grid.RepositionExit(to);
        }
        Vector2Int enemyGridPos = EnemyMovement.INSTANCE.GetEnemyGridPos();
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        var enemyEdgeAndDistance = this.grid.GetClosestEdgeAndDistance(this.grid.GetEdgeDistances(enemyGridPos.x, enemyGridPos.y));
        Debug.Log("Enemy generation: " + enemyEdgeAndDistance.first);
        var playerEdgeAndDistance = this.grid.GetClosestEdgeAndDistance(this.grid.GetEdgeDistances(currentGridPos.x, currentGridPos.y));
        Debug.Log("Player generation: " + playerEdgeAndDistance.first);
        this.generateAfter = math.max(enemyEdgeAndDistance.second, 2);
        if (step % this.generateAfter == 0 && this.grid.ShouldGenerate(5, enemyGridPos))
        {
            this.grid.CollapseWorld();
            this.grid.IncreaseGrid(this.grid.GetNextGenPos(enemyGridPos));
            this.grid.InstantiateMissing();

            this.gui.FillList();
        }
        if (enemyEdgeAndDistance.first != playerEdgeAndDistance.first)
        {
            Debug.Log("generating for player");
            this.generateAfter = math.max(playerEdgeAndDistance.second, 2);
            if (step % this.generateAfter == 0 && this.grid.ShouldGenerate(5, currentGridPos))
            {
                this.grid.CollapseWorld();
                this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos));
                this.grid.InstantiateMissing();

                this.gui.FillList();
            }
        }
    }

    /// <summary>
    /// Function to be called whenever the player clicks in the world, handles placing player-selected tiles
    /// </summary>
    /// <param name="clicked">Clicked game object</param>
    public void OnClick(GameObject clicked)
    {
        GridObj selected = this.grid.GetGridObjFromGameObj(clicked);
        if (selected == null || (selected.GetGridType() != GridType.REPLACEABLE && selected.GetGridType() != GridType.MANUAL_REPLACEABLE)) return;
        if (!this.gui.HasSelectedObj()) return;

        GridObj virtualObj = this.gui.GetSelected();

        int cost = virtualObj.PlacementCost;

        if (!this.playerResources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie!");
            return;
        }
        this.playerResources.Spend(cost);

        GridObj toPlace = new GridObj(selected.GetGridPos(), virtualObj.GetWallStatus());
        toPlace.UpdateWallStatus(this.grid.GetNeighbors(toPlace));
        this.grid.PlaceObj(toPlace);

        this.gui.RemoveSelected(false);
    }

    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    public Grid GetCurrentGrid() { return this.grid; }
    public PrefabLibrary GetPrefabLibrary() { return this.prefabLibrary; }
    public PlayerMovement GetPlayerMovement() { return this.playerMovement; }
}
