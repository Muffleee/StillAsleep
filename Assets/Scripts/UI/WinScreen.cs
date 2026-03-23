using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject nextRoundPanel;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button nextRoundQuit;
    [SerializeField] private ToggleGroup wfcChoice;
    [SerializeField] private GameObject StatsPanel;

    [SerializeField] private GameObject loseScreenPanel;
    [SerializeField] private TMP_Text loseText;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseQuitButton;
    [SerializeField] private GameObject LoseStatsPanel;

    void Start()
    {
        if (this.winScreenPanel != null)
            this.winScreenPanel.SetActive(false);
        if (this.nextRoundPanel != null)
            this.nextRoundPanel.SetActive(false);
        if (this.loseScreenPanel != null)
            this.loseScreenPanel.SetActive(false);

        if (this.restartButton != null)
            this.restartButton.onClick.AddListener(this.RestartGame);
        if (this.nextRoundButton != null)
            this.nextRoundButton.onClick.AddListener(this.StartNextRound);
        if (this.quitButton != null)
            this.quitButton.onClick.AddListener(this.QuitGame);
        if (this.nextRoundQuit != null)
            this.nextRoundQuit.onClick.AddListener(this.QuitGame);
        if (this.loseRestartButton != null)
            this.loseRestartButton.onClick.AddListener(this.RestartGame);
        if (this.loseQuitButton != null)
            this.loseQuitButton.onClick.AddListener(this.QuitGame);
    }
    public void ShowWinScreen()
    {
        if (GameManager.INSTANCE.IsTutorialCurrently() || GameManager.INSTANCE.GetTutManager().IsInEndphase()) { GameManager.INSTANCE.OnWin(WeightType.NORMAL); return; }
        if (this.nextRoundPanel != null)
        {
            this.nextRoundPanel.SetActive(true);
            if (GameManager.INSTANCE.GetRound() % 3 == 0)
                ShowPhaseStats();
            else
            {
                this.winText.text = "ROUND COMPLETED";
                this.StatsPanel.SetActive(false);
                this.wfcChoice.gameObject.SetActive(true);
            }
        }
        Time.timeScale = 0f;
    }

    public void ShowLoseScreen(string reason = "Game Over!")
    {
        if (this.loseScreenPanel != null)
        {
            this.loseScreenPanel.SetActive(true);

            if (this.loseText != null)
                this.loseText.text = "You lose!\n" + reason;

            if (this.nextRoundPanel != null) this.nextRoundPanel.SetActive(false);
            if (this.winScreenPanel != null) this.winScreenPanel.SetActive(false);

            ShowLoseStats(reason);
        }
        Time.timeScale = 0f;
    }

    private void ShowPhaseStats()
    {
        this.wfcChoice.gameObject.SetActive(false);
        this.StatsPanel.SetActive(true);
        this.winText.text = "PHASE COMPLETED";

        TMP_Text statsText = this.StatsPanel.GetComponentInChildren<TMP_Text>();
        if (statsText != null)
        {
            int phase = GameManager.INSTANCE.GetPhase();
            int score = ScoreManager.INSTANCE.GetScore();
            int highScore = ScoreManager.INSTANCE.GetHighScore();

            statsText.text = "PHASE STATS\n\n";
            statsText.text += "Phase:         " + phase + "\n";
            statsText.text += "Score:         " + score + "\n";
            statsText.text += "Highscore:     " + highScore + "\n";

            if (score >= highScore && score > 0)
                statsText.text += "\nNew Highscore!";
        }
    }
    private void ShowLoseStats(string reason)
    {
        if (this.LoseStatsPanel == null) return;

        this.LoseStatsPanel.SetActive(true);

        TMP_Text statsText = this.LoseStatsPanel.GetComponentInChildren<TMP_Text>();
        if (statsText != null)
        {
            int phase = GameManager.INSTANCE.GetPhase();
            int totalRounds = ((phase - 1) * 3) + GameManager.INSTANCE.GetRound();
            int score = ScoreManager.INSTANCE.GetScore();
            int highScore = ScoreManager.INSTANCE.GetHighScore();

            statsText.text = "GAME OVER\n\n";
            statsText.text += "Cause of Death:     " + reason + "\n\n";
            statsText.text += "Phase achieved:     " + phase + "\n";
            statsText.text += "Total Rounds:    " + totalRounds + "\n";
            statsText.text += "Overall Score:         " + score + "\n";
            statsText.text += "Highscore:           " + highScore + "\n";

            if (score >= highScore && score > 0)
                statsText.text += "\nNew Highscore!";
        }
    }

    private void StartNextRound()
    {
        WeightType weight = WeightType.NORMAL;
        if (this.wfcChoice.IsActive())
        {
            switch (wfcChoice.GetFirstActiveToggle().name)
            {
                case "OPEN": weight = WeightType.OPEN; break;
                case "NORMAL": weight = WeightType.NORMAL; break;
                case "CLOSED": weight = WeightType.CLOSED; break;
                default: weight = WeightType.NORMAL; break;
            }
        }
        if (this.nextRoundPanel != null)
        {
            this.nextRoundPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        GameManager.INSTANCE.OnWin(weight);
    }

    private void RestartGame()
    {
        AudioManager.Instance.PlayButtonClick();
        MainMenu.tutorial = false;
        Time.timeScale = 1f;
        PlayerMovement.INSTANCE.SetCurrentGridPos(new Vector2Int(0, 0));
        PlayerMovement.INSTANCE.SetLastGridPos(new Vector2Int(0, 0));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        AudioManager.Instance.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void HideWinScreen()
    {
        if (this.winScreenPanel != null)
        {
            this.winScreenPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public bool IsWinLoseActive()
    {
        bool winActive = this.winScreenPanel.activeSelf;
        bool loseActive = this.loseScreenPanel.activeSelf;
        bool nextRoundActive = this.nextRoundPanel.activeSelf;
        return winActive || loseActive || nextRoundActive;
    }
}