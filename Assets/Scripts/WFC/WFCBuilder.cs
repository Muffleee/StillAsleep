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
    private Vector2Int currPos = new Vector2Int(0, 0);
    private List<WallPos> randomWalls = new List<WallPos>();
    private List<Vector2Int> neighbours = new List<Vector2Int>();

    // TODO: Get this from GridObj
    private int offsets = 1;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject destructibleWallPrefab;
    private List<Vector2Int> _toCollapse = new List<Vector2Int>();

    private GridObj[,] grid;

    /// <summary>
    /// initializing the grid, clearing the collapse-list and start the collapsing process from the first node
    /// </summary>
    void Start()
    {
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
        return (v2int.x < width && v2int.x > -1 && v2int.y < height && v2int.y > -1);
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
        grid[currPos.x, currPos.y] = new GridObj(currPos, wallPrefab, floorPrefab, destructibleWallPrefab, WallPosToWallStatus(randomSelection, wallStatus));
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
                        grid[i, j].PlaceWallAt(WallPos.LEFT, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.LEFT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        grid[i, j].PlaceWallAt(WallPos.LEFT);     // leftmost x
                    }
                }
                if (i == width - 1)
                {
                    if (j == randRight)
                    {
                        grid[i, j].PlaceWallAt(WallPos.RIGHT, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.RIGHT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        grid[i, j].PlaceWallAt(WallPos.RIGHT);  // rightmost x
                    }
                }

                if (j == 0)
                {
                    if (i == randBottom)
                    {
                        grid[i, j].PlaceWallAt(WallPos.FRONT, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.FRONT).AddListener(DestructionCallback);
                    }
                    else
                    {
                        grid[i, j].PlaceWallAt(WallPos.FRONT);   // frontmost z
                    }
                }

                if (j == height - 1)
                {
                    if (i == randTop)
                    {
                        grid[i, j].PlaceWallAt(WallPos.BACK, WallType.DESTRUCTIBLE);
                        grid[i, j].GetDestructibleWallCb(WallPos.BACK).AddListener(DestructionCallback);
                    }
                    else
                    {
                        grid[i, j].PlaceWallAt(WallPos.BACK);  // backmost z
                    }
                }
            }
        }
    }
    
    // TODO generate new map parts
    /// <summary>
    /// Called when a wall self destructs
    /// </summary>
    /// <param name="gridObj"></param>
    /// <param name="wallPos"></param>
    private void DestructionCallback(GridObj gridObj, WallPos wallPos)
    {
        Vector2Int gp = gridObj.GetGridPos();
        Debug.Log($"Destroyed wall at {wallPos.ToString()} for GridObj at [{gp.x}, {gp.y}]");
    }
}
