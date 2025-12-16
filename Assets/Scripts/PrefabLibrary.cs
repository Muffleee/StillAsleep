using System.Collections;
using System.Collections.Generic;
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


    public GameObject GetRandomWallPrefab()
    {
        return this.prefabWalls[Random.Range(0, this.prefabWalls.Count)];
    }

    public GameObject GetRandomFloorPrefab()
    {
        return this.prefabFloors[Random.Range(0, this.prefabFloors.Count)];
    }
}
