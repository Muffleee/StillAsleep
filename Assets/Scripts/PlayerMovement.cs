using System.Collections;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Attached to the player, this class handles the player's movement throughout the game.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public static Vector2Int currentGridPos, lastGridPos;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private WinScreen winScreen;

    public UnityEvent<Vector2Int, Vector2Int, WallPos, long> onPlayerMoved = new UnityEvent<Vector2Int, Vector2Int, WallPos, long>();
    private bool DEBUG = false;
    private int stepCounter = 0;
    private bool isMoving = false;
    
    /// <summary>
    /// Move the player to an initial position and add listeners for any destructible walls.
    /// </summary>
    private void Start()
    {
        currentGridPos = GridObj.WorldPosToGridPos(this.transform.position, gameManager.GetCurrentGrid().GetWorldOffsetX(), gameManager.GetCurrentGrid().GetWorldOffsetY());
        foreach(var wall in FindObjectsOfType< DestructibleWall >())
        {
            wall.onDestroy.AddListener(OnWallDestroyed);
        }
    }

    /// <summary>
    /// Check for the player's input each frame and handles movements accordingly. Only allows one move at a time.
    /// </summary>
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
    /// Assert whether a movement in a given direction is valid and, if so, execute that move.
    /// </summary>
    /// <param name="wallPos">Direction in which the player wants to move.</param>
    private void TryMove(WallPos wallPos)
    {
        if (IsValidMove(wallPos))
        {
            MovePlayer(wallPos);
        }
        else
        {
            if(DEBUG) Debug.Log("Movement was blocked by wall");
        }
        return;
    }

    /// <summary>
    /// Check if a movement in a given direction is valid.
    /// Validity is based on the tile type.
    /// </summary>
    /// <param name="wallPos">Movement direction to be checked.</param>
    /// <returns></returns>
    private bool IsValidMove(WallPos wallPos)
    {   
        //if(true) return true; // TODO fix this script lol
        Grid cGrid = gameManager.GetCurrentGrid();
        Vector2Int next = GetNextGridPos(wallPos);
        if (!cGrid.IsInsideGrid(next)) return false;

        GridObj nextObj = cGrid.GetGridArray()[next.x, next.y];
        
        GridObj current = cGrid.GetGridArray()[currentGridPos.x, currentGridPos.y];
        return current.GetInteract().IsValidMove(current, nextObj, wallPos);
    }

    /// <summary>
    /// Get the movement vector in world space for a given direction.
    /// </summary>
    /// <param name="wallPos">Direction for which the vector shall be calculated.</param>
    /// <returns></returns>
    private Vector3 GetMoveDir(WallPos wallPos)
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
    private Vector2Int GetMoveDirGrid(WallPos wallPos)
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
    /// Move the player in a given direction if they aren't already in motion.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="wallPos"></param>
    private void MovePlayer(WallPos wallPos)
    {
        if (!isMoving)
        {
            StartCoroutine(MovementCoroutine(wallPos));
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
    /// Get the grid position after a move in a given direction.
    /// </summary>
    /// <param name="wallPos">Direction to be checked.</param>
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
    /// Move the player in a given direction. Set the new currentGridPos and the lastGridPos.
    /// Invoke UnityEvent onPlayerMoved
    /// </summary>
    /// <param name="wallPos">Direction of movement</param>
    /// <returns></returns>
    private IEnumerator MovementCoroutine(WallPos wallPos)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + GetMoveDir(wallPos);

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
        
        CheckForExit(destinationTile);

        onPlayerMoved?.Invoke(lastGridPos, currentGridPos, wallPos, stepCounter);
        gameManager.OnMove(lastGridPos, currentGridPos, wallPos, stepCounter);
        if(DEBUG) Debug.Log("Event fired");
        isMoving = false;
        if(DEBUG) Debug.Log(stepCounter);
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
            if(DEBUG) Debug.Log($"Wand an {wallPos} bei {gridObj} wurde entfernt — Movement-Check aktualisiert.");
        }
    }

    //Checks if we went through an exit
    private void CheckForExit(GridObj currentTile)
    {
        if (currentTile == null) return;

        if(currentTile.GetWallAt(WallPos.FRONT) == WallType.EXIT || currentTile.GetWallAt(WallPos.BACK) == WallType.EXIT || currentTile.GetWallAt(WallPos.RIGHT) == WallType.EXIT || currentTile.GetWallAt(WallPos.LEFT) == WallType.EXIT)
        {
            if (winScreen != null)
            {
                winScreen.ShowWinScreen();
            } else Debug.LogWarning("Kein WinScreen gefunden");  
        }
    }
}