using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float CountDown(float timer)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            
        }
        return timer;
    }
}