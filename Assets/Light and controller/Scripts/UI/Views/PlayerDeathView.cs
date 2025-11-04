using System.Collections;
using DG.Tweening;
using Light_and_controller.Scripts;
using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using UnityEngine;

namespace Light_and_controller.Scripts.UI.Views
{
    public class PlayerDeathView : MonoBehaviour
    {
        [Header("Death Fade Settings")]
        [SerializeField] private float deathFadeDuration = 0.5f;
        [SerializeField] private Ease deathFadeEase = Ease.InCubic;
        
        [Header("Respawn Delay")]
        [SerializeField] private float delayAfterFade = 0.1f;

        private bool isHandlingDeath = false;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (!isHandlingDeath)
            {
                StartCoroutine(HandleDeathCoroutine());
            }
        }

        private IEnumerator HandleDeathCoroutine()
        {
            isHandlingDeath = true;

            // Fade all lights to black quickly
            GlobalLightEvents.FadeToBlack(deathFadeDuration, deathFadeEase);

            // Wait for fade to complete
            yield return new WaitForSeconds(deathFadeDuration + delayAfterFade);

            // Reload current level
            var currentLevel = SceneLoader.GetCurrentLevel();
            if (currentLevel.HasValue)
            {
                SceneLoader.LoadLevel(currentLevel.Value);
            }

            // Wait a frame for scene to load
            yield return null;

            // Fade lights back in
            GlobalLightEvents.FadeToFull(deathFadeDuration, deathFadeEase);

            isHandlingDeath = false;
        }
    }
}
