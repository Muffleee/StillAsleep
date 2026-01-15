using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Visibility : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CamFollow camFollow;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private bool PathOnlyGoesAwayWhenZoomedOutANDMoving;
    private Vector2Int lastPlayerGridPos;
    private void Start()
    {
        lastPlayerGridPos = gameManager.GetPlayerMovement().GetCurrentGridPos();
    }

    private void Update()
    {
        UpdatePath();
    }

    private void UpdatePath()
    {
        PlayerMovement player = gameManager.GetPlayerMovement();
        EnemyMovement enemy = EnemyMovement.INSTANCE;
        Vector2Int currentGridPos = player.GetCurrentGridPos();
        
        if(PathOnlyGoesAwayWhenZoomedOutANDMoving)
        {
            if (currentGridPos != lastPlayerGridPos)
            {
                lastPlayerGridPos = currentGridPos;
                bool canSeeEnemy = camFollow.IsPositionVisibleInCamera(enemy.transform.position);
            
                if (canSeeEnemy) pathfinding.SpawnPath(null);
                else pathfinding.SpawnPath(pathfinding.FindPath(currentGridPos, enemy.GetEnemyGridPos()));
            }
        } else
        {
            bool canSeeEnemy = camFollow.IsPositionVisibleInCamera(enemy.transform.position);
            
            if (canSeeEnemy) pathfinding.SpawnPath(null);
            else pathfinding.SpawnPath(pathfinding.FindPath(currentGridPos, enemy.GetEnemyGridPos()));
        }
        
        
    }
}
