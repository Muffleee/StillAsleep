using UnityEngine;
using UnityEngine.Events;

public class DestructibleWall : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private float startDelay = 0f;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;

    private Timer timer;
    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();

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

    private System.Collections.IEnumerator StartTimerWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        timer.StartTimer();
    }

    private void Update()
    {
        if (timer == null || !timer.IsRunning || rend == null) return;

        float progress = 1f - (timer.GetTimeRemaining() / destroyTime);
        rend.material.color = Color.Lerp(startColor, endColor, progress);

        // Pause/Resume example
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timer.IsRunning && !Input.GetKey(KeyCode.LeftShift))
                timer.Pause();
            else
                timer.Resume();
        }
    }

    private void DestroyWall()
    {
        Debug.Log($"{gameObject.name} destroyed after {destroyTime} seconds!");
        Destroy(gameObject);
    }
}
