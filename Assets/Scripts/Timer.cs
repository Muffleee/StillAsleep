using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class handling timers.
/// </summary>
public class Timer : MonoBehaviour
{
    private float duration;
    private float timeRemaining;
    private UnityEvent onFinish;
    private Coroutine timerRoutine;
    private bool isPaused = false;

    public bool IsRunning => timerRoutine != null;

    /// <summary>
    /// Initialise a timer with a given duration and an UnityEvent which shall be called once the timer ends.
    /// </summary>
    /// <param name="duration">Duration of the timer.</param>
    /// <param name="onFinish">Event to be called upon the timer's completion.</param>
    public void Init(float duration, UnityEvent onFinish)
    {
        this.duration = duration;
        this.onFinish = onFinish;
    }

    /// <summary>
    /// Start the timer.
    /// </summary>
    public void StartTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(RunTimer());
    }

    /// <summary>
    /// Stop the timer.
    /// </summary>
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

    /// <summary>
    /// Run the timer
    /// </summary>
    /// <returns>null</returns>
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