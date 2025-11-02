using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    private float duration;
    private float timeRemaining;
    private UnityEvent onFinish;
    private Coroutine timerRoutine;
    private bool isPaused = false;

    public bool IsRunning => timerRoutine != null;

    public void Init(float duration, UnityEvent onFinish)
    {
        this.duration = duration;
        this.onFinish = onFinish;
    }

    public void StartTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(RunTimer());
    }

    public void StopTimer()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
    }

    public void Pause() => isPaused = true;
    public void Resume() => isPaused = false;
    public float GetTimeRemaining() => Mathf.Max(0f, timeRemaining);

    private IEnumerator RunTimer()
    {
        timeRemaining = duration;

        while (timeRemaining > 0f)
        {
            if (!isPaused)
                timeRemaining -= Time.deltaTime;

            yield return null;
        }

        timeRemaining = 0f;
        onFinish?.Invoke();
        timerRoutine = null;
    }
}
