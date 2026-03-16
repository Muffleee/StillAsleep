using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class OpponentCon : IMapCondition
{

    public int Difficulty()
    {
        return 0;
    }

    public void Initiate(int _)
    {
        Opponent.INSTANCE.gameObject.SetActive(true);
        Opponent.INSTANCE.StartCondition();
    }

    public void Deactivate()
    {
        Opponent.INSTANCE.EndCondition();
        Opponent.INSTANCE.gameObject.SetActive(false);
    }
}
