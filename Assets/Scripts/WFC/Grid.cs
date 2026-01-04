using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class handling the gane's grid and its procedual generation through Wave Function Collapse.
/// </summary>
public class Grid
{
    public int width => this.grid.GetLength(0);
    public int height => this.grid.GetLength(1);
    private GridObj[,] grid;
    public UnityEvent<GridObj, string> tutorialUpdate = new UnityEvent<GridObj, string> ();
    /// <summary>
    /// Count the times the grid has grown so far.
    /// </summary>
    private int worldOffsetX = 0;
    private int worldOffsetY = 0;
    private Exit exit;
    /// <summary>
    /// The tutorial booleans so that everything is only introduced once
    /// </summary>
    private bool tutorial = true;
    private bool jumpingIntro = false;
    private bool exitIntro = false;
    private bool trapIntro = false;
    private bool replaceableIntro = false;
    private bool manReplaceableIntro = false;
    /// <summary>
    /// Create a new grid given an initial size.
    /// </summary>
    /// <param name="width">Initial grid width</param>
    /// <param name="height">Initial grid height</param>
    public Grid(int width, int height)
    {
        this.grid = new GridObj[width, height];
    }

    /// <summary>
    /// Place a GridObj in the grid at the GridObj's current world position. Will destroy any existing GridObj at this position.
    /// </summary>
    /// <param name="gridObj">GridObj to be placed</param>
    public void PlaceObj(GridObj gridObj)
    {
        this.PlaceObj(gridObj, gridObj.GetWorldPos(this.worldOffsetX, this.worldOffsetY));
    }

    /// <summary>
    /// Place a GridObj in the grid at the given world position. Will destroy any existing GridObj at this position.
    /// </summary>
    /// <param name="gridObj">GridObj to be placed</param>
    /// <param name="pos">World position at which the GridObj is to be placed</param>
    public void PlaceObj(GridObj gridObj, Vector3 pos)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(pos, this.worldOffsetX, this.worldOffsetY);

        if (this.grid[gridPos.x, gridPos.y] != null)
        {
            this.grid[gridPos.x, gridPos.y].DestroyObj();
        }
        
        gridObj.SetGridPos(gridPos);
        this.grid[gridPos.x, gridPos.y] = gridObj;
        if (gridObj.GetInteract() == null)
        {
            gridObj.SetGridType(GridType.REGULAR);
        }

        Dictionary<WallPos, GridObj> neighbors = new Dictionary<WallPos, GridObj>() { { WallPos.FRONT, this.GetAdjacentGridObj(gridObj, WallPos.FRONT) }, 
                                                                                            { WallPos.BACK, this.GetAdjacentGridObj(gridObj, WallPos.BACK) }, 
                                                                                            { WallPos.LEFT, this.GetAdjacentGridObj(gridObj, WallPos.LEFT) }, 
                                                                                            { WallPos.RIGHT, this.GetAdjacentGridObj(gridObj, WallPos.RIGHT) } };
        this.grid[gridPos.x, gridPos.y].InstantiateObj(this.worldOffsetX, this.worldOffsetY, neighbors);
        this.InstantiateMissingWalls(gridObj);
        
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
            GridObj neighbour = this.GetAdjacentGridObj(gridObj.GetGridPos(), wPos);
            if (gridObj.HasWallAt(wPos) && neighbour != null && neighbour.GetGridType() != GridType.REPLACEABLE)
            {
                neighbour.PlaceWallAt(WallStatus.GetOppositePos(wPos), this.worldOffsetX, this.worldOffsetY);
                neighbour.InstantiateWall(WallStatus.GetOppositePos(wPos), this.worldOffsetX, this.worldOffsetY);
            }
        }
    }

    /// <summary>
    /// Perform the Wave Function Collapse.
    /// </summary>
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
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (this.grid[x, y] == null && this.HasAnyNonNullNeighbor(pos))
                    toProcess.Enqueue(pos);
            }
        }
        if (toProcess.Count == 0)
            toProcess.Enqueue(new Vector2Int(this.width / 2, this.height / 2));

        Vector2Int[] offsets = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        WallPos[] sides = { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT };

        HashSet<Vector2Int> enqueued = new HashSet<Vector2Int>(toProcess);

        while (toProcess.Count > 0)
        {
            Vector2Int pos = toProcess.Dequeue();
            int x = pos.x, y = pos.y;

            if (this.grid[x, y] != null) continue;

            List<GridObj> candidates = new List<GridObj>(allPlaceables);

            for (int i = 0; i < 4; i++)
            {
                Vector2Int nPos = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                if (!this.IsInsideGrid(nPos)) continue;

                GridObj neighbor = this.grid[nPos.x, nPos.y];
                if (neighbor == null) continue;

                WallPos sideFromMe = sides[i];

                List<GridObj> filtered = new List<GridObj>();
                foreach (GridObj cand in candidates)
                {
                    if (cand.IsCompatible(neighbor, sideFromMe))
                        filtered.Add(cand.Clone());
                }
                this.IncreaseWeight(filtered, neighbor, sideFromMe);
                candidates = filtered;
                
                if (candidates.Count == 0) break;
            }

            if (candidates.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"No candidates for ({x},{y}); leaving empty this pass.");
                continue;
            }

            GridObj chosenTemplate = this.PickWeightedRandom(candidates);
            candidates.Remove(chosenTemplate);
            this.grid[x, y] = new GridObj(new Vector2Int(x, y), chosenTemplate.GetWallStatus().Clone());
            while (!this.CheckSolvability(new Vector2Int(x,y)))
            {
                if (candidates.Count == 0)
                {
                    chosenTemplate = new GridObj(new WallStatus(), GameManager.emptyWeight);
                    this.grid[x, y] = new GridObj(new Vector2Int(x, y), chosenTemplate.GetWallStatus().Clone());
                    break;
                }
                chosenTemplate = this.PickWeightedRandom(candidates);
                candidates.Remove(chosenTemplate);
                this.grid[x, y] = new GridObj(new Vector2Int(x, y), chosenTemplate.GetWallStatus().Clone());
            }
            
            this.SetRandomGridType(this.grid[x,y]);

            if(this.grid[x,y].GetGridType() == GridType.MANUAL_REPLACEABLE) this.grid[x,y].RemoveAllWalls();
            
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nPos = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                if (!this.IsInsideGrid(nPos)) continue;
                if (this.grid[nPos.x, nPos.y] != null) continue;
                if (enqueued.Add(nPos))
                    toProcess.Enqueue(nPos);
            }
        }
        
    }
    private void IncreaseWeight(List<GridObj> filtered, GridObj neighbor, WallPos side)
    {
        if (side == WallPos.FRONT || side == WallPos.BACK)
        {
            foreach(GridObj cand in filtered)
            {
                if(cand.GetWallStatus().left == neighbor.GetWallStatus().left)
                {
                    cand.SetWeight(cand.GetWeight() + 2);
                }
                if (cand.GetWallStatus().right == neighbor.GetWallStatus().right)
                {
                    cand.SetWeight(cand.GetWeight() + 2);
                }
            }

        } else
        {
            foreach (GridObj cand in filtered)
            {
                if (cand.GetWallStatus().front == neighbor.GetWallStatus().front)
                {
                    cand.SetWeight(cand.GetWeight() + 2);
                }
                if (cand.GetWallStatus().back == neighbor.GetWallStatus().back)
                {
                    cand.SetWeight(cand.GetWeight() + 2);
                }
            }
        }
    }
    /// <summary>
    /// Sets the given GridObj to a random object type.
    /// </summary>
    /// <param name="gridObj">GridObj to be randomised.</param>
    public void SetRandomGridType(GridObj gridObj)
    {   
        int Trapchance = GameManager.trapWeight;
        int JumpingBadChance = GameManager.jumpingWeight;
        int PlaceHolderChance = GameManager.replacableWeight;
        int rand = UnityEngine.Random.Range(0, 100);
        if(rand < Trapchance)
        {
            gridObj.SetGridType(GridType.TRAP);
            gridObj.SetFloorPrefab(GameManager.INSTANCE.GetPrefabLibrary().prefabTrap);
        }
        else if(rand > Trapchance && rand < (JumpingBadChance + Trapchance  ))
        {
            gridObj.SetGridType(GridType.JUMPINGPAD);
            gridObj.SetFloorPrefab(GameManager.INSTANCE.GetPrefabLibrary().prefabJumppad);
        }
         else if(rand > (JumpingBadChance + Trapchance) && rand < (PlaceHolderChance + JumpingBadChance + Trapchance))
        {
            gridObj.SetGridType(GridType.MANUAL_REPLACEABLE);
        } else
        {
            gridObj.SetGridType(GridType.REGULAR);
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
            if (!this.IsInsideGrid(n)) continue;
            if (this.grid[n.x, n.y] != null) return true;
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
            int weight = node.GetWeight();
            totalWeight += weight;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (var node in nodes)
        {
            int weight = node.GetWeight();
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
        for (int w = 0; w < this.width; w++)
        {
            for (int h = 0; h < this.height; h++)
            {
                GridObj obj = this.grid[w, h];
                if (obj == null || obj.IsInstantiated()) continue;
                Dictionary<WallPos, GridObj> neighbors = new Dictionary<WallPos, GridObj>() { { WallPos.FRONT, this.GetAdjacentGridObj(obj, WallPos.FRONT) },
                                                                                            { WallPos.BACK, this.GetAdjacentGridObj(obj, WallPos.BACK) },
                                                                                            { WallPos.LEFT, this.GetAdjacentGridObj(obj, WallPos.LEFT) },
                                                                                            { WallPos.RIGHT, this.GetAdjacentGridObj(obj, WallPos.RIGHT) } };
                obj.InstantiateObj(this.worldOffsetX, this.worldOffsetY, neighbors);
                if (tutorial) StartTutorial(obj);
            }
        }
    }
    /// <summary>
    /// Setting the tutorialText to introduce the player
    /// </summary>
    /// <param name="type"> what type of grid is going to be introduced</param>
    private void StartTutorial(GridObj obj)
    {
        GridType type = obj.GetGridType();
        switch (type)
        {
            case GridType.JUMPINGPAD: 
                if (!jumpingIntro) tutorialUpdate.Invoke(obj, "This is a jumping pad. \n With it you can jump over adjacent walls.");
                jumpingIntro = true;
                break;
            case GridType.REPLACEABLE: 
                if (!replaceableIntro) tutorialUpdate.Invoke(obj, "This is a replaceable tile.\n You can place a tile from your inventory there. The playfield will expand in direction of the replacable tiles.");
                replaceableIntro = true;
                break;
            case GridType.MANUAL_REPLACEABLE: 
                if (!manReplaceableIntro) tutorialUpdate.Invoke(obj, "This is a manually replaceable tile. \n This is also a replacable tile, but it will not be filled unless you place one of your tiles.");
                manReplaceableIntro = true; 
                break;
            case GridType.TRAP: 
                if (!trapIntro) tutorialUpdate.Invoke(obj, "This is a trap. \n Standing on it will cost you energy.");
                trapIntro = true;
                break;
            case GridType.REGULAR: break;
        }
        if (jumpingIntro && replaceableIntro && manReplaceableIntro && trapIntro && exitIntro) tutorial = false;
    }
    /// <summary>
    /// Increase the size of the grid by 1 in each direction.
    /// </summary>
    /*
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
    */

    public void IncreaseGrid(WallPos direction)
    {
        int addLeft = 0, addRight = 0, addFront = 0, addBack = 0;

        // Determine which side to expand
        switch (direction)
        {
            case WallPos.FRONT:  addFront = 1; break;
            case WallPos.BACK:   addBack = 1; break;
            case WallPos.LEFT:   addLeft = 1; break;
            case WallPos.RIGHT:  addRight = 1; break;
        }

        int newW = this.width + addLeft + addRight;
        int newH = this.height + addFront + addBack;

        GridObj[,] newGrid = new GridObj[newW, newH];

        // Copy old tiles into shifted positions
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                GridObj old = this.grid[x, y];
                if (old == null) continue;

                int nx = x + addLeft;   // shift right if expanding on LEFT
                int ny = y + addFront;  // shift up if expanding on FRONT

                old.SetGridPos(new Vector2Int(nx, ny));
                newGrid[nx, ny] = old;
            }
        }

        // Create replaceables ONLY at the expanded edge
        // FRONT
        if (addFront == 1)
        {
            for (int x = 0; x < newW; x++)
                newGrid[x, 0] = this.MakeReplaceable(new Vector2Int(x, 0));
        }

        // BACK
        if (addBack == 1)
        {
            int y = newH - 1;
            for (int x = 0; x < newW; x++)
                newGrid[x, y] = this.MakeReplaceable(new Vector2Int(x, y));
        }

        // LEFT
        if (addLeft == 1)
        {
            for (int y = 0; y < newH; y++)
                newGrid[0, y] = this.MakeReplaceable(new Vector2Int(0, y));
        }

        // RIGHT
        if (addRight == 1)
        {
            int x = newW - 1;
            for (int y = 0; y < newH; y++)
                newGrid[x, y] = this.MakeReplaceable(new Vector2Int(x, y));
        }

        // Update world offset
        this.worldOffsetX += addLeft;
        this.worldOffsetY += addFront;

        PlayerMovement.currentGridPos = new Vector2Int(PlayerMovement.currentGridPos.x + addLeft, PlayerMovement.currentGridPos.y + addFront);

        this.grid = newGrid;
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
        return obj;
    }

    /// <summary>
    /// Check whether a given grid position is within the current grid's bounds.
    /// </summary>
    /// <param name="v">Grid position to be checked.</param>
    /// <returns></returns>
    public bool IsInsideGrid(Vector2Int v)
    {
        return v.x >= 0 && v.x < this.width && v.y >= 0 && v.y < this.height;
    }

    /// <summary>
    /// Retreive a GridObj from its given GameObject.
    /// </summary>
    /// <param name="gameObj">GameObject whose GridObj should be retreived.</param>
    /// <returns>According GridObj if found, else null</returns>
    public GridObj GetGridObjFromGameObj(GameObject gameObj)
    {
        Vector2Int gridPos = GridObj.WorldPosToGridPos(gameObj.transform.position, this.worldOffsetX, this.worldOffsetY);
        if (!this.IsInsideGrid(gridPos)) return null;
        return this.grid[gridPos.x, gridPos.y];
    }

    /// <summary>
    /// Returns adjacent GridObj in direction, or null
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridObj GetAdjacentGridObj(GridObj gridObj, WallPos direction)
    {
        return this.GetAdjacentGridObj(gridObj.GetGridPos(), direction);
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
        if (this.IsInsideGrid(targetPos))
        {
            return this.grid[targetPos.x, targetPos.y];
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

        for (int w = 0; w < this.width; w++)
        {
            for (int h = 0; h < this.height; h++)
            {
                if (this.grid[w, h] == null) continue;
                float dist = (pos - this.grid[w, h].GetWorldPos()).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = this.grid[w, h];
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
    public void CreateExit(Vector2Int pos, int worldOffsetX, int worldOffsetY)
    {
        this.exit = new Exit(new GridObj(pos, new WallStatus(WallType.EXIT, WallType.NONE, WallType.NONE, WallType.NONE)), new Pair<GridObj, WallPos>(null, WallPos.FRONT), worldOffsetX, worldOffsetY);
        // this.exit.gridObj.InstantiateObj(growthIndex); // TODO fix this
    }

    /// <summary>
    /// Moves the exit away from the player.
    /// </summary>
    /// <param name="playerPosition"></param>
    public void RepositionExit(Vector2Int playerGridPosition)
    {
        GridObj currentExitGridObj = this.exit.gridObj;
        
        Vector2Int optimalRepositionVector = playerGridPosition - currentExitGridObj.GetGridPos();
        float optimalRepositionDirection = math.atan2(optimalRepositionVector.y, optimalRepositionVector.x) * 2 / math.PI + 2;

        WallPos[] optimalRepositionWallPos = new WallPos[3];
        optimalRepositionWallPos[0] = WallStatus.IntToWallPos((int)Math.Round(optimalRepositionDirection, MidpointRounding.AwayFromZero));
        if (optimalRepositionDirection % 1 < .5)
        {
            optimalRepositionWallPos[1] = WallStatus.IntToWallPos((int)Math.Ceiling(optimalRepositionDirection));
            optimalRepositionWallPos[2] = WallStatus.IntToWallPos((int)Math.Round(optimalRepositionDirection, MidpointRounding.AwayFromZero) + 3);
        }
        else
        {
            optimalRepositionWallPos[1] = WallStatus.IntToWallPos((int)Math.Floor(optimalRepositionDirection));
            optimalRepositionWallPos[2] = WallStatus.IntToWallPos((int)Math.Round(optimalRepositionDirection, MidpointRounding.AwayFromZero) + 1);
        }        

        foreach (WallPos direction in optimalRepositionWallPos) if(TryMoveExit(direction)) return;


        bool TryMoveExit(WallPos direction)
        {
            GridObj currentExitGridObj = this.exit.gridObj;
            GridObj newExitGridObj = this.PlaceExit(this.GetAdjacentGridObj(currentExitGridObj, direction));

            if (newExitGridObj == null || !newExitGridObj.HasExit()) return false;

            currentExitGridObj?.RemoveExitWalls();

            WallPos exitPos = newExitGridObj.GetExitPos();
            WallPos exitOppositePos = WallStatus.GetOppositePos(exitPos);
            GridObj adjacentGridObj = this.GetAdjacentGridObj(newExitGridObj, exitPos);

            this.exit.gridObj = newExitGridObj;

            return true;
        }
    }

    /// <summary>
    /// Attempts to place an exit on a free wall of the given GridObj.
    /// Returns the GridObj if successful, otherwise null.
    /// </summary>
    /// <param name="gridObj"></param>
    /// <returns></returns>
    private GridObj PlaceExit(GridObj gridObj)
    {
        if (gridObj == null || gridObj.GetGridType() == GridType.REPLACEABLE) return null;

        List<WallPos> free = gridObj.GetFreeWalls();
        if (free.Count == 0) return null;

        WallPos chosen = free[UnityEngine.Random.Range(0, free.Count)];
        gridObj.PlaceWallAt(chosen, WallType.EXIT, this.worldOffsetX, this.worldOffsetY);
        if(!exitIntro) tutorialUpdate.Invoke(gridObj, "This is the exit. \n Your goal is to reach it. It moves away from you, but only between tiles with no walls! \n Maybe you can make use of this feature...");
        exitIntro = true;
        return gridObj;
    }
    
    public bool IsInstantiated() { return this.worldOffsetX > 0 || this.worldOffsetY > 0; }
    public GridObj[,] GetGridArray() { return this.grid; }
    public int GetWorldOffsetX() {  return this.worldOffsetX; }
    public int GetWorldOffsetY() {  return this.worldOffsetY; }

    /// <summary>
    /// Get the GridObj at the given grid position.
    /// </summary>
    /// <param name="pos">Grid position to be searched.</param>
    /// <returns></returns>
    public GridObj GetGridObj(Vector2Int pos)
    {
        if (this.IsInsideGrid(pos))
        {
            return this.grid[pos.x, pos.y];
        }
        return null;
    }

    /// <summary>
    /// Returns the next direction of map generation
    /// </summary>
    /// <returns></returns>
    public WallPos GetNextGenPos()
    {
        if (PlayerMovement.currentGridPos == null) return WallPos.BACK;
       
        Dictionary<WallPos, int> distances = this.GetPlayerToEdgeDistances();
        return this.GetClosestEdge(distances);
    }

    /// <summary>
    /// Calculates the distance to each edge of the map
    /// </summary>
    /// <returns></returns>
    public Dictionary<WallPos, int> GetPlayerToEdgeDistances()
    {
        int playerX = PlayerMovement.currentGridPos.x;
        int playerY = PlayerMovement.currentGridPos.y;
        
        // Distances from the player to each edge
        return new Dictionary<WallPos, int>
        {
            { WallPos.LEFT, playerX },
            { WallPos.RIGHT, this.width - 1 - playerX },
            { WallPos.FRONT, playerY },
            { WallPos.BACK, this.height - 1 - playerY }
        };
    }

    /// <summary>
    /// Calculate the closest edge, use with GetPlayerToEdgeDistances()
    /// </summary>
    /// <param name="distances"></param>
    /// <returns></returns>
    public WallPos GetClosestEdge(Dictionary<WallPos, int> distances)
    {
        return this.GetClosestEdgeAndDistance(distances).first;
    }

    public Pair<WallPos, int> GetClosestEdgeAndDistance(Dictionary<WallPos, int> distances)
    {
        // Find the direction with the smallest distance
        WallPos closestDir = WallPos.BACK;
        int minDist = int.MaxValue;

        foreach (var kvp in distances)
        {
            if (kvp.Value < minDist)
            {
                minDist = kvp.Value;
                closestDir = kvp.Key;
            }
        }

        return new Pair<WallPos, int>(closestDir, minDist);
    }

    public bool IsInsideGridArray(int x, int y)
    {
        return x >= 0 && x < this.width && y >= 0 && y < this.height;
    }

    /// <summary>
    /// Returns true if the smallest distance to an edge of the player is less than genRange
    /// </summary>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public bool ShouldGenerate(int genRange)
    {
        Pair<WallPos, int> closestEdge = this.GetClosestEdgeAndDistance(this.GetPlayerToEdgeDistances());
        return closestEdge.second < genRange;
    }

    /// <summary>
    /// Returns true if the grid has no completely closed of rooms
    /// </summary>
    /// <returns></returns>
    /// 
    //TODO: Return false if there is no way from now to the goal (not even by using crystals to place tiles)
    private bool CheckSolvability(Vector2Int startingPosition)
    {
        if (this.width == 0 || this.height == 0) return true;
        Grid incGrid = new Grid(this.width + 2, this.height + 2);
        GridObj[,] incGridArray = incGrid.GetGridArray();
        for(int x = 0; x < incGrid.width; x++)
        {
            for(int y = 0; y < incGrid.height; y++)
            {
                if (x == 0 || y == 0 || x == incGrid.width - 1 || y == incGrid.height - 1|| this.grid[x-1,y-1] == null)
                {
                    GridObj tile = new GridObj(new WallStatus(), GameManager.emptyWeight);
                    tile.SetGridPos(new Vector2Int(x, y));
                    incGridArray[x, y] = tile;
                }
                else
                {
                    GridObj tile = this.grid[x - 1, y - 1].Clone();
                    tile.SetGridPos(new Vector2Int(x, y));
                    incGridArray[x, y] = tile;
                }
            }
        }
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        stack.Push(startingPosition);

        while (stack.Count > 0)
        {
            Vector2Int pos = stack.Pop();
            if (visited.Contains(pos)) continue;
            visited.Add(pos);
            GridObj current = incGrid.GetGridObj(pos);

            if (current == null) continue;
            //TODO: Add adjacent Tiles if current is a jumping pad and energy is not zero -> should find shortest path to goal and check if its reachable with energy crystals?
            if (!current.HasWallAt(WallPos.BACK))
            {
                GridObj neighbour = incGrid.GetAdjacentGridObj(current, WallPos.BACK);
                if(neighbour!=null && !neighbour.HasWallAt(WallPos.FRONT))
                {
                    stack.Push(neighbour.GetGridPos());
                }
            }
            if (!current.HasWallAt(WallPos.FRONT))
            {
                GridObj neighbour = incGrid.GetAdjacentGridObj(current, WallPos.FRONT);
                if (neighbour != null && !neighbour.HasWallAt(WallPos.BACK))
                {
                    stack.Push(neighbour.GetGridPos());
                }
            }
            if (!current.HasWallAt(WallPos.LEFT))
            {
                GridObj neighbour = incGrid.GetAdjacentGridObj(current, WallPos.LEFT);
                if (neighbour != null && !neighbour.HasWallAt(WallPos.RIGHT))
                {
                    stack.Push(neighbour.GetGridPos());
                }
            }
            if (!current.HasWallAt(WallPos.RIGHT))
            {
                GridObj neighbour = incGrid.GetAdjacentGridObj(current, WallPos.RIGHT);
                if (neighbour != null && !neighbour.HasWallAt(WallPos.LEFT))
                {
                    stack.Push(neighbour.GetGridPos());
                }
            }
        }
        
        for(int x = 0; x < incGrid.width; x++)
        {
            for(int y = 0; y < incGrid.height;y++)
            {
                // Only returns false if the problem tile is on the edge, which indicates that it's a newly generated tile we are checking
                // and not a closed off room the player created itself by placing his tiles
                if(!visited.Contains(new Vector2Int(x, y)) && (x == 1 || x == width-2|| y == 1 || y == height-2))
                {
                    // TODO: if energy is zero and no crystals are in reach
                    return false;
                }
            }
        }
        return true;
    }
}
