using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private bool isExtended = true;
    [SerializeField] private GameObject spikeObj;
    private const float retractDistance = -1f;
    private const float duration = 1.5f;

    private void Awake()
    {
        if(!this.isExtended) this.Retract();
    }

    public void Extend(bool toggle)
    {
        if(toggle == this.isExtended) return;
        if(this.isExtended) this.Retract();
        else this.Extend();
    }

    public void Toggle()
    {
        this.Extend(!this.isExtended);
    }

    private void Extend()
    {   
        this.isExtended = true;
        StartCoroutine(MoveUpCoroutine(this.spikeObj, -retractDistance, duration));
    }

    private void Retract()
    {   
        this.isExtended = false;
        StartCoroutine(MoveUpCoroutine(this.spikeObj, retractDistance, duration));
    }

    private IEnumerator MoveUpCoroutine(GameObject obj, float distance, float duration)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = startPos + Vector3.up * distance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        obj.transform.position = targetPos;
    }
}
