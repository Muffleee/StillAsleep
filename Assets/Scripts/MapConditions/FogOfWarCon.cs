using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarCon : IMapCondition
{
    public int Difficulty()
    {
        return 3;
    }

    public void Initiate(int _)
    {
        FogOfWarScript.INSTANCE.SetIsActive(true);
    }

    public void Deactivate()
    {
        FogOfWarScript.INSTANCE.SetIsActive(false);
    }
}
