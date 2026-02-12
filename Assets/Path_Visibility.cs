using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Visibility : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CamFollow camFollow;
    [SerializeField] private Pathfinding pathfinding;

    private void FixedUpdate()
    {
        UpdatePath();
    }

    private void UpdatePath()
    {   
        EnemyMovement enemy = EnemyMovement.INSTANCE;
        bool canSeeEnemy = camFollow.IsPositionVisibleInCamera(enemy.transform.position);
        
        if(canSeeEnemy) 
        {
            pathfinding.SpawnPath(new List<GridObj>());
            return;    
        }
        
        PlayerMovement player = gameManager.GetPlayerMovement();
        Vector2Int currentGridPos = player.GetCurrentGridPos();
        pathfinding.SpawnPath(pathfinding.FindPath(currentGridPos, enemy.GetEnemyGridPos()));
    }
}
