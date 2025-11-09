using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Light_and_controller.Scripts.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            newGameButton.onClick.AddListener(OnNewGameButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        private void OnDisable()
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            newGameButton.onClick.RemoveListener(OnNewGameButtonClicked);
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }


        private void OnContinueButtonClicked()
        {
            Debug.Log("Continue button clicked");
        }

        private void OnNewGameButtonClicked()
        {
            SceneLoader.LoadScene(SceneName.LevelTestAll);
            SceneLoader.UnloadScene(SceneName.MainMenu);
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("Settings button clicked");
        }

        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            if (Application.isEditor)
            {
                // Stop the editor
                EditorApplication.isPlaying = false;
                return;
            }
            #endif
            Application.Quit();
        }
    }
}