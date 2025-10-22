using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField] private Vector3 camOffset = new Vector3(0f, 7f, -4.5f);
    [SerializeField] private Transform target;
    private void Update()
    {
        this.transform.position = target.position + camOffset;
    }
}
