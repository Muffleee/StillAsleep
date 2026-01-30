using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeManager : MonoBehaviour
{   
    [SerializeField] Volume globalVolume;
    [SerializeField] private Vector2 vignetteIntensities = new Vector2(0.6f, 0.8f);
    [SerializeField] private float vignetteGrowthSpeed = 0.05f;
    private Vignette vignette;
    private bool vignetteValRising = true;

    private void Start()
    {
        this.globalVolume.profile.TryGet<Vignette>(out this.vignette);
    }
    private void FixedUpdate()
    {
        this.AnimateVignette();
    }

    private void AnimateVignette()
    {   
        if(this.vignette == null) return;
        if(vignetteValRising)
        {
            this.vignette.intensity.value = math.min(this.vignette.intensity.value + this.vignetteGrowthSpeed, this.vignetteIntensities.y);
            if(this.vignette.intensity.value >= this.vignetteIntensities.y) this.vignetteValRising = false;
        } else
        {
            this.vignette.intensity.value = math.max(this.vignette.intensity.value - this.vignetteGrowthSpeed, this.vignetteIntensities.x);
            if(this.vignette.intensity.value <= this.vignetteIntensities.x) this.vignetteValRising = true;
        }
    }
}
