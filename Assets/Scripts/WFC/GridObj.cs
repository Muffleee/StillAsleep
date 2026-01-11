using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

/// <summary>
/// Class describing a singular grid object/tile.
/// </summary>
public class GridObj
{
    public static float PLACEMENT_FACTOR = 2f;
    public static float WALL_OFFSET = 1f;
    private bool isPlaceable = true;
    private GameObject wallPrefab, floorPrefab, destructibleWallPrefab, exitPrefab;
    private Vector2Int gridPos;
    private WallStatus wallStatus;
    private GameObject parentObj = null;
    private GameObject floorObj = null;
    private Dictionary<WallPos, GameObject> wallObjs = new Dictionary<WallPos, GameObject>() { { WallPos.FRONT, null }, { WallPos.BACK, null }, { WallPos.LEFT, null }, { WallPos.RIGHT, null } };
    private UnityEvent<GridObj, WallPos>[] destructibleWallCallbacks = new UnityEvent<GridObj, WallPos>[] { null, null, null, null };
    private UnityEvent<GridObj, WallPos>[] exitCallbacks = new UnityEvent<GridObj, WallPos>[] { null, null, null, null };
    private List<GridObj>[] compatibleObjs = null;
    private GridType gridType = GridType.REGULAR;
    private IInteractable interactable = null;
    private int weight = 0;

    [SerializeField] private GameObject energyCrystalPrefab;

    [SerializeField] private int placementCost = 1;
    public int PlacementCost => this.placementCost;

    /// <summary>
    /// Create a GridObj given a grid position and a WallStatus, as well as some prefabs
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="wallStatus"></param>
    public GridObj(Vector2Int gridPos, GameObject wallPrefab, GameObject floorPrefab, GameObject destructibleWallPrefab, GameObject exitPrefab, WallStatus wallStatus, int weight)
    {
        this.gridPos = gridPos;
        this.wallPrefab = wallPrefab;
        this.floorPrefab = floorPrefab;
        this.wallStatus = wallStatus;
        this.destructibleWallPrefab = destructibleWallPrefab;
        this.exitPrefab = exitPrefab;
        this.weight = weight;
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

        this.wallPrefab = builder.GetPrefabLibrary().GetRandomWallPrefab();
        this.floorPrefab = builder.GetPrefabLibrary().GetRandomFloorPrefab();
        this.destructibleWallPrefab = builder.GetPrefabLibrary().prefabDestructibleWall;
        this.exitPrefab = builder.GetPrefabLibrary().prefabExit;
        this.energyCrystalPrefab = builder.GetPrefabLibrary().prefabEnergyCrystal;
        GameManager.AllGridObjs.Add(this);
    }

    /// <summary>
    /// Create virtual, non placeable GridObj that knows no position
    /// </summary>
    /// <param name="wallStatus"></param>
    public GridObj(WallStatus wallStatus, int weight)
    {
        this.wallStatus = wallStatus;
        this.weight = weight;
        this.isPlaceable = false;
    }

    private void InitType(GridType type)
    {
        switch (type)
        {
            case GridType.REGULAR: 
                this.interactable = new Regular();
                break;
            case GridType.TRAP: 
                this.interactable = new Trap(); 
                break;
            case GridType.JUMPINGPAD: 
                this.interactable = new JumpingPads();
                break;
            case GridType.REPLACEABLE: 
                this.interactable = new Replaceable();
                break;
            case GridType.MANUAL_REPLACEABLE:
                this.interactable = new ManualReplaceable();
                break;
        }
    }

    /// <summary>
    /// Calculates world position from grid indexes
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="worldOffsetX"></param>
    /// <param name="worldOffsetY"></param>
    /// <returns></returns>
    public static Vector2Int WorldPosToGridPos(Vector3 pos, int worldOffsetX, int worldOffsetY)
    {
        int gx = Mathf.RoundToInt((pos.x / GridObj.PLACEMENT_FACTOR) + worldOffsetX);
        int gy = Mathf.RoundToInt((pos.z / GridObj.PLACEMENT_FACTOR) + worldOffsetY);

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
        objs.Add(new GridObj(new WallStatus(), GameManager.emptyWeight));

        // corridors
        GridObj corridor = new GridObj(new WallStatus(WallType.REGULAR, WallType.REGULAR, WallType.NONE, WallType.NONE), GameManager.corridorWeight);
        for (int i = 0; i < 2; i++)
        {
            objs.Add(corridor.Clone());
            corridor.RotateClockwise();
        }

        // only one wall
        GridObj oneWall = new GridObj(new WallStatus(WallType.REGULAR, WallType.NONE, WallType.NONE, WallType.NONE), GameManager.oneWallWeight);
        for (int i = 0; i < 4; i++)
        {
            objs.Add(oneWall.Clone());
            oneWall.RotateClockwise();
        }

        // corners
        GridObj corner = new GridObj(new WallStatus(WallType.REGULAR, WallType.NONE, WallType.REGULAR, WallType.NONE), GameManager.cornerWeight);
        for (int i = 0; i < 4; i++)
        {
            objs.Add(corner.Clone());
            corner.RotateClockwise();
        }

        return objs;
    }

    /// <summary>
    /// Static method to calculate world pos from grid pos
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="worldOffsetX"></param>
    /// <param name="worldOffsetY"></param>
    /// <returns></returns>
    public static Vector3 GridPosToWorldPos(Vector2Int gridPos, int worldOffsetX, int worldOffsetY)
    {
        return new Vector3((gridPos.x - worldOffsetX) * GridObj.PLACEMENT_FACTOR, 0, (gridPos.y - worldOffsetY) * GridObj.PLACEMENT_FACTOR);
    }

    /// <summary>
    /// Check if another GridObj should be placed next to this one on the given WallPos side
    /// </summary>
    /// <param name="other"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool IsCompatible(GridObj other, WallPos side)
    {   
        if(this.GetGridType() == GridType.MANUAL_REPLACEABLE || other.GetGridType() == GridType.MANUAL_REPLACEABLE) return true;
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
        return this.GetWorldPos(0, 0);
    }

    /// <summary>
    /// Get the world position of this object
    /// </summary>
    /// <returns> Vector3 </returns>
    public Vector3 GetWorldPos(int worldOffsetX, int worldOffsetY)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call GetWorldPos() on non placeable GridObj");

        return new Vector3((this.gridPos.x - worldOffsetX) * GridObj.PLACEMENT_FACTOR, 0, (this.gridPos.y - worldOffsetY) * GridObj.PLACEMENT_FACTOR);
    }

    /// <summary>
    /// Checks if a wall exists on this side of the object
    /// </summary>
    /// <param name="wallPos"> Side to check </param>
    /// <returns> bool </returns>
    public bool HasWallAt(WallPos wallPos)
    {
        return this.wallStatus.HasWallAt(wallPos);
    }

    /// <summary>
    /// Overload to call InstantiateObj(0)
    /// </summary>
    public void InstantiateObj()
    {
        this.InstantiateObj(0, 0, new Dictionary<WallPos, GridObj>());
    }

    /// <summary>
    /// Instantiate the object in its current state into the game world
    /// </summary>
    public void InstantiateObj(int worldOffsetX, int worldOffsetY, Dictionary<WallPos, GridObj> neighbors)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call InstantiateObj() on non placeable GridObj");
        if (this.parentObj != null)
        {
            Debug.LogWarning("Attempted to instantiate already existing GridObj");
            return;
        }

        if(this.gridType == GridType.REPLACEABLE || this.gridType == GridType.MANUAL_REPLACEABLE)
        {
            this.floorPrefab = GameManager.INSTANCE.GetPrefabLibrary().prefabReplaceable;
        }

        Vector3 worldPos = this.GetWorldPos(worldOffsetX, worldOffsetY);
        this.parentObj = GameObject.Instantiate(new GameObject($"Parent at [{worldPos.x}], {worldPos.y}, {worldPos.z}"), worldPos, Quaternion.identity);
        this.floorObj = GameObject.Instantiate(this.floorPrefab, this.GetWorldPos(worldOffsetX, worldOffsetY), Quaternion.identity);

        this.floorObj.transform.SetParent(this.parentObj.transform);
        this.interactable.SetColor(this.floorObj);

        if (this.wallStatus.HasWallAt(WallPos.FRONT))
        {
            if (neighbors[WallPos.FRONT] == null || neighbors[WallPos.FRONT].GetWallObjs()[WallPos.BACK] == null) this.InstantiateWall(WallPos.FRONT, this.GetWallAt(WallPos.FRONT), worldOffsetX, worldOffsetY);
        }
        if (this.wallStatus.HasWallAt(WallPos.BACK))
        {
            if (neighbors[WallPos.BACK] == null || neighbors[WallPos.BACK].GetWallObjs()[WallPos.FRONT] == null) this.InstantiateWall(WallPos.BACK, this.GetWallAt(WallPos.BACK), worldOffsetX, worldOffsetY);
        }
        if (this.wallStatus.HasWallAt(WallPos.LEFT))
        {
            if (neighbors[WallPos.LEFT] == null || neighbors[WallPos.LEFT].GetWallObjs()[WallPos.RIGHT] == null) this.InstantiateWall(WallPos.LEFT, this.GetWallAt(WallPos.LEFT), worldOffsetX, worldOffsetY);
        }
        if (this.wallStatus.HasWallAt(WallPos.RIGHT))
        {
            if (neighbors[WallPos.RIGHT] == null || neighbors[WallPos.RIGHT].GetWallObjs()[WallPos.LEFT] == null) this.InstantiateWall(WallPos.RIGHT, this.GetWallAt(WallPos.RIGHT), worldOffsetX, worldOffsetY);
        }

        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null)
        {
            float energyRatio = (float)pr.CurrentEnergy / pr.MaxEnergy;

            float baseChance = 0.10f; 
            float spawnChance = baseChance * (1.5f - energyRatio);
            spawnChance = Mathf.Clamp(spawnChance, 0.02f, 0.25f); 
            //      spawnChance = baseChance * (1.5 - energyRatio)
            //        → Spieler mit wenig Energie erhalten bis zu +50 % höhere Spawn-Chance
            //        → Spieler mit voller Energie erhalten 50 % weniger Spawn-Chance

            int baseMax = 6;
            int bonus = 10;
            int maxCrystals = baseMax + Mathf.FloorToInt((1f - energyRatio) * bonus);
            //      maxCrystals = baseMax + (1 - energyRatio) * bonus
            //        → Obergrenze steigt bei wenig Energie (bis zu 16)
            //        → Obergrenze sinkt bei viel Energie (mindestens 6)

            if (this.gridType == GridType.REGULAR && UnityEngine.Random.value < spawnChance)
            {
                EnergyCrystal.PrepareSpawn(this.GetWorldPos(worldOffsetX, worldOffsetY), maxCrystals);
                GameObject.Instantiate(this.energyCrystalPrefab, this.GetWorldPos(worldOffsetX, worldOffsetY), Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Overload to place WallType.REGULAR
    /// </summary>
    /// <param name="wallPos"></param>
    public void PlaceWallAt(WallPos wallPos, int worldOffsetX, int worldOffsetY)
    {
        this.PlaceWallAt(wallPos, WallType.REGULAR, worldOffsetX, worldOffsetY);
    }

    /// <summary>
    /// Place a wall on a chosen side
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    public void PlaceWallAt(WallPos wallPos, WallType wallType, int worldOffsetX, int worldOffsetY)
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

        this.InstantiateWall(wallPos, wallType, worldOffsetX, worldOffsetY);
    }

    /// <summary>
    /// Returns true if this GridObj has at least one exit
    /// </summary>
    /// <returns></returns>
    public bool HasExit()
    {
        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(this.GetWallAt(pos) != WallType.EXIT) continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the first found exit pos, null if there is no exit on this GridObj.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public WallPos? GetExitPos()
    {
        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(this.GetWallAt(pos) != WallType.EXIT) continue;
            return pos;
        }
        return null;
    }

    /// <summary>
    /// Overloaded method to instantiate a WallType.REGULAR wall
    /// </summary>
    /// <param name="wallPos"></param>
    public void InstantiateWall(WallPos wallPos, int worldOffsetX, int worldOffsetY)
    {
        this.InstantiateWall(wallPos, WallType.REGULAR, worldOffsetX, worldOffsetY);
    }

    /// <summary>
    /// Replace the floor prefab with GameObject prefab
    /// </summary>
    /// <param name="prefab"></param>
    public void ReplaceFloorPrefab(GameObject prefab, int worldOffsetX, int worldOffsetY)
    {
        this.floorPrefab = prefab;
        if (!this.IsInstantiated()) return;
        GameObject.Destroy(this.floorObj);
        this.floorObj = GameObject.Instantiate(this.floorPrefab, this.GetWorldPos(worldOffsetX, worldOffsetY), Quaternion.identity);
        this.floorObj.transform.SetParent(this.parentObj.transform);
    }

    /// <summary>
    /// Instantiate a wall and only change the data on the in-game object, helper method
    /// </summary>
    /// <param name="wallPos"> The side to place the wall at </param>
    public void InstantiateWall(WallPos wallPos, WallType wallType, int worldOffsetX, int worldOffsetY)
    {
        if (!this.isPlaceable) throw new System.Exception("Attempted to call InstantiateWall() on non placeable GridObj");
        if (this.parentObj == null) return;
        if (this.wallObjs[wallPos] != null)
        {
            GameObject.Destroy(this.wallObjs[wallPos]);
            this.wallObjs[wallPos] = null;
        }

        if (wallType == WallType.NONE)
        {
            this.RemoveWall(wallPos);
            return;
        }

        GameObject newWall = GameObject.Instantiate(this.GetWallPrefab(wallType), WallStatus.GetWallWorldPos(this.GetWorldPos(worldOffsetX, worldOffsetY), wallPos), Quaternion.Euler(WallStatus.GetWallRotation(wallPos)));

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
            /* Disable for now
            Exit exit = newWall.GetComponentInChildren<Exit>();
            exit.gridObj = this;
            exit.wallPos = wallPos;
            UnityEvent<GridObj, WallPos> cb = new UnityEvent<GridObj, WallPos>();
            exit.onDestroy = cb;
            this.exitCallbacks[WallStatus.WallPosToInt(wallPos)] = cb;
            */
        }

        newWall.transform.SetParent(this.parentObj.transform);
        this.wallObjs[wallPos] = newWall;
    }

    /// <summary>
    /// Remove a wall if it exists, also deletes its GameObject instance
    /// </summary>
    /// <param name="wallPos"> The side the wall is at </param>
    public void RemoveWall(WallPos wallPos)
    {
        this.wallStatus.RemoveWallAt(wallPos);
        int index = WallStatus.WallPosToInt(wallPos);
        this.exitCallbacks[index] = null;
        this.destructibleWallCallbacks[index] = null;
        GameObject obj = this.wallObjs[wallPos];
        if (obj == null) return;
        this.wallObjs[wallPos] = null;
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
        foreach(WallPos wallPos in Enum.GetValues(typeof(WallPos))){
            if (this.wallObjs[wallPos] == null) continue;
            GameObject.Destroy(this.wallObjs[wallPos]);
            this.wallObjs[wallPos] = null;
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
        return this.wallObjs[wallPos];
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
        WallType front = this.wallStatus.GetWallAt(WallPos.FRONT);
        WallType back  = this.wallStatus.GetWallAt(WallPos.BACK);
        WallType left  = this.wallStatus.GetWallAt(WallPos.LEFT);
        WallType right = this.wallStatus.GetWallAt(WallPos.RIGHT);

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
    /// Removes all exit walls from this GridObj and replaces them with normal ones
    /// </summary>
    public void RemoveExitWalls()
    {
        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(this.GetWallAt(pos) != WallType.EXIT) continue;
            this.RemoveWall(pos);
        }
    }

    /// <summary>
    /// Removes all walls
    /// </summary>
    public void RemoveAllWalls()
    {
        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(!this.HasWallAt(pos)) continue;
            this.RemoveWall(pos);
        }
    }

    /// <summary>
    /// Returns a list of all free WallPos that have no wall
    /// </summary>
    /// <returns></returns>
    public List<WallPos> GetFreeWalls()
    {   
        List<WallPos> list = new List<WallPos>();
        foreach(WallPos pos in Enum.GetValues(typeof(WallPos)))
        {
            if(this.HasWallAt(pos)) continue;
            list.Add(pos);
        }
        return list;
    }
    /// <summary>
    /// Updates this gridObj and its neighbours wallStatus. 
    /// If one WallStatus is none, it'll take the other one to set for both, else this gridObjs' wallstatus will be set for both
    /// </summary>
    /// <param name="neighbours"></param>
    public void UpdateWallStatus(Dictionary<WallPos, GridObj> neighbours)
    {
        foreach(WallPos wPos in Enum.GetValues(typeof(WallPos)))
        {
            WallPos oppWPos = WallStatus.GetOppositePos(wPos);
            if (neighbours[wPos] == null) continue;
            // Uncomment if placed walls shouldn't overwrite exits
            // if (neighbours[wPos].GetWallAt(oppWPos) == WallType.EXIT) this.wallStatus.PlaceWallAt(wPos, WallType.EXIT);
            // if (this.wallStatus.GetWallAt(wPos) == WallType.EXIT) neighbours[wPos].wallStatus.PlaceWallAt(oppWPos, WallType.EXIT);
            if (this.wallStatus.GetWallAt(wPos) != neighbours[wPos].GetWallStatus().GetWallAt(oppWPos))
            {
                WallType newWallType = (this.wallStatus.GetWallAt(wPos) == WallType.NONE) ? neighbours[wPos].GetWallStatus().GetWallAt(oppWPos) : this.wallStatus.GetWallAt(wPos);
                this.wallStatus.PlaceWallAt(wPos, newWallType);
                neighbours[wPos].GetWallStatus().PlaceWallAt(oppWPos, newWallType);
            }
        }        
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
        GridObj clone = new GridObj(this.gridPos, this.wallPrefab, this.floorPrefab, this.destructibleWallPrefab, this.exitPrefab, this.wallStatus.Clone(), this.weight);

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
    public Dictionary<WallPos, GameObject> GetWallObjs() { return this.wallObjs; }
    public UnityEvent<GridObj, WallPos>[] GetDestructibleWallCallbacks() { return this.destructibleWallCallbacks; }
    public UnityEvent<GridObj, WallPos>[] GetExitCallbacks() { return this.exitCallbacks; }
    public List<GridObj>[] GetCompatibleObjs() { return this.compatibleObjs; }
    public bool IsPlaceable() { return this.isPlaceable; }
    public GridType GetGridType() { return this.gridType; }
    public Vector2Int GetGridPos() { return this.gridPos; }
    public WallStatus GetWallStatus() { return this.wallStatus; }
    public IInteractable GetInteract() { return this.interactable; }
    public int GetWeight() { return this.weight; }

    // Generic setters

    public void SetparentObj(GameObject parentObj) { this.parentObj = parentObj; }
    public void SetFloorObj(GameObject floorObj) { this.floorObj = floorObj; }
    public void SetWallObjs(Dictionary<WallPos, GameObject> wallObjs) { this.wallObjs = wallObjs; }
    public void SetDestructibleWallCallbacks(UnityEvent<GridObj, WallPos>[] destructibleWallCallbacks) { this.destructibleWallCallbacks = destructibleWallCallbacks; }
    public void SetExitCallbacks(UnityEvent<GridObj, WallPos>[] exitCallbacks) { this.exitCallbacks = exitCallbacks; }
    public void SetCompatibleObjs(List<GridObj>[] compatibleObjs) { this.compatibleObjs = compatibleObjs; }
    public void SetIsPlaceable(bool isPlaceable) { this.isPlaceable = isPlaceable; }
    public void SetGridType(GridType gridType) { this.gridType = gridType; this.InitType(gridType); }
    public void SetGridPos(Vector2Int gridPos) { this.gridPos = gridPos; }
    public void SetWeight(int weight) { this.weight = weight; }
    public void SetFloorPrefab(GameObject prefab) { this.floorPrefab = prefab; }
}

public enum GridType
{
    REGULAR, REPLACEABLE, MANUAL_REPLACEABLE, TRAP, JUMPINGPAD
}