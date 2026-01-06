using UnityEngine;

public class ExitArrowUI : MonoBehaviour
{
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private Transform player;

    private Grid grid;

    void Start()
    {
        grid = FindObjectOfType<GameManager>().GetCurrentGrid();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        GridObj exit = grid.GetExit();
        if (exit == null || player == null) return;

        Vector3 exitWorldPos = exit.GetWorldPos1();
        Vector3 dir = exitWorldPos - player.position;

        dir.y = 0f; // nur XZ-Ebene

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0, 0, -angle);
    }
}
