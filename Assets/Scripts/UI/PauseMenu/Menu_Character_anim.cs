using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Character_anim : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float floatHeight = 80f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float swayAmountX = 20f;
    [SerializeField] private float swayAmountZ = 80f;
    [SerializeField] private float swaySpeed = 0.5f;
    [SerializeField] private PlayerResources player;

    [Header("Crystals")]
    [SerializeField] private GameObject crystalPrefab;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float crystalBobHeight = 20f;
    [SerializeField] private float crystalBobSpeed = 1.5f;

    private Vector3 startPosition;
    private List<Vector3> crystalStartPositions = new List<Vector3>();
    private List<Transform> crystalTransforms = new List<Transform>();
    private int lastEnergyCount = -1;

    void Start()
    {
        startPosition = transform.position;
        RefreshCrystals();
    }

    void RefreshCrystals()
    {
        int current = player.getEnergy();
        if (current == lastEnergyCount) return;

        foreach (var t in crystalTransforms)
            if (t != null) Destroy(t.gameObject);

        crystalTransforms.Clear();
        crystalStartPositions.Clear();

        for (int i = 0; i < current; i++)
        {
            float angle = (360f / current) * i;
            var crystal = Instantiate(crystalPrefab, transform);

            foreach (var t in crystal.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = LayerMask.NameToLayer("MenuCharacter");

            crystal.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 localPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * orbitRadius;
            crystal.transform.localPosition = localPos;

            crystalStartPositions.Add(localPos);
            crystalTransforms.Add(crystal.transform);
        }

        lastEnergyCount = current;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        transform.Rotate(0f, rotationSpeed * Time.unscaledDeltaTime, 0f);
        float newY = startPosition.y + Mathf.Sin(t * floatSpeed) * floatHeight;
        float newX = startPosition.x + Mathf.Sin(t * swaySpeed * 0.7f) * swayAmountX;
        float newZ = startPosition.z + Mathf.Sin(t * swaySpeed * 0.5f) * swayAmountZ;
        transform.position = new Vector3(newX, newY, newZ);

        for (int i = crystalTransforms.Count - 1; i >= 0; i--)
        {
            if (crystalTransforms[i] == null)
            {
                crystalTransforms.RemoveAt(i);
                crystalStartPositions.RemoveAt(i);
                continue;
            }

            float phase = i * (Mathf.PI * 2f / crystalTransforms.Count);
            float bob = Mathf.Sin(t * crystalBobSpeed + phase) * crystalBobHeight;
            crystalTransforms[i].localPosition = crystalStartPositions[i] + new Vector3(0f, bob, 0f);
        }

        RefreshCrystals();
    }
}