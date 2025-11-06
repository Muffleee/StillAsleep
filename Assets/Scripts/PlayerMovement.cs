using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GridObj CurrentGridObj;

    public UnityEvent<GridObj, WallPos, long> onPlayerMoved = new UnityEvent<GridObj, WallPos, long>();
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
        if (isValidMove(CurrentGridObj, wallPos))
        {
            Vector3 direction = GetMoveDir(wallPos);

            MovePlayer(direction, wallPos);
        }
        else
        {
            Debug.Log("Movement was blocked by wall");
        }
        return;
    }

    private bool isValidMove(GridObj gridObj, WallPos wallPos)
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
    private void MovePlayer(Vector3 direction,WallPos wallPos )
    {
        if (!isMoving)
        {
            StartCoroutine(MovementCoroutine(direction,wallPos));
        }
        
    }

    private void FindNearestGridObj()
    {
        if (WFCBuilder2.AllGridObjs == null || WFCBuilder2.AllGridObjs.Count == 0)
        {
            Debug.LogWarning("Keine GridObjekte gefunden. Ist das Level schon generiert?");
            return;
        }

        float minDist = Mathf.Infinity;
        GridObj nearest = null;

        foreach (var g in WFCBuilder2.AllGridObjs)
        {
            float dist = Vector3.Distance(transform.position, g.GetWorldPos());
            if (dist < minDist)
            {
                minDist = dist;
                nearest = g;
            }
        }

        if (nearest != null)
        {
            CurrentGridObj = nearest;
            if (stepCounter == 0)
                Debug.Log($"Player steht auf GridObj {nearest.GetGridPos()}");
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
        onPlayerMoved?.Invoke(CurrentGridObj, wallPos, stepCounter);
        Debug.Log("Event fired");
        isMoving = false;
        Debug.Log(stepCounter);
    }
    
    private void OnWallDestroyed(GridObj gridObj, WallPos wallPos)
    {
        if (gridObj != null)
        {
            gridObj.RemoveWall(wallPos);
            Debug.Log($"Wand an {wallPos} bei {gridObj} wurde entfernt — Movement-Check aktualisiert.");
        }
    }


}
