using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface IInteractable
{
    void SetColor(GameObject obj);
    void OnUse(GridObj obj);
    bool IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos);
}

public class Regular : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        return;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        return;
    }

    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE);
    }
}

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
        // =========================================================================
        // PLACEHOLDER LOGIC: later trap effects go here
        // =========================================================================
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

    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return !curr.HasWallAt(wPos) && nextObj != null && (nextObj.GetGridType() != GridType.REPLACEABLE);
    }
}

public class Replaceable : IInteractable
{
    void IInteractable.SetColor(GameObject obj)
    {
        obj.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
    void IInteractable.OnUse(GridObj obj)
    {
        throw new System.NotImplementedException();
    }

    bool IInteractable.IsValidMove(GridObj curr, GridObj nextObj, WallPos wPos)
    {
        return false;
    }
}
