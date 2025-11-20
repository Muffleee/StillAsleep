using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class Grid
{
    public int width => this.grid.GetLength(0);
    public int height => this.grid.GetLength(1);
    private GridObj[,] grid;
    private int growthIndex = 0;
    private Exit exit;

    public Grid(int width, int height)
    {
        this.grid = new GridObj[width, height];
    }

    public void PlaceObj(GridObj gridObj)
    {
        this.PlaceObj(gridObj, gridObj.GetWorldPos(this.growthIndex));
    }

    public void PlaceObj(GridObj gridObj, Vector3 pos)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(pos, this.growthIndex);

        if (this.grid[gridPos.x, gridPos.y] != null)
        {
            this.grid[gridPos.x, gridPos.y].DestroyObj();
        }
        
        gridObj.SetGridPos(gridPos);
        this.grid[gridPos.x, gridPos.y] = gridObj;
        this.grid[gridPos.x, gridPos.y].InstantiateObj(growthIndex);
        InstantiateMissingWalls(gridObj, gridPos);
    }
    /// <summary>
    /// check for the gridObj at gridPos if there is any wall where the neighbour also hasn't instantiated a wall and then instantiate it
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="gridPos"></param>
    private void InstantiateMissingWalls(GridObj gridObj, Vector2Int gridPos)
    {
        WallPos[] wallPos = new WallPos[] { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT };
        foreach (WallPos wPos in wallPos)
        {
            GridObj neighbour = this.GetAdjacentGridObj(gridPos, wPos);
            if (gridObj.HasWallAt(wPos) && neighbour != null && neighbour.GetGridType() != GridType.REPLACEABLE)
            {
                neighbour.PlaceWallAt(WallStatus.GetOppositePos(wPos), growthIndex);
                neighbour.InstantiateWall(WallStatus.GetOppositePos(wPos), growthIndex);
            }
        }
    }
    public void CollapseWorld()
    {
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                var cell = this.grid[x, y];
                if (cell == null) continue;
                if (cell.GetGridType() != GridType.REPLACEABLE) continue;
                cell.DestroyObj();
                this.grid[x, y] = null;
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
                if (grid[x, y] == null && HasAnyNonNullNeighbor(x, y))
                    toProcess.Enqueue(new Vector2Int(x, y));
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

    private bool HasAnyNonNullNeighbor(int x, int y)
    {
        Vector2Int[] offsets = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        foreach (var o in offsets)
        {
            int nx = x + o.x, ny = y + o.y;
            if (!IsInsideGrid(new Vector2Int(nx, ny))) continue;
            if (grid[nx, ny] != null) return true;
        }
        return false;
    }

    // To use later
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

    public void InstantiateMissing()
    {
        for (int w = 0; w < this.width; w++)
        {
            for (int h = 0; h < this.height; h++)
            {
                GridObj obj = this.grid[w, h];
                if (obj == null || obj.IsInstantiated()) continue;
                obj.InstantiateObj(this.growthIndex);
            }
        }
    }

    public void IncreaseGrid()
    {
        int newW = this.width + 2;
        int newH = this.height + 2;
        this.growthIndex++;

        GridObj[,] newGrid = new GridObj[newW, newH];
        
        if(this.exit != null && this.growthIndex == this.exit.growthIndex)
        {
            Vector2Int exitPos = this.exit.gridObj.GetGridPos();
            newGrid[exitPos.x, exitPos.y] = exit.gridObj;
        }

        // copy old objects into the middle
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                GridObj oldObj = this.grid[x, y];
                if (oldObj == null) continue;
                oldObj.SetGridPos(new Vector2Int(x + 1, y + 1)); // update position
                newGrid[x + 1, y + 1] = oldObj;
            }
        }

        // create new REPLACEABLE border objects
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

        this.grid = newGrid;
    }

    private GridObj MakeReplaceable(Vector2Int pos)
    {
        GridObj obj = new GridObj(pos, new WallStatus());
        obj.SetGridType(GridType.REPLACEABLE);
        return obj;
    }

    public void FillOuterSpaceWithReplaceables()
    {
        for (int w = 0; w < this.width; w++)
        {
            GridObj a = new GridObj(new Vector2Int(w, 0), new WallStatus());
            a.SetGridType(GridType.REPLACEABLE);
            this.grid[w , 0] = a;

            GridObj b = new GridObj(new Vector2Int(w, this.height - 1), new WallStatus());
            b.SetGridType(GridType.REPLACEABLE);
            this.grid[w, this.height - 1] = b;
        }

        for (int h = 1; h < this.height - 1; h++)
        {
            GridObj a = new GridObj(new Vector2Int(0, h), new WallStatus());
            a.SetGridType(GridType.REPLACEABLE);
            this.grid[0, h] = a;

            GridObj b = new GridObj(new Vector2Int(this.width - 1, h), new WallStatus());
            b.SetGridType(GridType.REPLACEABLE);
            this.grid[this.width - 1, h] = b;
        }
    }

    public bool IsInsideGrid(Vector2Int v)
    {
        return v.x >= 0 && v.x < this.width && v.y >= 0 && v.y < this.height;
    }

    public GridObj GetGridObjFromGameObj(GameObject gameObj)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(gameObj.transform.position, growthIndex);
        if (!this.IsInsideGrid(gridPos)) return null;
        return this.grid[gridPos.x, gridPos.y];
    }

    /// <summary>
    /// Returns adjacent GridObj in WallPos direction or null
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridObj GetAdjacentGridObj(GridObj gridObj, WallPos direction)
    {
        return this.GetAdjacentGridObj(gridObj.GetGridPos(), direction);
    }

    /// <summary>
    /// Returns adjacent GridObj in WallPos direction or null
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
            return this.grid[targetPos.x, targetPos.y];
        }
        
        return null;
    }

    public GridObj GetNearestGridObj(Vector3 pos)
    {
        float minDist = Mathf.Infinity;
        GridObj nearest = null;

        for (int w = 0; w < this.width; w++)
        {
            for (int h = 0; h < this.height; h++)
            {
                if (this.grid[w, h] == null) continue;
                float dist = Vector3.Distance(pos, this.grid[w, h].GetWorldPos());
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = this.grid[w, h];
                }
            }
        }
        return nearest;
    }
    
    public void CreateExit(Vector2Int pos, int growthIndex)
    {
        this.exit = new Exit(new GridObj(pos, new WallStatus(WallType.EXIT, WallType.NONE, WallType.NONE, WallType.NONE)), growthIndex);
        // this.exit.gridObj.InstantiateObj(growthIndex); // TODO fix this
    }

    /// <summary>
    /// Places a new exit ond an adjacent GridObj in the given direction. If no wall is free or no object exists it will try to place the exit in another direction.
    /// </summary>
    /// <param name="direction"></param>
    public void RepositionExit(WallPos direction)
    {   
        // if(this.exit.growthIndex < this.growthIndex) return; // TODO Somehow this does not work properly, no idea why
        if(this.growthIndex < 1) return; // TODO remove magic number once code above is fixed
        Debug.Log("Repositioned " + this.growthIndex);
        GridObj exit = this.exit.gridObj;
        Vector2Int exitPos = exit.GetGridPos();
        GridObj newExit = this.GetAdjacentGridObj(exit, direction);

        newExit = this.PlaceExit(newExit);

        if(newExit != null)
        {
            this.exit.gridObj = newExit;
            this.grid[exitPos.x, exitPos.y].RemoveExitWalls();
            return;
        }

        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(pos == direction) continue;
            newExit = this.GetAdjacentGridObj(exit, pos);
            newExit = this.PlaceExit(newExit);
            if(newExit != null)
            {
                this.exit.gridObj = newExit;
                this.grid[exitPos.x, exitPos.y].RemoveExitWalls();
                break;
            }
        }
    }

    /// <summary>
    /// Attempts to place an exit on a random free spot. If no spot is free, it returns null
    /// </summary>
    /// <param name="gridObj"></param>
    /// <returns></returns>
    private GridObj PlaceExit(GridObj gridObj)
    {   
        if(gridObj == null || gridObj.GetGridType() == GridType.REPLACEABLE) return null;
        List<WallPos> free = gridObj.GetFreeWalls();
        if(free.Count == 0) return null;

        gridObj.PlaceWallAt(free[UnityEngine.Random.Range(0, free.Count)], WallType.EXIT, this.growthIndex);
        return gridObj;
    }
    
    public bool IsInstantiated() { return this.growthIndex > 0; }
    public GridObj[,] GetGridArray() { return this.grid; }
    public int GetGrowthIndex() {  return this.growthIndex; }
}
