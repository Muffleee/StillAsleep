using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private int startEnergy = 0;
    [SerializeField] private int maxEnergy = 10;

    private int currentEnergy;

    public int CurrentEnergy => currentEnergy;

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
