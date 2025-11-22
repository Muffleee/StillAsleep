using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerMovementAdapter : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private DiceSystem diceSystem;
    
    private PlayerMovement playerMovement;
    private bool originalInputsEnabled = true;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
        
        if (diceSystem == null)
            diceSystem = FindObjectOfType<DiceSystem>();
    }

    private void Update()
    {
        // Block original WASD inputs if dice system is active
        if (turnManager != null && !turnManager.CanPlayerMove())
        {
            BlockMovementInput();
        }
    }

    public bool ShouldBlockMovement()
    {
        if (turnManager == null || diceSystem == null)
            return false;

        return !turnManager.CanPlayerMove();
    }

    private void BlockMovementInput()
    {
        // This prevents PlayerMovement from detecting the input
        // Alternative: modify PlayerMovement.Update() to call ShouldBlockMovement()
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || 
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            if (!turnManager.CanPlayerMove())
            {
                Debug.Log("Cannot move! Roll dice first (R) or Skip turn (K)");
            }
        }
    }
}