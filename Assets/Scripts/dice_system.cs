using UnityEngine;
using UnityEngine.Events;

public class DiceSystem : MonoBehaviour
{
    [Header("Dice Settings")]
    [SerializeField] private int minMovementSteps = 1;
    [SerializeField] private int maxMovementSteps = 6;
    [SerializeField] private int minTileReward = 1;
    [SerializeField] private int maxTileReward = 3;

    [Header("Skip Bonus")]
    [SerializeField] private int skipBonusPerTurn = 1;
    [SerializeField] private int maxSkipBonus = 3;

    [Header("Events")]
    public UnityEvent<int, int> onDiceRolled = new UnityEvent<int, int>(); 
    public UnityEvent<int> onSkipBonusChanged = new UnityEvent<int>();

    private int currentSkipBonus = 0;
    private int rolledMovementSteps = 0;
    private int rolledTileReward = 0;
    private bool hasRolledThisTurn = false;

    public int RolledMovementSteps => rolledMovementSteps;
    public int RolledTileReward => rolledTileReward;
    public int CurrentSkipBonus => currentSkipBonus;
    public bool HasRolledThisTurn => hasRolledThisTurn;

    public void RollDice()
    {
        if (hasRolledThisTurn)
        {
            Debug.LogWarning("Already rolled this turn!");
            return;
        }
        int baseMovement = Random.Range(minMovementSteps, maxMovementSteps + 1);
        rolledMovementSteps = Mathf.Min(baseMovement + currentSkipBonus, maxMovementSteps + maxSkipBonus);
        int baseTiles = Random.Range(minTileReward, maxTileReward + 1);
        rolledTileReward = Mathf.Min(baseTiles + currentSkipBonus, maxTileReward + maxSkipBonus);

        hasRolledThisTurn = true;

        currentSkipBonus = 0;
        onSkipBonusChanged?.Invoke(currentSkipBonus);

        onDiceRolled?.Invoke(rolledMovementSteps, rolledTileReward);
        
        Debug.Log($"Dice rolled! Movement: {rolledMovementSteps} steps, Tiles: {rolledTileReward}");
    }

    public void SkipTurn()
    {
        if (hasRolledThisTurn)
        {
            Debug.LogWarning("Cannot skip after rolling!");
            return;
        }

        currentSkipBonus = Mathf.Min(currentSkipBonus + skipBonusPerTurn, maxSkipBonus);
        onSkipBonusChanged?.Invoke(currentSkipBonus);
        
        Debug.Log($"Turn skipped! Skip bonus: {currentSkipBonus}");
        
        EndTurn();
    }

    public bool UseMovementStep()
    {
        if (rolledMovementSteps > 0)
        {
            rolledMovementSteps--;
            Debug.Log($"Movement step used. Remaining: {rolledMovementSteps}");
            return true;
        }
        return false;
    }

    public bool CanMove()
    {
        return hasRolledThisTurn && rolledMovementSteps > 0;
    }

    public void EndTurn()
    {
        rolledMovementSteps = 0;
        hasRolledThisTurn = false;
        Debug.Log("Turn ended. Ready for next turn.");
    }

    public void ForceEndTurn()
    {
        if (rolledMovementSteps <= 0)
        {
            EndTurn();
        }
    }

    public void GetRollPreview(out int minMovement, out int maxMovement, out int minTiles, out int maxTiles)
    {
        minMovement = minMovementSteps + currentSkipBonus;
        maxMovement = Mathf.Min(maxMovementSteps + currentSkipBonus, maxMovementSteps + maxSkipBonus);
        minTiles = minTileReward + currentSkipBonus;
        maxTiles = Mathf.Min(maxTileReward + currentSkipBonus, maxTileReward + maxSkipBonus);
    }
}