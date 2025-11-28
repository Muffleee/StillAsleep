using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public static Vector2Int currentGridPos, lastGridPos;
    [SerializeField] private GameManager gameManager;

    public UnityEvent<Vector2Int, Vector2Int, WallPos, long> onPlayerMoved = new UnityEvent<Vector2Int, Vector2Int, WallPos, long>();
    private bool DEBUG = false;
    private int stepCounter = 0;
    private bool isMoving = false;
    private void Start()
    {
        currentGridPos = GridObj.WorldPosToGridPos(this.transform.position, gameManager.GetCurrentGrid().GetGrowthIndex());
        foreach(var wall in FindObjectsOfType< DestructibleWall >())
        {
            wall.onDestroy.AddListener(OnWallDestroyed);
        }
    }

    private void Update()
    {
        if (isMoving)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W)) { TryMove(WallPos.BACK); }
        else if (Input.GetKeyDown(KeyCode.S)) { TryMove(WallPos.FRONT); }
        else if (Input.GetKeyDown(KeyCode.A)) { TryMove(WallPos.LEFT); }
        else if (Input.GetKeyDown(KeyCode.D)) { TryMove(WallPos.RIGHT); };
    }

    /// <summary>
    /// if the move in direction of wallPos is valif, call MovePlayer
    /// </summary>
    /// <param name="wallPos"></param>
    private void TryMove(WallPos wallPos)
    {
        if (IsValidMove(wallPos))
        {
            Vector3 direction = GetMoveDir(wallPos);

            MovePlayer(direction, wallPos);
        }
        else
        {
            if(DEBUG) Debug.Log("Movement was blocked by wall");
        }
        return;
    }

    /// <summary>
    /// checks: - is there a wall in direction of wallPos?
    /// - is the next gridObj to move to a replacable?
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    private bool IsValidMove(WallPos wallPos)
    {
        Grid cGrid = gameManager.GetCurrentGrid();
        Vector2Int next = GetNextGridPos(wallPos);
        if (!cGrid.IsInsideGrid(next)) return false;

        GridObj nextObj = cGrid.GetGridArray()[next.x, next.y];
        
        GridObj current = cGrid.GetGridArray()[currentGridPos.x, currentGridPos.y];
        return current.GetInteract().IsValidMove(current, nextObj, wallPos);
    }

    /// <summary>
    /// the offset (for movement) in a specific direction (wallPos)
    /// In WorldPos
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    private UnityEngine.Vector3 GetMoveDir(WallPos wallPos)
    {
        if (wallPos == WallPos.BACK) { return new Vector3(0,0, GridObj.PLACEMENT_FACTOR); }
        else if (wallPos == WallPos.FRONT) { return new Vector3(0,0, -GridObj.PLACEMENT_FACTOR); }
        else if (wallPos == WallPos.RIGHT) { return new Vector3(GridObj.PLACEMENT_FACTOR, 0,0); }
        else if (wallPos == WallPos.LEFT) { return new Vector3(-GridObj.PLACEMENT_FACTOR, 0,0); };
        return Vector3.zero;
    }

    /// <summary>
    /// the offset that needs to be moved depending on the wall position - in gridPos, not worldPos!
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    private Vector2Int GetMoveDirGrid(WallPos wallPos)
    {
        Vector2Int moveGrid = new Vector2Int(0, 0);
        switch (wallPos)
        {
            case WallPos.FRONT:
                moveGrid = new Vector2Int(0, -1);
                break;
            case WallPos.BACK:
                moveGrid = new Vector2Int(0, 1);
                break;
            case WallPos.LEFT:
                moveGrid = new Vector2Int(-1, 0);
                break;
            case WallPos.RIGHT:
                moveGrid = new Vector2Int(1, 0);
                break;

        }
        return moveGrid;
    }

    /// <summary>
    /// Moving the player if it's not already in motion
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="wallPos"></param>
    private void MovePlayer(Vector3 direction, WallPos wallPos)
    {
        if (!isMoving)
        {
            StartCoroutine(MovementCoroutine(direction, wallPos));
        }

    }

    // rewrite code so that this returns nearest object and set it when calling this method
    // Not used right now
    private void FindNearestGridObj()
    {
        if (gameManager.GetCurrentGrid() == null || !gameManager.GetCurrentGrid().IsInstantiated())
        {
            if(DEBUG) Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return;
        }

        GridObj nearest = gameManager.GetCurrentGrid().GetNearestGridObj(transform.position);

        if (nearest != null)
        {
            lastGridPos = currentGridPos;
            currentGridPos = nearest.GetGridPos();
            //gameManager.SetCurrentGridPos(currentGridPos);
            if (stepCounter == 0)
                if(DEBUG) Debug.Log($"Player steht auf GridObj {nearest.GetGridPos()}");
        }
    }

    /// <summary>
    /// Get the next gridPos depending on the wallPosition
    /// </summary>
    /// <param name="wallPos"> equals the direction in which the player wants to go</param>
    /// <returns></returns>
    private Vector2Int GetNextGridPos(WallPos wallPos)
    {
        if (gameManager.GetCurrentGrid() == null || !gameManager.GetCurrentGrid().IsInstantiated())
        {
            if (DEBUG) Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return new Vector2Int(0,0);
        }
        Vector2Int next = currentGridPos + GetMoveDirGrid(wallPos);
        return next;
    }

    

    /// <summary>
    /// Moving the player by direction. Setting the new currentGridPos and the lastGridPos.
    /// Invoking Unity Event onPlayerMoved
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    private IEnumerator MovementCoroutine(Vector3 direction, WallPos wallPos)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction;

        while (elapsed < duration)
        {
            float time = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(startPos, endPos, time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        stepCounter++;

        lastGridPos = currentGridPos;
        currentGridPos = GetNextGridPos(wallPos);

        transform.position = endPos;

        //traps detection on movment 
        Grid cGrid = gameManager.GetCurrentGrid();
    
        // Look up the GridObj using the array accessor method already used in IsValidMove
        GridObj destinationTile = cGrid.GetGridArray()[currentGridPos.x, currentGridPos.y];

        destinationTile.GetInteract().OnUse(destinationTile);
        //if (destinationTile != null && destinationTile.IsTrap()) 
        //{
        //    // Call your dedicated static class to handle the effect
        //    InGameTrapManager.ExecuteTrapEffect(destinationTile); 
        //}
        ////end of trap detection

        onPlayerMoved?.Invoke(lastGridPos, currentGridPos, wallPos, stepCounter);
        gameManager.OnMove(lastGridPos, currentGridPos, wallPos, stepCounter);
        if(DEBUG) Debug.Log("Event fired");
        isMoving = false;
        if(DEBUG) Debug.Log(stepCounter);
    }
    
    private void OnWallDestroyed(GridObj gridObj, WallPos wallPos)
    {
        if (gridObj != null)
        {
            gridObj.RemoveWall(wallPos);
            if(DEBUG) Debug.Log($"Wand an {wallPos} bei {gridObj} wurde entfernt — Movement-Check aktualisiert.");
        }
    }


}
