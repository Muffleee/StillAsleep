using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHover : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        // Hover in LOCAL space
        float newY = startLocalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);

        // Rotate normally
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
