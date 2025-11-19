using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameManager gameManager;

    [Header("Events")]
    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnEnd = new UnityEvent();
    public UnityEvent<TurnPhase> onPhaseChanged = new UnityEvent<TurnPhase>();

    private TurnPhase currentPhase = TurnPhase.WaitingForRoll;
    private int turnCounter = 0;

    public TurnPhase CurrentPhase => currentPhase;
    public int TurnCounter => turnCounter;

    private void Start()
    {
        if (diceSystem == null)
            diceSystem = FindObjectOfType<DiceSystem>();
        
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        diceSystem.onDiceRolled.AddListener(OnDiceRolled);

        if (playerMovement != null)
        {
            playerMovement.onPlayerMoved.AddListener(OnPlayerMoved);
        }
        StartNewTurn();
    }

    private void StartNewTurn()
    {
        turnCounter++;
        currentPhase = TurnPhase.WaitingForRoll;
        onTurnStart?.Invoke();
        onPhaseChanged?.Invoke(currentPhase);
        
        Debug.Log($"=== Turn {turnCounter} Started ===");
        Debug.Log("Press R to Roll or Press K to Skip and gain bonus");
    }

    private void Update()
    {
        switch (currentPhase)
        {
            case TurnPhase.WaitingForRoll:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    diceSystem.RollDice();
                }
                else if (Input.GetKeyDown(KeyCode.K))
                {
                    SkipTurn();
                }
                break;

            case TurnPhase.Moving:
                if (!diceSystem.CanMove())
                {
                    EndCurrentTurn();
                }
                break;
        }
    }

    private void OnDiceRolled(int movement, int tiles)
    {
        currentPhase = TurnPhase.Moving;
        onPhaseChanged?.Invoke(currentPhase);

        if (gameManager != null)
        {
            Debug.Log($"Player awarded {tiles} tiles");
        }
        Debug.Log($"Movement phase: You have {movement} steps to move");
    }

    private void OnPlayerMoved(Vector2Int lastPos, Vector2Int currentPos, WallPos direction, long stepCount)
    {
        if (currentPhase == TurnPhase.Moving)
        {
            diceSystem.UseMovementStep();
            diceSystem.ForceEndTurn();
        }
    }

    private void SkipTurn()
    {
        if (currentPhase != TurnPhase.WaitingForRoll)
            return;

        diceSystem.SkipTurn();
        EndCurrentTurn();
    }

    private void EndCurrentTurn()
    {
        currentPhase = TurnPhase.TurnEnd;
        onPhaseChanged?.Invoke(currentPhase);
        onTurnEnd?.Invoke();

        Debug.Log($"=== Turn {turnCounter} Ended ===\n");
        Invoke(nameof(StartNewTurn), 0.5f);
    }

    public bool CanPlayerMove()
    {
        return currentPhase == TurnPhase.Moving && diceSystem.CanMove();
    }
}

public enum TurnPhase
{
    WaitingForRoll,  
    Moving,          
    TurnEnd          
}