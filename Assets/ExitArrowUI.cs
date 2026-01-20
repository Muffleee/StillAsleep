using UnityEngine;

public class ExitArrowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform arrowRect;

    [Header("Settings")]
    [SerializeField] private float rotationOffset = 0f;

    private void Update()
    {
        if (ExitManager.Instance == null) return;
        if (ExitManager.Instance.CurrentExit == null) return;

        Vector3 toExit =
            ExitManager.Instance.CurrentExit.position - player.position;

        toExit.y = 0f;

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0f;

        float angle = Vector3.SignedAngle(
            cameraForward,
            toExit,
            Vector3.up
        );

        arrowRect.localRotation =
            Quaternion.Euler(0f, 0f, -angle + rotationOffset);
    }
}

