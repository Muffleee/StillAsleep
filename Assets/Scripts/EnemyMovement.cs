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
    private bool DEBUG = false;
    public UnityEvent lose = new UnityEvent();
    public static EnemyMovement INSTANCE;
    private bool isInstantiated = false;
    int stepCounter = 0;
    [SerializeField] public int destroyWall = 3;

    private void Awake()
    {
        INSTANCE = this;
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


        WallPos wPos = new WallPos();
        
        foreach (WallPos wallPos in Enum.GetValues(typeof(WallPos)))
        {
            if (this.IsValidMove(wallPos) == MoveType.WALK && GetNextGridPos(wallPos) != lastGridPos && GetNextGridPos(wallPos) != playerPos)
            {
                Debug.Log("Adding " + wallPos + "to be allowed");
                allowed.Add(wallPos);
            }
        }
        if (stepCounter % destroyWall == 0)
        {
            foreach (WallPos wallPos in Enum.GetValues(typeof(WallPos)))
            {
                Vector2Int nextPos = GetNextGridPos(wallPos);
                Grid thisGrid = this.gameManager.GetCurrentGrid();
                if (!thisGrid.IsInsideGrid(nextPos)) continue;
                if (nextPos != lastGridPos && nextPos != playerPos 
                    && thisGrid.GetGridArray()[nextPos.x, nextPos.y].GetGridType() != GridType.REPLACEABLE 
                    && thisGrid.GetGridArray()[nextPos.x, nextPos.y].GetGridType() != GridType.MANUAL_REPLACEABLE 
                    && thisGrid.GetGridArray()[this.gridPos.x, this.gridPos.y].HasWallAt(wallPos))
                {
                    Debug.Log("Adding " + wallPos + "to be maybe destroyed");
                    destroyNextWall.Add(wallPos);
                }
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
            if (allowed.Contains(WallPos.RIGHT))
            {
                wPos = WallPos.RIGHT;
                Debug.Log("Choosing: " + wPos);
            }
            else if (destroyNextWall.Contains(WallPos.RIGHT))
            {
                Vector2Int nextPos = GetNextGridPos(WallPos.RIGHT);
                wPos = WallPos.RIGHT;
                Debug.Log("planning to destroy wall: " + wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(WallPos.RIGHT);
                this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallPos.LEFT);
            }
        }
        else if (diffX > 0 && (allowed.Contains(WallPos.LEFT) || destroyNextWall.Contains(WallPos.LEFT)))
        {
            if (allowed.Contains(WallPos.LEFT))
            {
                wPos = WallPos.LEFT;
                Debug.Log("Choosing: " + wPos);
            }
            else if (destroyNextWall.Contains(WallPos.LEFT))
            {
                Vector2Int nextPos = GetNextGridPos(WallPos.LEFT);
                wPos = WallPos.LEFT;
                Debug.Log("planning to destroy wall: " + wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(WallPos.LEFT);
                this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallPos.RIGHT);
            }
        }
        else if (diffY <= 0 && (allowed.Contains(WallPos.BACK) || destroyNextWall.Contains(WallPos.BACK)))
        {
            if (allowed.Contains(WallPos.BACK))
            {
                wPos = WallPos.BACK;
                Debug.Log("Choosing: " + wPos);
            }
            else if (destroyNextWall.Contains(WallPos.BACK))
            {
                Vector2Int nextPos = GetNextGridPos(WallPos.BACK);
                wPos = WallPos.BACK;
                Debug.Log("planning to destroy wall: " + wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(WallPos.BACK);
                this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallPos.FRONT);
            }
        }
        else if (diffY > 0 && (allowed.Contains(WallPos.FRONT) || destroyNextWall.Contains(WallPos.FRONT)))
        {
            if (allowed.Contains(WallPos.FRONT))
            {
                wPos = WallPos.FRONT;
                Debug.Log("Choosing: " + wPos);
            }
            else if (destroyNextWall.Contains(WallPos.FRONT))
            {
                Vector2Int nextPos = GetNextGridPos(WallPos.FRONT);
                wPos = WallPos.FRONT;
                Debug.Log("planning to destroy wall: " + wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(WallPos.FRONT);
                this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallPos.BACK);
            }
        }
        else
        {
            if(allowed.Count == 0)
            {
                wPos = destroyNextWall[0];
                Debug.Log("planning to destroy wall: " + wPos);
                Vector2Int nextPos = GetNextGridPos(wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y].RemoveWall(wPos);
                this.gameManager.GetCurrentGrid().GetGridArray()[nextPos.x, nextPos.y].RemoveWall(WallStatus.GetOppositePos(wPos));
            } else
            {
                wPos = allowed[0];
            }
            Debug.Log("sorry no other is good! Choosing: " + wPos);
        }

        Debug.Log("choosing next direction: " + wPos);
        return wPos;
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

    /// <summary>
    /// Rotate the enemy model
    /// </summary>
    /// <param name="dir"></param>
    private void RotateModel(WallPos dir)
    {
        int rotation;
        switch (dir)
        {
            case WallPos.FRONT:
                rotation = 180;
                break;
            case WallPos.LEFT:
                rotation = -90;
                break;
            case WallPos.RIGHT:
                rotation = 90;
                break;
            default:
                rotation = 0;
                break;
        }
        this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
    }
}
