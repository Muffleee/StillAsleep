using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Doorbehaviour : MonoBehaviour
{
    [SerializeField] private float time = 10f;
    [SerializeField] private Timer Timer;
    private float startTime;
    private MeshRenderer meshRenderer;

   

    private void Start()
    {
        startTime = time;
        meshRenderer = GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        time = Timer.CountDown(time);
       
        ColorChange(time);
        //TpAway();
        print(time);

    }
    
    private void ColorChange(float time)
    {
        float tt = time / startTime;
        Color brightGreen = new Color(0.6f, 1f, 0.6f);
        Color darkGreen = new Color(0f, 0.3f, 0f);

        Color currentColor = Color.Lerp(darkGreen,brightGreen, tt);
        meshRenderer.material.color = currentColor;
    }
}
