using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

/// <summary>
/// Class handling the gane's grid and its procedual generation through Wave Function Collapse.
/// </summary>
public class Grid
{
    public int width => grid.GetLength(0);
    public int height => grid.GetLength(1);
    private GridObj[,] grid;

    /// <summary>
    /// Count the times the grid has grown so far.
    /// </summary>
    private int growthIndex = 0;
    private Exit exit;

    /// <summary>
    /// Create a new grid given an initial size.
    /// </summary>
    /// <param name="width">Initial grid width</param>
    /// <param name="height">Initial grid height</param>
    public Grid(int width, int height)
    {
        grid = new GridObj[width, height];
    }

    /// <summary>
    /// Place a GridObj in the grid at the GridObj's current world position. Will destroy any existing GridObj at this position.
    /// </summary>
    /// <param name="gridObj">GridObj to be placed</param>
    public void PlaceObj(GridObj gridObj)
    {
        PlaceObj(gridObj, gridObj.GetWorldPos(growthIndex));
    }

    /// <summary>
    /// Place a GridObj in the grid at the given world position. Will destroy any existing GridObj at this position.
    /// </summary>
    /// <param name="gridObj">GridObj to be placed</param>
    /// <param name="pos">World position at which the GridObj is to be placed</param>
    public void PlaceObj(GridObj gridObj, Vector3 pos)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(pos, growthIndex);

        if (grid[gridPos.x, gridPos.y] != null)
        {
            grid[gridPos.x, gridPos.y].DestroyObj();
        }
        
        gridObj.SetGridPos(gridPos);
        grid[gridPos.x, gridPos.y] = gridObj;
        if (gridObj.GetInteract() == null) gridObj.InitType(GridType.REGULAR);
        grid[gridPos.x, gridPos.y].InstantiateObj(growthIndex);
        InstantiateMissingWalls(gridObj);
    }

    /// <summary>
    /// Instantiate any missing walls in the GridObjs adjacent to the given GridObj.
    /// </summary>
    /// <param name="gridObj"></param>
    private void InstantiateMissingWalls(GridObj gridObj)
    {
        WallPos[] wallPos = new WallPos[] { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT };
        foreach (WallPos wPos in wallPos)
        {
            GridObj neighbour = GetAdjacentGridObj(gridObj.GetGridPos(), wPos);
            if (gridObj.HasWallAt(wPos) && neighbour != null && neighbour.GetGridType() != GridType.REPLACEABLE)
            {
                neighbour.PlaceWallAt(WallStatus.GetOppositePos(wPos), growthIndex);
                neighbour.InstantiateWall(WallStatus.GetOppositePos(wPos), growthIndex);
            }
        }
    }

    /// <summary>
    /// Perform the Wave Function Collapse.
    /// </summary>
    public void CollapseWorld()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = grid[x, y];
                if (cell == null) continue;
                if (cell.GetGridType() != GridType.REPLACEABLE) continue;
                cell.DestroyObj();
                grid[x, y] = null;
            }
        }

        List<GridObj> allPlaceables = GridObj.GetPossiblePlaceables();
        foreach (GridObj obj in allPlaceables)
            obj.InitCompatibleList();

        Queue<Vector2Int> toProcess = new Queue<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (grid[x, y] == null && HasAnyNonNullNeighbor(pos))
                    toProcess.Enqueue(pos);
            }
        }
        if (toProcess.Count == 0)
            toProcess.Enqueue(new Vector2Int(width / 2, height / 2));

        Vector2Int[] offsets = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        WallPos[] sides = { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT };

        HashSet<Vector2Int> enqueued = new HashSet<Vector2Int>(toProcess);

        while (toProcess.Count > 0)
        {
            Vector2Int pos = toProcess.Dequeue();
            int x = pos.x, y = pos.y;

            if (grid[x, y] != null) continue;

            List<GridObj> candidates = new List<GridObj>(allPlaceables);

            for (int i = 0; i < 4; i++)
            {
                Vector2Int nPos = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                if (!IsInsideGrid(nPos)) continue;

                GridObj neighbor = grid[nPos.x, nPos.y];
                if (neighbor == null) continue;

                WallPos sideFromMe = sides[i];

                List<GridObj> filtered = new List<GridObj>();
                foreach (GridObj cand in candidates)
                {
                    if (cand.IsCompatible(neighbor, sideFromMe))
                        filtered.Add(cand);
                }
                candidates = filtered;

                if (candidates.Count == 0) break;
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"No candidates for ({x},{y}); leaving empty this pass.");
                continue;
            }

            GridObj chosenTemplate = PickWeightedRandom(candidates);
            grid[x, y] = new GridObj(new Vector2Int(x, y), chosenTemplate.GetWallStatus().Clone());
            SetRandomGridType(grid[x,y]);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nPos = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                if (!IsInsideGrid(nPos)) continue;
                if (grid[nPos.x, nPos.y] != null) continue;
                if (enqueued.Add(nPos))
                    toProcess.Enqueue(nPos);
            }
        }
    }

    /// <summary>
    /// Sets the given GridObj to a random object type.
    /// </summary>
    /// <param name="gridObj">GridObj to be randomised.</param>
    public void SetRandomGridType(GridObj gridObj)
    {   
        int Trapchance = 5;
        int JumpingBadChance= 7;
        int PlaceHolderChance=0;
        int rand = UnityEngine.Random.Range(0, 100);
        if(rand <= Trapchance)
        {
            gridObj.InitType(GridType.TRAP);
        }
        else if(rand > Trapchance && rand < (JumpingBadChance +Trapchance  ))
        {
            gridObj.InitType(GridType.JUMPINGPAD);
        }
         else if(rand > (JumpingBadChance+Trapchance) && rand < (PlaceHolderChance+JumpingBadChance +Trapchance  ))
        {
            //Regular should be changed later for place Holder whatever that is (maybe teleport)
            gridObj.InitType(GridType.REGULAR);
        } else
        {
            gridObj.InitType(GridType.REGULAR);
        }
    }
    
    /// <summary>
    /// Check whether the tile at the given position has any non-null adjacent tiles.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool HasAnyNonNullNeighbor(Vector2Int pos)
    {
        Vector2Int[] offsets = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        foreach (var o in offsets)
        {
            Vector2Int n = pos + o;
            if (!IsInsideGrid(n)) continue;
            if (grid[n.x, n.y] != null) return true;
        }
        return false;
    }

    /// <summary>
    /// Pick a random GridObj from a list of GridObjs.
    /// Weighting to be added later.
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns>Random GridObj</returns>
    private GridObj PickWeightedRandom(List<GridObj> nodes)
    {
        if (nodes.Count == 0) return null;

        int totalWeight = 0;
        foreach (var node in nodes)
        {
            int weight = node.IsPlaceable() ? 1 : 1;
            totalWeight += weight;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (var node in nodes)
        {
            int weight = node.IsPlaceable() ? 1 : 1; 
            if (roll < weight) return node;
            roll -= weight;
        }

        return nodes[nodes.Count - 1];
    }

    /// <summary>
    /// Instantiate any non-instantiated GridObjs in the grid.
    /// </summary>
    public void InstantiateMissing()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                GridObj obj = grid[w, h];
                if (obj == null || obj.IsInstantiated()) continue;
                obj.InstantiateObj(growthIndex);
            }
        }
    }

    /// <summary>
    /// Increase the size of the grid by 1 in each direction.
    /// </summary>
    public void IncreaseGrid()
    {
        int newW = width + 2;
        int newH = height + 2;
        growthIndex++;

        GridObj[,] newGrid = new GridObj[newW, newH];
        
        if(exit != null && growthIndex == exit.growthIndex)
        {
            Vector2Int exitPos = exit.gridObj.GetGridPos();
            newGrid[exitPos.x, exitPos.y] = exit.gridObj;
        }

        // Copy old objects into the middle.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridObj oldObj = grid[x, y];
                if (oldObj == null) continue;
                oldObj.SetGridPos(new Vector2Int(x + 1, y + 1)); // update position
                newGrid[x + 1, y + 1] = oldObj;
            }
        }

        // Create new REPLACEABLE border objects.
        for (int x = 0; x < newW; x++)
        {
            if(newGrid[x, 0] == null) newGrid[x, 0] = MakeReplaceable(new Vector2Int(x, 0));
            if(newGrid[x, newH - 1] == null) newGrid[x, newH - 1] = MakeReplaceable(new Vector2Int(x, newH - 1));
        }

        for (int y = 1; y < newH - 1; y++)
        {
            if(newGrid[0, y] == null) newGrid[0, y] = MakeReplaceable(new Vector2Int(0, y));
            if(newGrid[newW - 1, y] == null) newGrid[newW - 1, y] = MakeReplaceable(new Vector2Int(newW - 1, y));
        }

        // Adjustment for the grid array growing by 2 in the positive direction while the actual grid grows by 1 in both directions.
        PlayerMovement.currentGridPos = new Vector2Int(PlayerMovement.currentGridPos.x + 1, PlayerMovement.currentGridPos.y + 1);

        grid = newGrid;
    }

    /// <summary>
    /// Creates a new replaceable GridObj at the given grid position.
    /// </summary>
    /// <param name="pos">Position at which the new replaceable GridObj should be created.</param>
    /// <returns>Newly created GridObj</returns>
    private GridObj MakeReplaceable(Vector2Int pos)
    {
        GridObj obj = new GridObj(pos, new WallStatus());
        obj.SetGridType(GridType.REPLACEABLE);
        obj.InitType(GridType.REPLACEABLE);
        return obj;
    }

    /// <summary>
    /// Check whether a given grid position is within the current grid's bounds.
    /// </summary>
    /// <param name="v">Grid position to be checked.</param>
    /// <returns></returns>
    public bool IsInsideGrid(Vector2Int v)
    {
        return v.x >= 0 && v.x < width && v.y >= 0 && v.y < height;
    }

    /// <summary>
    /// Retreive a GridObj from its given GameObject.
    /// </summary>
    /// <param name="gameObj">GameObject whose GridObj should be retreived.</param>
    /// <returns>According GridObj if found, else null</returns>
    public GridObj GetGridObjFromGameObj(GameObject gameObj)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(gameObj.transform.position, growthIndex);
        if (!IsInsideGrid(gridPos)) return null;
        return grid[gridPos.x, gridPos.y];
    }

    /// <summary>
    /// Returns adjacent GridObj in direction, or null
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridObj GetAdjacentGridObj(GridObj gridObj, WallPos direction)
    {
        return GetAdjacentGridObj(gridObj.GetGridPos(), direction);
    }

    /// <summary>
    /// Returns adjacent GridObj in WallPos direction, or null
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridObj GetAdjacentGridObj(Vector2Int pos, WallPos direction)
    {
        Vector2Int targetPos = pos;

        switch (direction)
        {
            case WallPos.LEFT:
                targetPos += new Vector2Int(-1, 0);
                break;
            case WallPos.RIGHT:
                targetPos += new Vector2Int(1, 0);
                break;
            case WallPos.FRONT:
                targetPos += new Vector2Int(0, -1);
                break;
            case WallPos.BACK:
                targetPos += new Vector2Int(0, 1);
                break;
        }
        if (IsInsideGrid(targetPos))
        {
            return grid[targetPos.x, targetPos.y];
        }
        else
        {
            return null;
        }

    }

    /// <summary>
    /// Retreive the GridObj closest to the given world position.
    /// </summary>
    /// <param name="pos">World position from where the GridObj should be searched.</param>
    /// <returns></returns>
    public GridObj GetNearestGridObj(Vector3 pos)
    {
        float minDist = Mathf.Infinity;
        GridObj nearest = null;

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                if (grid[w, h] == null) continue;
                float dist = (pos - grid[w, h].GetWorldPos()).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = grid[w, h];
                }
            }
        }
        return nearest;
    }
    
    /// <summary>
    /// Create a new exit at a given grid position and with a given growthIndex.
    /// </summary>
    /// <param name="pos">Grid position at which the exit should be placed.</param>
    /// <param name="growthIndex">Growth index to be passed to the Exit</param>
    public void CreateExit(Vector2Int pos, int growthIndex)
    {
        exit = new Exit(new GridObj(pos, new WallStatus(WallType.EXIT, WallType.NONE, WallType.NONE, WallType.NONE)), growthIndex);
        // this.exit.gridObj.InstantiateObj(growthIndex); // TODO fix this
    }

    /// <summary>
    /// Place a new exit on an adjacent GridObj in the given direction. If no wall is free or no object exists it will try to place the exit in another direction.
    /// </summary>
    /// <param name="direction">Direction in which the exit should be moved.</param>
    public void RepositionExit(WallPos direction)
    {   
        // if(this.exit.growthIndex < this.growthIndex) return; // TODO Somehow this does not work properly, no idea why
        if(growthIndex < 1) return; // TODO remove magic number once code above is fixed
        Debug.Log("Repositioned " + growthIndex);
        GridObj exit = this.exit.gridObj;
        Vector2Int exitPos = exit.GetGridPos();
        GridObj newExit = GetAdjacentGridObj(exit, direction);

        newExit = PlaceExit(newExit);

        if(newExit != null)
        {
            this.exit.gridObj = newExit;
            grid[exitPos.x, exitPos.y].RemoveExitWalls();
            return;
        }

        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(pos == direction) continue;
            newExit = GetAdjacentGridObj(exit, pos);
            newExit = PlaceExit(newExit);
            if(newExit != null)
            {
                this.exit.gridObj = newExit;
                grid[exitPos.x, exitPos.y].RemoveExitWalls();
                break;
            }
        }
    }

    /// <summary>
    /// Attempt to place an exit on a random free spot. If no spot is free, returns null
    /// </summary>
    /// <param name="gridObj"></param>
    /// <returns>The newly placed exit GridObj, or null if no space</returns>
    private GridObj PlaceExit(GridObj gridObj)
    {   
        if(gridObj == null || gridObj.GetGridType() == GridType.REPLACEABLE) return null;
        List<WallPos> free = gridObj.GetFreeWalls();
        if(free.Count == 0) return null;

        gridObj.PlaceWallAt(free[UnityEngine.Random.Range(0, free.Count)], WallType.EXIT, growthIndex);
        return gridObj;
    }
    
    public bool IsInstantiated() { return growthIndex > 0; }
    public GridObj[,] GetGridArray() { return grid; }
    public int GetGrowthIndex() {  return growthIndex; }

    /// <summary>
    /// Get the GridObj at the given grid position.
    /// </summary>
    /// <param name="pos">Grid position to be searched.</param>
    /// <returns></returns>
    public GridObj GetGridObj(Vector2Int pos)
    {
        if (IsInsideGrid(pos))
        {
            return grid[pos.x, pos.y];
        }
        return null;
    }
}
