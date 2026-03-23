using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainScannerEffect : MonoBehaviour
{
    [SerializeField] private float size = 21f;
    [SerializeField] private float speed = 7f;
    [SerializeField] private ParticleSystem terrainScanner;
    public static TerrainScannerEffect INSTANCE;
    public static int maxDistance = 5;
    private float distance = 0f;

    void Awake()
    {
        INSTANCE = this;
    }
    private void Start()
    {
        distance = GridObj.PLACEMENT_FACTOR * maxDistance;
    }

    /// <summary>
    /// Play the terrain scanner repetitions times with delay ms between scans 
    /// </summary>
    /// <param name="repetitions"></param>
    /// <param name="delay"></param>
    public void PlayTerrainScanner(int repetitions, float delay)
    {
        if(repetitions < 1) repetitions = 1;
        for(int i = 1; i <= repetitions; i++)
        {
            this.Invoke(nameof(PlayTerrainScanner), delay * (i - 1));
        }
    }

    /// <summary>
    /// play the terrain scanner once
    /// </summary>
    private void PlayTerrainScanner()
    {
        var main = terrainScanner.main;
        main.startLifetime = distance / speed;
        main.startSize = size;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        terrainScanner.Play();
    }
}
