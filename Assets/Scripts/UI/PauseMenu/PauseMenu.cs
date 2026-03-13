using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private PauseMenuInventory pauseInventoryUI;
    [SerializeField] private AudioManager AudioManager;

    [SerializeField] private TMPro.TextMeshProUGUI highscoreText;
    [SerializeField] private TMPro.TextMeshProUGUI currentPhaseText;
    [SerializeField] private TMPro.TextMeshProUGUI currentRoundText;

    void Start()
    {
        if (this.pauseMenuPanel != null) this.pauseMenuPanel.SetActive(false);

        if (this.restartButton != null) this.restartButton.onClick.AddListener(this.RestartGame);

        if(this.quitButton != null) this.quitButton.onClick.AddListener(this.QuitGame);

        if(this.resumeButton != null) this.resumeButton.onClick.AddListener(this.HidePauseMenu);
        if(this.optionsButton != null) this.optionsButton.onClick.AddListener(ShowOptions);
        
        if(this.backButton != null) this.backButton.onClick.AddListener(HideOptions);

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
            this.mainPanel.SetActive(true);
            this.optionsPanel.SetActive(false);
            highscoreText.text = "CURRENT\nHIGHSCORE\n" + PlayerPrefs.GetInt("Highscore", 0);
            currentPhaseText.text = "PHASE\n  " + GameManager.INSTANCE.GetPhase();
            currentRoundText.text = "ROUND\n" + (GameManager.INSTANCE.GetRound() + 1)  + " / 3";

            Time.timeScale = 0f;
            pauseInventoryUI.Show();
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

    public bool IsPauseMenuActive()
    {
        return this.pauseMenuPanel != null && this.pauseMenuPanel.activeSelf;
    }

    private void ShowOptions()
    {   
        this.mainPanel.SetActive(false);
        this.optionsPanel.SetActive(true);
    }

    private void HideOptions()
    {
        this.optionsPanel.SetActive(false);
        this.mainPanel.SetActive(true);
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
