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

    public bool IsRunning => this.timerRoutine != null;

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
        if (this.timerRoutine != null)
            this.StopCoroutine(this.timerRoutine);

        this.timerRoutine = this.StartCoroutine(this.RunTimer());
    }

    /// <summary>
    /// Stop the timer.
    /// </summary>
    public void StopTimer()
    {
        if (this.timerRoutine != null)
        {
            this.StopCoroutine(this.timerRoutine);
            this.timerRoutine = null;
        }
    }

    public void Pause() => this.isPaused = true;
    public void Resume() => this.isPaused = false;
    public float GetTimeRemaining() => Mathf.Max(0f, this.timeRemaining);

    /// <summary>
    /// Run the timer
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator RunTimer()
    {
        this.timeRemaining = this.duration;

        while (this.timeRemaining > 0f)
        {
            if (!this.isPaused)
                this.timeRemaining -= Time.deltaTime;

            yield return null;
        }

        this.timeRemaining = 0f;
        this.onFinish?.Invoke();
        this.timerRoutine = null;
    }
}