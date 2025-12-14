using Unity.Mathematics;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private int startEnergy = 1;
    [SerializeField] private int maxEnergy = 12;

    private int currentEnergy;

    public int CurrentEnergy => this.currentEnergy;
    public int MaxEnergy => this.maxEnergy;

    void Start()
    {
        this.currentEnergy = this.startEnergy;
    }

    public bool CanAfford(int cost)
    {
        return this.currentEnergy >= cost;
    }

    public bool Spend(int cost)
    {
        if (!this.CanAfford(cost))
            return false;

        this.currentEnergy -= cost;
        return true;
    }

    public void RemoveEnergy(int amount)
    {
        this.currentEnergy = Mathf.Max(0, this.currentEnergy - amount);
    }

    public void AddEnergy(int amount)
    {
        this.currentEnergy = Mathf.Min(this.currentEnergy + amount, this.maxEnergy);
    }
}
