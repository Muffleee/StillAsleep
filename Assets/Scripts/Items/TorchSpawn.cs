using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSpawn : MonoBehaviour
{
    private GameObject torch = null;

    public void SpawnTorch()
    {
        this.torch = GameObject.Instantiate(GameManager.INSTANCE.GetPrefabLibrary().GetRandomTorchPrefab(), this.transform.position, this.transform.rotation);
        this.torch.transform.SetParent(this.transform);
    }

    public void DestroyTorch()
    {   
        if(this.torch == null) return;
        GameObject.Destroy(this.torch);
        this.torch = null;
    }

    public void SpawnTorchByChance(float chance)
    {
        if (UnityEngine.Random.value <= chance)
        {
            this.SpawnTorch();
        }
    }

    public bool IsSpawned()
    {
        return this.torch != null;
    }

    void Start()
    {
        this.SpawnTorchByChance(0.05f);
    }
}
