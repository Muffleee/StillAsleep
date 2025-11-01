using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.Rendering.Universal.Internal;

public class GridObj
{
    public static float PLACEMENT_FACTOR = 2f;
    public static float WALL_OFFSET = 0.96f;
    private GameObject wallPrefab;
    private GameObject floorPrefab;
    private Vector2Int gridPos;
    private WallStatus wallStatus;
    private GameObject parentObj = null;
    private GameObject floorObj = null;
    private GameObject[] wallObjs = new GameObject[] { null, null, null, null };

    /// <summary>
    /// Create a GridObj given a Vector3Int (grid position) and a WallStatus
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="wallStatus"></param>
    public GridObj(Vector2Int gridPos, GameObject wallPrefab, GameObject floorPrefab, WallStatus wallStatus)
    {
        this.gridPos = gridPos;
        this.wallPrefab = wallPrefab;
        this.floorPrefab = floorPrefab;
        this.wallStatus = wallStatus;
    }

    /// <summary>
    /// Get the position of this object in the virtual grid
    /// </summary>
    /// <returns> Vector2Int </returns>
    public Vector2Int GetGridPos()
    {
        return this.gridPos;
    }

    /// <summary>
    /// Get the world position of this object
    /// </summary>
    /// <returns> Vector3 </returns>
    public Vector3 GetWorldPos()
    {
        return new Vector3(this.gridPos.x * GridObj.PLACEMENT_FACTOR, 0, this.gridPos.y * GridObj.PLACEMENT_FACTOR);
    }

    /// <summary>
    /// Checks if a wall exists on this side of the object
    /// </summary>
    /// <param name="wallPos"> Side to check </param>
    /// <returns> bool </returns>
    public bool HasWallAt(WallPos wallPos)
    {
        return wallStatus.HasWallAt(wallPos);
    }

    /// <summary>
    /// Instantiate the object in its current state into the game world
    /// </summary>
    public void InstantiateObj()
    {
        if (this.parentObj != null)
        {
            Debug.LogWarning("Attempted to instantiate already existing GridObj");
            return;
        }
        Vector3 worldPos = this.GetWorldPos();
        this.parentObj = GameObject.Instantiate(new GameObject($"Parent at [{worldPos.x}], {worldPos.y}, {worldPos.z}"), worldPos, Quaternion.identity);
        this.floorObj = GameObject.Instantiate(floorPrefab, this.GetWorldPos(), Quaternion.identity);
        this.floorObj.transform.SetParent(this.parentObj.transform);

        if (this.wallStatus.HasWallAt(WallPos.FRONT))
        {
            this.InstantiateWall(WallPos.FRONT);
        }
        if (this.wallStatus.HasWallAt(WallPos.BACK))
        {
            this.InstantiateWall(WallPos.BACK);
        }
        if (this.wallStatus.HasWallAt(WallPos.LEFT))
        {
            this.InstantiateWall(WallPos.LEFT);
        }
        if (this.wallStatus.HasWallAt(WallPos.RIGHT))
        {
            this.InstantiateWall(WallPos.RIGHT);
        }
    }

    /// <summary>
    /// Place a wall on a chosen side
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    public void PlaceWallAt(WallPos wallPos)
    {
        if (this.HasWallAt(wallPos)) return;
        this.wallStatus.PlaceWallAt(wallPos);

        if (this.parentObj == null)
        {
            Debug.LogWarning("Attempted to place wall on NULL GridObj");
            return;
        }

        this.InstantiateWall(wallPos);
    }

    /// <summary>
    /// Overloaded method to instantiate a WallType.REGULAR wall
    /// </summary>
    /// <param name="wallPos"></param>
    private void InstantiateWall(WallPos wallPos)
    {
        this.InstantiateWall(wallPos, WallType.REGULAR);
    }

    /// <summary>
    /// Instantiate a wall and only change the data on the in-game object, helper method
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    private void InstantiateWall(WallPos wallPos, WallType wallType)
    {   
        // TODO change prefab according to WallType
        if (this.parentObj == null) return;
        int index = WallStatus.WallPosToInt(wallPos);
        if (this.wallObjs[index] != null) return;

        GameObject newWall = GameObject.Instantiate(wallPrefab, WallStatus.GetWallWorldPos(this.GetWorldPos(), wallPos), Quaternion.Euler(WallStatus.GetWallRotation(wallPos)));
        newWall.transform.SetParent(this.parentObj.transform);
        this.wallObjs[index] = newWall;
    }

    /// <summary>
    /// Remove a wall if it exists, also deletes its GameObject instance
    /// </summary>
    /// <param name="wallPos"> The side the wall is at </param>
    public void RemoveWall(WallPos wallPos)
    {
        int index = WallStatus.WallPosToInt(wallPos);
        GameObject obj = this.wallObjs[index];
        if (obj == null) return;
        this.wallObjs[index] = null;
        GameObject.Destroy(obj);
    }

    /// <summary>
    /// Destroy the GameObject instance
    /// </summary>
    public void DestroyObj()
    {
        GameObject.Destroy(this.floorObj);
        this.floorObj = null;

        for (int i = 0; i < this.wallObjs.Length; i++)
        {
            GameObject.Destroy(this.wallObjs[i]);
            this.wallObjs[i] = null;
        }

        GameObject.Destroy(this.parentObj);
        this.parentObj = null;
    }

    /// <summary>
    /// Get WallType at position
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public WallType GetWallAt(WallPos wallPos)
    {
        return this.wallStatus.GetWallAt(wallPos);
    }
}
