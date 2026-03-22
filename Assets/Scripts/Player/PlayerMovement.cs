using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Attached to the player, this class handles the player's movement throughout the game.
/// </summary>
public class PlayerMovement : Movement
{
    [SerializeField] private WinScreen winScreen;
    [SerializeField] private PauseMenu pausescreen;
    [SerializeField] private PlayerAnim anim;
    [SerializeField] private float playerGroundOffsetY = 0.9f;
    [SerializeField] private float respawnTrapVisualDuration = 1.5f;
    [SerializeField] private float respawnTrapVisualYOffset = 0.01f;
    public UnityEvent<Vector2Int, Vector2Int, WallPos, long> onPlayerMoved = new UnityEvent<Vector2Int, Vector2Int, WallPos, long>();
    private readonly bool DEBUG = false;
    private bool isMoving = false;
    private GameObject respawnTrapVisualObj = null;
    private GameObject respawnHiddenFloorObj = null;
    private Vector3 spawnWorldPos;
    private WallPos? bufferedMove = null;
    private bool isLocked = false;
    private Vector2Int spawnGridPos;
    public static PlayerMovement INSTANCE { get; private set; }

    private void Awake()
    {
        INSTANCE = this;
    }
    /// <summary>
    /// Move the player to an initial position and add listeners for any destructible walls.
    /// </summary>
    private void Start()
    {
        this.gridPos = GridObj.WorldPosToGridPos(this.transform.position, GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
        foreach(var wall in FindObjectsOfType< DestructibleWall >())
        {
            wall.onDestroy.AddListener(this.OnWallDestroyed);
        }
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        Vector3 tileWorld = GridObj.GridPosToWorldPos(gridPos, g.GetWorldOffsetX(), g.GetWorldOffsetY());
        playerGroundOffsetY = transform.position.y - tileWorld.y;
        RotateModel(WallPos.FRONT);
        StartCoroutine(CaptureSpawnAfterInit());
    }
    private System.Collections.IEnumerator CaptureSpawnAfterInit()
    {
        yield return null;
        spawnWorldPos = transform.position;
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        spawnGridPos = GridObj.WorldPosToGridPos(
            this.transform.position,
            g.GetWorldOffsetX(),
            g.GetWorldOffsetY()
        );
    }

    /// <summary>
    /// Check for the player's input each frame and handles movements accordingly. Only allows one move at a time.
    /// </summary>
    private void Update()
    {   
        if(this.isLocked || GameManager.INSTANCE.IsTutorialOpen()) return;

        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            if(this.winScreen.IsWinLoseActive()) return;
            if(this.pausescreen.IsPauseMenuActive())
            {
                this.pausescreen.HidePauseMenu();
            }
            else
            {
                this.pausescreen.ShowPauseMenu();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.winScreen.ShowWinScreen();
        }
        if (Input.GetKeyDown(KeyCode.W)) { this.TryMove(WallPos.BACK, false); }
        else if (Input.GetKeyDown(KeyCode.S)) { this.TryMove(WallPos.FRONT, false); }
        else if (Input.GetKeyDown(KeyCode.A)) { this.TryMove(WallPos.LEFT, false); }
        else if (Input.GetKeyDown(KeyCode.D)) { this.TryMove(WallPos.RIGHT, false); };
    }

    /// <summary>
    /// Assert whether a movement in a given direction is valid and, if so, execute that move.
    /// </summary>
    /// <param name="wallPos">Direction in which the player wants to move.</param>
    private void TryMove(WallPos wallPos, bool isIce)
    {   
        if(!this.isMoving)
        {   
            MoveType mt = this.IsValidMove(wallPos);
            if (mt != MoveType.INVALID)
            {   
                if(isIce) mt = MoveType.SLIDE;
                if(mt == MoveType.TRAP) this.LockMovement(3.292f); // lock for longer animation of trap
                this.StartMovement(wallPos, mt);
            }
            else
            {
                RotateModel(wallPos);
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
        if (GameManager.INSTANCE.GetCurrentGrid() == null || !GameManager.INSTANCE.GetCurrentGrid().IsInstantiated())
        {
            if(this.DEBUG) Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return;
        }

        GridObj nearest = GameManager.INSTANCE.GetCurrentGrid().GetNearestGridObj(this.transform.position);

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
        GameManager.INSTANCE.WhileMove(this.gridPos, this.GetNextGridPos(wallPos), wallPos, stepCounter);
        Vector3 startPos = this.transform.position;
        Vector3 endPos = startPos + this.GetMoveDir(wallPos);

        RotateModel(wallPos);
        anim.TriggerMoveAnim(mt);

        GridObj currentTile = GameManager.INSTANCE.GetCurrentGrid().GetGridArray()[this.gridPos.x, this.gridPos.y];
        Animator animator = currentTile.GetFloorObj().GetComponentInChildren<Animator>();
        if(animator != null) currentTile.GetInteract().TriggerAnimation(animator, mt);

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
        ScoreManager.INSTANCE?.AddScore(-1, false, "Move");

        lastGridPos = this.gridPos;
        this.gridPos = this.GetNextGridPos(wallPos);
        
        this.transform.position = endPos;
        //traps detection on movment 
        Grid cGrid = GameManager.INSTANCE.GetCurrentGrid();
    
        // Look up the GridObj using the array accessor method already used in IsValidMove
        GridObj destinationTile = cGrid.GetGridArray()[this.gridPos.x, this.gridPos.y];

        destinationTile.GetInteract().OnUse(destinationTile);
        if ((destinationTile.GetGridType() == GridType.TRAP || destinationTile.GetGridType() == GridType.HIDDENTRAP) && mt != MoveType.TRAP)
        {
            anim.TriggerMoveAnim(MoveType.TRAP);
            this.LockMovement(3.292f);
        }
        if(destinationTile.GetGridType() == GridType.ICE)
        {
            this.TryMove(wallPos, true);
        }
        //if (destinationTile != null && destinationTile.IsTrap()) 
        //{
        //    // Call your dedicated static class to handle the effect
        //    InGameTrapManager.ExecuteTrapEffect(destinationTile); 
        //}
        ////end of trap detection

        //this.CheckForExit(destinationTile);

        this.onPlayerMoved?.Invoke(lastGridPos, this.gridPos, wallPos, this.stepCounter);
        GameManager.INSTANCE.OnMove(lastGridPos, this.gridPos, wallPos, this.stepCounter);
        if(this.DEBUG) Debug.Log("Event fired");
        this.isMoving = false;
        if(this.DEBUG) Debug.Log(this.stepCounter);
        
        while(this.isLocked)
        {
            yield return null;
        }

        if (bufferedMove.HasValue) 
        {   
            MoveType mtb = this.IsValidMove(bufferedMove.Value);
            if(mtb != MoveType.INVALID) StartCoroutine(MovementCoroutine(bufferedMove.Value, mtb));
            if(mtb == MoveType.TRAP) this.LockMovement(3.292f);
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
    public void RespawnToSpawn()
    {
        CleanupRespawnTrapVisual();

        StopAllCoroutines();
        isMoving = false;
        bufferedMove = null;

        transform.position = spawnWorldPos;

        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        gridPos = GridObj.WorldPosToGridPos(transform.position, g.GetWorldOffsetX(), g.GetWorldOffsetY());
        ShowRespawnTrapVisualAtPlayer();
    }
    private void CleanupRespawnTrapVisual()
    {
        CancelInvoke(nameof(CleanupRespawnTrapVisual));

        if (respawnTrapVisualObj != null)
        {
            Destroy(respawnTrapVisualObj);
            respawnTrapVisualObj = null;
        }

        if (respawnHiddenFloorObj != null)
        {
            respawnHiddenFloorObj.SetActive(true);
            respawnHiddenFloorObj = null;
        }
    }

    private void ShowRespawnTrapVisualAtPlayer()
    {
        CleanupRespawnTrapVisual();

        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        if (g != null)
        {
            GridObj tile = g.GetGridObj(gridPos);
            if (tile != null)
            {
                GameObject floor = tile.GetFloorObj();
                if (floor != null)
                {
                    respawnHiddenFloorObj = floor;
                    floor.SetActive(false);
                }
            }
        }

        GameObject trapPrefab = GameManager.INSTANCE.GetPrefabLibrary().prefabTrap;
        if (trapPrefab == null) return;

        Vector3 spawnPos = transform.position;
        spawnPos.y = respawnTrapVisualYOffset;

        respawnTrapVisualObj = Instantiate(trapPrefab, spawnPos, Quaternion.identity);

        if (g != null)
        {
            GridObj tile = g.GetGridObj(gridPos);
            if (tile != null && tile.GetparentObj() != null)
            {
                respawnTrapVisualObj.transform.SetParent(tile.GetparentObj().transform, true);
            }
        }

        Invoke(nameof(CleanupRespawnTrapVisual), respawnTrapVisualDuration);
    }

    public void TeleportToGridPos(Vector2Int targetGridPos)
    {
        StopAllCoroutines();
        isMoving = false;
        bufferedMove = null;

        lastGridPos = gridPos;
        gridPos = targetGridPos;

        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        Vector3 basePos = GridObj.GridPosToWorldPos(targetGridPos, g.GetWorldOffsetX(), g.GetWorldOffsetY());

        transform.position = basePos + new Vector3(0f, playerGroundOffsetY, 0f);
    }

    public void LockMovement(float timeSecs)
    {
        this.isLocked = true;
        Invoke(nameof(UnlockMovement), timeSecs);
    }

    public void UnlockMovement()
    {
        this.isLocked = false;
    }

    public bool IsLocked() { return this.isLocked; }
    public Vector2Int GetCurrentGridPos()
    {
        if (this.gridPos == null)
            this.gridPos = GridObj.WorldPosToGridPos(this.transform.position, GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
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

    public WallPos GetFacing()
    {
        return this.facing;
    }
    
    public void SetLastGridPos(Vector2Int newLastGridPos)
    {
        lastGridPos = newLastGridPos;
    }
    public void ResetPlayerState()
    {
        StopAllCoroutines(); // Stops any active movement Lerps
        CancelInvoke(nameof(UnlockMovement)); // Kills the 3-second trap timer
        this.isLocked = false;
        this.isMoving = false;
        this.bufferedMove = null;
        
        // Forces the animator back to the Idle state
        if (anim != null) 
        {
            anim.TriggerMoveAnim(MoveType.INVALID); 
        }
    }
    // ------------------------------------------------------------------------------
}


public enum MoveType
{
    INVALID, WALK, JUMP, TRAP, SLIDE
}
