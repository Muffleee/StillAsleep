using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrefabLibrary : MonoBehaviour
{
    
    [SerializeField] public GameObject prefabEnergyCrystal;
    [SerializeField] public List<GameObject> prefabWalls;
    [SerializeField] public GameObject prefabDestructibleWall;
    [SerializeField] public GameObject prefabExit;
    [SerializeField] public List<GameObject> prefabFloors;
    [SerializeField] public GameObject prefabReplaceable;
    [SerializeField] public GameObject prefabTrap;
    [SerializeField] public GameObject prefabJumppad;
    [SerializeField] public List<GameObject> torchPrefabs;
    [SerializeField] public GameObject fogPrefab;
    [SerializeField] public GameObject prefabIce;
    [SerializeField] public GameObject prefabRotating;
    [SerializeField] public TMP_Text countdownText;


    public GameObject GetRandomWallPrefab()
    {
        return this.prefabWalls[Random.Range(0, this.prefabWalls.Count)];
    }

    public GameObject GetRandomFloorPrefab()
    {
        return this.prefabFloors[Random.Range(0, this.prefabFloors.Count)];
    }

    public GameObject GetRandomTorchPrefab()
    {
        return this.torchPrefabs[Random.Range(0, this.torchPrefabs.Count)];
    }
}
