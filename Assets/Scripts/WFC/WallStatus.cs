using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStatus
{
    [SerializeField] public bool back = false;
    [SerializeField] public bool front = false;
    [SerializeField] public bool left = false;
    [SerializeField] public bool right = false;

    public WallStatus() { }
    public WallStatus(bool front, bool back, bool left, bool right)
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

    public bool HasWallAt(WallPos wallPos)
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
                return false;
        }
    }

    public void PlaceWallAt(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.FRONT:
                this.front = true;
                break;
            case WallPos.BACK:
                this.back = true;
                break;
            case WallPos.LEFT:
                this.left = true;
                break;
            case WallPos.RIGHT:
                this.right = true;
                break;
        }
    }
}

public enum WallPos
{
    FRONT, BACK, LEFT, RIGHT
}
