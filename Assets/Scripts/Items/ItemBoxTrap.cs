using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxTrap : MonoBehaviour
{   
    [SerializeField] Animator animR;
    [SerializeField] Animator animL;
    void Start()
    {
        this.Open();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Open()
    {
        animR.SetTrigger("OpenBox");
        animL.SetTrigger("OpenBox");
    }

    private void Close()
    {
        animR.SetTrigger("CloseBox");
        animL.SetTrigger("CloseBox");
    }
}
