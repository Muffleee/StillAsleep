using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGrid
{
    // Start is called before the first frame update
    public GridObj[,] FillInitialGridLayout(GridObj[,] grid)
    {
        grid[0, 0] = new GridObj(new Vector2Int(0, 0), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[1, 0] = new GridObj(new Vector2Int(1, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.REGULAR));
        grid[2, 0] = new GridObj(new Vector2Int(2, 0), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.REGULAR, WallType.NONE));
        grid[3, 0] = new GridObj(new Vector2Int(3, 0), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
        grid[4, 0] = new GridObj(new Vector2Int(4, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));

        grid[0, 1] = new GridObj(new Vector2Int(0, 1), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[1, 1] = new GridObj(new Vector2Int(1, 1), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.REGULAR));
        grid[2, 1] = new GridObj(new Vector2Int(2, 1), new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.REGULAR, WallType.NONE));
        grid[3, 1] = new GridObj(new Vector2Int(3, 1), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[4, 1] = new GridObj(new Vector2Int(4, 1), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));

        grid[0, 2] = new GridObj(new Vector2Int(0, 2), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
        grid[1, 2] = new GridObj(new Vector2Int(1, 2), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[2, 2] = new GridObj(new Vector2Int(2, 2), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[3, 2] = new GridObj(new Vector2Int(3, 2), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[4, 2] = new GridObj(new Vector2Int(4, 2), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE));

        grid[0, 3] = new GridObj(new Vector2Int(0, 3), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.REGULAR));
        grid[1, 3] = new GridObj(new Vector2Int(1, 3), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.REGULAR));
        grid[2, 3] = new GridObj(new Vector2Int(2, 3), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));
        grid[3, 3] = new GridObj(new Vector2Int(3, 3), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.REGULAR));
        grid[4, 3] = new GridObj(new Vector2Int(4, 3), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));

        grid[0, 4] = new GridObj(new Vector2Int(0, 4), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));
        grid[1, 4] = new GridObj(new Vector2Int(1, 4), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.REGULAR));
        grid[2, 4] = new GridObj(new Vector2Int(2, 4), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));
        grid[3, 4] = new GridObj(new Vector2Int(3, 4), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.REGULAR));
        grid[4, 4] = new GridObj(new Vector2Int(4, 4), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));

        foreach (GridObj el in grid)
        {
            el.SetGridType(GridType.REGULAR);
        }
        return grid;
    }

    public GridObj[,] IncreaseFirstTime(GridObj[,] grid)
    {
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        if(grid[0, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 0].DestroyObj();
            grid[0, 0] = new GridObj(new Vector2Int(0, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));
            grid[0, 0].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[0, 0]);
        if (grid[0, 2].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 2].DestroyObj();
            grid[0, 2] = new GridObj(new Vector2Int(0, 2), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.REGULAR, WallType.NONE));
            grid[0, 2].SetGridType(GridType.JUMPINGPAD);
        }
        g.PlaceObj(grid[0, 2]);
        if (grid[0, 1].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 1].DestroyObj();
            grid[0, 1] = new GridObj(new Vector2Int(0, 1), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
            if(grid[0, 2].GetGridType() == GridType.JUMPINGPAD) grid[0, 1].SetGridType(GridType.REGULAR);
            else grid[0, 1].SetGridType(GridType.JUMPINGPAD);
        }
        g.PlaceObj(grid[0, 1]);
        if (grid[0, 3].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 3].DestroyObj();
            grid[0, 3] = new GridObj(new Vector2Int(0, 3), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.REGULAR));
            grid[0, 3].SetGridType(GridType.TRAP);
        }
        g.PlaceObj(grid[0, 3]);
        if (grid[0, 4].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 4].DestroyObj();
            grid[0, 4] = new GridObj(new Vector2Int(0, 4), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.REGULAR, WallType.NONE));
            if (grid[0, 3].GetGridType() == GridType.TRAP) grid[0, 4].SetGridType(GridType.REGULAR);
            else grid[0, 4].SetGridType(GridType.TRAP);
        }
        g.PlaceObj(grid[0, 4]);
        return grid;
    }
}
