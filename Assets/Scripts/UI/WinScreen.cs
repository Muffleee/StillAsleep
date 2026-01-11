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

    void Start()
    {
        if (this.winScreenPanel != null)
            this.winScreenPanel.SetActive(false);

        if (this.restartButton != null)
            this.restartButton.onClick.AddListener(this.RestartGame);

        if(this.quitButton != null)
            this.quitButton.onClick.AddListener(this.QuitGame);
    }

    public void ShowWinScreen(string message = "You Win!")
    {
        if (this.winScreenPanel != null)
        {
            this.winScreenPanel.SetActive(true);

            if (this.winText != null)
                this.winText.text = message;

            //Pause Game
            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        //Unpause Game
        Time.timeScale = 1f;
        PlayerMovement.INSTANCE.SetCurrentGridPos(new Vector2Int(0, 0));
        PlayerMovement.INSTANCE.SetLastGridPos(new Vector2Int(0, 0));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        //Unpause Game
        Time.timeScale = 1f;

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
