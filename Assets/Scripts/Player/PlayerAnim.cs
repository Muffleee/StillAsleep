using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [SerializeField] private Animator animator;

    /// <summary>
    /// Trigger any movement animation, MoveType.INVALID will trigger idle animation
    /// </summary>
    /// <param name="mt"></param>
    /// <param name="toggle"></param>
    public void TriggerMoveAnim(MoveType mt)
    {   
        string trigger;
        switch (mt)
        {
            case MoveType.WALK:
                trigger = "TriggerWalk";
                break;
            case MoveType.JUMP:
                trigger = "TriggerJump";
                break;
            case MoveType.TRAP:
                trigger = "TriggerTrap";
                break;
            default:
                trigger = "TriggerIdle";
                break;
        }
        this.animator.SetTrigger(trigger);
    }
}
