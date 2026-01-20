using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TorchAnimation : MonoBehaviour
{
    [SerializeField] private Light pLight;
    [SerializeField] private Vector2 intensityRange = new Vector2(1f, 10f);
    [SerializeField] private float flickerSpeed = 5f;
    [SerializeField] private float smoothing = 5f;

    private float noiseOffset;

    private void Update()
    {
        AnimateTorchFlame();
    }

    private void AnimateTorchFlame()
    {
        noiseOffset += Time.deltaTime * flickerSpeed;

        float noise = Mathf.PerlinNoise(noiseOffset, 0f);

        float targetIntensity = Mathf.Lerp(intensityRange.x, intensityRange.y, noise);

        pLight.intensity = Mathf.Lerp(
            pLight.intensity,
            targetIntensity,
            Time.deltaTime * smoothing
        );
    }
}