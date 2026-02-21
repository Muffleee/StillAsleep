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
    [SerializeField] private long MaxGridArea = 50;
    [SerializeField] private IngameUI gui;
    [SerializeField] private int corridor = 0;
    [SerializeField] private int corner = 0;
    [SerializeField] private int oneWall = 0;
    [SerializeField] private int empty = 0;
    [SerializeField] private int jumping = 0;
    [SerializeField] private int manualReplacable = 0;
    [SerializeField] private int trap = 0;
    [SerializeField] private int hiddenTrap = 0;
    [SerializeField] private PrefabLibrary prefabLibrary;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private GameObject Audio;

    public static int emptyWeight;
    public static int corridorWeight;
    public static int cornerWeight;
    public static int oneWallWeight;
    public static int jumpingWeight;
    public static int manualReplacableWeight;
    public static int trapWeight;
    public static int hiddenTrapWeight;
    public static GameManager INSTANCE;

    [SerializeField] private GameObject player;
    private PlayerResources playerResources;

    public static List<GridObj> AllGridObjs = new List<GridObj>();
    private Queue<(GridObj, string)> tutorials = new Queue<(GridObj, string)>();
    bool tutorialOpen = false;

    private int phase;
    private int round;
    /*
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject destructibleWallPrefab;
    public GameObject exitPrefab;

    public GameObject energyCrystalPrefab;
    */

    Grid grid;

    private void Awake()
    {
        if (AudioManager.Instance == null)
        {
            Instantiate(Audio);
        }
    }
    /// <summary>
    /// Initializes the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        
        INSTANCE = this;
        
        this.grid = new Grid(this.width, this.height);
        grid.tutorialUpdate.AddListener(UpdateTutorialText);
        this.playerResources = this.player.GetComponent<PlayerResources>();

        NewPhase();
    }
    /// <summary>
    /// Sets starting weights so the initial grid is very open and no special tiles
    /// </summary>
    private void SetStartingWeights()
    {
        emptyWeight = 20;
        corridorWeight = 5;
        cornerWeight = 2;
        oneWallWeight = 1;
        jumpingWeight = 0;
        manualReplacableWeight = 0;
        trapWeight = 0;
        hiddenTrapWeight = 0;
    }
    /// <summary>
    /// sets the static weights
    /// </summary>
    private void SetWeights(WeightType weightType)
    {
        switch (weightType)
        {
            case WeightType.NORMAL:
                corridorWeight = this.corridor;
                cornerWeight = this.corner;
                oneWallWeight = this.oneWall;
                emptyWeight = this.empty;
                jumpingWeight = this.jumping;
                manualReplacableWeight = this.manualReplacable;
                trapWeight = this.trap;
                hiddenTrapWeight = this.hiddenTrap;
                break;
            case WeightType.CLOSED:
                corridorWeight = 9;
                cornerWeight = 10;
                oneWallWeight = 4;
                emptyWeight = 2;
                jumpingWeight = 8;
                manualReplacableWeight = 4;
                trapWeight = 3;
                hiddenTrapWeight = 4;
                break;
            case WeightType.OPEN:
                corridorWeight = 3;
                cornerWeight = 2;
                oneWallWeight = 10;
                emptyWeight = 12;
                jumpingWeight = 3;
                manualReplacableWeight = 10;
                trapWeight = 8;
                hiddenTrapWeight = 3;
                break;
            case WeightType.START:
                emptyWeight = 20;
                corridorWeight = 5;
                cornerWeight = 2;
                oneWallWeight = 1;
                jumpingWeight = 0;
                manualReplacableWeight = 0;
                trapWeight = 0;
                hiddenTrapWeight = 0;
                break;
        }
        
    }
    /// <summary>
    /// if the player clicks the left mouse button, the tutorial text closes and opens the next one if one is in line
    /// </summary>
    private void Update()
    {
        if (tutorialOpen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gui.CloseTutorialText();
                tutorialOpen = false;
                if (tutorials.Count > 0)
                {
                    tutorialOpen = true;
                    (GridObj, string) next= tutorials.Dequeue();
                    gui.OpenTutorialText(next.Item1.GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY()), next.Item2);
                }
            }
        }
        
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
        GridObj toObj = this.grid.GetGridObj(to);
        if(toObj != null && toObj.GetGridType() == GridType.TRAP)
        {
            this.playerMovement.LockMovement(2f);
        }
        if(step % this.replaceExitAfter == 0)
        {
            //this.grid.RepositionExit(to);
        }
        Vector2Int enemyGridPos = EnemyMovement.INSTANCE.GetEnemyGridPos();
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        var enemyEdgeAndDistance = this.grid.GetClosestEdgeAndDistance(this.grid.GetEdgeDistances(enemyGridPos.x, enemyGridPos.y));
        var playerEdgeAndDistance = this.grid.GetClosestEdgeAndDistance(this.grid.GetEdgeDistances(currentGridPos.x, currentGridPos.y));
        this.generateAfter = math.max(enemyEdgeAndDistance.second, 2);
        if (step % this.generateAfter == 0 && this.grid.ShouldGenerate(5, enemyGridPos))
        {
            this.grid.CollapseWorld();
            this.grid.IncreaseGrid(this.grid.GetNextGenPos(enemyGridPos),MaxGridArea);
            this.grid.InstantiateMissing();

            this.gui.FillList();
        }
        if (enemyEdgeAndDistance.first != playerEdgeAndDistance.first)
        {
            this.generateAfter = math.max(playerEdgeAndDistance.second, 2);
            if (step % this.generateAfter == 0 && this.grid.ShouldGenerate(5, currentGridPos))
            {
                this.grid.CollapseWorld();
                this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos),MaxGridArea);
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
        AudioManager.Instance.PlayTilePlacing();

        this.gui.RemoveSelected(false);
    }
    /// <summary>
    /// Calls a function in gui to set the tutorial text if one is not already open
    /// enqeues the tutorial to the line
    /// </summary>
    /// <param name="text"></param>
    private void UpdateTutorialText(GridObj obj, string text)
    {
        tutorials.Enqueue((obj,text));
        if (tutorialOpen) return;
        (GridObj, string) next = tutorials.Dequeue();
        gui.OpenTutorialText(next.Item1.GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY()), next.Item2);
        tutorialOpen = true;
    }

    public void OnWin(WeightType weightType)
    {
        if(this.round % 3 == 0)
        {
            Debug.Log("You win!");
            NewPhase();
        } else
        {
            NewRound(weightType);
        }
    }
    private void NewPhase()
    {
        this.grid.DestroyGrid();
        EnergyCrystal.DestroyAllCrystals();
        playerResources.ResetEnergy();
        this.SetStartingWeights();
        this.grid.SetNewGrid(this.width, this.height);
        this.grid.CollapseWorld();
        this.SetWeights(WeightType.NORMAL);
        PlayerMovement.INSTANCE.ResetFigure(new Vector2Int(0,0));
        
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos), MaxGridArea);

        this.grid.InstantiateMissing();
        this.gui.FillList();
        EnemyMovement.INSTANCE.InstantiateEnemy(new Vector2Int(3,3));
        NextCondition();
        NewRound(WeightType.START);
    }
    private void NextCondition()
    {
        Debug.Log("Setting next condition");
    }
    /// <summary>
    /// Increase the maxGridArea each round
    /// Increase EnemyDifficulty each round
    /// Influence the WFC Algorithm
    /// </summary>
    private void NewRound(WeightType weights)
    {
        switch (this.round)
        {
            case 0:
                MaxGridArea = 75;
                break;
            case 1:
                MaxGridArea = 500;
                break;
            case 2:
                MaxGridArea = 1000;
                break;
            default:
                MaxGridArea = 1000;
                break;
        }
        SetWeights(weights);
        this.round = (this.round + 1) % 3;
        Debug.Log(this.round);

    }
    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    public Grid GetCurrentGrid() { return this.grid; }
    public PrefabLibrary GetPrefabLibrary() { return this.prefabLibrary; }
    public PlayerMovement GetPlayerMovement() { return this.playerMovement; }
    public bool IsTutorialOpen() { return this.tutorialOpen; }
    public EnemyMovement GetEnemyMovement() { return this.enemyMovement; }
    public Pathfinding GetPathfinding() { return this.pathfinding; }
}

public enum WeightType
{
    OPEN, CLOSED, NORMAL, START
}
