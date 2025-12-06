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
        if (winScreenPanel != null)
            winScreenPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if(quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowWinScreen(string message = "You Win!")
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);

            if (winText != null)
                winText.text = message;

            //Pause Game
            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        //Unpause Game
        Time.timeScale = 1f;
        PlayerMovement.currentGridPos = new Vector2Int(0, 0);
        PlayerMovement.lastGridPos = new Vector2Int(0, 0);
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
        if(winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
