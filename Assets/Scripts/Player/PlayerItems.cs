using UnityEngine;

/// - 1: Time Reversal Module (rewinds the enemy a few steps)
/// - 2: Wall Breaker (breaks a wall in the direction the player last tried to move)
/// - 3: Toggle Sludge placement mode
/// - 4: Grappling Hook (dash until next wall in the last chosen direction)
/// - 5: Reflector Shield (ignore traps for a number of steps)
/// - 6: Scanner (reveal hidden traps for a short time)
public class PlayerItems : MonoBehaviour
{
    [Header("Keybinds")]
    [SerializeField] private KeyCode timeReversalKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode wallBreakerKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode sludgeToggleKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode grapplingHookKey = KeyCode.Alpha4;
    [SerializeField] private KeyCode reflectorShieldKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode scannerKey = KeyCode.Alpha6;

    private PlayerResources resources;
    private float nextTimeReversalTime = 0f;
    private float nextWallBreakerTime = 0f;
    private float nextGrapplingHookTime = 0f;
    private float nextReflectorShieldTime = 0f;
    private float nextScannerTime = 0f;

    private bool sludgePlaceMode = false;

    private void Awake()
    {
        resources = GetComponent<PlayerResources>();
    }

    private void Update()
    {
        if (GameManager.INSTANCE == null) return;
        if (GameManager.INSTANCE.IsTutorialOpen()) return;

        if (PlayerMovement.INSTANCE != null && PlayerMovement.INSTANCE.IsLocked()) return;

        if (Input.GetKeyDown(timeReversalKey))
        {
            TryUseTimeReversal();
        }

        if (Input.GetKeyDown(wallBreakerKey))
        {
            TryUseWallBreaker();
        }

        if (Input.GetKeyDown(sludgeToggleKey))
        {
            sludgePlaceMode = !sludgePlaceMode;
        }

        if (Input.GetKeyDown(grapplingHookKey))
        {
            TryUseGrapplingHook();
        }

        if (Input.GetKeyDown(reflectorShieldKey))
        {
            TryUseReflectorShield();
        }

        if (Input.GetKeyDown(scannerKey))
        {
            TryUseScanner();
        }
    }

    public bool TryHandleWorldClick(GridObj tile)
    {
        if (!sludgePlaceMode) return false;

        if (tile == null) return true;

        if (tile.GetGridType() == GridType.REPLACEABLE || tile.GetGridType() == GridType.MANUAL_REPLACEABLE) return true;

        if (tile.GetGridType() == GridType.SLUDGE) return true;

        GameManager gm = GameManager.INSTANCE;
        Grid g = gm.GetCurrentGrid();
        PrefabLibrary lib = gm.GetPrefabLibrary();

        if (lib == null || lib.prefabSludge == null) return true;

        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return true;

        int cost = gm.GetSludgeEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Sludge!");
            return true;
        }

        resources.Spend(cost);

        tile.SetGridType(GridType.SLUDGE);
        tile.ReplaceFloorPrefab(lib.prefabSludge, g.GetWorldOffsetX(), g.GetWorldOffsetY());

        // Optional color/tint if the prefab uses a standard material.
        if (tile.GetFloorObj() != null && tile.GetInteract() != null)
        {
            tile.GetInteract().SetColor(tile.GetFloorObj());
        }

        // Exit placement mode after a successful placement.
        sludgePlaceMode = false;
        return true;
    }

    private void TryUseTimeReversal()
    {
        GameManager gm = GameManager.INSTANCE;
        if (Time.time < nextTimeReversalTime) return;
        if (EnemyMovement.INSTANCE == null) return;
        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return;

        int cost = gm.GetTimeReversalEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Zeitumkehr!");
            return;
        }

        resources.Spend(cost);
        EnemyMovement.INSTANCE.Rewind(gm.GetTimeReversalSteps());

        nextTimeReversalTime = Time.time + Mathf.Max(0f, gm.GetTimeReversalCooldown());
    }

    private void TryUseWallBreaker()
    {
        GameManager gm = GameManager.INSTANCE;
        if (Time.time < nextWallBreakerTime) return;
        if (PlayerMovement.INSTANCE == null) return;
        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return;

        Grid grid = gm.GetCurrentGrid();
        if (grid == null) return;

        Vector2Int currPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        WallPos dir = PlayerMovement.INSTANCE.FacingDir;
        Vector2Int nextPos = currPos + OffsetFor(dir);

        if (!grid.IsInsideGrid(nextPos)) return;

        GridObj curr = grid.GetGridObj(currPos);
        GridObj next = grid.GetGridObj(nextPos);
        if (curr == null || next == null) return;

        if (next.GetGridType() == GridType.REPLACEABLE || next.GetGridType() == GridType.MANUAL_REPLACEABLE) return;

        WallPos opp = WallStatus.GetOppositePos(dir);

        bool hasWall = curr.HasWallAt(dir) || next.HasWallAt(opp);
        if (!hasWall) return;

        if (curr.GetWallAt(dir) == WallType.EXIT || next.GetWallAt(opp) == WallType.EXIT) return;

        int cost = gm.GetWallBreakerEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Wall Breaker!");
            return;
        }

        resources.Spend(cost);

        curr.RemoveWall(dir);
        next.RemoveWall(opp);

        nextWallBreakerTime = Time.time + Mathf.Max(0f, gm.GetWallBreakerCooldown());
    }


    private void TryUseGrapplingHook()
    {
        GameManager gm = GameManager.INSTANCE;
        if (Time.time < nextGrapplingHookTime) return;
        if (PlayerMovement.INSTANCE == null) return;
        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return;

        Grid grid = gm.GetCurrentGrid();
        if (grid == null) return;

        Vector2Int startPos = PlayerMovement.INSTANCE.GetCurrentGridPos();
        WallPos dir = PlayerMovement.INSTANCE.FacingDir;

        Vector2Int target = FindGrappleTarget(grid, startPos, dir, gm.GetGrapplingHookMaxRange());
        if (target == startPos) return;

        int cost = gm.GetGrapplingHookEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Greifhaken!");
            return;
        }

        resources.Spend(cost);
        PlayerMovement.INSTANCE.PerformInstantMove(target, dir, MoveType.JUMP);

        nextGrapplingHookTime = Time.time + Mathf.Max(0f, gm.GetGrapplingHookCooldown());
    }

    private Vector2Int FindGrappleTarget(Grid grid, Vector2Int startPos, WallPos dir, int maxRange)
    {
        Vector2Int pos = startPos;
        Vector2Int step = OffsetFor(dir);
        WallPos opp = WallStatus.GetOppositePos(dir);

        for (int i = 0; i < Mathf.Max(1, maxRange); i++)
        {
            Vector2Int nextPos = pos + step;
            if (!grid.IsInsideGrid(nextPos)) break;

            GridObj curr = grid.GetGridObj(pos);
            GridObj next = grid.GetGridObj(nextPos);
            if (curr == null || next == null) break;

            if (curr.HasWallAt(dir) || next.HasWallAt(opp)) break;

            if (next.GetGridType() == GridType.REPLACEABLE || next.GetGridType() == GridType.MANUAL_REPLACEABLE) break;

            pos = nextPos;
        }

        return pos;
    }

    private void TryUseReflectorShield()
    {
        GameManager gm = GameManager.INSTANCE;
        if (Time.time < nextReflectorShieldTime) return;
        if (PlayerMovement.INSTANCE == null) return;
        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return;

        int cost = gm.GetReflectorShieldEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Reflektorschild!");
            return;
        }

        resources.Spend(cost);

        PlayerMovement.INSTANCE.ActivateTrapShield(gm.GetReflectorShieldSteps());

        nextReflectorShieldTime = Time.time + Mathf.Max(0f, gm.GetReflectorShieldCooldown());
    }

    private void TryUseScanner()
    {
        GameManager gm = GameManager.INSTANCE;
        if (Time.time < nextScannerTime) return;
        if (resources == null) resources = GetComponent<PlayerResources>();
        if (resources == null) return;

        int cost = gm.GetScannerEnergyCost();
        if (!resources.CanAfford(cost))
        {
            Debug.Log("Nicht genug Energie für Scanner!");
            return;
        }

        resources.Spend(cost);

        gm.ActivateScanner(gm.GetScannerDuration());

        nextScannerTime = Time.time + Mathf.Max(0f, gm.GetScannerCooldown());
    }

    private static Vector2Int OffsetFor(WallPos wallPos)
    {
        switch (wallPos)
        {
            case WallPos.LEFT: return new Vector2Int(-1, 0);
            case WallPos.RIGHT: return new Vector2Int(1, 0);
            case WallPos.BACK: return new Vector2Int(0, 1);
            case WallPos.FRONT: return new Vector2Int(0, -1);
            default: return Vector2Int.zero;
        }
    }
}
