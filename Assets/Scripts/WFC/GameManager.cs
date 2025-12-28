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
    [SerializeField] private int jumping = 0;
    [SerializeField] private int replacable = 0;
    [SerializeField] private int trap = 0;
    [SerializeField] private PrefabLibrary prefabLibrary;
    [SerializeField] private PlayerMovement playerMovement;

    public static int emptyWeight;
    public static int corridorWeight;
    public static int cornerWeight;
    public static int oneWallWeight;
    public static int jumpingWeight;
    public static int replacableWeight;
    public static int trapWeight;
    public static GameManager INSTANCE;

    [SerializeField] private GameObject player;

    public static List<GridObj> AllGridObjs = new List<GridObj>();
    private Queue<(GridObj, string)> tutorials = new Queue<(GridObj, string)>();
    bool tutorialOpen = false;
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
        this.SetStartingWeights();
        this.grid = new Grid(this.width, this.height);
        grid.tutorialUpdate.AddListener(UpdateTutorialText);
        this.grid.CollapseWorld();
        this.SetWeights();
        this.grid.IncreaseGrid(this.grid.GetNextGenPos());

        this.grid.CreateExit(new Vector2Int(4, 4), 0, 1);
        this.grid.InstantiateMissing();
        this.gui.FillList();
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
        replacableWeight = 0;
        trapWeight = 0;
    }
    /// <summary>
    /// sets the static weights
    /// </summary>
    private void SetWeights()
    {
        corridorWeight = this.corridor;
        cornerWeight = this.corner;
        oneWallWeight = this.oneWall;
        emptyWeight = this.empty;
        jumpingWeight = this.jumping;
        replacableWeight = this.replacable;
        trapWeight = this.trap;
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
        if(step % this.replaceExitAfter == 0)
        {
            this.grid.RepositionExit(to);
        }

        this.generateAfter = math.max(this.grid.GetClosestEdgeAndDistance(this.grid.GetPlayerToEdgeDistances()).second, 2);
        if (step % this.generateAfter == 0 && this.grid.ShouldGenerate(5))
        {
            this.grid.CollapseWorld();
            this.grid.IncreaseGrid(this.grid.GetNextGenPos());
            this.grid.InstantiateMissing();

            this.gui.FillList();
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

        PlayerResources pr = this.player.GetComponent<PlayerResources>();
        int cost = virtualObj.PlacementCost;

        if (!pr.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie!");
            return;
        }
        pr.Spend(cost);

        GridObj toPlace = new GridObj(selected.GetGridPos(), virtualObj.GetWallStatus());
        Dictionary<WallPos, GridObj> neighbors = new Dictionary<WallPos, GridObj>() { { WallPos.FRONT, this.grid.GetAdjacentGridObj(toPlace, WallPos.FRONT) },
                                                                                            { WallPos.BACK, this.grid.GetAdjacentGridObj(toPlace, WallPos.BACK) },
                                                                                            { WallPos.LEFT, this.grid.GetAdjacentGridObj(toPlace, WallPos.LEFT) },
                                                                                            { WallPos.RIGHT, this.grid.GetAdjacentGridObj(toPlace, WallPos.RIGHT) } };
        toPlace.UpdateWallStatus(neighbors);
        this.grid.PlaceObj(toPlace);

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
    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    public Grid GetCurrentGrid() { return this.grid; }
    public PrefabLibrary GetPrefabLibrary() { return this.prefabLibrary; }
    public PlayerMovement GetPlayerMovement() { return this.playerMovement; }
    public bool IsTutorialOpen() { return this.tutorialOpen; }
}
