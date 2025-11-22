using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class describing a destructible wall on a specific GridObj at a specific WallPos.
/// </summary>
public class DestructibleWall : MonoBehaviour
{
    public GridObj gridObj;
    public WallPos wallPos;
    public UnityEvent<GridObj, WallPos> onDestroy;

    [SerializeField] bool debug = false;
    [Header("Timer Settings")]
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private float startDelay = 0f;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;

    private Timer timer;
    private MeshRenderer rend;

    /// <summary>
    /// Initialise the wall with its timer and the appropriate events
    /// </summary>
    private void Start()
    {
        rend = GetComponent<MeshRenderer>();

        if (rend != null)
            rend.material.color = startColor;

        UnityEvent onFinish = new UnityEvent();
        onFinish.AddListener(DestroyWall);

        timer = gameObject.AddComponent<Timer>();
        timer.Init(destroyTime, onFinish);

        if (startDelay > 0f)
            StartCoroutine(StartTimerWithDelay());
        else
            timer.StartTimer();
    }

    /// <summary>
    /// Hold the timer execution for the defined start delay.
    /// </summary>
    /// <returns></returns>
    private System.Collections.IEnumerator StartTimerWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        timer.StartTimer();
    }

    /// <summary>
    /// Render the wall's colour depending on the remaining time.
    /// </summary>
    private void Update()
    {
        if (timer == null || !timer.IsRunning || rend == null) return;

        float progress = 1f - (timer.GetTimeRemaining() / destroyTime);
        rend.material.color = Color.Lerp(startColor, endColor, progress);
    }

    /// <summary>
    /// Destroy the wall.
    /// </summary>
    private void DestroyWall()
    {
        if (debug) Debug.Log($"{gameObject.name} destroyed after {destroyTime} seconds!");
        onDestroy.Invoke(this.gridObj, this.wallPos);
        Destroy(this.gameObject);
    }
}