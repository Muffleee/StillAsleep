using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class describing a destructible exit. Has a fixed time after which the exit is destroyed.
/// </summary>
public class DestructiveExit : MonoBehaviour
{
    public GridObj gridObj;
    public WallPos wallPos;
    public UnityEvent<GridObj, WallPos> onDestroy;
    [SerializeField] private float time = 10f;
    private Timer timer;
    
    private float startTime;
    private MeshRenderer meshRenderer;
    
    /// <summary>
    /// Initialise the timer, its associated end event, and the progression-based rendering of the exit.
    /// </summary>
    private void Start()
    {
        this.startTime = this.time;
        this.meshRenderer = this.GetComponent<MeshRenderer>();

        // event erstellen wird ausgelöst wenn Timer abläuft
        UnityEvent finishedEvent = new UnityEvent();
        finishedEvent.AddListener(this.OnTimerFinished);

        this.timer = this.gameObject.AddComponent<Timer>();

        // timer initialisieren & starten
        this.timer.Init(this.time, finishedEvent);
        this.timer.StartTimer();
    }

    /// <summary>
    /// Every frame, change the colour according to the remaining time.
    /// </summary>
    private void Update()
    {
        float timeLeft = this.timer.GetTimeRemaining();
        this.ColorChange(timeLeft);
    }

    /// <summary>
    /// Change the colour of the exit according to the time remaining.
    /// </summary>
    /// <param name="timeRemaining">Time remaining in seconds.</param>
    private void ColorChange(float timeRemaining)
    {
        float t = timeRemaining / this.startTime;
        Color brightGreen = new Color(0.6f, 1f, 0.6f);
        Color darkGreen = new Color(0f, 0.3f, 0f);

        Color currentColor = Color.Lerp(darkGreen, brightGreen, t);
        this.meshRenderer.material.color = currentColor;
    }

    /// <summary>
    /// Once the timer is finished, invoke the onDestroy event and destroy this GameObject.
    /// </summary>
    private void OnTimerFinished()
    {
        this.onDestroy.Invoke(this.gridObj, this.wallPos);
        Destroy(this.gameObject);
    }
}