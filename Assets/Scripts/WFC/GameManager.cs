using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main game manager class, handles game initialization, world generation, and move and click events
/// </summary>
public class GameManager : MonoBehaviour
{   
    public static int emptyWeight;
    public static int corridorWeight;
    public static int cornerWeight;
    public static int oneWallWeight;
    public static int jumpingWeight;
    public static int manualReplacableWeight;
    public static int trapWeight;
    public static int hiddenTrapWeight;
    public static int iceWeight;
    public static int rotatingWeight;
    public static int spikeWeight;
    public static GameManager INSTANCE;

    [Header("General Settings")]
    [SerializeField] int generateAfter = 4;
    [SerializeField] int replaceExitAfter = 2;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private long MaxGridArea = 50;
    [SerializeField] private IngameUI gui;

    [Header("Weight Balancing")]
    [SerializeField] private int corridor = 0;
    [SerializeField] private int corner = 0;
    [SerializeField] private int oneWall = 0;
    [SerializeField] private int empty = 0;
    [SerializeField] private int jumping = 0;
    [SerializeField] private int manualReplacable = 0;
    [SerializeField] private int trap = 0;
    [SerializeField] private int hiddenTrap = 0;
    [SerializeField] private int ice = 0;
    [SerializeField] private int rotating = 0;
    [SerializeField] private int spike = 0;
    [SerializeField] private bool enableEnergyCrystals = true;
    [SerializeField, Range(0f, 1f)] private float crystalBaseChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float crystalMinChance = 0.02f;
    [SerializeField, Range(0f, 1f)] private float crystalMaxChance = 0.25f;
    [SerializeField] private float crystalEnergyBias = 1.5f;
    [SerializeField] private int crystalBaseMax = 6;
    [SerializeField] private int crystalBonusMax = 10;

    [Header("Item Spawning")]
    [SerializeField] private bool enableItemSpawning = true;
    [SerializeField, Range(0f, 1f)] private float itemSpawnChance = 0.05f; 
    [SerializeField] private GameObject[] spawnableItemPrefabs; 
    
    [Header("References")]
    [SerializeField] private PrefabLibrary prefabLibrary;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private FogOfWarScript fogCondition;
    [SerializeField] private GameObject Audio;
    [SerializeField] private WinScreen WinScreen;
    [SerializeField] private GameObject player;
    [SerializeField] private TutorialManager tutManager;
    [SerializeField] private PlayerAnim anim;

    private PlayerResources playerResources;
    private List<IMapCondition> allMapConditions = new List<IMapCondition> { new FogOfWarCon(), new CountdownCond(), new OpponentCon()};
    private IMapCondition currentCond;

    [HideInInspector] public UnityEvent NoCrystals = new UnityEvent();
    public static List<GridObj> AllGridObjs = new List<GridObj>();
    private bool tutorial = false;

    private int phase;
    private int round;
    private Grid grid;
    private const bool DEBUG = false;

    private void Awake()
    {
        INSTANCE = this;
        if (AudioManager.Instance == null)
        {
            Instantiate(Audio);
        }
        this.tutorial = MainMenu.tutorial;
    }
    /// <summary>
    /// Initializes the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        fogCondition.SetIsActive(false);
        
        
        this.grid = new Grid(this.width, this.height);
        this.playerResources = this.player.GetComponent<PlayerResources>();
        if (tutorial)
        {
            tutManager.StartTutorial(grid);
        }
        else
        {
            NewPhase();
        }
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
                iceWeight = this.ice;
                rotatingWeight = this.rotating;
                spikeWeight = this.spike;
                break;
            case WeightType.CLOSED:
                corridorWeight = 10;
                cornerWeight = 12;
                oneWallWeight = 3;
                emptyWeight = 2;
                jumpingWeight = 8;
                manualReplacableWeight = 4;
                trapWeight = 3;
                hiddenTrapWeight = 4;
                iceWeight = 7;
                rotatingWeight = 7;
                spikeWeight = 8;
                break;
            case WeightType.OPEN:
                corridorWeight = 2;
                cornerWeight = 3;
                oneWallWeight = 7;
                emptyWeight = 12;
                jumpingWeight = 3;
                manualReplacableWeight = 10;
                trapWeight = 8;
                hiddenTrapWeight = 3;
                iceWeight = 4;
                rotatingWeight = 4;
                spikeWeight = 4;
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
                iceWeight = 0;
                rotatingWeight = 0;
                spikeWeight = 0;
                break;
        }
        
    }
    /// <summary>
    /// if the player clicks the left mouse button, the tutorial text closes and opens the next one if one is in line
    /// </summary>
    private void Update()
    {
        
    }

    public void WhileMove(Vector2Int from, Vector2Int to, WallPos direction, long step)
    {
        if (step % 2 == 0)
        {
            this.grid.Spike();
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
        if (tutorial) return;
        enemyMovement.MoveEnemy();
        this.RefreshFog();
        this.grid.RotateTiles();

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
            this.RefreshFog();

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
                this.RefreshFog();

                this.gui.FillList();
            }
        }
    }

    public void AfterEnemyMove()
    {
        
    }
    /// <summary>
    /// Make the player lose the game
    /// </summary>
    public void LoseGame(string loseMessage = "Game Over!")
    {   
        this.anim.TriggerAnim("TriggerLose");
        this.playerMovement.LockMovement(2f);
        StartCoroutine(LoseGameAfterTime(2f, loseMessage));
    }

    private IEnumerator LoseGameAfterTime(float delay, string loseMessage)
    {
        yield return new WaitForSeconds(delay);
        if (tutorial || tutManager.IsInEndphase()) tutManager.OnLose();
        else WinScreen.ShowLoseScreen(loseMessage);
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
            if(DEBUG) Debug.Log("Nicht genug Energie!");
            AudioManager.Instance.PlayNoCrystal();
            this.NoCrystals.Invoke();
            return;
        }
        this.playerResources.Spend(cost);

        GridObj toPlace = new GridObj(selected.GetGridPos(), virtualObj.GetWallStatus().Clone());
        toPlace.UpdateWallStatus(this.grid.GetNeighbors(toPlace));
        this.grid.PlaceObj(toPlace);
        if (tutorial) tutManager.PlacedTile();
        AudioManager.Instance.PlayTilePlacing();

        this.gui.RemoveSelected(false);
    }

    /// <summary>
    /// Spawns an Energy Crystal on a freshly instantiated REGULAR tile based on player energy.
    /// Centralized here so tuning happens in one place (like WFC weights).
    /// </summary>
    public bool TrySpawnEnergyCrystal(GridObj tile, int worldOffsetX, int worldOffsetY)
    {
        if (!enableEnergyCrystals) return false;
        if (tile == null) return false;
        if (prefabLibrary == null || prefabLibrary.prefabEnergyCrystal == null) return false;
        if (playerResources == null) return false;
        if (tile.GetGridType() != GridType.REGULAR) return false;
        if (tutorial) { tutManager.SpawnCrystalOnObject(tile, worldOffsetX, worldOffsetY); return true; }

        float denom = Mathf.Max(1, playerResources.MaxEnergy);
        float energyRatio = playerResources.CurrentEnergy / denom; // 0..1

        float spawnChance = crystalBaseChance * (crystalEnergyBias - energyRatio);
        spawnChance = Mathf.Clamp(spawnChance, crystalMinChance, crystalMaxChance);

        int maxCrystals = crystalBaseMax + Mathf.FloorToInt((1f - energyRatio) * crystalBonusMax);
        maxCrystals = Mathf.Max(0, maxCrystals);

        if (UnityEngine.Random.value >= spawnChance) return false;

        Vector3 worldPos = tile.GetWorldPos(worldOffsetX, worldOffsetY);
        EnergyCrystal.PrepareSpawn(worldPos, maxCrystals);
        Instantiate(prefabLibrary.prefabEnergyCrystal, worldPos, Quaternion.identity);
        return true;
    }
    

  public bool TrySpawnItem(GridObj tile, int worldOffsetX, int worldOffsetY)
    {
        if (tutorial) return true;
        if (!enableItemSpawning) return false;
        if (tile == null || tile.GetGridType() != GridType.REGULAR) return false;
        // Prevent items from spawning on the player's current position ---
        if (PlayerMovement.INSTANCE != null && tile.GetGridPos() == PlayerMovement.INSTANCE.GetCurrentGridPos())
        {
            return false; 
        }
        if (spawnableItemPrefabs == null || spawnableItemPrefabs.Length == 0)
        {
            Debug.LogWarning("Item Spawning is enabled, but the prefab array is empty!");
            return false;
        }

        // 1. Check if ANY item should spawn on this tile
        if (UnityEngine.Random.value > itemSpawnChance) return false;

        // 2. Calculate the total weight
        int totalWeight = 0;
        foreach (GameObject prefab in spawnableItemPrefabs)
        {
            if (prefab != null)
            {
                ItemPickup pickupComponent = prefab.GetComponent<ItemPickup>();
                if (pickupComponent != null && pickupComponent.GetItemData() != null)
                {
                    totalWeight += pickupComponent.GetItemData().GetSpawnWeight();
                }
            }
        }

        if (totalWeight == 0)
        {
            Debug.LogWarning("Total item spawn weight is 0! Make sure prefabs have ItemPickup attached and Item Data assigned.");
            return false;
        }

        // 3. Roll the dice
        int randomRoll = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        GameObject selectedPrefab = null;

        // 4. Find the winner
        foreach (GameObject prefab in spawnableItemPrefabs)
        {
            if (prefab != null)
            {
                ItemPickup pickupComponent = prefab.GetComponent<ItemPickup>();
                if (pickupComponent != null && pickupComponent.GetItemData() != null)
                {
                    cumulativeWeight += pickupComponent.GetItemData().GetSpawnWeight();
                    if (randomRoll < cumulativeWeight)
                    {
                        selectedPrefab = prefab;
                        break;
                    }
                }
            }
        }

        // 5. Spawn it!
        if (selectedPrefab != null)
        {
            Vector3 worldPos = tile.GetWorldPos(worldOffsetX, worldOffsetY);
            worldPos.y += 0.5f;
            Instantiate(selectedPrefab, worldPos, Quaternion.identity);
            if(DEBUG) Debug.Log($"Spawned a {selectedPrefab.name} at {worldPos}"); // Confirms it worked!
            return true;
        }
        return false;
    }

    public void OnWin(WeightType weightType)
    {
        if (tutorial || tutManager.IsInEndphase()) { tutManager.OnWin(); return; }
        // Free the player from any active trap animations or locks
        if (PlayerMovement.INSTANCE != null)
        {
            PlayerMovement.INSTANCE.ResetPlayerState();
        }
        
        if(this.round % 3 == 0)
        {
            NewPhase();
            ScoreManager.INSTANCE.AddScore(this.phase * 500, true, "New Phase");
        } else
        {
            NewRound(weightType);
            ScoreManager.INSTANCE?.AddScore(100, true, "New Round");
        }
    }
    public void NewPhase()
    {
        if(currentCond != null) currentCond.Deactivate();
        this.grid.DestroyGrid();
        EnergyCrystal.DestroyAllCrystals();
        playerResources.ResetEnergy();
        this.SetWeights(WeightType.START);
        this.grid.SetNewGrid(this.width, this.height);
        this.grid.CollapseWorld();
        this.SetWeights(WeightType.NORMAL);
        PlayerMovement.INSTANCE.ResetFigure(new Vector2Int(2,0));
        
        Vector2Int currentGridPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        this.grid.IncreaseGrid(this.grid.GetNextGenPos(currentGridPos), MaxGridArea);

        this.grid.InstantiateMissing();
        this.gui.FillList();
        ChangeEnemyMovement();
        //if(this.phase != 0) 
            NextCondition();
        this.phase++;
        NewRound(WeightType.START);
    }
    private void ChangeEnemyMovement()
    {
        switch (this.phase)
        {
            case 0: EnemyMovement.INSTANCE.SetEnemyDifficulty(EnemyDifficultySetting.VERY_EASY); break;
            case 2: EnemyMovement.INSTANCE.SetEnemyDifficulty(EnemyDifficultySetting.EASY); break;
            case 4: EnemyMovement.INSTANCE.SetEnemyDifficulty(EnemyDifficultySetting.MEDIUM); break;
            case 6: EnemyMovement.INSTANCE.SetEnemyDifficulty(EnemyDifficultySetting.HARD); break;
            case 8: EnemyMovement.INSTANCE.SetEnemyDifficulty(EnemyDifficultySetting.VERY_HARD); break;
            default: break;
        }
    }
    private void NextCondition()
    {
        if (allMapConditions.Count <= 0) return;
        List<IMapCondition> possible = allMapConditions.Where(n => n.Difficulty() <= this.phase).ToList();
        if(possible.Count <= 0) return;
        Unity.Mathematics.Random rnd = new Unity.Mathematics.Random((uint)Environment.TickCount);
        int index = rnd.NextInt(possible.Count);
        possible[index].Initiate(this.phase);
        currentCond = possible[index];
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
        PlaceEnemy();
        this.round = (this.round + 1) % 3;

    }

    private void PlaceEnemy()
    {
        Vector2Int enemyPos = new Vector2Int(0, 0);
        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        if (playerPos.x > (this.grid.width / 2))
        {
            for (int i = 3; i >= 0; i--)
            {
                enemyPos.x = playerPos.x - i;
                if (this.grid.IsInsideGrid(enemyPos)) break;
            }
        }
        else
        {
            for (int i = 3; i >= 0; i--)
            {
                enemyPos.x = playerPos.x + i;
                if (this.grid.IsInsideGrid(enemyPos)) break;
            }
        }
        if (playerPos.y > (this.grid.height / 2))
        {
            for (int i = 3; i >= 0; i--)
            {
                enemyPos.y = playerPos.y - i;
                if (this.grid.IsInsideGrid(enemyPos)) break;
            }
        }
        else
        {
            for (int i = 3; i >= 0; i--)
            {
                enemyPos.y = playerPos.y + i;
                if (this.grid.IsInsideGrid(enemyPos)) break;
            }
        }
        EnemyMovement.INSTANCE.InstantiateEnemy(enemyPos);
    }

    private void RefreshFog()
    {
        if (fogCondition == null) return;
        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        Vector2Int enemyPos = EnemyMovement.INSTANCE.GetEnemyGridPos();
        fogCondition.RefreshFog(this.grid, playerPos, enemyPos);
    }
    /// <summary>
    /// Gets the grid in its current state
    /// </summary>
    /// <returns>Grid</returns>
    public Grid GetCurrentGrid() { return this.grid; }
    public PrefabLibrary GetPrefabLibrary() { return this.prefabLibrary; }
    public PlayerMovement GetPlayerMovement() { return this.playerMovement; }
    public EnemyMovement GetEnemyMovement() { return this.enemyMovement; }
    public Pathfinding GetPathfinding() { return this.pathfinding; }
    public TutorialManager GetTutManager() { return this.tutManager; }
    public IMapCondition GetMapCondition(int index) { return allMapConditions[index]; }
    public bool IsMovingDisabled() { return tutManager.IsMovingDisabled(); }
    public bool IsPlacingDisabled() { return tutManager.IsPlacingDisabled(); }
    public bool IsTutorialCurrently() { return tutorial; }
    public void SetTutorialCurrently(bool tut) { tutorial = tut; }
    public int GetRound() { return this.round; }
    public int GetPhase() {  return this.phase; }
    public GameObject GetItemPrefab(int index) { return spawnableItemPrefabs[index]; }
    public void ResetPhaseRound() {  this.phase = 0; this.round = 0; }
    public IMapCondition GetCurrentCondition() { return this.currentCond; }
}

public enum WeightType
{
    OPEN, CLOSED, NORMAL, START
}