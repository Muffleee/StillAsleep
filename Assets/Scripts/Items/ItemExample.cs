using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemExample : IItemBehaviour
{
    public override bool Use()
    {
        print("Example Item used.");
        return true;
    }
}
