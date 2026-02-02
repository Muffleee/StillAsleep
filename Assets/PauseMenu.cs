using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    void Start()
    {
        if (this.PauseMenuPanel != null) this.PauseMenuPanel.SetActive(false);

        if (this.restartButton != null) this.restartButton.onClick.AddListener(this.RestartGame);

        if(this.quitButton != null) this.quitButton.onClick.AddListener(this.QuitGame);

        if(this.resumeButton != null) this.resumeButton.onClick.AddListener(this.ResumeGame);
    }

    void Update()
    {
        if (this.PauseMenuPanel.active)
        {
            if(Input.GetKeyDown(KeyCode.Escape)) hidePauseMenu();
        }
    }

    public void ShowPauseMenu()
    {
        if (this.PauseMenuPanel != null)
        {
            this.PauseMenuPanel.SetActive(true);

            //Pause Game
            Time.timeScale = 0f;
        }
    }

    public void hidePauseMenu()
    {
        if(this.PauseMenuPanel != null)
        {
            this.PauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void ResumeGame()
    {
        AudioManager.Instance.PlayButtonClick();
        hidePauseMenu();
    }

    private void RestartGame()
    {
        AudioManager.Instance.PlayButtonClick();
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

        SceneManager.LoadScene("Menu"); // Start Menu
    }
}
