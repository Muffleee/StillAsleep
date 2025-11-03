using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    private float timeRemaining;
    private bool isRunning = false;
    private UnityEvent onFinish;

    public void Init(float timeInSeconds, UnityEvent onFinishEvent)
    {
        timeRemaining = timeInSeconds;
        onFinish = onFinishEvent;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    private void Update()
    {
        if (!isRunning || timeRemaining <= 0f) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            onFinish?.Invoke(); // Event auslösen
        }
    }
}