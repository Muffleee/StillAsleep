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

        int roll = Random.Range(0, totalWeight);
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
            newGrid[x, 0] = MakeReplaceable(new Vector2Int(x, 0));
            newGrid[x, newH - 1] = MakeReplaceable(new Vector2Int(x, newH - 1));
        }

        for (int y = 1; y < newH - 1; y++)
        {
            newGrid[0, y] = MakeReplaceable(new Vector2Int(0, y));
            newGrid[newW - 1, y] = MakeReplaceable(new Vector2Int(newW - 1, y));
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
        else
        {
            return null;
        }
    }

    public GridObj GetNearestGridObj(Vector3 pos)
    {
        float minDist = Mathf.Infinity;
        GridObj nearest = null;

        for(int w = 0; w < this.width; w++)
        {
            for(int h = 0; h < this.height; h++)
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

    public void RemoveObjectsBehindPlayer(Vector2Int playerGridPos, WallPos moveDirection)
    {
       
        WallPos behindDirection = GetBehindDirection(moveDirection);
        
        Vector2Int posBehindPlayer = playerGridPos;
        
        switch (behindDirection)
        {
            case WallPos.FRONT:
                posBehindPlayer += new Vector2Int(0, -1);
                break;
            case WallPos.BACK:
                posBehindPlayer += new Vector2Int(0, 1);
                break;
            case WallPos.LEFT:
                posBehindPlayer += new Vector2Int(-1, 0);
                break;
            case WallPos.RIGHT:
                posBehindPlayer += new Vector2Int(1, 0);
                break;
        }
        
       
        if (!IsInsideGrid(posBehindPlayer)) return;
        
      
        GridObj objBehind = grid[posBehindPlayer.x, posBehindPlayer.y];
        if (objBehind != null && objBehind.GetGridType() == GridType.REPLACEABLE)
        {
            objBehind.DestroyObj();
            grid[posBehindPlayer.x, posBehindPlayer.y] = null;
        }
    }

  
    private WallPos GetBehindDirection(WallPos moveDirection)
    {
        switch (moveDirection)
        {
            case WallPos.FRONT: return WallPos.BACK;   // Wenn nach vorne, dann ist hinten zurück
            case WallPos.BACK: return WallPos.FRONT;   // Wenn nach hinten, dann ist hinten vorne
            case WallPos.LEFT: return WallPos.RIGHT;   // Wenn nach links, dann ist hinten rechts
            case WallPos.RIGHT: return WallPos.LEFT;   // Wenn nach rechts, dann ist hinten links
            default: return WallPos.BACK;
        }
    }
    
    public bool IsInstantiated() { return this.growthIndex > 0; }
    public GridObj[,] GetGridArray() { return this.grid; }
    public int GetGrowthIndex() {  return this.growthIndex; }
}
