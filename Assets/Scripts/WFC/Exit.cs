using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit
{
    public GridObj gridObj;
    public int growthIndex;
    public Pair<GridObj, WallPos> adjacent;

    public Exit(GridObj gridObj, Pair<GridObj, WallPos> adjacent, int growthIndex)
    {
        this.gridObj = gridObj;
        this.growthIndex = growthIndex;
        this.adjacent = adjacent;    
    }

    
}
