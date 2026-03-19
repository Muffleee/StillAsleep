using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class OpponentCon : IMapCondition
{

    public int Difficulty()
    {
        return 4;
    }

    public void Initiate(int phase)
    {
        Opponent.INSTANCE.gameObject.SetActive(true);
        Opponent.INSTANCE.SetDifficulty(phase);
        Opponent.INSTANCE.StartCondition();
    }

    public void Deactivate()
    {
        Opponent.INSTANCE.EndCondition();
        Opponent.INSTANCE.gameObject.SetActive(false);
    }
}
