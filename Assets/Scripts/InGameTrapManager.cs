using UnityEngine;

public static class InGameTrapManager 
{
    public static void ExecuteTrapEffect(GridObj tile)
    {
        if (tile == null || !tile.IsTrap()) 
        {
            return;
        }
        
        Debug.Log($"Trap triggered at {tile.GetGridPos()}! Executing effect.");

        // 1. Reset the visual indicator (change color back to default)
        ResetTrapVisual(tile); 
        
        // 2. Mark the tile as no longer a trap in the data model
        tile.SetTrap(false); 

        // =========================================================================
        // PLACEHOLDER LOGIC: later trap effects go here
        // =========================================================================
    }
    
    private static void ResetTrapVisual(GridObj tile)
    {
        GameObject floorObj = tile.GetFloorObj();
        if (floorObj != null)
        {
            MeshRenderer renderer = floorObj.GetComponentInChildren<MeshRenderer>();
            
            if (renderer != null && tile.GetGridType() == GridType.REGULAR) 
            {
                // ensures to get the specific instance of the material for this tile.
                //in previous version because of using the same materials other floor tiles that were not 
                //trap would also change color to white when walking over them
                Material matInstance = renderer.material; 
                
                // Only reset the color if it is currently red (a trap for now)
                if (matInstance.color == Color.red)
                {
                    //make it white(there used to be a trap here)  
                    matInstance.color = Color.white; 
                }
            }
        }
    }
}