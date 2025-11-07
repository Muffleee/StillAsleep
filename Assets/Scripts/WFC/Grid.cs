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
    private List<Vector2Int> toCollapse = new List<Vector2Int>();
    private List<GridObj> possibilities = new List<GridObj>();
    private int growthIndex = 0;

    public Grid(int width, int height)
    {
        this.grid = new GridObj[width, height];
    }

    public void CollapseWorld()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = null;

        List<GridObj> allPlaceables = GridObj.GetPossiblePlaceables();
        foreach (var obj in allPlaceables)
            obj.InitCompatibleList();

        List<GridObj>[,] cellPossibilities = new List<GridObj>[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cellPossibilities[x, y] = new List<GridObj>(allPlaceables);

        Queue<Vector2Int> toProcess = new Queue<Vector2Int>();
        Vector2Int center = new Vector2Int(width / 2, height / 2);
        toProcess.Enqueue(center);

        while (toProcess.Count > 0)
        {
            Vector2Int pos = toProcess.Dequeue();
            int x = pos.x;
            int y = pos.y;

            if (grid[x, y] != null) continue;

            List<GridObj> possibilities = cellPossibilities[x, y];
            if (possibilities.Count == 0)
            {
                Debug.LogWarning($"No possible tiles for ({x},{y})");
                continue;
            }

            GridObj chosenTemplate = PickWeightedRandom(possibilities);

            grid[x, y] = new GridObj(new Vector2Int(x, y), chosenTemplate.GetWallStatus().Clone());

            Vector2Int[] offsets = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
            WallPos[] sides = { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT };

            for (int i = 0; i < 4; i++)
            {
                Vector2Int nPos = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                if (!IsInsideGrid(nPos)) continue;
                if (grid[nPos.x, nPos.y] != null) continue;

                List<GridObj> neighborPossibilities = cellPossibilities[nPos.x, nPos.y];
                List<GridObj> valid = new List<GridObj>();

                foreach (var node in neighborPossibilities)
                {
                    if (AreCompatible(chosenTemplate, node, sides[i]))
                        valid.Add(node);
                }

                if (valid.Count > 0)
                {
                    cellPossibilities[nPos.x, nPos.y] = valid;
                    toProcess.Enqueue(nPos);
                }
            }
        }
    }

    private bool AreCompatible(GridObj a, GridObj b, WallPos from)
    {
        WallPos opposite = Opposite(from);

        return a.HasWallAt(from) == b.HasWallAt(opposite);
    }

    private WallPos Opposite(WallPos p)
    {
        switch (p)
        {
            case WallPos.FRONT: return WallPos.BACK;
            case WallPos.BACK:  return WallPos.FRONT;
            case WallPos.LEFT:  return WallPos.RIGHT;
            case WallPos.RIGHT: return WallPos.LEFT;
        }
        return WallPos.FRONT;
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
}
