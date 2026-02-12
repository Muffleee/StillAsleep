using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapCondition
{
    public int Difficulty();
    public void Initiate(int level);
}