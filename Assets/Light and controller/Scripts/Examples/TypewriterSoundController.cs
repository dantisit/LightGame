using EasyTextEffects;
using LightGame.Audio;
using UnityEngine;

namespace Light_and_controller.Scripts.Examples
{
    /// <summary>
    /// Plays typewriter sound effects for each character as it appears
    /// </summary>
    public class TypewriterSoundController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextEffect textEffect;
        
        [Header("Sound Settings")]
        [SerializeField] private SoundData typewriterSound;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField] private float pitchMin = 0.95f;
        [SerializeField] private float pitchMax = 1.05f;
        
        [Header("Optional: Skip Spaces")]
        [SerializeField] private bool skipSpaces = true;

        private System.Collections.Generic.HashSet<int> playedCharacters = new System.Collections.Generic.HashSet<int>();

        private void OnEnable()
        {
            if (textEffect != null)
            {
                textEffect.OnCharacterStart += OnCharacterTyped;
            }
        }

        private void OnDisable()
        {
            if (textEffect != null)
            {
                textEffect.OnCharacterStart -= OnCharacterTyped;
            }
        }

        private void OnCharacterTyped(int charIndex)
        {
            Debug.Log($"OnCharacterTyped called for character index: {charIndex}");
            
            // Optional: Skip spaces and punctuation
            if (skipSpaces && textEffect.text != null)
            {
                if (charIndex < textEffect.text.text.Length)
                {
                    char c = textEffect.text.text[charIndex];
                    if (char.IsWhiteSpace(c))
                    {
                        Debug.Log($"Skipping whitespace at index {charIndex}");
                        return;
                    }
                }
            }

            // Play typewriter sound
            Debug.Log($"Playing sound for character {charIndex}");
            PlayTypewriterSound();
        }

        private void PlayTypewriterSound()
        {
            if (typewriterSound == null)
            {
                Debug.LogWarning("Typewriter sound not assigned!");
                return;
            }

            if (randomizePitch)
            {
                float pitch = Random.Range(pitchMin, pitchMax);
                typewriterSound.Pitch = pitch;
                SoundManager.PlaySound(typewriterSound);
            }
            else
            {
                SoundManager.PlaySound(typewriterSound);
            }
        }
    }
}
