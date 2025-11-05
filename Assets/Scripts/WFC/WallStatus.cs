using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStatus
{
    [SerializeField] public WallType back = WallType.NONE;
    [SerializeField] public WallType front = WallType.NONE;
    [SerializeField] public WallType left = WallType.NONE;
    [SerializeField] public WallType right = WallType.NONE;

    public WallStatus() { }
    public WallStatus(WallType front, WallType back, WallType left, WallType right)
    {
        this.front = front;
        this.back = back;
        this.left = left;
        this.right = right;
    }

    public static int WallPosToInt(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                return 0;
            case WallPos.BACK:
                return 1;
            case WallPos.LEFT:
                return 2;
            case WallPos.RIGHT:
                return 3;
            default:
                return 0;
        }
    }

    public static Vector3 GetWallRotation(WallPos wallPos)
    {
        float yRot = wallPos == WallPos.LEFT || wallPos == WallPos.RIGHT ? 90f : 0f;
        return new Vector3(0f, yRot, 0f);
    }

    public static Vector3 GetWallWorldPos(Vector3 center, WallPos wallPos)
    {
        Vector3 offsetVec;

        switch (wallPos)
        {
            case WallPos.FRONT:
                offsetVec = new Vector3(0f, 0f, -GridObj.WALL_OFFSET);
                break;
            case WallPos.BACK:
                offsetVec = new Vector3(0f, 0f, GridObj.WALL_OFFSET);
                break;
            case WallPos.LEFT:
                offsetVec = new Vector3(-GridObj.WALL_OFFSET, 0f, 0f);
                break;
            case WallPos.RIGHT:
                offsetVec = new Vector3(GridObj.WALL_OFFSET, 0f, 0f);
                break;
            default:
                offsetVec = new Vector3(0f, 0f, 0f);
                break;
        }

        return center + offsetVec;
    }

    public static WallPos GetOppositePos(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                return WallPos.BACK;
            case WallPos.BACK:
                return WallPos.FRONT;
            case WallPos.LEFT:
                return WallPos.RIGHT;
            default:
                return WallPos.LEFT;
        }
    }

    public bool HasWallAt(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                return this.front != WallType.NONE;
            case WallPos.BACK:
                return this.back != WallType.NONE;
            case WallPos.LEFT:
                return this.left != WallType.NONE;
            case WallPos.RIGHT:
                return this.right != WallType.NONE;
            default:
                return false;
        }
    }

    public WallType GetWallAt(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                return this.front;
            case WallPos.BACK:
                return this.back;
            case WallPos.LEFT:
                return this.left;
            case WallPos.RIGHT:
                return this.right;
            default:
                return WallType.NONE;
        }
    }

    public void PlaceWallAt(WallPos wallPos)
    {
        this.PlaceWallAt(wallPos, WallType.REGULAR);
    }

    public void PlaceWallAt(WallPos wallPos, WallType wallType)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                this.front = wallType;
                break;
            case WallPos.BACK:
                this.back = wallType;
                break;
            case WallPos.LEFT:
                this.left = wallType;
                break;
            case WallPos.RIGHT:
                this.right = wallType;
                break;
        }
    }

    public void RemoveWallAt(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                this.front = WallType.NONE;
                break;
            case WallPos.BACK:
                this.back = WallType.NONE;
                break;
            case WallPos.LEFT:
                this.left = WallType.NONE;
                break;
            case WallPos.RIGHT:
                this.right = WallType.NONE;
                break;
        }
    }
}

public enum WallPos
{
    FRONT, BACK, LEFT, RIGHT
}

public enum WallType
{
    NONE, EXIT, DESTRUCTIBLE, REGULAR
}