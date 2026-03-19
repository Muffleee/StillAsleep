using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


/// <summary>
/// Attached to the player, this class handles the player's movement throughout the game.
/// </summary>
public class EnemyMovement : Movement
{
    [SerializeField] private WinScreen winScreen;
    private bool DEBUG = false;
    public UnityEvent lose = new UnityEvent();
    public static EnemyMovement INSTANCE;
    private bool isInstantiated = false;
    int stepCounter = 0;
    private EnemyDifficulty difficulty = new EnemyDifficulty(EnemyDifficultySetting.VERY_EASY);
    private readonly List<Vector2Int> positionHistory = new List<Vector2Int>();
    private Vector2Int? stickyTrapGridPos = null;
    private int stickyTrapTurnsLeft = 0;
    private ItemBoxTrap activeBoxTrap = null; 
    private const int MAX_HISTORY_SIZE = 20;
    private bool isTrapTriggered = false; 

    private void Awake()
    {
        INSTANCE = this;
        this.model = this.gameObject;
        this.gameObject.SetActive(false);
    }
    /// <summary>
    /// Move the player to an initial position and add listeners for any destructible walls.
    /// </summary>
    private void Start()
    {

        this.gridPos = GridObj.WorldPosToGridPos(this.transform.position, this.gameManager.GetCurrentGrid().GetWorldOffsetX(), this.gameManager.GetCurrentGrid().GetWorldOffsetY());
        foreach (var wall in FindObjectsOfType<DestructibleWall>())
        {
            wall.onDestroy.AddListener(this.OnWallDestroyed);
        }
        this.RotateModel(WallPos.FRONT);
    }

    /// <summary>
    /// Instantiating the Enemy
    /// </summary>
    /// <param name="pos"></param>
    public void InstantiateEnemy(Vector2Int pos)
    {
        // If the enemy has a trap when the round resets, destroy it
        if (activeBoxTrap != null)
        {
            Destroy(activeBoxTrap.gameObject);
        }

        // Wipe the enemy's memory of the trap so it doesn't stay stuck
        positionHistory.Clear();
        stickyTrapGridPos = null;
        stickyTrapTurnsLeft = 0;
        activeBoxTrap = null;
        isTrapTriggered = false;

        if (isInstantiated) { ResetFigure(pos); return; }
        if (!gameManager.GetCurrentGrid().IsInsideGrid(pos))
        {
            Debug.LogWarning("You are trying to instantiate the enemy outside of the grid! Don't do that");
            return;
        }
        positionHistory.Clear();
        stickyTrapGridPos = null;
        stickyTrapTurnsLeft = 0;
        activeBoxTrap = null;
        
        this.gridPos = pos;
        Vector3 newPosition = this.gameManager.GetCurrentGrid().GetGridArray()[pos.x, pos.y].GetWorldPos(this.gameManager.GetCurrentGrid().GetWorldOffsetX(), this.gameManager.GetCurrentGrid().GetWorldOffsetY());
        newPosition.y = 1;
        this.transform.position = newPosition;
        this.gameObject.SetActive(true);
        isInstantiated = true;
    }

    public void PlaceStickyTrap(Vector2Int trapGridPos, int stuckTurns, ItemBoxTrap boxTrap = null)
    {
        stickyTrapGridPos = trapGridPos;
        stickyTrapTurnsLeft = Mathf.Max(1, stuckTurns);
        activeBoxTrap = boxTrap;
        isTrapTriggered = false; 
    }

    public void Rewind(int steps)
    {
        if (!isInstantiated) return;
        if (steps <= 0) return;
        if (positionHistory.Count == 0) return;

        StopAllCoroutines();

        int targetIndex = Mathf.Max(positionHistory.Count - steps, 0);
        Vector2Int rewindTarget = positionHistory[targetIndex];

        positionHistory.RemoveRange(targetIndex, positionHistory.Count - targetIndex);
        this.lastGridPos = rewindTarget;
        this.ResetFigure(rewindTarget);
    }

    public Vector2Int GetEnemyGridPos()
    {
        return this.gridPos;
    }
    public void SetEnemyGridPos(Vector2Int newGridPos)
    {
        this.gridPos = newGridPos;
    }

    /// <summary>
    /// Moving the Enemey
    /// </summary>
    public void MoveEnemy()
    {
        if (!isInstantiated) return;

        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        if (playerPos.x == this.gridPos.x && playerPos.y == this.gridPos.y)
        {
            if (this.winScreen != null) this.winScreen.ShowWinScreen();
            return; // Stop doing anything else, the game is won!
        }

        // --- TRAP TRIGGER LOGIC ---
        if (stickyTrapGridPos.HasValue && this.gridPos == stickyTrapGridPos.Value && stickyTrapTurnsLeft > 0)
        {
            if (!isTrapTriggered)
            {
                if (activeBoxTrap != null) activeBoxTrap.ToggleOpen();
                isTrapTriggered = true;
            }

            stickyTrapTurnsLeft--;

            if (stickyTrapTurnsLeft <= 0)
            {
                if (activeBoxTrap != null)
                {
                    Destroy(activeBoxTrap.gameObject);
                    activeBoxTrap = null;
                }
                stickyTrapGridPos = null;
            }
            return; 
        }

        stepCounter++;
        WallPos? direction = GetNextEnemyDir();
        if (direction != null)
        {
            positionHistory.Add(this.gridPos);
            if (positionHistory.Count > MAX_HISTORY_SIZE)
            {
                positionHistory.RemoveAt(0);
            }
            this.RotateModel(direction.Value);
            this.StartMovement(direction.Value, MoveType.WALK);
        }
        GameManager.INSTANCE.AfterEnemyMove();
    }

    /// <summary>
    /// Calculating the best next enemy position
    /// </summary>
    /// <returns></returns>
    private WallPos? GetNextEnemyDir()
    {
        List<WallPos> allowed = new List<WallPos>();
        List<WallPos> destroyNextWall = new List<WallPos>();
        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();


        int diffX = playerPos.x - this.gridPos.x;
        int diffY = playerPos.y - this.gridPos.y;

        Grid thisGrid = this.gameManager.GetCurrentGrid();
        bool found = false;
        WallPos bestDir = new WallPos();

        foreach (WallPos wallPos in Enum.GetValues(typeof(WallPos)))
        {
            if (this.IsValidMove(wallPos) == MoveType.WALK)
            {
                allowed.Add(wallPos);
            }
            else if (stepCounter % this.difficulty.GetDestroyWallsAfter() == 0 && this.IsValidMove(wallPos) == MoveType.JUMP)
            {
                destroyNextWall.Add(wallPos);
            }
        }

        if (allowed.Count <= 0 && destroyNextWall.Count <= 0) return null;
        else if (diffX <= 0 && (allowed.Contains(WallPos.RIGHT) || destroyNextWall.Contains(WallPos.RIGHT)))
        {
            bestDir = WallPos.RIGHT;
            found = true;      
        }
        else if (diffX > 0 && (allowed.Contains(WallPos.LEFT) || destroyNextWall.Contains(WallPos.LEFT)))
        {
            bestDir = WallPos.LEFT;
            found = true;
        }
        else if (diffY <= 0 && (allowed.Contains(WallPos.BACK) || destroyNextWall.Contains(WallPos.BACK)))
        {
            bestDir = WallPos.BACK;
            found = true;
        }
        else if (diffY > 0 && (allowed.Contains(WallPos.FRONT) || destroyNextWall.Contains(WallPos.FRONT)))
        {
            bestDir = WallPos.FRONT;
            found = true;
        }

        if(found)
        {
            List<WallPos> allMoves = new List<WallPos>();
            allMoves.AddRange(allowed);
            allMoves.AddRange(destroyNextWall);

            if (allMoves.Count > 1 && UnityEngine.Random.value < difficulty.MisstepChance())
            {
                bestDir = allMoves[UnityEngine.Random.Range(0, allMoves.Count)];
            }
            
            if (!allowed.Contains(bestDir) && destroyNextWall.Contains(bestDir)) DestroyWallHelper(bestDir);
        } else
        {
            if (allowed.Count == 0)
            {
                bestDir = destroyNextWall[0];
                DestroyWallHelper(bestDir);   
            }
            else
            {
                bestDir = allowed[0];
            }
        }

        return bestDir;
    }
    /// <summary>
    /// Helper for GetNextEnemyDir to destroy walls if wanted
    /// </summary>
    /// <param name="wPos"></param>
    private void DestroyWallHelper(WallPos wPos)
    {
        Vector2Int nextPos = GetNextGridPos(wPos);
            
        this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(wPos);
        this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallStatus.GetOppositePos(wPos));
    }

    protected override MoveType IsValidMove(WallPos wallPos)
    {
        Grid cGrid = this.gameManager.GetCurrentGrid();
        Vector2Int next = this.GetNextGridPos(wallPos);

        if (!cGrid.IsInsideGrid(next) || next == PlayerMovement.INSTANCE.GetCurrentGridPos() || next == lastGridPos) return MoveType.INVALID;

        GridObj nextObj = cGrid.GetGridArray()[next.x, next.y];

        if (nextObj.GetGridType() == GridType.REPLACEABLE) return MoveType.INVALID;

        if (!cGrid.IsInsideGrid(gridPos.x, gridPos.y))
        {
            Debug.Log($"Enemy Movement: GridPos out of bounds: {gridPos.x}, {gridPos.y}");
            return MoveType.INVALID;
        }

        GridObj current = cGrid.GetGridArray()[gridPos.x, gridPos.y];

        if(current.HasWallAt(wallPos) || nextObj.HasWallAt(WallStatus.GetOppositePos(wallPos)))
            return MoveType.JUMP;
        else
            return MoveType.WALK;
    }

    /// <summary>
    /// Called whenever a wall gets destroyed. Removes the respective wall at the WallPos of the GridObj.
    /// </summary>
    /// <param name="gridObj">GridObj of which a wall has been destroyed.</param>
    /// <param name="wallPos">Specific wall side which has been destroyed.</param>
    private void OnWallDestroyed(GridObj gridObj, WallPos wallPos)
    {
        if (gridObj != null)
        {
            gridObj.RemoveWall(wallPos);
            if (this.DEBUG) Debug.Log($"Wand an {wallPos} bei {gridObj} wurde entfernt � Movement-Check aktualisiert.");
        }
    }

    public void SetEnemyDifficulty(EnemyDifficultySetting setting)
    {
        this.difficulty.SetDifficultySetting(setting);
    }

    public EnemyDifficultySetting GetEnemyDifficulty()
    {
        return this.difficulty.GetDifficultySetting();
    }
}

public enum EnemyDifficultySetting
{
    VERY_EASY, EASY, MEDIUM, HARD, VERY_HARD
}

public class EnemyDifficulty
{
    private EnemyDifficultySetting setting;
    
    public EnemyDifficulty(EnemyDifficultySetting setting)
    {
        this.setting = setting;
    }

    public void SetDifficultySetting(EnemyDifficultySetting setting)
    {
        this.setting = setting;
    }

    /// <summary>
    /// Enemy can destroy walls after x many steps
    /// </summary>
    /// <returns></returns>
    public int GetDestroyWallsAfter()
    {
        switch (this.setting)
        {
            case EnemyDifficultySetting.VERY_EASY:
                return 6;
            case EnemyDifficultySetting.EASY:
                return 5;
            case EnemyDifficultySetting.MEDIUM:
                return 4;
            case EnemyDifficultySetting.HARD:
                return 3;
            default:
                return 2;
        }
    }

    /// <summary>
    /// Enemy has x chance of making a random misstep instead of taking the best step
    /// </summary>
    /// <returns></returns>
    public float MisstepChance()
    {
        switch (this.setting)
        {
            case EnemyDifficultySetting.VERY_EASY:
                return 0.55f;
            case EnemyDifficultySetting.EASY:
                return 0.35f;
            case EnemyDifficultySetting.MEDIUM:
                return 0.25f;
            case EnemyDifficultySetting.HARD:
                return 0.15f;
            default:
                return 0f;
        }
    }

    public EnemyDifficultySetting GetDifficultySetting()
    {
        return this.setting;
    }
}