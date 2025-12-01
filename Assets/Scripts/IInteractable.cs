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
        if (activated)
        {
            ActivateTrap();

            // 1. Reset the visual indicator (change color back to default)

            ResetTrapVisual(obj);
        }

        // 2. Mark the tile as no longer a trap in the data model
        activated = false;
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
        GameObject floorObj = tile.GetFloorObj();
        if (floorObj != null)
        {
            MeshRenderer renderer = floorObj.GetComponentInChildren<MeshRenderer>();

            if (renderer != null)
            {
                // ensures to get the specific instance of the material for this tile.
                //in previous version because of using the same materials other floor tiles that were not 
                //trap would also change color to white when walking over them
                Material matInstance = renderer.material;

                // Only reset the color if it is currently red (a trap for now)
                //if (matInstance.color == Color.red)
                //{
                    //make it white(there used to be a trap here)  
                    matInstance.color = Color.white;
                //}
            }
        }
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
        if (pr != null && pr.CurrentEnergy >0 && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE)&& curr.HasWallAt(wPos))
        {
            pr.Spend(1);   // 1 Energie abziehen
            return nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE);
        }
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE);
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
}