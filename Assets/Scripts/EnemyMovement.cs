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
        Debug.Log("new enemyGridPos: " + newGridPos.x + ", " +  newGridPos.y);
        this.gridPos = newGridPos;
    }
    public void MoveEnemy()
    {
        if (!isInstantiated) return;
        WallPos? direction = GetNextEnemyDir();
        if (direction != null)
        {   
            this.RotateModel(direction.Value);
            this.StartMovement(direction.Value, MoveType.WALK);
        }
        else
        {
            Debug.Log("Enemy can't move anywhere: " + this.gridPos.x + ", " + this.gridPos.y);
        }
        
    }
    private WallPos? GetNextEnemyDir()
    {
        List<WallPos> allowed = new List<WallPos>();

        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();

        Debug.Log("Enemy Position: " + this.gridPos.x + ", " + this.gridPos.y);

        int diffX = playerPos.x - this.gridPos.x;
        int diffY = playerPos.y - this.gridPos.y;

        Debug.Log($"x-difference: {diffX}, y-difference: {diffY}");

        WallPos wPos = new WallPos();

        foreach (WallPos wallPos in Enum.GetValues(typeof(WallPos)))
        {
            if(this.IsValidMove(wallPos) == MoveType.WALK && GetNextGridPos(wallPos) != lastGridPos && GetNextGridPos(wallPos) != playerPos)
            {
                allowed.Add(wallPos);
            }
        }
        if (diffX == 0 && diffY == 0)
        {
            this.winScreen.ShowWinScreen();
            return null;
        }
        else if (allowed.Count <= 0) return null;
        else if (diffX <= 0 && allowed.Contains(WallPos.RIGHT)) wPos = WallPos.RIGHT;
        else if (diffX > 0 && allowed.Contains(WallPos.LEFT)) wPos = WallPos.LEFT;
        else if (diffY <= 0 && allowed.Contains(WallPos.BACK)) wPos = WallPos.BACK;
        else if (diffY > 0 && allowed.Contains(WallPos.FRONT)) wPos = WallPos.FRONT;
        else wPos = allowed[0];
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
