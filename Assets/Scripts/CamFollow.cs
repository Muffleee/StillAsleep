using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to a camera, this class ensures the camera follows the player's position.
/// </summary>
public class CamFollow : MonoBehaviour
{
    [SerializeField] private Vector3 camOffset = new Vector3(0f, 7f, -4.5f);
    [SerializeField] private Transform target;
    private void Update()
    {
        transform.position = target.position + camOffset;
    }
}
