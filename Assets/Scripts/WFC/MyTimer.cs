using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MyTimer : MonoBehaviour
{
    [SerializeField] private float time = 5f;
    private UnityEvent onFinish;
    private Coroutine coroutine;
    private float timeRemaining;
    private bool isRunning = false;

    public void Init(float time, UnityEvent onFinish)
    {
        this.time = time;
        this.onFinish = onFinish;
    }

    public void StartTimer()
    {
        if (isRunning)
            StopTimer();

        timeRemaining = time;
        coroutine = StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        isRunning = false;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    private IEnumerator TimerRoutine()
    {
        isRunning = true;
        while (timeRemaining > 0f)
        {
            yield return null;
            timeRemaining -= Time.deltaTime;
        }

        isRunning = false;
        onFinish?.Invoke();
    }
}
