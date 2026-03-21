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

    public GridObj[,] IncreaseSecondTime(GridObj[,] grid)
    {
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        if (grid[0,0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[0, 0].DestroyObj();
            grid[0, 0] = new GridObj(new Vector2Int(0, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));
            grid[0, 0].SetGridType(GridType.ROTATING);
            g.AddToRotating(grid[0, 0]);
        }
        g.PlaceObj(grid[0, 0]);
        if (grid[1,0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[1, 0].DestroyObj();
            grid[1, 0] = new GridObj(new Vector2Int(1, 0), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
            grid[1, 0].SetGridType(GridType.ICE);
        }
        g.PlaceObj(grid[1, 0]);
        if (grid[2, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[2, 0].DestroyObj();
            grid[2, 0] = new GridObj(new Vector2Int(2, 0), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
            grid[2, 0].SetGridType(GridType.ROTATING);
            g.AddToRotating(grid[2, 0]);
        }
        g.PlaceObj(grid[2, 0]);
        if (grid[3, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[3, 0].DestroyObj();
            grid[3, 0] = new GridObj(new Vector2Int(3, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.NONE));
            grid[3, 0].SetGridType(GridType.ICE);
        }
        g.PlaceObj(grid[3, 0]);
        if (grid[4, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[4, 0].DestroyObj();
            grid[4, 0] = new GridObj(new Vector2Int(4, 0), new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.NONE, WallType.NONE));
            grid[4, 0].SetGridType(GridType.SPIKE);
            g.AddToSpike(grid[4, 0]);
        }
        g.PlaceObj(grid[4, 0]);
        if (grid[5, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[5, 0].DestroyObj();
            grid[5, 0] = new GridObj(new Vector2Int(5, 0), new WallStatus(WallType.NONE, WallType.NONE, WallType.NONE, WallType.REGULAR));
            grid[5, 0].SetGridType(GridType.SPIKE);
            g.AddToSpike(grid[5, 0]);
        }
        g.PlaceObj(grid[5, 0]);

        return grid;
    }

    public GridObj[,] IncreaseThirdTime(GridObj[,] grid)
    {
        Grid g = GameManager.INSTANCE.GetCurrentGrid();
        if (grid[6, 0].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 0].DestroyObj();
            grid[6, 0] = new GridObj(new Vector2Int(6, 0), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.REGULAR, WallType.NONE));
            grid[6, 0].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 0]);
        if (grid[6, 1].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 1].DestroyObj();
            grid[6, 1] = new GridObj(new Vector2Int(6, 1), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.NONE));
            grid[6, 1].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 2]);
        if (grid[6, 2].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 2].DestroyObj();
            grid[6, 2] = new GridObj(new Vector2Int(6, 2), new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.REGULAR));
            grid[6, 2].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 2]);
        if (grid[6, 3].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 3].DestroyObj();
            grid[6, 3] = new GridObj(new Vector2Int(6, 3), new WallStatus(WallType.NONE, WallType.NONE, WallType.REGULAR, WallType.NONE));
            grid[6, 3].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 3]);
        if (grid[6, 4].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 4].DestroyObj();
            grid[6, 4] = new GridObj(new Vector2Int(6, 4), new WallStatus(WallType.NONE, WallType.REGULAR, WallType.NONE, WallType.REGULAR));
            grid[6, 4].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 4]);
        if (grid[6, 5].GetGridType() is GridType.REPLACEABLE)
        {
            grid[6, 5].DestroyObj();
            grid[6, 5] = new GridObj(new Vector2Int(6, 5), new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.NONE, WallType.NONE));
            grid[6, 5].SetGridType(GridType.REGULAR);
        }
        g.PlaceObj(grid[6, 5]);
        return grid;
    }

}
