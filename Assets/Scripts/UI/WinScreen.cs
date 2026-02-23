using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Security.Cryptography.X509Certificates;

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

    void Start()
    {
        if (this.winScreenPanel != null)
            this.winScreenPanel.SetActive(false);

        if (this.nextRoundPanel != null)
            this.nextRoundPanel.SetActive(false);


        if (this.restartButton != null)
            this.restartButton.onClick.AddListener(this.RestartGame);

        if (this.nextRoundButton != null)
            this.nextRoundButton.onClick.AddListener(this.StartNextRound);

        if (this.quitButton != null)
            this.quitButton.onClick.AddListener(this.QuitGame);

        if (this.nextRoundQuit != null)
            this.nextRoundQuit.onClick.AddListener(this.QuitGame);
    }

    public void ShowWinScreen(string message = "You Win!")
    {
        //if (this.winScreenPanel != null)
        //{
        //    this.winScreenPanel.SetActive(true);

        //    if (this.winText != null)
        //        this.winText.text = message;

        //    //Pause Game
        //    Time.timeScale = 0f;
        //}
        if(this.nextRoundPanel != null)
        {
            this.nextRoundPanel.SetActive(true);
            if (GameManager.INSTANCE.GetRound() % 3 == 0) this.wfcChoice.gameObject.SetActive(false);
            else this.wfcChoice.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    private void StartNextRound()
    {
        WeightType weight = WeightType.NORMAL;
        if (this.wfcChoice.IsActive() == false)
        {
            GameManager.INSTANCE.OnWin(WeightType.START);
        }
        else
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
        //Unpause Game
        Time.timeScale = 1f;
        PlayerMovement.INSTANCE.SetCurrentGridPos(new Vector2Int(0, 0));
        PlayerMovement.INSTANCE.SetLastGridPos(new Vector2Int(0, 0));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        AudioManager.Instance.PlayButtonClick();
        //Unpause Game
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }

    public void HideWinScreen()
    {
        if(this.winScreenPanel != null)
        {
            this.winScreenPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}