using System;
using LightGame.Audio;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class SceneRoot : MonoBehaviour
    {
        public static SceneRoot Instance { get; private set; }
        

        private void Start()
        {
            Instance = this;
            ExecuteEvents.ExecuteHierarchy<IInitializable>(gameObject, null, (x, _) => x.Initialize());
            var mainTheme = Addressables.LoadAssetAsync<SoundData>("Sounds/MainTheme").WaitForCompletion();
            if(!SoundManager.IsMusicPlaying()) SoundManager.PlayMusic(mainTheme, 2f);
        }
    }
}