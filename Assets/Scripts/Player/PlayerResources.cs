using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private int startEnergy = 1;
    [SerializeField] private int maxEnergy = 12;

    private int currentEnergy;

    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;

    void Start()
    {
        currentEnergy = startEnergy;
    }

    public bool CanAfford(int cost)
    {
        return currentEnergy >= cost;
    }

    public bool Spend(int cost)
    {
        if (!CanAfford(cost))
            return false;

        currentEnergy -= cost;
        return true;
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }
}
