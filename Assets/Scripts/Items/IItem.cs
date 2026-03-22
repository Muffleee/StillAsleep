using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    public int GetEnergyCost();
    public int GetSpawnWeight();
    public Sprite GetIcon();
    public GameObject GetPrefab();
    public bool OnUse();
    public string GetName();
    public string GetDescription();
}

public static class ItemHelper
{
    public static WallPos GetPlayerFacingDirection()
    {
        // Using your original movement calculation!
        if (PlayerMovement.INSTANCE == null)
        {
            return WallPos.FRONT;
        }

        return PlayerMovement.INSTANCE.GetFacing();
    }
}

/// <summary>
/// Time reversal - resets enemy the given amount of steps
/// </summary>
public class TimeReversalItem : IItem
{
    private const int ENERGY_COST = 2;
    private const int SPAWN_WEIGHT = 5;
    private const int REWIND_STEPS = 4;

    int IItem.GetEnergyCost() => ENERGY_COST;
    int IItem.GetSpawnWeight() => SPAWN_WEIGHT;

    Sprite IItem.GetIcon()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.iconItemClock;
    }

    GameObject IItem.GetPrefab()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.prefabItemClock;
    }

    string IItem.GetName() => "Lucid Dial";
    string IItem.GetDescription() => $"Rewind time and reset your ghost {REWIND_STEPS} steps.";

    bool IItem.OnUse()
    {
        if (EnemyMovement.INSTANCE == null) return false;
        EnemyMovement.INSTANCE.Rewind(REWIND_STEPS);
        return true; // Successfully used
    }
}

/// <summary>
/// Spitzhacke -> Zerstoert die Wand in Blickrichtung des Spielers.
/// </summary>
public class WallBreakerItem : IItem
{
    private const int ENERGY_COST = 2;
    private const int SPAWN_WEIGHT = 4;

    int IItem.GetEnergyCost() => ENERGY_COST;
    int IItem.GetSpawnWeight() => SPAWN_WEIGHT;

    Sprite IItem.GetIcon()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.iconItemPickaxe;
    }

    GameObject IItem.GetPrefab()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.prefabItemPickaxe;
    }

    string IItem.GetName() => "Pickaxe";
    string IItem.GetDescription() => "Use it to destroy a wall you are looking at.";

    bool IItem.OnUse()
    {
        if (GameManager.INSTANCE == null) return false;
        if (PlayerMovement.INSTANCE == null) return false;

        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        if (grid == null) return false;

        Vector2Int playerPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        WallPos direction = ItemHelper.GetPlayerFacingDirection();

        GridObj current = grid.GetGridObj(playerPos);
        GridObj next = grid.GetAdjacentGridObj(playerPos, direction);

        if (current == null || next == null) return false;

        WallPos opposite = WallStatus.GetOppositePos(direction);
        bool hasWallToBreak = current.HasWallAt(direction) || next.HasWallAt(opposite);

        if (!hasWallToBreak) 
        {
            Debug.LogWarning("There is no wall in front of you to break!");
            return false; 
        }

        if (GameManager.INSTANCE.IsTutorialCurrently() == true) GameManager.INSTANCE.GetTutManager().NextStep();
        current.RemoveWall(direction);
        next.RemoveWall(opposite);
        return true; // Successfully used
    }
}

/// <summary>
/// Klebefalle -> Markiert das Feld vor dem Spieler. Wenn der Gegner dieses Feld betritt,
/// bleibt er dort fuer kurze Zeit stehen.
/// </summary>
public class SludgeItem : IItem
{
    private const int ENERGY_COST = 2;
    private const int SPAWN_WEIGHT = 4;
    private const int STUCK_TURNS = 2;

    int IItem.GetEnergyCost() => ENERGY_COST;
    int IItem.GetSpawnWeight() => SPAWN_WEIGHT;

    Sprite IItem.GetIcon()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.iconItemTrapForcefield;
    }

    GameObject IItem.GetPrefab()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.prefabItemTrapForcefield;
    }

    string IItem.GetName() => "Stasis Crate";
    string IItem.GetDescription() => "Deploy on a field to trap your ghost when it walks over it.";

    bool IItem.OnUse()
    {
        if (GameManager.INSTANCE == null || EnemyMovement.INSTANCE == null) return false;

        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        if (grid == null) return false;

        // RAYCAST FROM MOUSE TO FIND TARGET TILE
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        GridObj target = null;

        // Find the specific tile we clicked on
        foreach (var hit in hits)
        {
            GridObj obj = grid.GetGridObjFromGameObj(hit.collider.transform.root.gameObject);
            if (obj != null && obj.GetGridType() != GridType.REPLACEABLE && obj.GetGridType() != GridType.MANUAL_REPLACEABLE)
            {
                target = obj;
                break;
            }
        }

        // --- THE FIX: If the mouse missed a valid tile, cancel and show a warning! ---
        if (target == null)
        {
            Debug.LogWarning("Invalid placement! You must click directly on a valid map tile.");
            return false; // Returns false so you don't lose the item or energy
        }
        // -----------------------------------------------------------------------------

        // SPAWN THE TRAPBOX AT TARGET LOCATION
        Vector3 spawnPos = target.GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY());
        GameObject boxPrefab = GameManager.INSTANCE.GetPrefabLibrary().prefabItemBoxTrap; 

        if (boxPrefab != null)
        {
            GameObject trapVisual = GameObject.Instantiate(boxPrefab, spawnPos, Quaternion.identity);
            ItemBoxTrap boxTrapScript = trapVisual.GetComponent<ItemBoxTrap>();

            EnemyMovement.INSTANCE.PlaceStickyTrap(target.GetGridPos(), STUCK_TURNS, boxTrapScript);
            return true; // Successfully used!
        }
        
        return false;
    }
}

/// <summary>
/// Scanner -> Macht Hidden Traps fuer einige Sekunden sichtbar.
/// </summary>
public class ScannerItem : IItem
{
    private const int ENERGY_COST = 1;
    private const int SPAWN_WEIGHT = 4;
    private const float REVEAL_SECONDS = 4f;

    private class RendererColorSnapshot
    {
        public Renderer renderer;
        public Color color;

        public RendererColorSnapshot(Renderer renderer, Color color)
        {
            this.renderer = renderer;
            this.color = color;
        }
    }

    int IItem.GetEnergyCost() => ENERGY_COST;
    int IItem.GetSpawnWeight() => SPAWN_WEIGHT;

    Sprite IItem.GetIcon()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.iconItemScanner;
    }

    GameObject IItem.GetPrefab()
    {
        if (GameManager.INSTANCE == null) return null;
        PrefabLibrary library = GameManager.INSTANCE.GetPrefabLibrary();
        if (library == null) return null;
        return library.prefabItemTerrainScanner;
    }

    string IItem.GetName() => "Scanner";
    string IItem.GetDescription() => $"Use it to reveal all hidden traps in your proximity.";

    bool IItem.OnUse()
    {
        if (GameManager.INSTANCE == null) return false;

        if (TerrainScannerEffect.INSTANCE != null)
        {
            TerrainScannerEffect.INSTANCE.PlayTerrainScanner(1, 0f);
        }

        GameManager.INSTANCE.StartCoroutine(RevealHiddenTrapsCoroutine(REVEAL_SECONDS));
        return true; // Successfully used
    }

    private IEnumerator RevealHiddenTrapsCoroutine(float revealDuration)
    {
        if (GameManager.INSTANCE == null) yield break;

        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        if (grid == null) yield break;

        GridObj[,] gridArray = grid.GetGridArray();
        List<RendererColorSnapshot> snapshots = new List<RendererColorSnapshot>();

        foreach (GridObj tile in gridArray)
        {
            if (tile == null || tile.GetGridType() != GridType.HIDDENTRAP) continue;

            GameObject floorObj = tile.GetFloorObj();
            if (floorObj == null) continue;

            Renderer[] renderers = floorObj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer currentRenderer in renderers)
            {
                if (currentRenderer == null || currentRenderer.material == null) continue;
                if (!currentRenderer.material.HasProperty("_Color")) continue;

                snapshots.Add(new RendererColorSnapshot(currentRenderer, currentRenderer.material.color));
                currentRenderer.material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(revealDuration);

        foreach (RendererColorSnapshot snapshot in snapshots)
        {
            if (snapshot == null || snapshot.renderer == null || snapshot.renderer.material == null) continue;
            if (!snapshot.renderer.material.HasProperty("_Color")) continue;

            snapshot.renderer.material.color = snapshot.color;
        }
    }
}