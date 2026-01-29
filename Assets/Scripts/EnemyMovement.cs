using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Attached to the player, this class handles the player's movement throughout the game.
/// </summary>
public class EnemyMovement : Movement
{
    [SerializeField] private WinScreen winScreen;
    [SerializeField] private int destroyWall = 3;
    private bool DEBUG = false;
    public UnityEvent lose = new UnityEvent();
    public static EnemyMovement INSTANCE;
    private bool isInstantiated = false;
    int stepCounter = 0;
    
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
        if (!gameManager.GetCurrentGrid().IsInsideGrid(pos))
        {
            Debug.LogWarning("You are trying to instantiate the enemy outside of the grid! Don't do that");
            return;
        }
        this.gridPos = pos;
        Vector3 newPosition = this.gameManager.GetCurrentGrid().GetGridArray()[pos.x, pos.y].GetWorldPos();
        newPosition.y = 1;
        this.transform.position = newPosition;
        this.gameObject.SetActive(true);
        isInstantiated = true;
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
        stepCounter++;
        WallPos? direction = GetNextEnemyDir();
        if (direction != null)
        {   
            this.RotateModel(direction.Value);
            this.StartMovement(direction.Value, MoveType.WALK);
        }
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
        WallPos wPos = new WallPos();

        foreach (WallPos wallPos in Enum.GetValues(typeof(WallPos)))
        {
            if (this.IsValidMove(wallPos) == MoveType.WALK)
            {
                allowed.Add(wallPos);
            }
            else if (stepCounter % destroyWall == 0 && this.IsValidMove(wallPos) == MoveType.JUMP)
            {
                destroyNextWall.Add(wallPos);
            }
        }
        if (diffX == 0 && diffY == 0)
            {
                this.winScreen.ShowWinScreen();
                return null;
            }
            else if (allowed.Count <= 0 && destroyNextWall.Count <= 0) return null;
            else if (diffX <= 0 && (allowed.Contains(WallPos.RIGHT) || destroyNextWall.Contains(WallPos.RIGHT)))
            {
                wPos = WallPos.RIGHT;
                if(!allowed.Contains(WallPos.RIGHT) && destroyNextWall.Contains(WallPos.RIGHT)) DestroyWallHelper(WallPos.RIGHT);
                
            }
            else if (diffX > 0 && (allowed.Contains(WallPos.LEFT) || destroyNextWall.Contains(WallPos.LEFT)))
            {
                wPos = WallPos.LEFT;
                if (!allowed.Contains(WallPos.LEFT) && destroyNextWall.Contains(WallPos.LEFT)) DestroyWallHelper(WallPos.LEFT);

            }
            else if (diffY <= 0 && (allowed.Contains(WallPos.BACK) || destroyNextWall.Contains(WallPos.BACK)))
            {
                wPos = WallPos.BACK;
                if (!allowed.Contains(WallPos.BACK) && destroyNextWall.Contains(WallPos.BACK)) DestroyWallHelper(WallPos.BACK);
            
            }
            else if (diffY > 0 && (allowed.Contains(WallPos.FRONT) || destroyNextWall.Contains(WallPos.FRONT)))
            {
                wPos = WallPos.FRONT;
                if (!allowed.Contains(WallPos.FRONT) && destroyNextWall.Contains(WallPos.FRONT)) DestroyWallHelper(WallPos.FRONT);
            }
            else
            {
                if (allowed.Count == 0)
                {
                    wPos = destroyNextWall[0];
                    DestroyWallHelper(wPos);
                    
                }
                else
                {
                    wPos = allowed[0];
                }
            }

        return wPos;
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

}
