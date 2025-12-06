using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages any of the player's clicks in the 3D world, e.g. in order to select tiles.
/// </summary>
public class ClickManager : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    void Update()
    {
        // Detect if the primary (left) mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera through the mouse position and potentially return a collider hit by it
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj == null) return;
                this.gameManager.OnClick(hitObj);
            }
        }
    }
}