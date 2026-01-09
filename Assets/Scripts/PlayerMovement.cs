using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Attached to the player, this class handles the player's movement throughout the game.
/// </summary>
public class PlayerMovement : Movement
{
    [SerializeField] private WinScreen winScreen;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private PlayerAnim anim;
    public UnityEvent<Vector2Int, Vector2Int, WallPos, long> onPlayerMoved = new UnityEvent<Vector2Int, Vector2Int, WallPos, long>();
    private bool DEBUG = false;
    private int stepCounter = 0;
    private bool isMoving = false;
    private WallPos? bufferedMove = null;
    public static PlayerMovement INSTANCE;

    private void Awake()
    {
        INSTANCE = this;
    }
    /// <summary>
    /// Move the player to an initial position and add listeners for any destructible walls.
    /// </summary>
    private void Start()
    {
        Debug.Log("Player: Grid: " + gameManager.GetCurrentGrid());
        if (gameManager.GetCurrentGrid() != null) this.gridPos = GridObj.WorldPosToGridPos(this.transform.position, this.gameManager.GetCurrentGrid().GetWorldOffsetX(), this.gameManager.GetCurrentGrid().GetWorldOffsetY());
        foreach(var wall in FindObjectsOfType< DestructibleWall >())
        {
            wall.onDestroy.AddListener(this.OnWallDestroyed);
        }
        RotateModel(WallPos.FRONT);
    }

    /// <summary>
    /// Check for the player's input each frame and handles movements accordingly. Only allows one move at a time.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) { this.TryMove(WallPos.BACK); }
        else if (Input.GetKeyDown(KeyCode.S)) { this.TryMove(WallPos.FRONT); }
        else if (Input.GetKeyDown(KeyCode.A)) { this.TryMove(WallPos.LEFT); }
        else if (Input.GetKeyDown(KeyCode.D)) { this.TryMove(WallPos.RIGHT); };
    }

    /// <summary>
    /// Assert whether a movement in a given direction is valid and, if so, execute that move.
    /// </summary>
    /// <param name="wallPos">Direction in which the player wants to move.</param>
    private void TryMove(WallPos wallPos)
    {   
        if(!this.isMoving)
        {   
            MoveType mt = this.IsValidMove(wallPos);
            if (mt != MoveType.INVALID)
            {   
                this.StartMovement(wallPos, mt);
            }
            else
            {
                if(this.DEBUG) Debug.Log("Movement was blocked by wall");
            }
        } else
        {
            this.bufferedMove = wallPos;
        }
    }
    

    // rewrite code so that this returns nearest object and set it when calling this method
    // Not used right now
    private void FindNearestGridObj()
    {
        if (this.gameManager.GetCurrentGrid() == null || !this.gameManager.GetCurrentGrid().IsInstantiated())
        {
            if(this.DEBUG) Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return;
        }

        GridObj nearest = this.gameManager.GetCurrentGrid().GetNearestGridObj(this.transform.position);

        if (nearest != null)
        {
            lastGridPos = this.gridPos;
            this.gridPos = nearest.GetGridPos();
            //gameManager.SetCurrentGridPos(currentGridPos);
            if (this.stepCounter == 0)
                if(this.DEBUG) Debug.Log($"Player steht auf GridObj {nearest.GetGridPos()}");
        }
    }

    /// <summary>
    /// Move the player in a given direction. Set the new currentGridPos and the lastGridPos.
    /// Invoke UnityEvent onPlayerMoved
    /// </summary>
    /// <param name="wallPos">Direction of movement</param>
    /// <returns></returns>
    protected override IEnumerator MovementCoroutine(WallPos wallPos, MoveType mt)
    {   
        float totalDuration = 0.5f;
        float chargeDuration = mt == MoveType.JUMP ? 0.1f : 0f;
        float moveDuration = totalDuration - chargeDuration;
        float elapsed = 0f;
        this.isMoving = true;
        Vector3 startPos = this.transform.position;
        Vector3 endPos = startPos + this.GetMoveDir(wallPos);

        RotateModel(wallPos);
        anim.TriggerMoveAnim(mt);

        yield return null; // use this to get less sliding with the animations

        while (elapsed < totalDuration)
        {   
            elapsed += Time.deltaTime;
            if(elapsed < chargeDuration)
            {
                yield return null;
                continue;
            }
            float time = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            this.transform.position = Vector3.Lerp(startPos, endPos, time);
            
            yield return null;
        }
        this.stepCounter++;

        lastGridPos = this.gridPos;
        this.gridPos = this.GetNextGridPos(wallPos);
        Debug.Log("Player: " + this.gridPos.x + ", " + this.gridPos.y);
        this.transform.position = endPos;
        //traps detection on movment 
        Grid cGrid = this.gameManager.GetCurrentGrid();
    
        // Look up the GridObj using the array accessor method already used in IsValidMove
        GridObj destinationTile = cGrid.GetGridArray()[this.gridPos.x, this.gridPos.y];

        destinationTile.GetInteract().OnUse(destinationTile);
        //if (destinationTile != null && destinationTile.IsTrap()) 
        //{
        //    // Call your dedicated static class to handle the effect
        //    InGameTrapManager.ExecuteTrapEffect(destinationTile); 
        //}
        ////end of trap detection

        //this.CheckForExit(destinationTile);

        this.onPlayerMoved?.Invoke(lastGridPos, this.gridPos, wallPos, this.stepCounter);
        this.gameManager.OnMove(lastGridPos, this.gridPos, wallPos, this.stepCounter);
        if(this.DEBUG) Debug.Log("Event fired");
        this.isMoving = false;
        if(this.DEBUG) Debug.Log(this.stepCounter);
        
        if (bufferedMove.HasValue) 
        {   
            MoveType mtb = this.IsValidMove(bufferedMove.Value);
            if(mtb != MoveType.INVALID) StartCoroutine(MovementCoroutine(bufferedMove.Value, mtb));
            bufferedMove = null;
        } else 
        {
            anim.TriggerMoveAnim(MoveType.INVALID);
        }
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
            if(this.DEBUG) Debug.Log($"Wand an {wallPos} bei {gridObj} wurde entfernt — Movement-Check aktualisiert.");
        }
    }

    //Checks if we went through an exit
    private void CheckForExit(GridObj currentTile)
    {
        if (currentTile == null) return;

        if(currentTile.GetWallAt(WallPos.FRONT) == WallType.EXIT || currentTile.GetWallAt(WallPos.BACK) == WallType.EXIT || currentTile.GetWallAt(WallPos.RIGHT) == WallType.EXIT || currentTile.GetWallAt(WallPos.LEFT) == WallType.EXIT)
        {
            if (this.winScreen != null)
            {
                this.winScreen.ShowWinScreen();
            } else Debug.LogWarning("Kein WinScreen gefunden");  
        }
    }

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
        this.playerModel.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
    }

    public Vector2Int GetCurrentGridPos()
    {
        if (this.gridPos == null)
            this.gridPos = GridObj.WorldPosToGridPos(this.transform.position, this.gameManager.GetCurrentGrid().GetWorldOffsetX(), this.gameManager.GetCurrentGrid().GetWorldOffsetY());
        return this.gridPos;
    }
    public void SetCurrentGridPos(Vector2Int newGridPos)
    {
        this.gridPos = newGridPos;
    }
    public Vector2Int GetLastGridPos()
    {
        if (lastGridPos == null)
            lastGridPos = GetCurrentGridPos();
        return lastGridPos;
    }
    public void SetLastGridPos(Vector2Int newLastGridPos)
    {
        lastGridPos = newLastGridPos;
    }
}


public enum MoveType
{
    INVALID, WALK, JUMP
}