using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    
    [SerializeField] protected GameManager gameManager;
    protected Vector2Int lastGridPos;
    protected Vector2Int gridPos;
    /// <summary>
    /// Check if a movement in a given direction is valid.
    /// Validity is based on the tile type.
    /// </summary>
    /// <param name="wallPos">Movement direction to be checked.</param>
    /// <returns></returns>
    protected MoveType IsValidMove(WallPos wallPos)
    {
        Grid cGrid = this.gameManager.GetCurrentGrid();
        Vector2Int next = this.GetNextGridPos(wallPos);
        if (!cGrid.IsInsideGrid(next)) return MoveType.INVALID;

        GridObj nextObj = cGrid.GetGridArray()[next.x, next.y];
        GridObj current = cGrid.GetGridArray()[gridPos.x, gridPos.y];
        return current.GetInteract().IsValidMove(current, nextObj, wallPos);
    }
    /// <summary>
    /// Get the movement vector in world space for a given direction.
    /// </summary>
    /// <param name="wallPos">Direction for which the vector shall be calculated.</param>
    /// <returns></returns>
    protected Vector3 GetMoveDir(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.BACK => new Vector3(0, 0, GridObj.PLACEMENT_FACTOR),
            WallPos.FRONT => new Vector3(0, 0, -GridObj.PLACEMENT_FACTOR),
            WallPos.LEFT => new Vector3(-GridObj.PLACEMENT_FACTOR, 0, 0),
            WallPos.RIGHT => new Vector3(GridObj.PLACEMENT_FACTOR, 0, 0),
            _ => Vector3.zero
        };
    }

    /// <summary>
    /// Get the movement vector in grid space for a given direction.
    /// </summary>
    /// <param name="wallPos">Direction for which the vector shall be calculated.</param>
    /// <returns></returns>
    protected Vector2Int GetMoveDirGrid(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.BACK => new Vector2Int(0, 1),
            WallPos.FRONT => new Vector2Int(0, -1),
            WallPos.LEFT => new Vector2Int(-1, 0),
            WallPos.RIGHT => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// Get the grid position after a move in a given direction.
    /// </summary>
    /// <param name="wallPos">Direction to be checked.</param>
    /// <returns></returns>
    protected Vector2Int GetNextGridPos(WallPos wallPos)
    {
        if (this.gameManager.GetCurrentGrid() == null || !this.gameManager.GetCurrentGrid().IsInstantiated())
        {
            Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return new Vector2Int(0, 0);
        }
        Vector2Int next = gridPos + this.GetMoveDirGrid(wallPos);
        return next;
    }
    /// <summary>
    /// Move the player in a given direction if they aren't already in motion.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="wallPos"></param>
    protected void StartMovement(WallPos wallPos, MoveType mt)
    {
        this.StartCoroutine(this.MovementCoroutine(wallPos, mt));
    }
    /// <summary>
    /// Move the player in a given direction. Set the new currentGridPos and the lastGridPos.
    /// Invoke UnityEvent onPlayerMoved
    /// </summary>
    /// <param name="wallPos">Direction of movement</param>
    /// <returns></returns>
    protected virtual IEnumerator MovementCoroutine(WallPos wallPos, MoveType mt)
    {
        float totalDuration = 0.3f;
        float chargeDuration = mt == MoveType.JUMP ? 0.1f : 0f;
        float moveDuration = totalDuration - chargeDuration;
        float elapsed = 0f;
        Vector3 startPos = this.transform.position;
        Vector3 endPos = startPos + this.GetMoveDir(wallPos);


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

            yield return null;
        }
        this.lastGridPos = this.gridPos;
        gridPos = this.GetNextGridPos(wallPos);
        Debug.Log("Enemy: " + gridPos.x + ", " + gridPos.y);
        this.transform.position = endPos;

        //traps detection on movment 
        Grid cGrid = this.gameManager.GetCurrentGrid();

        // Look up the GridObj using the array accessor method already used in IsValidMove
        GridObj destinationTile = cGrid.GetGridArray()[gridPos.x,gridPos.y];

        destinationTile.GetInteract().OnUse(destinationTile);
    }
}
