using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    
    void Start()
    {
        if (this.pauseMenuPanel != null) this.pauseMenuPanel.SetActive(false);

        if (this.restartButton != null) this.restartButton.onClick.AddListener(this.RestartGame);

        if(this.quitButton != null) this.quitButton.onClick.AddListener(this.QuitGame);

        if(this.resumeButton != null) this.resumeButton.onClick.AddListener(this.HidePauseMenu);
        if(this.optionsButton != null) this.optionsButton.onClick.AddListener(ShowOptions);
        
        if(this.backButton != null)
            this.backButton.onClick.AddListener(HideOptions);
        if(this.soundSlider != null) 
        {
            this.soundSlider.onValueChanged.AddListener(delegate {OnSoundSliderChange();});
            this.soundSlider.value = AudioManager.GetSoundVolume();
        }
        if(this.musicSlider != null) 
        {
            this.musicSlider.onValueChanged.AddListener(delegate {OnMusicSliderChange();});
            this.musicSlider.value = AudioManager.GetMusicVolume();
        }
    }

    public void ShowPauseMenu()
    {
        if (this.pauseMenuPanel != null)
        {
            this.pauseMenuPanel.SetActive(true);

            //Pause Game
            Time.timeScale = 0f;
        }
    }

    public void HidePauseMenu()
    {
        if(this.pauseMenuPanel != null)
        {
            AudioManager.Instance.PlayButtonClick();
            this.pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
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

    public bool IsPauseMenuActive()
    {
        return this.pauseMenuPanel != null && this.pauseMenuPanel.activeSelf;
    }

    private void ShowOptions()
    {   
        this.pauseMenuPanel.SetActive(false);
        this.optionsPanel.SetActive(true);
    }

    private void HideOptions()
    {
        this.optionsPanel.SetActive(false);
        this.pauseMenuPanel.SetActive(true);
    }

    private void OnSoundSliderChange()
    {
        AudioManager.SetSoundVolume(this.soundSlider.value);
    }

    private void OnMusicSliderChange()
    {
        AudioManager.SetMusicVolume(this.musicSlider.value);
    }
}
