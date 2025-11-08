using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEditor.Progress;

public class WFCBuilder2 : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int lowerWidthBound = 0;
    private int lowerHeightBound = 0;
    private Vector2Int currPos = new Vector2Int(0, 0);
    private List<WallPos> randomWalls = new List<WallPos>();
    private List<Vector2Int> neighbours = new List<Vector2Int>();
    public static List<GridObj> AllGridObjs = new List<GridObj>();


    // TODO: Get this from GridObj
    private int offsets = 1;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject destructibleWallPrefab;
    public GameObject exitPrefab;
    private List<Vector2Int> _toCollapse = new List<Vector2Int>();

    private Dictionary<Vector2Int, GridObj[,]> allGrids;
    //private List<GridObj[,]> allGrids;
    private GridObj[,] grid;

    Grid gridObj;

    /// <summary>
    /// initializing the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
        this.gridObj = new Grid(this.width, this.height);

        gridObj.CollapseWorld();
        gridObj.InstantiateMissing();
        gridObj.IncreaseGrid();
        gridObj.InstantiateMissing();
        gridObj.CollapseWorld();
        gridObj.IncreaseGrid();
        gridObj.InstantiateMissing();
        gridObj.CollapseWorld();
        gridObj.IncreaseGrid();
        gridObj.InstantiateMissing();


        if (true) return; // TODO remove old code
        allGrids = new Dictionary<Vector2Int, GridObj[,]>();
        grid = new GridObj[width, height];
        _toCollapse.Clear();
        _toCollapse.Add(currPos);
        CollapseWorld();

    }
    /// <summary>
    /// Collapsing the Nodes
    /// </summary>
    private void CollapseWorld()
    {
        while (_toCollapse.Count > 0)
        {
            currPos = _toCollapse[0];
            _toCollapse.RemoveAt(0);

            if (!IsInsideGrid(currPos) || grid[currPos.x, currPos.y] != null)
                continue;
            WallStatus wallStatus = new WallStatus();
            GenerateNeighbours();
            GetAllPossibleWalls(wallStatus);
            GenerateRandomNode(wallStatus);
        }
        InstantiateGrid();
        allGrids[new Vector2Int(lowerWidthBound, lowerHeightBound)] = grid;
    }
    /// <summary>
    /// Generating the neighbours and adding them to the Collapse
    /// </summary>
    private void GenerateNeighbours()
    {
        neighbours.Clear();
        neighbours.Add(new Vector2Int(currPos.x + offsets, currPos.y)); // Right of currpos
        neighbours.Add(new Vector2Int(currPos.x - offsets, currPos.y)); // Left of currpos
        neighbours.Add(new Vector2Int(currPos.x, currPos.y + offsets)); // back of currpos
        neighbours.Add(new Vector2Int(currPos.x, currPos.y - offsets)); // front of currpos

        foreach (Vector2Int pos in neighbours)
        {
            if (!_toCollapse.Contains(pos) && IsInsideGrid(pos) && grid[pos.x, pos.y] == null) _toCollapse.Add(pos);
        }
    }

    /// <summary>
    /// check if inside the defined grid
    /// </summary>
    /// <param name="v2int"> the position we want to check </param>
    /// <returns></returns>
    private bool IsInsideGrid(Vector2 v2int)
    {
        return (v2int.x < width && v2int.x >-1 && v2int.y < height && v2int.y > -1);
    }

    /// <summary>
    /// checking the surrounding nodes for walls and setting the wallStatus accordingly
    /// if no node is set, we add the corresponding wall to the randomWalls
    /// </summary>
    /// <param name="wallStatus"></param>
    void GetAllPossibleWalls(WallStatus wallStatus)
    {
        randomWalls.Clear();
        if (IsInsideGrid(neighbours[0]) && grid[neighbours[0].x, neighbours[0].y] != null)
        {
            if (grid[currPos.x + offsets, currPos.y].HasWallAt(WallPos.LEFT))
            {
                wallStatus.PlaceWallAt(WallPos.RIGHT, grid[currPos.x + offsets, currPos.y].GetWallTypeAt(WallPos.LEFT));
            }
        }
        else
        {
            randomWalls.Add(WallPos.RIGHT);
        }

        if (IsInsideGrid(neighbours[1]) && grid[neighbours[1].x, neighbours[1].y] != null)
        {
            if (grid[currPos.x - offsets, currPos.y].HasWallAt(WallPos.RIGHT))
            {
                wallStatus.PlaceWallAt(WallPos.LEFT, grid[currPos.x - offsets, currPos.y].GetWallTypeAt(WallPos.RIGHT));
            }

        }
        else
        {
            randomWalls.Add(WallPos.LEFT);
        }
        if (IsInsideGrid(neighbours[2]) && grid[neighbours[2].x, neighbours[2].y] != null)
        {
            if (grid[currPos.x, currPos.y + offsets].HasWallAt(WallPos.FRONT))
            {
                wallStatus.PlaceWallAt(WallPos.BACK, grid[currPos.x, currPos.y + offsets].GetWallTypeAt(WallPos.FRONT));
            }

        }
        else
        {
            randomWalls.Add(WallPos.BACK);
        }
        if (IsInsideGrid(neighbours[3]) && grid[neighbours[3].x, neighbours[3].y] != null)
        {
            if (grid[currPos.x, currPos.y - offsets].HasWallAt(WallPos.BACK))
            {
                wallStatus.PlaceWallAt(WallPos.FRONT, grid[currPos.x, currPos.y - offsets].GetWallTypeAt(WallPos.BACK));
            }
        }
        else
        {
            randomWalls.Add(WallPos.FRONT);
        }
    }
    /// <summary>
    /// Randomly chooses the WallPos inside randomWalls to get random wall positions
    /// Then adding the new GridObj to the grid by also setting the wallStatus new according to the new wall positions
    /// </summary>
    /// <param name="wallStatus"> giving the wallStatus</param>
    void GenerateRandomNode(WallStatus wallStatus)
    {
        List<WallPos> randomSelection = new List<WallPos>();
        if (randomWalls.Count > 0)
        {
            int randomCount = Random.Range(1, randomWalls.Count);
            randomSelection = randomWalls
            .OrderBy(_ => Random.value)
            .Take(randomCount)
            .ToList();
        }
        grid[currPos.x, currPos.y] = new GridObj(new Vector2Int(lowerWidthBound + currPos.x, lowerHeightBound + currPos.y), WallPosToWallStatus(randomSelection, wallStatus));
    }

    /// <summary>
    /// Converting the WallPos to wallStatus given a list of wallPos
    /// </summary>
    /// <param name="wallPositions"> The list of wallPositions to turn into a WallStatus</param>
    /// <param name="wallStatus"> The wallStatus that may have predefined walls already</param>
    /// <returns></returns>
    private WallStatus WallPosToWallStatus(List<WallPos> wallPositions, WallStatus wallStatus)
    {
        for (int i = 0; i < wallPositions.Count; i++)
        {
            switch (wallPositions[i])
            {
                case WallPos.FRONT:
                    wallStatus.PlaceWallAt(WallPos.FRONT); break;
                case WallPos.BACK:
                    wallStatus.PlaceWallAt(WallPos.BACK); break;
                case WallPos.LEFT:
                    wallStatus.PlaceWallAt(WallPos.LEFT); break;
                case WallPos.RIGHT:
                    wallStatus.PlaceWallAt(WallPos.RIGHT); break;
            }
        }
        return wallStatus;
    }

    /// <summary>
    /// Instantiating the grid
    /// </summary>
    private void InstantiateGrid()
    {

        int randLeft = Random.Range(0, height - 1);
        int randRight = Random.Range(0, height - 1);
        int randBottom = Random.Range(0, width - 1);
        int randTop = Random.Range(0, width - 1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j] == null) continue;
                grid[i, j].InstantiateObj();
                AllGridObjs.Add(grid[i, j]);

                /*
                    Here we fill in random destuctible walls (one on each side)
                    To get the DestructibleWall script, you can simply call GridObj.GetWallObjAt(WallPos wallPos) and the DestructibleWall component will be on the only *child* of the returned object
                    We also access the destruction callback and add DestructionCallback() as a listener
                */

                // Fill in missing edge walls, i = x, j = z
                if (i == 0)
                {
                    if (j == randLeft)
                    {
                        //grid[i, j].PlaceWallAt(WallPos.LEFT, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.LEFT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        //grid[i, j].PlaceWallAt(WallPos.LEFT);     // leftmost x
                    }
                }
                if (i == width - 1)
                {
                    if (j == randRight)
                    {
                        //grid[i, j].PlaceWallAt(WallPos.RIGHT, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.RIGHT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        //grid[i, j].PlaceWallAt(WallPos.RIGHT);  // rightmost x
                    }
                }

                if (j == 0)
                {
                    if (i == randBottom)
                    {
                        //grid[i, j].PlaceWallAt(WallPos.FRONT, WallType.EXIT); // for now lets always place it at the bottom side
                        grid[i, j].GetExitCb(WallPos.FRONT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        //grid[i, j].PlaceWallAt(WallPos.FRONT);   // frontmost z
                    }
                }

                if (j == height - 1)
                {
                    if (i == randTop)
                    {
                        //grid[i, j].PlaceWallAt(WallPos.BACK, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.BACK).AddListener(DestructionCallback);
                    }
                    else
                    {
                        //grid[i, j].PlaceWallAt(WallPos.BACK);  // backmost z
                    }
                }
            }
        }
    }

    // TODO generate new map parts
    /// <summary>
    /// Called when a wall self destructs
    /// creates a new grid, sets its bounds in the global position and starts a new collapse progress if there is not one already
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="wallPos"></param>
    private void DestructionCallback(GridObj gridObj, WallPos wallPos)
    {
        if (true) return; // TODO remove old code
        grid = new GridObj[width, height];
        Vector2Int gp = gridObj.GetGridPos();
        Debug.Log($"Destroyed wall at {wallPos.ToString()} for GridObj at [{gp.x}, {gp.y}]");

        SetNewBounds(gp, wallPos);

        bool alreadyExpanded = allGrids.ContainsKey(new Vector2Int(lowerWidthBound, lowerHeightBound));

        if (!alreadyExpanded)
        {
            currPos = new Vector2Int(0, 0);
            _toCollapse.Clear();
            _toCollapse.Add(currPos);
            CollapseWorld();
        }

        GridObj adj = GetAdjacentGridObj(gridObj.GetGridPos(), wallPos);
        if (adj == null) return;
        adj.RemoveWall(WallStatus.GetOppositePos(wallPos));
    }
    
    /// <summary>
    /// Called when an exit self destructs
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="wallPos"></param>
    private void ExitDestructionCallback(GridObj gridObj, WallPos wallPos)
    {
        
    }

    /// <summary>
    /// Sets new bounds of a grid dependant on the current grid Position and the direction to expand
    /// </summary>
    /// <param name="gridPos"> The position of the node where the next grid will expand from</param>
    /// <param name="wallPos"> The direction in which the grid will expand </param>
    private void SetNewBounds(Vector2Int gridPos, WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                lowerHeightBound = gridPos.y - height;
                lowerWidthBound = Mathf.FloorToInt(gridPos.x / (float)width) * width;
                break;
            case WallPos.BACK:
                lowerHeightBound = gridPos.y + 1;
                lowerWidthBound = Mathf.FloorToInt(gridPos.x / (float)width) * width;
                break;
            case WallPos.LEFT:
                lowerWidthBound = gridPos.x - width;
                lowerHeightBound = Mathf.FloorToInt(gridPos.y / height) * height;
                break;
            case WallPos.RIGHT:
                lowerWidthBound = gridPos.x + 1;
                lowerHeightBound = Mathf.FloorToInt(gridPos.y / height) * height;
                break;
        }
    }

    /// <summary>
    /// Returns adjacent GridObj in WallPos direction or null
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private GridObj GetAdjacentGridObj(Vector2Int pos, WallPos direction)
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

        foreach (var pair in allGrids)
        {
            GridObj[,] grid = pair.Value;
            
            for(int x = 0; x < grid.GetLength(0); x++)
            {
                for(int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y].GetGridPos() == targetPos) return grid[x, y];
                }
            }
        }

        return null;
    }
}
