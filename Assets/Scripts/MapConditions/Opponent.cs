using System;
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
        isActive = false;
        this.gameObject.SetActive(false);
    }

    public void SetDifficulty(int phase)
    {
        switch (phase)
        {
            case < 5: stuckMax = 5; break;
            case < 7: stuckMax = 3; break;
            case < 9: stuckMax = 2; break;
            case < 13: stuckMax = 1; break;
            default: stuckMax = 1; break;
        }
    }
    public void StartCondition()
    {
        StartEnemy();
        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(OnPlayerMove);
    }
    public void EndCondition()
    {
        PlayerMovement.INSTANCE.onPlayerMoved.RemoveListener(OnPlayerMove);
        isActive = false;
        this.transform.gameObject.SetActive(false);
        
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
            this.StartMovement(direction.Value, MoveType.WALK);
            PlayerLose();
        }
    }
    private bool PlayerLose()
    {
        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();

        if (playerPos == this.gridPos) 
        { 
            GameManager.INSTANCE.LoseGame("Well... you were squished!");
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

        if(Math.Abs(diffX) > Math.Abs(diffY))
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
        if (this.gridPos.x < playerPos.x && IsValidMove(WallPos.RIGHT) is (MoveType.WALK or MoveType.TRAP) && this.GetNextGridPos(WallPos.RIGHT).x != enemyPos.x)
        {
            direction = WallPos.RIGHT;
            stuckFor = 0;
        }
        else if (this.gridPos.x > playerPos.x && IsValidMove(WallPos.LEFT) is (MoveType.WALK or MoveType.TRAP) && this.GetNextGridPos(WallPos.LEFT).x != enemyPos.x)
        {
            direction = WallPos.LEFT;
            stuckFor = 0;
        }
        return direction;
    }

    private WallPos? CheckY(Vector2Int playerPos, Vector2Int enemyPos)
    {
        WallPos? direction = null;
        if (this.gridPos.y < playerPos.y && IsValidMove(WallPos.BACK) is (MoveType.WALK or MoveType.TRAP) && this.GetNextGridPos(WallPos.BACK).y != enemyPos.y)
        {
            direction = WallPos.BACK;
            stuckFor = 0;
        }
        else if (this.gridPos.y > playerPos.y && IsValidMove(WallPos.FRONT) is (MoveType.WALK or MoveType.TRAP) && this.GetNextGridPos(WallPos.FRONT).y != enemyPos.y)
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
    private Quaternion GetNextRot(WallPos wPos)
    {
        switch (wPos)
        {
            case WallPos.FRONT:
                return Quaternion.AngleAxis(-90f, Vector3.right) * this.transform.rotation;
            case WallPos.BACK:
                return Quaternion.AngleAxis(90f, Vector3.right) * this.transform.rotation;
            case WallPos.RIGHT:
                return Quaternion.AngleAxis(-90f, Vector3.forward) * this.transform.rotation;
            case WallPos.LEFT:
                return Quaternion.AngleAxis(90f, Vector3.forward) * this.transform.rotation;
            default: return Quaternion.identity;
        }
    }
    protected override IEnumerator MovementCoroutine(WallPos wallPos, MoveType mt)
    {
        float totalDuration = 0.3f;
        float chargeDuration = mt == MoveType.JUMP ? 0.1f : 0f;
        float moveDuration = totalDuration - chargeDuration;
        float elapsed = 0f;
        Vector3 startPos = this.transform.position;
        Vector3 endPos = startPos + this.GetMoveDir(wallPos);
        Quaternion startRot = this.transform.rotation;
        Quaternion endRot = this.GetNextRot(wallPos);

        this.lastGridPos = this.gridPos;
        this.gridPos = this.GetNextGridPos(wallPos);

        yield return null; // use this to get less sliding with the animations

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            if (elapsed < chargeDuration)
            {
                yield return null;
                continue;
            }
            float time = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            this.transform.position = Vector3.Lerp(startPos, endPos, time);
            this.transform.rotation = Quaternion.Slerp(startRot, endRot, time);

            yield return null;
        }
        this.transform.position = endPos;

    }

    public bool IsActive() { return isActive; }
}
