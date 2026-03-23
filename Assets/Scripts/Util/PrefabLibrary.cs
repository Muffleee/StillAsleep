using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefabLibrary : MonoBehaviour
{
    
    [SerializeField] public GameObject prefabEnergyCrystal;
    [SerializeField] public List<GameObject> prefabWalls;
    [SerializeField] public GameObject prefabDestructibleWall;
    [SerializeField] public GameObject prefabExit;
    [SerializeField] public List<GameObject> prefabFloors;
    [SerializeField] public List<GameObject> prefabFloorsUnderside;
    [SerializeField] public GameObject prefabReplaceable;
    [SerializeField] public GameObject prefabTrap;
    [SerializeField] public GameObject prefabJumppad;
    [SerializeField] public List<GameObject> torchPrefabs;
    [SerializeField] public GameObject fogPrefab;
    [SerializeField] public GameObject prefabIce;
    [SerializeField] public GameObject prefabRotating;
    [SerializeField] public SpikeTrap prefabSpike;
    [SerializeField] public TMP_Text countdownText;
    [SerializeField] public Image countdownFill; 
    [SerializeField] public GameObject prefabItemClock;
    [SerializeField] public GameObject prefabItemPickaxe;
    [SerializeField] public GameObject prefabItemTrapForcefield;
    [SerializeField] public GameObject prefabItemBoxTrap; 
    [SerializeField] public GameObject prefabItemTerrainScanner;

    [SerializeField] public Sprite iconItemClock;
    [SerializeField] public Sprite iconItemPickaxe;
    [SerializeField] public Sprite iconItemTrapForcefield;
    [SerializeField] public Sprite iconItemScanner;


    public GameObject GetRandomWallPrefab()
    {
        return this.prefabWalls[Random.Range(0, this.prefabWalls.Count)];
    }

    public GameObject GetRandomFloorPrefab()
    {
        return this.prefabFloors[Random.Range(0, this.prefabFloors.Count)];
    }

    public GameObject GetRandomUnderfloorPrefab()
    {
        return this.prefabFloorsUnderside[Random.Range(0, this.prefabFloorsUnderside.Count)];
    }

    public GameObject GetRandomTorchPrefab()
    {
        return this.torchPrefabs[Random.Range(0, this.torchPrefabs.Count)];
    }
}
