using UnityEngine;

/// <summary>
/// Interface describing an interactable GridObject.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Set the colour of the GridObj.
    /// </summary>
    /// <param name="obj">GridObj of which the colour should be changed.</param>
    void SetColor(GameObject obj);

    /// <summary>
    /// Executed if the tile is used/stepped upon.
    /// </summary>
    /// <param name="obj">Current grid object.</param>
    void OnUse(GridObj obj);
    
    /// <summary>
    /// Check whether a given move is valid.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    bool IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos);

    /// <summary>
    /// Returns the prefab or null
    /// </summary>
    /// <returns></returns>
    GameObject GetPrefab();
}

/// <summary>
/// Class describing a regular tile with no special functionality.
/// </summary>
public class Regular : IInteractable
{
    void IInteractable.SetColor(GameObject obj) {return;}
    void IInteractable.OnUse(GridObj obj) {return;}

    /// <summary>
    /// Check whether a given move is valid. Movement is valid if there are no walls between the origin and the destination, and if the destination isn't a replaceable tile.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE);
    }

    GameObject IInteractable.GetPrefab()
    {
        return null;
    }
}

/// <summary>
/// Class describing a trap. No functionality at the moment except being red and turning back to white once stepped on.
/// </summary>
public class Trap : IInteractable
{
    private bool activated = true;
    void IInteractable.SetColor(GameObject obj)
    {
        obj.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        if (this.activated)
        {
            this.ActivateTrap();

            // 1. Reset the visual indicator (change color back to default)

            this.ResetTrapVisual(obj);
        }

        // 2. Mark the tile as no longer a trap in the data model
        this.activated = false;
    }

    private void ActivateTrap()
    {
        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null)
        {
            pr.Spend(3);   // 1 Energie abziehen
        }
    }

    private void ResetTrapVisual(GridObj tile)
    {
        tile.ReplaceFloorPrefab(GameManager.INSTANCE.GetPrefabLibrary().prefabFloor, GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
    }

    /// <summary>
    /// Check whether a given move is valid. Movement is valid if there are no walls between the origin and the destination, and if the destination isn't a replaceable tile.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE);
    }

    GameObject IInteractable.GetPrefab()
    {
        return GameManager.INSTANCE.GetPrefabLibrary().prefabTrap;
    }
}

/// <summary>
/// Class describing a jumping pad. Allows the player to jump over walls.
/// </summary>
public class JumpingPads : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        obj.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        //can be extended later in case we wanna have an effect when standing on a jumbad boucning for example
    }

    /// <summary>
    /// Check whether a given move is valid. Movement is valid unless the destination tile is invalid or replaceable.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null && pr.CurrentEnergy > 0 && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && curr.HasWallAt(wPos))
        {
            pr.Spend(1);   // 1 Energie abziehen
            return nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE);
        }
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE);
    }
    
    GameObject IInteractable.GetPrefab()
    {
        return GameManager.INSTANCE.GetPrefabLibrary().prefabJumppad;
    }
}

/// <summary>
/// Class describing a replaceable tile. It cannot be moved onto and poses as a placeholder for the next WFC call,.
/// </summary>
public class Replaceable : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        obj.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        // Should never be called
    }

    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return false;
    }

    GameObject IInteractable.GetPrefab()
    {
        return null;
    }
}

public class ManualReplaceable : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        obj.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        return;
    }

    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE);
    }
    
    GameObject IInteractable.GetPrefab()
    {
        return null;
    }
}