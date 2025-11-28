using System.Collections.Generic;
using UnityEngine;

public class EnergyCrystal : MonoBehaviour
{
    [SerializeField] private int energyValue = 5;

    public static List<EnergyCrystal> allCrystals = new List<EnergyCrystal>();

    private void Awake()
    {
        allCrystals.Add(this);
    }

    private void OnDestroy()
    {
        allCrystals.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerResources pr = other.GetComponent<PlayerResources>();

        if (pr != null)
        {
            pr.AddEnergy(energyValue);
            Destroy(gameObject);
        }
    }

    public static void PrepareSpawn(Vector3 targetPos, int maxCount)
    {

        if (allCrystals.Count < maxCount) return;

        EnergyCrystal furthestCrystal = null;
        float maxDist = -1f;

        foreach (EnergyCrystal crystal in allCrystals)
        {
            if (crystal == null) continue;

            float dist = Vector3.Distance(crystal.transform.position, targetPos);
            if (dist > maxDist)
            {
                maxDist = dist;
                furthestCrystal = crystal;
            }
        }

        if (furthestCrystal != null)
        {
            Destroy(furthestCrystal.gameObject);
        }
    }
}