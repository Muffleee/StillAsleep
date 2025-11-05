using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Exit : MonoBehaviour
{
    public GridObj gridObj;
    public WallPos wallPos;
    public UnityEvent<GridObj, WallPos> onDestroy;
    [SerializeField] private float time = 10f;
    private Timer timer;
    
    private float startTime;
    private MeshRenderer meshRenderer;
    
    private void Start()
    {
        startTime = time;
        meshRenderer = GetComponent<MeshRenderer>();

        // event erstellen wird ausgelöst wenn Timer abläuft
        UnityEvent finishedEvent = new UnityEvent();
        finishedEvent.AddListener(OnTimerFinished);

        timer = gameObject.AddComponent<Timer>();

        // timer initialisieren & starten
        timer.Init(time, finishedEvent);
        timer.StartTimer();
    }

    private void Update()
    {
        float timeLeft = timer.GetTimeRemaining();
        ColorChange(timeLeft);
    }

    private void ColorChange(float timeRemaining)
    {
        float t = timeRemaining / startTime;
        Color brightGreen = new Color(0.6f, 1f, 0.6f);
        Color darkGreen = new Color(0f, 0.3f, 0f);

        Color currentColor = Color.Lerp(darkGreen, brightGreen, t);
        meshRenderer.material.color = currentColor;
    }

    private void OnTimerFinished()
    {   
        onDestroy.Invoke(this.gridObj, this.wallPos);
        Destroy(this.gameObject);
    }
}