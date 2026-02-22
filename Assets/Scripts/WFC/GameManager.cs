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
    [SerializeField] private long MaxGridArea = 1000;
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

    
    private PlayerItems playerItems;

    [Header("Energy Crystals")]
    [SerializeField] private bool enableEnergyCrystals = true;
    [SerializeField, Range(0f, 1f)] private float crystalBaseChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float crystalMinChance = 0.02f;
    [SerializeField, Range(0f, 1f)] private float crystalMaxChance = 0.25f;
    [SerializeField] private float crystalEnergyBias = 1.5f;
    [SerializeField] private int crystalBaseMax = 6;
    [SerializeField] private int crystalBonusMax = 10;



    [Header("Items")]
    [SerializeField] private int timeReversalEnergyCost = 2;
    [SerializeField, Range(1, 10)] private int timeReversalSteps = 3;
    [SerializeField] private float timeReversalCooldown = 8f;

    [SerializeField] private int wallBreakerEnergyCost = 1;
    [SerializeField] private float wallBreakerCooldown = 2f;

    [SerializeField] private int sludgeEnergyCost = 2;
    [SerializeField, Range(1, 10)] private int sludgeStuckSteps = 3;
    [SerializeField] private bool sludgeConsumeOnTrigger = true;


    [Header("New Items")]
    [SerializeField] private int grapplingHookEnergyCost = 2;
    [SerializeField] private float grapplingHookCooldown = 4f;
    [SerializeField, Range(1, 200)] private int grapplingHookMaxRange = 60;

    [SerializeField] private int reflectorShieldEnergyCost = 2;
    [SerializeField, Range(1, 20)] private int reflectorShieldSteps = 5;
    [SerializeField] private float reflectorShieldCooldown = 10f;

    [SerializeField] private int scannerEnergyCost = 2;
    [SerializeField] private float scannerDuration = 4f;
    [SerializeField] private float scannerCooldown = 12f;
    [SerializeField] private Color scannerRevealColor = new Color(1f, 0.65f, 0.1f, 1f);

    private float scannerActiveUntil = 0f;

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
        this.SetStartingWeights();
        this.grid = new Grid(this.width, this.height);
        grid.tutorialUpdate.AddListener(UpdateTutorialText);
        this.playerResources = this.player.GetComponent<PlayerResources>();

        
        this.playerItems = this.player.GetComponent<PlayerItems>();
this.grid.CollapseWorld();
        this.SetWeights();
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos),MaxGridArea);

       // this.grid.CreateExit(new Vector2Int(4, 4), 0, 1);
        this.grid.InstantiateMissing();
        this.gui.FillList();
        // EnemyMovement.INSTANCE.SetEnemyGridPos();
        EnemyMovement.INSTANCE.InstantiateEnemy(new Vector2Int(3,3));
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
    private void SetWeights()
    {
        corridorWeight = this.corridor;
        cornerWeight = this.corner;
        oneWallWeight = this.oneWall;
        emptyWeight = this.empty;
        jumpingWeight = this.jumping;
        manualReplacableWeight = this.manualReplacable;
        trapWeight = this.trap;
        hiddenTrapWeight = this.hiddenTrap;
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
        // Use root object (floors/walls are often children).
        GameObject rootObj = clicked != null ? clicked.transform.root.gameObject : null;
        GridObj targetTile = rootObj != null ? this.grid.GetGridObjFromGameObj(rootObj) : null;

        // If an item mode wants to handle this click (e.g. Sludge placement), do it first.
        if (this.playerItems != null && this.playerItems.TryHandleWorldClick(targetTile))
        {
            return;
        }

        // Default behaviour: place a selected WFC-tile pattern on replaceable tiles.
        GridObj selected = targetTile;
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
    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    

    /// <summary>
    /// Spawns an Energy Crystal on a freshly instantiated REGULAR tile based on player energy.
    /// Centralized here so tuning happens in one place (like WFC weights).
    /// </summary>
    public void TrySpawnEnergyCrystal(GridObj tile, int worldOffsetX, int worldOffsetY)
    {
        if (!enableEnergyCrystals) return;
        if (tile == null) return;
        if (prefabLibrary == null || prefabLibrary.prefabEnergyCrystal == null) return;
        if (playerResources == null) return;
        if (tile.GetGridType() != GridType.REGULAR) return;

        // Avoid division by zero if someone sets MaxEnergy to 0 in the Inspector.
        float denom = Mathf.Max(1, playerResources.MaxEnergy);
        float energyRatio = playerResources.CurrentEnergy / denom; // 0..1

        float spawnChance = crystalBaseChance * (crystalEnergyBias - energyRatio);
        spawnChance = Mathf.Clamp(spawnChance, crystalMinChance, crystalMaxChance);

        int maxCrystals = crystalBaseMax + Mathf.FloorToInt((1f - energyRatio) * crystalBonusMax);
        maxCrystals = Mathf.Max(0, maxCrystals);

        if (UnityEngine.Random.value >= spawnChance) return;

        Vector3 worldPos = tile.GetWorldPos(worldOffsetX, worldOffsetY);
        EnergyCrystal.PrepareSpawn(worldPos, maxCrystals);
        Instantiate(prefabLibrary.prefabEnergyCrystal, worldPos, Quaternion.identity);
    }

public Grid GetCurrentGrid() { return this.grid; }
    public PrefabLibrary GetPrefabLibrary() { return this.prefabLibrary; }
    public PlayerMovement GetPlayerMovement() { return this.playerMovement; }
    public bool IsTutorialOpen() { return this.tutorialOpen; }
    public EnemyMovement GetEnemyMovement() { return this.enemyMovement; }
    public Pathfinding GetPathfinding() { return this.pathfinding; }

    // --- Item tuning getters (centralized like weights) ---
    
    // --------------------- Scanner (Reveal Hidden Traps) ---------------------
    public bool IsScannerActive()
    {
        return Time.time < scannerActiveUntil;
    }

    public void ActivateScanner(float durationSecs)
    {
        scannerActiveUntil = Time.time + Mathf.Max(0f, durationSecs);
        RevealAllHiddenTraps(true);

        CancelInvoke(nameof(DisableScanner));
        Invoke(nameof(DisableScanner), Mathf.Max(0f, durationSecs));
    }

    private void DisableScanner()
    {
        scannerActiveUntil = 0f;
        RevealAllHiddenTraps(false);
    }

    /// <summary>
    /// Called from GridObj.InstantiateObj/ReplaceFloorPrefab so newly spawned HiddenTraps also become visible while scanning.
    /// </summary>
    public void ApplyScannerRevealToTile(GridObj tile)
    {
        if (!IsScannerActive()) return;
        if (tile == null) return;
        if (tile.GetGridType() != GridType.HIDDENTRAP) return;

        tile.SetHiddenTrapReveal(true, scannerRevealColor);
    }

    private void RevealAllHiddenTraps(bool reveal)
    {
        if (grid == null) return;
        GridObj[,] arr = grid.GetGridArray();
        if (arr == null) return;

        int w = arr.GetLength(0);
        int h = arr.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                GridObj t = arr[x, y];
                if (t == null) continue;
                if (t.GetGridType() != GridType.HIDDENTRAP) continue;

                t.SetHiddenTrapReveal(reveal, scannerRevealColor);
            }
        }
    }
public int GetTimeReversalEnergyCost() { return this.timeReversalEnergyCost; }
    public int GetTimeReversalSteps() { return this.timeReversalSteps; }
    public float GetTimeReversalCooldown() { return this.timeReversalCooldown; }

    public int GetWallBreakerEnergyCost() { return this.wallBreakerEnergyCost; }
    public float GetWallBreakerCooldown() { return this.wallBreakerCooldown; }

    public int GetSludgeEnergyCost() { return this.sludgeEnergyCost; }
    public int GetSludgeStuckSteps() { return this.sludgeStuckSteps; }
    public bool GetSludgeConsumeOnTrigger() { return this.sludgeConsumeOnTrigger; }

    // --------------------- New Items Getters ---------------------
    public int GetGrapplingHookEnergyCost() { return grapplingHookEnergyCost; }
    public float GetGrapplingHookCooldown() { return grapplingHookCooldown; }
    public int GetGrapplingHookMaxRange() { return grapplingHookMaxRange; }

    public int GetReflectorShieldEnergyCost() { return reflectorShieldEnergyCost; }
    public int GetReflectorShieldSteps() { return reflectorShieldSteps; }
    public float GetReflectorShieldCooldown() { return reflectorShieldCooldown; }

    public int GetScannerEnergyCost() { return scannerEnergyCost; }
    public float GetScannerDuration() { return scannerDuration; }
    public float GetScannerCooldown() { return scannerCooldown; }
    public Color GetScannerRevealColor() { return scannerRevealColor; }

}

