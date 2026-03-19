using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxTrap : MonoBehaviour
{   
    [SerializeField] private float speed = 200f;
    [SerializeField] private GameObject vfx;
    [SerializeField] private Transform lidR;
    [SerializeField] private Transform lidL;
    private bool isOpen = true;
    private bool isRotating = false;

    public void ToggleOpen()
    {
        if(this.isOpen) this.Close();
        else this.Open();
    }

    private void Open()
    {   
        if(isOpen || isRotating) return;
        isOpen = true;
        isRotating = true;
        StartCoroutine(RotateLocalZCoroutine(this.lidR, 160f, this.speed));
        StartCoroutine(RotateLocalZCoroutine(this.lidL, -160f, this.speed));
    }

    private void Close()
    {   
        if(!isOpen || isRotating) return;
        isOpen = false;
        isRotating = true;
        StartCoroutine(RotateLocalZCoroutine(this.lidR, -160f, this.speed));
        StartCoroutine(RotateLocalZCoroutine(this.lidL, 160f, this.speed));
    }

    private void ToggleVfx()
    {
        this.vfx.SetActive(this.isOpen);
    }

    private IEnumerator RotateLocalZCoroutine(Transform target, float angle, float speed)
    {
        float rotated = 0f;
        float direction = Mathf.Sign(angle);
        float targetAmount = Mathf.Abs(angle);
        if(!this.isOpen) this.ToggleVfx();
        while (rotated < targetAmount)
        {
            float step = speed * Time.deltaTime;
            step = Mathf.Min(step, targetAmount - rotated);

            target.Rotate(0f, 0f, step * direction, Space.Self);

            rotated += step;
            yield return null;
        }
        if(this.isOpen) this.ToggleVfx();
        this.isRotating = false;
    }
}