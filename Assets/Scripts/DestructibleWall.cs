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
        this.rend = this.GetComponent<MeshRenderer>();

        if (this.rend != null)
            this.rend.material.color = this.startColor;

        UnityEvent onFinish = new UnityEvent();
        onFinish.AddListener(this.DestroyWall);

        this.timer = this.gameObject.AddComponent<Timer>();
        this.timer.Init(this.destroyTime, onFinish);

        if (this.startDelay > 0f)
            this.StartCoroutine(this.StartTimerWithDelay());
        else
            this.timer.StartTimer();
    }

    /// <summary>
    /// Hold the timer execution for the defined start delay.
    /// </summary>
    /// <returns></returns>
    private System.Collections.IEnumerator StartTimerWithDelay()
    {
        yield return new WaitForSeconds(this.startDelay);
        this.timer.StartTimer();
    }

    /// <summary>
    /// Render the wall's colour depending on the remaining time.
    /// </summary>
    private void Update()
    {
        if (this.timer == null || !this.timer.IsRunning || this.rend == null) return;

        float progress = 1f - (this.timer.GetTimeRemaining() / this.destroyTime);
        this.rend.material.color = Color.Lerp(this.startColor, this.endColor, progress);
    }

    /// <summary>
    /// Destroy the wall.
    /// </summary>
    private void DestroyWall()
    {
        if (this.debug) Debug.Log($"{this.gameObject.name} destroyed after {this.destroyTime} seconds!");
        this.onDestroy.Invoke(this.gridObj, this.wallPos);
        Destroy(this.gameObject);
    }
}