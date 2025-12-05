using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class describing a tile's walls.
/// </summary>
public class WallStatus
{
    [SerializeField] public WallType back = WallType.NONE;
    [SerializeField] public WallType front = WallType.NONE;
    [SerializeField] public WallType left = WallType.NONE;
    [SerializeField] public WallType right = WallType.NONE;

    /// <summary>
    /// Create a new WallStatus with no WallTypes set.
    /// </summary>
    public WallStatus() { }

    /// <summary>
    /// Create a new WallStatus with four given WallTypes for the respective sides.
    /// </summary>
    /// <param name="front"></param>
    /// <param name="back"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public WallStatus(WallType front, WallType back, WallType left, WallType right)
    {
        this.front = front;
        this.back = back;
        this.left = left;
        this.right = right;
    }

    /// <summary>
    /// Convert a WallPos/side to an according integer value.
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns>Integers 1 through 4 for front, back, left, and right respectively.</returns>
    public static int WallPosToInt(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.FRONT => 0,
            WallPos.BACK => 1,
            WallPos.LEFT => 2,
            WallPos.RIGHT => 3,
            _ => -1
        };
    }

    /// <summary>
    /// Convert an integer to the according WallPos value.
    /// Any integers larger than 3 outside of will be adjusted via modulo. Negative integers not accepted.
    /// </summary>
    /// <param name="wallPosInt"></param>
    /// <returns></returns>
    /// <exception cref="System.IndexOutOfRangeException"></exception>()
    public static WallPos IntToWallPos(int wallPosInt)
    {
        return (wallPosInt % 4) switch
        {
            0 => WallPos.FRONT,
            1 => WallPos.BACK,
            2 => WallPos.LEFT,
            3 => WallPos.RIGHT,
            _ => throw new System.IndexOutOfRangeException(),
        };
    }

    /// <summary>
    /// Returns the rotation of a WallPos.
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns>(0f, 0f, 0f) for FRONT and BACK, (0f, 90f, 0f) for LEFT and RIGHT</returns>
    public static Vector3 GetWallRotation(WallPos wallPos)
    {
        float yRot = wallPos == WallPos.LEFT || wallPos == WallPos.RIGHT ? 90f : 0f;
        return new Vector3(0f, yRot, 0f);
    }

    /// <summary>
    /// Get the world position of a wall given its WallPos and the center of the tile.
    /// </summary>
    /// <param name="center">World position of the center of wall's tile/GridObj.</param>
    /// <param name="wallPos">Side on which the wall is.</param>
    /// <returns>World position of the wall.</returns>
    public static Vector3 GetWallWorldPos(Vector3 center, WallPos wallPos)
    {
        var offsetVec = wallPos switch
        {
            WallPos.FRONT => new Vector3(0f, 0f, -GridObj.WALL_OFFSET),
            WallPos.BACK => new Vector3(0f, 0f, GridObj.WALL_OFFSET),
            WallPos.LEFT => new Vector3(-GridObj.WALL_OFFSET, 0f, 0f),
            WallPos.RIGHT => new Vector3(GridObj.WALL_OFFSET, 0f, 0f),
            _ => new Vector3(0f, 0f, 0f),
        };
        return center + offsetVec;
    }

    /// <summary>
    /// Get the side opposite of the given WallPos.
    /// </summary>
    /// <param name="wallPos">The side of which the opposite shall be determined.</param>
    /// <returns>WallPos of the opposite side.</returns>
    public static WallPos GetOppositePos(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.FRONT => WallPos.BACK,
            WallPos.BACK => WallPos.FRONT,
            WallPos.LEFT => WallPos.RIGHT,
            _ => WallPos.LEFT,
        };
    }

    /// <summary>
    /// Check whether there is a wall at a given WallPos.
    /// </summary>
    /// <param name="wallPos">Side to be checked.</param>
    /// <returns></returns>
    public bool HasWallAt(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.FRONT => this.front != WallType.NONE,
            WallPos.BACK => this.back != WallType.NONE,
            WallPos.LEFT => this.left != WallType.NONE,
            WallPos.RIGHT => this.right != WallType.NONE,
            _ => false,
        };
    }

    /// <summary>
    /// Get the wall type at a given WallPos.
    /// </summary>
    /// <param name="wallPos">Side from which a wall should be retreived</param>
    /// <returns>WallType at the searched side.</returns>
    public WallType GetWallAt(WallPos wallPos)
    {
        return wallPos switch
        {
            WallPos.FRONT => this.front,
            WallPos.BACK => this.back,
            WallPos.LEFT => this.left,
            WallPos.RIGHT => this.right,
            _ => WallType.NONE,
        };
    }

    /// <summary>
    /// Places a regular wall at the given WallPos.
    /// </summary>
    /// <param name="wallPos"></param>
    public void PlaceWallAt(WallPos wallPos)
    {
        this.PlaceWallAt(wallPos, WallType.REGULAR);
    }

    /// <summary>
    /// Places a wall of given WallType at the given WallPos.
    /// </summary>
    /// <param name="wallPos"></param>
    /// <param name="wallType"></param>
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
        
    /// <summary>
    /// Removes any walls at the given WallPos.
    /// </summary>
    /// <param name="wallPos"></param>
    public void RemoveWallAt(WallPos wallPos)
    {
        this.PlaceWallAt(wallPos, WallType.NONE);
    }

    /// <summary>
    /// Clone the WallStatus for use by other GridObj without editing the current one.
    /// </summary>
    /// <returns>Clone of the WallStatus.</returns>
    public WallStatus Clone()
    {
        return new WallStatus(
            this.front,
            this.back,
            this.left,
            this.right
        );
    }
}

/// <summary>
/// Enum describing all possible sides at which a wall could be positioned.
/// </summary>
public enum WallPos
{
    FRONT, BACK, LEFT, RIGHT
}

/// <summary>
/// Enum describing all possible wall types.
/// </summary>
public enum WallType
{
    NONE, EXIT, DESTRUCTIBLE, REGULAR
}