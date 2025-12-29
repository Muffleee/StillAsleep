using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


namespace MainMenu
{
    
    public class MainMenu : MonoBehaviour
    {

        [SerializeField] private GameObject MainMenuPanel;
        [SerializeField] private TMP_Text gameTitleText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Animator playerAnimator;

        [SerializeField] private string gameSceneName;
        void Start()
        {
            ShowMainMenu();

            if (this.startButton != null)
                startButton.onClick.AddListener(StartGame);

            if (this.quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            this.playerAnimator.SetTrigger("TriggerMenu");


        }

        public void ShowMainMenu()
        {
            if(this.MainMenuPanel != null)
            {
                MainMenuPanel.SetActive(true);
            }


        }

        private void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
