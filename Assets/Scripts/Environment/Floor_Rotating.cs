using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor_Rotating : MonoBehaviour
{   
    [SerializeField] private Transform gearLarge;
    [SerializeField] private Transform gearSmall;
    [SerializeField] private Vector3 gearLargeRot = new Vector3(0, 40, 0);
    [SerializeField] private Vector3 gearSmallRot = new Vector3(0, 80, 0);
    
    private void Update()
    {
        gearLarge.Rotate(gearLargeRot * Time.deltaTime);
        gearSmall.Rotate(gearSmallRot * Time.deltaTime);
    }
}