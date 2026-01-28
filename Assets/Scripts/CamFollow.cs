using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to a camera, this class ensures the camera follows the player's position.
/// </summary>
public class CamFollow : MonoBehaviour
{
    [SerializeField] private Vector3 camOffsetFurthest = new Vector3(0f, 13f, -8.5f);
    [SerializeField] private Vector3 camOffsetClosest = new Vector3(0f, 5f, -4f);
    [SerializeField] private Transform target;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Pathfinding pathfinding;
    private Vector3 currentPos;
    private Camera cam;
    private void Start()
    {
        this.cam = this.GetComponent<Camera>();
        this.currentPos = this.camOffsetFurthest;
    }

    private void Update()
    {
        HandleScroll();
        transform.position = target.position + currentPos;
    }

    private void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll == 0f) return;
        this.currentPos = this.MoveAlongLineByFraction(camOffsetClosest, camOffsetFurthest, currentPos, scroll);
    }

    /// <summary>
    /// Checks if a world position is visible within the camera's viewport
    /// </summary>
    public bool IsPositionVisibleInCamera(Vector3 worldPosition)
    {
        if (cam == null) return false;
        Vector3 viewportPoint = cam.WorldToViewportPoint(worldPosition);

        // Check if point is infront of camera
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0;
    }

    /// <summary>
    /// Move point P along the line AB by a fraction x of the AB length
    /// </summary>
    private Vector3 MoveAlongLineByFraction(Vector3 A, Vector3 B, Vector3 P, float x)
    {
        Vector3 AB = A - B;
        float abSqr = Vector3.Dot(AB, AB);
        if (abSqr == 0f) return P;

        float t = Vector3.Dot(P - B, AB) / abSqr;

        float tNew = Mathf.Clamp01(t + x);

        return B + tNew * AB;
    }
}
