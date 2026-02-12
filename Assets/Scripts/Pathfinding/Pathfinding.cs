using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

/// <summary>
/// Class handling pathfinding within the grid using the A* algorithm.
/// </summary>
public class Pathfinding : MonoBehaviour
{
    [SerializeField] public int weight = 15;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject pathObject;
    private Grid grid;
    private PathNode[,] nodeGrid;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    
    public List<GridObj> FindPath(Vector2Int start, Vector2Int end)
    {
        this.grid = this.gameManager.GetCurrentGrid();

        if (!this.grid.IsInsideGrid(start) || !this.grid.IsInsideGrid(end)) return null;

        this.nodeGrid = new PathNode[this.grid.width, this.grid.height];
        
        for (int x = 0; x < this.grid.width; x++)
        {
            for (int y = 0; y < this.grid.height; y++)
            {
                PathNode pathNode = new PathNode(this.grid.GetGridObj(x, y));
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();

                this.nodeGrid[x, y] = pathNode;
            }
        }

        PathNode startNode = this.nodeGrid[start.x, start.y];
        PathNode endNode = this.nodeGrid[end.x, end.y];

        startNode.gCost = 0;
        startNode.hCost = this.CalculateCost(startNode, endNode);
        startNode.CalculateFCost();

        this.openList = new List<PathNode> {startNode};
        this.closedList = new List<PathNode>();

        while (this.openList.Count > 0)
        {
            PathNode currentNode = this.GetLowestFCostNode(this.openList);
            if (currentNode == endNode) return this.CalculatePath(endNode);

            this.openList.Remove(currentNode);
            this.closedList.Add(currentNode);

            foreach (PathNode neighbourNode in this.GetPossibleNeighbours(currentNode))
            {
                if (this.closedList.Contains(neighbourNode)) continue;

                int tentativeGCost = currentNode.gCost + this.CalculateCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = this.CalculateCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!this.openList.Contains(neighbourNode)) {this.openList.Add(neighbourNode);}
                }
            }
        }
        return null;
    }

    private List<GridObj> CalculatePath(PathNode endNode)
    {
        List<GridObj> path = new List<GridObj>();
        path.Add(endNode.GetGridObj());

        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode.GetGridObj());
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();

        return path;
    }

    private int CalculateCost(PathNode start, PathNode end)
    {
        return this.weight * Grid.CalculateDistance(start.GetGridObj().GetGridPos(), end.GetGridObj().GetGridPos());
    }

    private List<PathNode> GetPossibleNeighbours(PathNode currentNode)
    {
        GridObj currentGridObj = currentNode.GetGridObj();
        List<PathNode> neighbours = new List<PathNode>();

        foreach (WallPos wPos in Enum.GetValues(typeof(WallPos)))
        {
            GridObj adjacentGridObj = this.grid.GetAdjacentGridObj(currentGridObj, wPos);
            if (currentGridObj.GetWallTypeAt(wPos) != WallType.REGULAR && GridObj.IsMovementAllowed(adjacentGridObj))
            {
                Vector2Int adjacentGridPos = adjacentGridObj.GetGridPos();
                neighbours.Add(this.nodeGrid[adjacentGridPos.x, adjacentGridPos.y]);
            }
        }

        return neighbours;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodes)
    {
        PathNode lowestFCostNode = pathNodes[0];

        for (int i = 1; i < pathNodes.Count; i++)
        {
            if (pathNodes[i].fCost < lowestFCostNode.fCost) lowestFCostNode = pathNodes[i];
        }

        return lowestFCostNode;
    }

    private List<GameObject> pathObjects = new List<GameObject>();
    private readonly Vector3 EMITTER_OFFSET = new Vector3(0, 0.5f, 0);
    public void SpawnPath(List<GridObj> path)
    {   
        if (path == null) path = new List<GridObj>();
        
        for (int i = this.pathObjects.Count - 1; i >= 0; i--)
        {
            GameObject debugObj = this.pathObjects[i];
            bool found = false;

            foreach (GridObj obj in path)
            {
                if (obj.GetWorldPos(this.grid.GetWorldOffsetX(), this.grid.GetWorldOffsetY()) + EMITTER_OFFSET == debugObj.transform.position)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Destroy(debugObj);
                this.pathObjects.RemoveAt(i);
            }
        }

        if (path != null)
        {
            foreach (GridObj node in path)
            {   
                bool exists = false;
                Vector3 worldPos = node.GetWorldPos(this.grid.GetWorldOffsetX(), this.grid.GetWorldOffsetY()) + EMITTER_OFFSET;

                foreach(GameObject obj in this.pathObjects)
                {
                    if(obj.transform.position == worldPos)
                    {
                        exists = true;
                        break;
                    }
                } 
                
                if(exists) continue;

                this.pathObjects.Add(Instantiate
                (
                    this.pathObject,
                    worldPos,
                    new Quaternion()
                ));
                
            }
        }
    }
}
