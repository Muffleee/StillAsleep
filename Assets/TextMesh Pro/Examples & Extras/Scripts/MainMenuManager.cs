using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;




public class MainMenu : MonoBehaviour
{

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private TMP_Text gameTitleText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Button HighScoreReset;

    [SerializeField] private string gameSceneName;

    public static bool tutorial;
    void Start()
    {
        ShowMainMenu();

        if (this.startButton != null)
            startButton.onClick.AddListener(StartGame);
        if(this.optionsButton != null)
            this.optionsButton.onClick.AddListener(ShowOptions);
        if (this.quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
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
        if(this.HighScoreReset != null)
            this.HighScoreReset.onClick.AddListener(delegate {PlayerPrefs.SetInt("Highscore", 0);});

        this.playerAnimator.SetTrigger("TriggerIdle");
    }

    public void ShowMainMenu()
    {
        if(this.mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    private void StartGame()
    {
        MainMenu.tutorial = tutorialToggle.isOn;
        AudioManager.Instance.PlayButtonClick();
        SceneManager.LoadScene(gameSceneName);
    }

    private void ShowOptions()
    {   
        this.mainMenuPanel.SetActive(false);
        this.optionsPanel.SetActive(true);
    }

    private void HideOptions()
    {
        this.optionsPanel.SetActive(false);
        this.mainMenuPanel.SetActive(true);
    }

    private void OnSoundSliderChange()
    {
        AudioManager.SetSoundVolume(this.soundSlider.value);
    }

    private void OnMusicSliderChange()
    {
        AudioManager.SetMusicVolume(this.musicSlider.value);
    }

    private void QuitGame()
    {
        AudioManager.Instance.PlayButtonClick();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}