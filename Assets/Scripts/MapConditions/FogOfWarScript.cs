using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarScript : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private int revealRadius = 3;
    [SerializeField] private int enemyRevealRadius = 1;
    public static FogOfWarScript INSTANCE { get; private set; }
    private void Awake() { INSTANCE = this;}
    private bool isActive = true;

    public void RefreshFog(Grid grid, Vector2Int playerPos, Vector2Int enemyPos)
    {   
        if(!isActive) return;
        if (grid == null) return;
        var arr = grid.GetGridArray();
        int w = arr.GetLength(0), h = arr.GetLength(1);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                var obj = arr[x, y];
                if (obj == null || !obj.IsInstantiated() || obj.isRevealed || obj.isFogged) continue;
                obj.SpawnFog();
            }

        // Permanently reveal around player
        RevealAround(grid, playerPos, revealRadius, true);

        // Temporarily clear around enemy 
        RevealAround(grid, enemyPos, enemyRevealRadius, false);
    }

    private void RevealAround(Grid grid, Vector2Int center, int radius, bool permanent)
    {
        var arr = grid.GetGridArray();
        int w = arr.GetLength(0), h = arr.GetLength(1);
        for (int a = -radius; a <= radius; a++)
            for (int b = -radius; b <= radius; b++)
            {
                int x = center.x + a, y = center.y + b;
                if (x < 0 || y < 0 || x >= w || y >= h) continue;
                var obj = arr[x, y];
                if (obj == null) continue;
                if (permanent)
                    obj.MarkRevealed();
                else
                    obj.DestroyFog();
            }
    }

    public void RevealAll()
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        GridObj[,] arr = grid.GetGridArray();
        for (int x = 0; x < arr.GetLength(0); x++)
            for (int y = 0; y < arr.GetLength(1); y++)
                arr[x, y]?.MarkRevealed();
    }

    public void ResetFog()
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        GridObj[,] arr = grid.GetGridArray();
        for (int x = 0; x < arr.GetLength(0); x++)
            for (int y = 0; y < arr.GetLength(1); y++)
                arr[x, y]?.ResetFogState();
    }

    public int  GetRevealRadius() { return revealRadius; }
    public void SetRevealRadius(int radius) { revealRadius = Mathf.Max(0, radius); }
    public void SetIsActive(bool isActive) 
    { 
        this.isActive = isActive; 
        RefreshFog(gameManager.GetCurrentGrid(), PlayerMovement.INSTANCE.GetCurrentGridPos(), EnemyMovement.INSTANCE.GetEnemyGridPos()); 
    }
}