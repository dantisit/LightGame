using System.Collections.Generic;
using UnityEngine;

namespace LightGame.Audio
{
    /// <summary>
    /// Static sound manager that handles sound playback without requiring a GameObject.
    /// Automatically manages sound player instances and pooling.
    /// </summary>
    public static class SoundManager
    {
        private static int maxConcurrentSounds = 10;
        private static Queue<UnitySoundPlayer> availablePlayers = new Queue<UnitySoundPlayer>();
        private static List<UnitySoundPlayer> activePlayers = new List<UnitySoundPlayer>();
        private static GameObject soundManagerRoot;
        private static bool isInitialized = false;

        /// <summary>
        /// Sets the maximum number of concurrent sounds that can play.
        /// </summary>
        public static void SetMaxConcurrentSounds(int max)
        {
            maxConcurrentSounds = max;
        }

        /// <summary>
        /// Initializes the sound manager. Called automatically on first use.
        /// </summary>
        private static void Initialize()
        {
            if (isInitialized)
                return;

            soundManagerRoot = new GameObject("SoundManager");
            Object.DontDestroyOnLoad(soundManagerRoot);

            // Pre-create sound player pool
            for (int i = 0; i < maxConcurrentSounds; i++)
            {
                CreateSoundPlayer();
            }

            isInitialized = true;
        }

        private static UnitySoundPlayer CreateSoundPlayer()
        {
            if (soundManagerRoot == null)
            {
                soundManagerRoot = new GameObject("SoundManager");
                Object.DontDestroyOnLoad(soundManagerRoot);
            }

            GameObject playerObj = new GameObject($"SoundPlayer_{availablePlayers.Count}");
            playerObj.transform.SetParent(soundManagerRoot.transform);

            UnitySoundPlayer player = playerObj.AddComponent<UnitySoundPlayer>();
            availablePlayers.Enqueue(player);

            return player;
        }

        /// <summary>
        /// Plays a sound using an available sound player.
        /// </summary>
        /// <param name="soundData">The sound to play</param>
        /// <param name="position">Optional world position for 3D sounds</param>
        public static void PlaySound(SoundData soundData, Vector3? position = null)
        {
            if (!isInitialized)
                Initialize();

            if (soundData == null)
            {
                Debug.LogWarning("Cannot play sound: SoundData is null");
                return;
            }

            UnitySoundPlayer player = GetAvailablePlayer();
            if (player == null)
            {
                Debug.LogWarning("No available sound players. Consider increasing maxConcurrentSounds.");
                return;
            }

            activePlayers.Add(player);
            soundData.PlayInternal(player, position);

            // Start monitoring for when sound ends
            CoroutineRunner.Instance.StartCoroutine(ReturnPlayerWhenFinished(player, soundData));
        }

        /// <summary>
        /// Plays a sound at a specific position in 3D space.
        /// </summary>
        public static void PlaySoundAtPosition(SoundData soundData, Vector3 position)
        {
            PlaySound(soundData, position);
        }

        /// <summary>
        /// Plays a 2D sound (no spatial audio).
        /// </summary>
        public static void PlaySound2D(SoundData soundData)
        {
            PlaySound(soundData, null);
        }

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        public static void StopAllSounds()
        {
            if (!isInitialized)
                return;

            foreach (var player in activePlayers)
            {
                if (player != null)
                    player.Stop();
            }

            // Move all active players back to available pool
            while (activePlayers.Count > 0)
            {
                availablePlayers.Enqueue(activePlayers[0]);
                activePlayers.RemoveAt(0);
            }
        }

        private static UnitySoundPlayer GetAvailablePlayer()
        {
            // Clean up any players that finished playing
            for (int i = activePlayers.Count - 1; i >= 0; i--)
            {
                if (activePlayers[i] != null && !activePlayers[i].IsPlaying())
                {
                    availablePlayers.Enqueue(activePlayers[i]);
                    activePlayers.RemoveAt(i);
                }
            }

            if (availablePlayers.Count > 0)
            {
                return availablePlayers.Dequeue();
            }

            return null;
        }

        private static System.Collections.IEnumerator ReturnPlayerWhenFinished(UnitySoundPlayer player, SoundData soundData)
        {
            // Wait until the sound is no longer playing
            while (player != null && player.IsPlaying())
            {
                yield return null;
            }

            // Return player to available pool
            if (activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                availablePlayers.Enqueue(player);
            }
        }

        /// <summary>
        /// Helper class to run coroutines for the static SoundManager.
        /// </summary>
        private class CoroutineRunner : MonoBehaviour
        {
            private static CoroutineRunner instance;
            public static CoroutineRunner Instance
            {
                get
                {
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SoundManager_CoroutineRunner");
                        instance = go.AddComponent<CoroutineRunner>();
                        Object.DontDestroyOnLoad(go);
                    }
                    return instance;
                }
            }
        }
    }
}
