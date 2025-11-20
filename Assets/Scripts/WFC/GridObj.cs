using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class GridObj
{
    public static float PLACEMENT_FACTOR = 2f;
    public static float WALL_OFFSET = 0.96f;
    private bool isPlaceable = true;
    private GameObject wallPrefab, floorPrefab, destructibleWallPrefab, exitPrefab;
    private Vector2Int gridPos;
    private WallStatus wallStatus;
    private GameObject parentObj = null;
    private GameObject floorObj = null;
    private GameObject[] wallObjs = new GameObject[] { null, null, null, null };
    private UnityEvent<GridObj, WallPos>[] destructibleWallCallbacks = new UnityEvent<GridObj, WallPos>[] { null, null, null, null };
    private UnityEvent<GridObj, WallPos>[] exitCallbacks = new UnityEvent<GridObj, WallPos>[] { null, null, null, null };
    private List<GridObj>[] compatibleObjs = null;
    private GridType gridType = GridType.REGULAR;
    private IInteractable interactable = null;

    /// <summary>
    /// Create a GridObj given a Vector2Int (grid position) and a WallStatus as well as some prefabs
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="wallStatus"></param>
    public GridObj(Vector2Int gridPos, GameObject wallPrefab, GameObject floorPrefab, GameObject destructibleWallPrefab, GameObject exitPrefab, WallStatus wallStatus)
    {
        this.gridPos = gridPos;
        this.wallPrefab = wallPrefab;
        this.floorPrefab = floorPrefab;
        this.wallStatus = wallStatus;
        this.destructibleWallPrefab = destructibleWallPrefab;
        this.exitPrefab = exitPrefab;
        this.isPlaceable = true;
    }

    /// <summary>
    /// Create a GridObj given a Vector2Int (grid position) and a WallStatus
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="wallStatus"></param>
    public GridObj(Vector2Int gridPos, WallStatus wallStatus)
    {
        this.gridPos = gridPos;
        this.wallStatus = wallStatus;
        this.isPlaceable = true;

        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            Debug.LogWarning("WARNING: Could not find GameManager, GridObj cannot be instantiated");
            return;
        }

        GameManager builder = gameManager.GetComponent<GameManager>();

        if(builder == null)
        {
            Debug.LogWarning("WARNING: Could not find WFCBuilder2 component on GameManager, GridObj cannot be instantiated");
            return;
        }

        this.wallPrefab = builder.wallPrefab;
        this.floorPrefab = builder.floorPrefab;
        this.destructibleWallPrefab = builder.destructibleWallPrefab;
        this.exitPrefab = builder.exitPrefab;
        GameManager.AllGridObjs.Add(this);
    }

    /// <summary>
    /// Create virtual, non placeable GridObj that knows no position
    /// </summary>
    /// <param name="wallStatus"></param>
    public GridObj(WallStatus wallStatus)
    {
        this.wallStatus = wallStatus;
        this.isPlaceable = false;
    }

    public void InitType(GridType type)
    {
        switch (type)
        {
            case GridType.REGULAR: 
                interactable = new Regular();
                gridType = GridType.REGULAR;
                break;
            case GridType.TRAP: 
                interactable = new Trap();
                gridType = GridType.TRAP; 
                break;
            case GridType.JUMPINGPAD: 
                interactable = new JumpingPads();
                gridType = GridType.JUMPINGPAD;
                break;
            case GridType.REPLACEABLE: 
                interactable = new Replaceable();
                gridType = GridType.REPLACEABLE;
                break;
        }
    }

    // TODO fixme
    public static Vector2Int WorldPosToGridPos(Vector3 pos, int growthIndex)
    {
        int gx = Mathf.RoundToInt((pos.x / GridObj.PLACEMENT_FACTOR) + growthIndex);
        int gy = Mathf.RoundToInt((pos.z / GridObj.PLACEMENT_FACTOR) + growthIndex);

        return new Vector2Int(gx, gy);
    }

    /// <summary>
    /// Get possible pieces in a list
    /// </summary>
    /// <returns></returns>
    public static List<GridObj> GetPossiblePlaceables()
    {
        List<GridObj> objs = new List<GridObj>();

        // empty (only floor)
        objs.Add(new GridObj(new WallStatus()));

        // corridors
        GridObj corridor = new GridObj(new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.NONE, WallType.NONE));
        for (int i = 0; i < 2; i++)
        {
            objs.Add(corridor.Clone());
            corridor.RotateClockwise();
        }

        // only one wall
        GridObj oneWall = new GridObj(new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE));
        for (int i = 0; i < 4; i++)
        {
            objs.Add(oneWall.Clone());
            oneWall.RotateClockwise();
        }

        // corners
        GridObj corner = new GridObj(new WallStatus(WallType.REGULAR, WallType.NONE, WallType.REGULAR, WallType.NONE));
        for (int i = 0; i < 4; i++)
        {
            objs.Add(corner.Clone());
            corner.RotateClockwise();
        }

        return objs;
    }

    /// <summary>
    /// Check if another GridObj should be placed next to this one on the given WallPos side
    /// </summary>
    /// <param name="other"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool IsCompatible(GridObj other, WallPos side)
    {
        switch (side)
        {
            case WallPos.FRONT: return this.HasWallAt(WallPos.FRONT) == other.HasWallAt(WallPos.BACK);
            case WallPos.BACK:  return this.HasWallAt(WallPos.BACK) == other.HasWallAt(WallPos.FRONT);
            case WallPos.LEFT:  return this.HasWallAt(WallPos.LEFT) == other.HasWallAt(WallPos.RIGHT);
            case WallPos.RIGHT: return this.HasWallAt(WallPos.RIGHT) == other.HasWallAt(WallPos.LEFT);
        }
        return false;
    }

    /// <summary>
    /// Init the list of compatible GridObjs
    /// </summary>
    public void InitCompatibleList() 
    { 
        List<GridObj> allObjs = GridObj.GetPossiblePlaceables(); 
        WallPos[] wallPos = new WallPos[] { WallPos.FRONT, WallPos.BACK, WallPos.LEFT, WallPos.RIGHT }; 
        this.compatibleObjs = new List<GridObj>[] { new List<GridObj>(), new List<GridObj>(), new List<GridObj>(), new List<GridObj>() }; 

        foreach (WallPos wPos in wallPos) 
        { 
            foreach (GridObj obj in allObjs) 
            { 
                if (this.IsCompatible(obj, wPos)) this.compatibleObjs[WallStatus.WallPosToInt(wPos)].Add(obj); 
            } 
        } 
    }

    /// <summary>
    /// Returns the compatibleObjs array, safely index using WallStatus.WallPosToInt(WallPos wallPos)
    /// </summary>
    /// <returns></returns>
    public List<GridObj>[] GetCompatibleObjsList()
    {
        return this.compatibleObjs;
    }

    /// <summary>
    /// Returns a list of compatible GridObjs at the given wallPos
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public List<GridObj> GetCompatibleObjs(WallPos wallPos) 
    { 
        if (this.compatibleObjs == null) this.InitCompatibleList(); 
        return this.compatibleObjs[WallStatus.WallPosToInt(wallPos)]; 
    }

    /// <summary>
    /// Overload to call GetWorldPos(0)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetWorldPos()
    {
        return this.GetWorldPos(0);
    }

    /// <summary>
    /// Get the world position of this object
    /// </summary>
    /// <returns> Vector3 </returns>
    public Vector3 GetWorldPos(int growthIndex)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call GetWorldPos() on non placeable GridObj");
        return new Vector3((this.gridPos.x - growthIndex) * GridObj.PLACEMENT_FACTOR, 0, (this.gridPos.y - growthIndex) * GridObj.PLACEMENT_FACTOR);
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
    /// Overload to call InstantiateObj(0)
    /// </summary>
    public void InstantiateObj()
    {
        this.InstantiateObj(0);
    }

    /// <summary>
    /// Instantiate the object in its current state into the game world
    /// </summary>
    public void InstantiateObj(int growthIndex)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call InstantiateObj() on non placeable GridObj");
        if (this.parentObj != null)
        {
            Debug.LogWarning("Attempted to instantiate already existing GridObj");
            return;
        }
        Vector3 worldPos = this.GetWorldPos(growthIndex);
        this.parentObj = GameObject.Instantiate(new GameObject($"Parent at [{worldPos.x}], {worldPos.y}, {worldPos.z}"), worldPos, Quaternion.identity);
        this.floorObj = GameObject.Instantiate(floorPrefab, this.GetWorldPos(growthIndex), Quaternion.identity);

        
        this.interactable.SetColor(floorObj);
        this.floorObj.transform.SetParent(this.parentObj.transform);

        if (this.wallStatus.HasWallAt(WallPos.FRONT))
        {
            this.InstantiateWall(WallPos.FRONT, growthIndex);
        }
        if (this.wallStatus.HasWallAt(WallPos.BACK))
        {
            this.InstantiateWall(WallPos.BACK, growthIndex);
        }
        if (this.wallStatus.HasWallAt(WallPos.LEFT))
        {
            this.InstantiateWall(WallPos.LEFT, growthIndex);
        }
        if (this.wallStatus.HasWallAt(WallPos.RIGHT))
        {
            this.InstantiateWall(WallPos.RIGHT, growthIndex);
        }
    }

    /// <summary>
    /// Overload to place WallType.REGULAR
    /// </summary>
    /// <param name="wallPos"></param>
    public void PlaceWallAt(WallPos wallPos, int growthIndex)
    {
        this.PlaceWallAt(wallPos, WallType.REGULAR, growthIndex);
    }

    /// <summary>
    /// Place a wall on a chosen side
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    public void PlaceWallAt(WallPos wallPos, WallType wallType, int growthIndex)
    {
        if (this.HasWallAt(wallPos))
        {
            this.RemoveWall(wallPos);
        }
        this.wallStatus.PlaceWallAt(wallPos, wallType);

        if (this.parentObj == null)
        {
            Debug.LogWarning("Attempted to place wall on NULL GridObj");
            return;
        }

        if (!this.isPlaceable) return;

        this.InstantiateWall(wallPos, wallType, growthIndex);
    }

    /// <summary>
    /// Overloaded method to instantiate a WallType.REGULAR wall
    /// </summary>
    /// <param name="wallPos"></param>
    public void InstantiateWall(WallPos wallPos, int growthIndex)
    {
        this.InstantiateWall(wallPos, WallType.REGULAR, growthIndex);
    }

    /// <summary>
    /// Instantiate a wall and only change the data on the in-game object, helper method
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    private void InstantiateWall(WallPos wallPos, WallType wallType, int growthIndex)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call InstantiateWall() on non placeable GridObj");
        if (this.parentObj == null) return;
        int index = WallStatus.WallPosToInt(wallPos);
        if (this.wallObjs[index] != null) return;

        if (wallType == WallType.NONE)
        {
            this.RemoveWall(wallPos);
            return;
        }

        GameObject newWall = GameObject.Instantiate(this.GetWallPrefab(wallType), WallStatus.GetWallWorldPos(this.GetWorldPos(growthIndex), wallPos), Quaternion.Euler(WallStatus.GetWallRotation(wallPos)));

        if (wallType == WallType.DESTRUCTIBLE)
        {
            DestructibleWall dw = newWall.GetComponentInChildren<DestructibleWall>();
            dw.gridObj = this;
            dw.wallPos = wallPos;
            UnityEvent<GridObj, WallPos> cb = new UnityEvent<GridObj, WallPos>();
            dw.onDestroy = cb;
            this.destructibleWallCallbacks[WallStatus.WallPosToInt(wallPos)] = cb;
        }
        else if (wallType == WallType.EXIT)
        {
            Exit exit = newWall.GetComponentInChildren<Exit>();
            exit.gridObj = this;
            exit.wallPos = wallPos;
            UnityEvent<GridObj, WallPos> cb = new UnityEvent<GridObj, WallPos>();
            exit.onDestroy = cb;
            this.exitCallbacks[WallStatus.WallPosToInt(wallPos)] = cb;
        }

        newWall.transform.SetParent(this.parentObj.transform);
        this.wallObjs[index] = newWall;
    }

    /// <summary>
    /// Remove a wall if it exists, also deletes its GameObject instance
    /// </summary>
    /// <param name="wallPos"> The side the wall is at </param>
    public void RemoveWall(WallPos wallPos)
    {
        this.wallStatus.RemoveWallAt(wallPos);
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
        if (!this.isPlaceable) throw new System.Exception("Attempted to call DestroyObj() on non placeable GridObj");
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
    /// Returns WallType at position
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public WallType GetWallTypeAt(WallPos wallPos)
    {
        return this.wallStatus.GetWallAt(wallPos);
    }

    /// <summary>
    /// Returns wall GameObject if instantiated, else null
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public GameObject GetWallObjAt(WallPos wallPos)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call GetWallObjAt() on non placeable GridObj");
        if (!this.HasWallAt(wallPos)) return null;
        return this.wallObjs[WallStatus.WallPosToInt(wallPos)];
    }

    /// <summary>
    /// Returns the wall prefab from type
    /// </summary>
    /// <param name="wallType"></param>
    /// <returns></returns>
    public GameObject GetWallPrefab(WallType wallType)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call GetWallPrefab() on non placeable GridObj");
        switch (wallType)
        {
            case WallType.DESTRUCTIBLE:
                return this.destructibleWallPrefab;
            case WallType.EXIT:
                return this.exitPrefab;
            default:
                return this.wallPrefab;
        }
    }

    /// <summary>
    /// Returns destructible wall callback or null
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public UnityEvent<GridObj, WallPos> GetDestructibleWallCb(WallPos wallPos)
    {
        return this.destructibleWallCallbacks[WallStatus.WallPosToInt(wallPos)];
    }

    /// <summary>
    /// Returns exit callback or null
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public UnityEvent<GridObj, WallPos> GetExitCb(WallPos wallPos)
    {
        return this.exitCallbacks[WallStatus.WallPosToInt(wallPos)];
    }

    /// <summary>
    /// Returns this.wallStatus.GetWallAt(wallPos)
    /// </summary>
    /// <param name="wallPos"></param>
    /// <returns></returns>
    public WallType GetWallAt(WallPos wallPos)
    {
        return this.wallStatus.GetWallAt(wallPos);
    }

    /// <summary>
    /// Rotate this object clockwise amount times
    /// </summary>
    /// <param name="amount"></param>
    public void RotateClockwise(int amount)
    {
        amount = amount % 4; // no need for more calculation than neccessary
        for (int i = 0; i < amount; i++) // could be done better in the future
        {
            this.RotateClockwise();
        }
    }

    /// <summary>
    /// Rotate the entire GridObj clockwise once
    /// </summary>
    public void RotateClockwise()
    {
        WallType front = wallStatus.GetWallAt(WallPos.FRONT);
        WallType back  = wallStatus.GetWallAt(WallPos.BACK);
        WallType left  = wallStatus.GetWallAt(WallPos.LEFT);
        WallType right = wallStatus.GetWallAt(WallPos.RIGHT);

        this.wallStatus.PlaceWallAt(WallPos.FRONT, left);
        this.wallStatus.PlaceWallAt(WallPos.RIGHT, front);
        this.wallStatus.PlaceWallAt(WallPos.BACK, right);
        this.wallStatus.PlaceWallAt(WallPos.LEFT, back);
    }

    /// <summary>
    /// Returns true if GridObj has been instantiated and its main GameObject is still existing
    /// </summary>
    /// <returns></returns>
    public bool IsInstantiated()
    {
        return this.parentObj != null;
    }

    /// <summary>
    /// Returns a makeshift name for this GridObj
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        string s = "";
        if (this.wallStatus.front != WallType.NONE) s += "F";
        if (this.wallStatus.back != WallType.NONE) s += "B";
        if (this.wallStatus.left != WallType.NONE) s += "L";
        if (this.wallStatus.right != WallType.NONE) s += "R";
        if (s == "") s += "E";

        return s;
    }

    /// <summary>
    /// Returns a clone of this GridObj
    /// </summary>
    /// <returns> GridObj clone </returns>
    public GridObj Clone()
    {
        GridObj clone = new GridObj(this.gridPos, this.wallPrefab, this.floorPrefab, this.destructibleWallPrefab, this.exitPrefab, this.wallStatus.Clone());

        clone.SetIsPlaceable(this.isPlaceable);
        clone.SetGridType(this.gridType);

        if (this.compatibleObjs != null)
        {
            List<GridObj>[] newCompat = new List<GridObj>[4];
            for (int i = 0; i < 4; i++)
            {
                if (this.compatibleObjs[i] == null)
                {
                    newCompat[i] = null;
                    continue;
                }

                newCompat[i] = new List<GridObj>();
                foreach (var obj in this.compatibleObjs[i])
                    newCompat[i].Add(obj.Clone());
            }
            clone.SetCompatibleObjs(newCompat);
        }

        return clone;
    }

    // Generic getters

    public GameObject GetparentObj() { return this.parentObj; }
    public GameObject GetFloorObj() { return this.floorObj; }
    public GameObject[] GetWallObjs() { return this.wallObjs; }
    public UnityEvent<GridObj, WallPos>[] GetDestructibleWallCallbacks() { return this.destructibleWallCallbacks; }
    public UnityEvent<GridObj, WallPos>[] GetExitCallbacks() { return this.exitCallbacks; }
    public List<GridObj>[] GetCompatibleObjs() { return this.compatibleObjs; }
    public bool IsPlaceable() { return this.isPlaceable; }
    public GridType GetGridType() { return this.gridType; }
    public Vector2Int GetGridPos() { return this.gridPos; }
    public WallStatus GetWallStatus() { return this.wallStatus; }
    public IInteractable GetInteract() { return this.interactable; }
    

    // Generic setters

    public void SetparentObj(GameObject parentObj) { this.parentObj = parentObj; }
    public void SetFloorObj(GameObject floorObj) { this.floorObj = floorObj; }
    public void SetWallObjs(GameObject[] wallObjs) { this.wallObjs = wallObjs; }
    public void SetDestructibleWallCallbacks(UnityEvent<GridObj, WallPos>[] destructibleWallCallbacks) { this.destructibleWallCallbacks = destructibleWallCallbacks; }
    public void SetExitCallbacks(UnityEvent<GridObj, WallPos>[] exitCallbacks) { this.exitCallbacks = exitCallbacks; }
    public void SetCompatibleObjs(List<GridObj>[] compatibleObjs) { this.compatibleObjs = compatibleObjs; }
    public void SetIsPlaceable(bool isPlaceable) { this.isPlaceable = isPlaceable; }
    public void SetGridType(GridType gridType) { this.gridType = gridType; }
    public void SetGridPos(Vector2Int gridPos) { this.gridPos = gridPos; }
}

public enum GridType
{
    REGULAR, REPLACEABLE, TRAP, JUMPINGPAD
}