using System;
using System.Collections;
using System.Threading.Tasks;
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
    MoveType IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos);

    /// <summary>
    /// Returns the prefab or null
    /// </summary>
    /// <returns></returns>
    GameObject GetPrefab();

    /// <summary>
    /// Triggers possible animations
    /// </summary>
    /// <param name="animator"></param>
    public void TriggerAnimation(Animator animator, MoveType mt);
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
    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        if(!curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE))
        {
            if(nextObj.GetGridType() == GridType.TRAP) return MoveType.TRAP;
            if(nextObj.GetGridType() == GridType.HIDDENTRAP)return MoveType.TRAP;
            return MoveType.WALK;
        }
        return MoveType.INVALID;
    }

    GameObject IInteractable.GetPrefab()
    {
        return null;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt) {}
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
            // Reset the visual indicator and grid type after the animation delay

            _ = this.ResetTrap(obj);
        }   
    }

    private void ActivateTrap()
    {
        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null)
        {
            pr.RemoveEnergy(3);
            AudioManager.Instance.PlayTrap();
        }
    }

    async Task ResetTrap(GridObj tile)
    {

        await Task.Delay(1500); // Wait for trap animation
            
        if(tile == null) return;
        tile.SetGridType(GridType.REGULAR);
        tile.ReplaceFloorPrefab(GameManager.INSTANCE.GetPrefabLibrary().GetRandomFloorPrefab(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
        this.activated = false;
    }

    /// <summary>
    /// Check whether a given move is valid. Movement is valid if there are no walls between the origin and the destination, and if the destination isn't a replaceable tile.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        if(!curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE))
        {
            if(nextObj.GetGridType() == GridType.TRAP) return MoveType.TRAP;
            if(nextObj.GetGridType() == GridType.HIDDENTRAP)return MoveType.TRAP;
            return MoveType.WALK;
        }
        return MoveType.INVALID;
    }

    GameObject IInteractable.GetPrefab()
    {
        return GameManager.INSTANCE.GetPrefabLibrary().prefabTrap;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt) {}
}

/// <summary>
/// Class describing a jumping pad. Allows the player to jump over walls.
/// </summary>
public class JumpingPads : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        // obj.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
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
    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null && pr.CurrentEnergy > 0 && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && curr.HasWallAt(wPos))
        {
            
            if(nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE))
            {
                pr.Spend(1);   // 1 Energie abziehen
                return MoveType.JUMP;
            }
            if(nextObj.GetGridType() == GridType.TRAP) return MoveType.TRAP;
            if(nextObj.GetGridType() == GridType.HIDDENTRAP) return MoveType.TRAP;
            return MoveType.WALK;
        }
        if(!curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE)) return MoveType.WALK;
        return MoveType.INVALID;
    }
    
    GameObject IInteractable.GetPrefab()
    {
        return GameManager.INSTANCE.GetPrefabLibrary().prefabJumppad;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt)
    {
        if(mt != MoveType.JUMP) return;
        animator.SetTrigger("TriggerAnim");
        AudioManager.Instance.PlayJumping();
    }
}

/// <summary>
/// Class describing a replaceable tile. It cannot be moved onto and poses as a placeholder for the next WFC call,.
/// </summary>
public class Replaceable : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
    }
    void IInteractable.OnUse(GridObj obj)
    {
        // Should never be called
    }

    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return MoveType.INVALID;
    }

    GameObject IInteractable.GetPrefab()
    {
        return null;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt) {}
}

public class ManualReplaceable : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
    }
    void IInteractable.OnUse(GridObj obj)
    {
        return;
    }

    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        if(nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE))
        {
            if(nextObj.GetGridType() == GridType.TRAP) return MoveType.TRAP;
            if(nextObj.GetGridType() == GridType.HIDDENTRAP)return MoveType.TRAP;
            return MoveType.WALK;
        }
        return MoveType.INVALID;
    }
    
    GameObject IInteractable.GetPrefab()
    {
        return null;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt) {}
}

/// <summary>
/// Class describing a trap. No functionality at the moment except being red and turning back to white once stepped on.
/// </summary>
public class HiddenTrap : IInteractable
{ private bool activated = true;
    void IInteractable.SetColor(GameObject obj)
    {
    }
    void IInteractable.OnUse(GridObj obj)
    {
        if (this.activated)
        {
            this.ActivateTrap();
            // Reset the visual indicator and grid type after the animation delay

            _ = this.ResetTrap(obj);
        }   
    }

    private void ActivateTrap()
    {
        PlayerResources pr = GameObject.FindObjectOfType<PlayerResources>();
        if (pr != null)
        {
            pr.RemoveEnergy(1);  
        }
    }

    async Task ResetTrap(GridObj tile)
    {

        await Task.Delay(1500); // Wait for trap animation
            
        if(tile == null) return;
        tile.SetGridType(GridType.REGULAR);
        tile.ReplaceFloorPrefab(GameManager.INSTANCE.GetPrefabLibrary().GetRandomFloorPrefab(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetX(), GameManager.INSTANCE.GetCurrentGrid().GetWorldOffsetY());
        this.activated = false;
    }

    /// <summary>
    /// Check whether a given move is valid. Movement is valid if there are no walls between the origin and the destination, and if the destination isn't a replaceable tile.
    /// </summary>
    /// <param name="curr">Origin GridObj</param>
    /// <param name="nextObj">Destination GridObj</param>
    /// <param name="wPos">Direction</param>
    /// <returns></returns>
    MoveType IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        if(!curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE) && (nextObj.GetGridType() != GridType.MANUAL_REPLACEABLE))
        {
            if(nextObj.GetGridType() == GridType.TRAP) return MoveType.TRAP;
            if(nextObj.GetGridType() == GridType.HIDDENTRAP)return MoveType.TRAP;
            return MoveType.WALK;
        }
        return MoveType.INVALID;
    }

    GameObject IInteractable.GetPrefab()
    {
        return GameManager.INSTANCE.GetPrefabLibrary().prefabTrap;
    }

    void IInteractable.TriggerAnimation(Animator animator, MoveType mt) {}
}