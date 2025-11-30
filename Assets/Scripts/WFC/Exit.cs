using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class describing the game's exit
/// </summary>
public class Exit
{
    public GridObj gridObj;
    public int worldOffsetX;
    public int worldOffsetY;
    public Pair<GridObj, WallPos> adjacent;

    /// <summary>
    /// Create the Exit from a GridObj and a growthIndex
    /// </summary>
    /// <param name="gridObj">GridObj at which the exit should be</param>
    /// <param name="growthIndex"></param>
    /// <param name="adjacent"></param>

    public Exit(GridObj gridObj, Pair<GridObj, WallPos> adjacent, int worldOffsetX, int worldOffsetY)
    {
        this.gridObj = gridObj;
        this.worldOffsetX = worldOffsetX;
        this.worldOffsetY = worldOffsetX;
        this.adjacent = adjacent;    
    }

    
}
