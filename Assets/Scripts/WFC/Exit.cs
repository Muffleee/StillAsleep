using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class describing the game's exit
/// </summary>
public class Exit
{
    public GridObj gridObj;
    public int growthIndex;
    public Pair<GridObj, WallPos> adjacent;

    /// <summary>
    /// Create the Exit from a GridObj and a growthIndex
    /// </summary>
    /// <param name="gridObj">GridObj at which the exit should be</param>
    /// <param name="growthIndex"></param>
    /// <param name="adjacent"></param>

    public Exit(GridObj gridObj, Pair<GridObj, WallPos> adjacent, int growthIndex)
    {
        this.gridObj = gridObj;
        this.growthIndex = growthIndex;
        this.adjacent = adjacent;    
    }

    
}
