using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridObj gridObj;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode = null;

    public PathNode(GridObj gridObj)
    {
        this.gridObj = gridObj;
    }

    public void CalculateFCost()
    {
        this.fCost = this.gCost + this.fCost;
    }

    public GridObj GetGridObj() {return this.gridObj;}

}
