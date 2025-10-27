using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Light_and_controller.Scripts
{
    public static class SceneLoader
    {
        private static string _currentSceneName = string.Empty;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // if (!IsSceneLoaded(SceneName.Shared))
            // {
            //     SceneManager.LoadScene(SceneName.Shared.KeyToString(), LoadSceneMode.Additive);
            // }
            _currentSceneName = SceneManager.GetActiveScene().name;
            InitializeScene(_currentSceneName);
        }
        
        private static void InitializeScene(string sceneName)
        {
        }

        public static void LoadScene(SceneName sceneName)
        {
            SceneManager.LoadScene(sceneName.KeyToString(), LoadSceneMode.Additive);
            InitializeScene(sceneName.KeyToString());
        }
        
        public static void UnloadScene(SceneName sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName.KeyToString());
        }

        public static bool IsSceneLoaded(SceneName sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName.KeyToString());
            Debug.Log($"Is scene '{sceneName}' loaded? {scene.IsValid() && scene.isLoaded}. Loaded scenes: {SceneManager.loadedSceneCount}");
            return scene.IsValid() && scene.isLoaded;
        }
    }

    public enum SceneName
    {
        Initialization,
        Shared,
        MainMenu,
        Level1,
        LevelTest
    }
}