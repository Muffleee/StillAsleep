using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GridObj currentGridObj, lastGridObj;
    [SerializeField] private GameManager gameManager;

    public UnityEvent<GridObj, GridObj, WallPos, long> onPlayerMoved = new UnityEvent<GridObj, GridObj, WallPos, long>();
    private bool DEBUG = false;
    private int stepCounter = 0;
    private bool isMoving = false;
    private void Start()
    {
        FindNearestGridObj();
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

    private void TryMove(WallPos wallPos)
    {
        if (IsValidMove(currentGridObj, wallPos))
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

    // TODO prevent movement onto REPLACEABLE GridObj and fix me please
    private bool IsValidMove(GridObj gridObj, WallPos wallPos)
    {
        return !gridObj.HasWallAt(wallPos);
    }

    private UnityEngine.Vector3 GetMoveDir(WallPos wallPos)
    {
        if (wallPos == WallPos.BACK) { return new Vector3(0,0, GridObj.PLACEMENT_FACTOR); }
        else if (wallPos == WallPos.FRONT) { return new Vector3(0,0, -GridObj.PLACEMENT_FACTOR); }
        else if (wallPos == WallPos.RIGHT) { return new Vector3(GridObj.PLACEMENT_FACTOR, 0,0); }
        else if (wallPos == WallPos.LEFT) { return new Vector3(-GridObj.PLACEMENT_FACTOR, 0,0); };
        return Vector3.zero;
    }
    private void MovePlayer(Vector3 direction, WallPos wallPos)
    {
        if (!isMoving)
        {
            StartCoroutine(MovementCoroutine(direction, wallPos));
        }

    }

    // rewrite code so that this returns nearest object and set it when calling this method
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
            lastGridObj = currentGridObj;
            currentGridObj = nearest;
            if (stepCounter == 0)
                if(DEBUG) Debug.Log($"Player steht auf GridObj {nearest.GetGridPos()}");
        }
    }

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
        FindNearestGridObj();
        transform.position = endPos;
        onPlayerMoved?.Invoke(lastGridObj, currentGridObj, wallPos, stepCounter);
        gameManager.OnMove(lastGridObj, currentGridObj, wallPos, stepCounter);
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
