using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : Movement
{
    private bool isActive = false;
    private int currentPhase;
    private int stuckFor = 0;
    private int stuckMax = 3;

    public static Opponent INSTANCE { get; private set; }
    private void Awake()
    {
        INSTANCE = this;
        this.gameObject.SetActive(false);
    }
    public void StartCondition()
    {
        StartEnemy();
        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(OnPlayerMove);
    }
    public void EndCondition()
    {
        PlayerMovement.INSTANCE.onPlayerMoved.RemoveListener(OnPlayerMove);
    }
    private void StartEnemy()
    {
        Vector2Int startPos = new Vector2Int(2, 2);
        GridObj startObj = GameManager.INSTANCE.GetCurrentGrid().GetGridArray()[startPos.x, startPos.y];
        Vector3 newPos = startObj.GetWorldPos(GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
        newPos.y = 1;
        this.transform.position = newPos;
        this.gridPos = startObj.GetGridPos();
        isActive = true;
    }
    private void OnPlayerMove(Vector2Int lastPlayerPos, Vector2Int playerPos, WallPos direction, long a)
    {
        MoveEnemy(playerPos);
    }
    /// <summary>
    /// Moving the Enemey
    /// </summary>
    public void MoveEnemy(Vector2Int playerPos)
    {
        if (!isActive) return;
        if (PlayerLose()) return;
        stepCounter++;
        WallPos? direction = EasyStep(playerPos);
        if (direction != null)
        {
            this.RotateModel(direction.Value);
            this.StartMovement(direction.Value, MoveType.WALK);
            PlayerLose();
        }
    }
    private bool PlayerLose()
    {
        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();

        if (playerPos == this.gridPos) 
        { 
            GameManager.INSTANCE.LoseGame("Oh oh, he caught you!");
            return true;
        }
        return false;
    }
    public Vector2Int GetGridPos()
    {
        return gridPos;
    }

    public void SetGridPos(Vector2Int newGridPos)
    {
        this.gridPos = newGridPos;
    }

    private WallPos? EasyStep(Vector2Int playerPos)
    {
        Vector2Int enemyPos = EnemyMovement.INSTANCE.GetEnemyGridPos();
        WallPos? direction = null;
        int diffX = playerPos.x - this.gridPos.x;
        int diffY = playerPos.y - this.gridPos.y;
        if(diffX > diffY)
        {
            direction = CheckX(playerPos, enemyPos, (stuckFor >= stuckMax));
            if(direction == null) direction = CheckY(playerPos, enemyPos, (stuckFor >= stuckMax));
        } else
        {
            direction = CheckY(playerPos, enemyPos, (stuckFor >= stuckMax));
            if (direction == null) direction = CheckX(playerPos, enemyPos, (stuckFor >= stuckMax));
        }
        if (direction == null) stuckFor++;
        return direction;
    }

    private WallPos? CheckX(Vector2Int playerPos, Vector2Int enemyPos)
    {
        WallPos? direction = null;
        if (this.gridPos.x < playerPos.x && IsValidMove(WallPos.RIGHT) == MoveType.WALK && this.GetNextGridPos(WallPos.RIGHT).x != enemyPos.x)
        {
            direction = WallPos.RIGHT;
            stuckFor = 0;
        }
        else if (this.gridPos.x > playerPos.x && IsValidMove(WallPos.LEFT) == MoveType.WALK && this.GetNextGridPos(WallPos.LEFT).x != enemyPos.x)
        {
            direction = WallPos.LEFT;
            stuckFor = 0;
        }
        return direction;
    }

    private WallPos? CheckY(Vector2Int playerPos, Vector2Int enemyPos)
    {
        WallPos? direction = null;
        if (this.gridPos.y < playerPos.y && IsValidMove(WallPos.BACK) == MoveType.WALK && this.GetNextGridPos(WallPos.BACK).y != enemyPos.y)
        {
            direction = WallPos.BACK;
            stuckFor = 0;
        }
        else if (this.gridPos.y > playerPos.y && IsValidMove(WallPos.FRONT) == MoveType.WALK && this.GetNextGridPos(WallPos.FRONT).y != enemyPos.y)
        {
            direction = WallPos.FRONT;
            stuckFor = 0;
        }
        return direction;
    }
    private WallPos? CheckX(Vector2Int playerPos, Vector2Int enemyPos, bool destroyWall)
    {
        if(!destroyWall) return CheckX(playerPos, enemyPos);
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        GridObj currObj = g.GetGridObj(this.gridPos);
        WallPos? direction = null;
        if (this.gridPos.x < playerPos.x && currObj.HasWallAt(WallPos.RIGHT) && g.GetGridObj(this.GetNextGridPos(WallPos.RIGHT)).GetGridType() is not (GridType.MANUAL_REPLACEABLE or GridType.REPLACEABLE) && this.GetNextGridPos(WallPos.RIGHT).x != enemyPos.x)
        {
            currObj.RemoveWall(WallPos.RIGHT);
            g.GetGridObj(this.GetNextGridPos(WallPos.RIGHT)).RemoveWall(WallPos.LEFT);
            direction = WallPos.RIGHT;
            stuckFor = 0;
        }
        else if (this.gridPos.x > playerPos.x && currObj.HasWallAt(WallPos.LEFT) && g.GetGridObj(this.GetNextGridPos(WallPos.LEFT)).GetGridType() is not (GridType.MANUAL_REPLACEABLE or GridType.REPLACEABLE) && this.GetNextGridPos(WallPos.LEFT).x != enemyPos.x)
        {
            currObj.RemoveWall(WallPos.LEFT);
            g.GetGridObj(this.GetNextGridPos(WallPos.LEFT)).RemoveWall(WallPos.RIGHT);
            direction = WallPos.LEFT;
            stuckFor = 0;
        }
        return direction;
    }
    private WallPos? CheckY(Vector2Int playerPos, Vector2Int enemyPos, bool destroyWall)
    {
        if (!destroyWall) return CheckY(playerPos, enemyPos);
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        GridObj currObj = g.GetGridObj(this.gridPos);
        WallPos? direction = null;
        if (this.gridPos.y < playerPos.y && currObj.HasWallAt(WallPos.BACK) && g.GetGridObj(this.GetNextGridPos(WallPos.BACK)).GetGridType() is not (GridType.MANUAL_REPLACEABLE or GridType.REPLACEABLE) && this.GetNextGridPos(WallPos.BACK).y != enemyPos.y)
        {
            currObj.RemoveWall(WallPos.BACK);
            g.GetGridObj(this.GetNextGridPos(WallPos.BACK)).RemoveWall(WallPos.FRONT);
            direction = WallPos.BACK;
            stuckFor = 0;
        }
        else if (this.gridPos.y > playerPos.y && currObj.HasWallAt(WallPos.FRONT) && g.GetGridObj(this.GetNextGridPos(WallPos.FRONT)).GetGridType() is not (GridType.MANUAL_REPLACEABLE or GridType.REPLACEABLE) && this.GetNextGridPos(WallPos.FRONT).y != enemyPos.y)
        {
            currObj.RemoveWall(WallPos.FRONT);
            g.GetGridObj(this.GetNextGridPos(WallPos.FRONT)).RemoveWall(WallPos.BACK);
            direction = WallPos.FRONT;
            stuckFor = 0;
        }
        return direction;
    }
}
