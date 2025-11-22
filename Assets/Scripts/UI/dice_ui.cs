using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GameManager gameManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI turnCounterText;
    [SerializeField] private TextMeshProUGUI movementStepsText;
    [SerializeField] private TextMeshProUGUI tileRewardText;
    [SerializeField] private TextMeshProUGUI tilesRemainingText;
    [SerializeField] private TextMeshProUGUI skipBonusText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color bonusColor = Color.yellow;
    [SerializeField] private Color highlightColor = Color.green;

    private void Start()
    {
        if (diceSystem == null)
            diceSystem = FindObjectOfType<DiceSystem>();

        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (diceSystem != null)
        {
            diceSystem.onDiceRolled.AddListener(OnDiceRolled);
            diceSystem.onSkipBonusChanged.AddListener(OnSkipBonusChanged);
        }

        if (turnManager != null)
        {
            turnManager.onTurnStart.AddListener(OnTurnStart);
            turnManager.onPhaseChanged.AddListener(OnPhaseChanged);
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (turnManager != null && turnCounterText != null)
        {
            turnCounterText.text = $"Turn: {turnManager.TurnCounter}";
        }

        if (diceSystem != null)
        {
            if (movementStepsText != null)
            {
                movementStepsText.text = $"Steps: {diceSystem.RolledMovementSteps}";
                movementStepsText.color = diceSystem.RolledMovementSteps > 0 ? highlightColor : normalColor;
            }

            if (tileRewardText != null && diceSystem.HasRolledThisTurn)
            {
                tileRewardText.text = $"Tiles Rolled: {diceSystem.RolledTileReward}";
            }

            if (tilesRemainingText != null && gameManager != null)
            {
                int remaining = gameManager.GetRemainingTilesToPlace();
                tilesRemainingText.text = $"Tiles Left: {remaining}";
                tilesRemainingText.color = remaining > 0 ? highlightColor : normalColor;
            }

            if (skipBonusText != null)
            {
                skipBonusText.text = $"Skip Bonus: +{diceSystem.CurrentSkipBonus}";
                skipBonusText.color = diceSystem.CurrentSkipBonus > 0 ? bonusColor : normalColor;
            }
        }

        if (turnManager != null && instructionsText != null)
        {
            switch (turnManager.CurrentPhase)
            {
                case TurnPhase.WaitingForRoll:
                    instructionsText.text = "[R] Roll Dice | [K] Skip Turn";
                    break;
                case TurnPhase.Moving:
                    string tilesInfo = gameManager != null ?
                        $" | Tiles: {gameManager.GetRemainingTilesToPlace()}" : "";
                    instructionsText.text = $"[WASD] Move | Steps: {diceSystem.RolledMovementSteps}{tilesInfo}";
                    break;
                case TurnPhase.TurnEnd:
                    instructionsText.text = "Turn Ending...";
                    break;
            }
        }
    }

    private void OnDiceRolled(int movement, int tiles)
    {
        UpdateUI();
        Debug.Log($"UI Updated: Movement={movement}, Tiles={tiles}");
    }

    private void OnSkipBonusChanged(int bonus)
    {
        UpdateUI();
        Debug.Log($"UI Updated: Skip Bonus={bonus}");
    }

    private void OnTurnStart()
    {
        UpdateUI();
    }

    private void OnPhaseChanged(TurnPhase phase)
    {
        if (phaseText != null)
        {
            phaseText.text = $"Phase: {phase}";
        }
        UpdateUI();
    }
}