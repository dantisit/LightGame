using System;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class SceneRoot : MonoBehaviour
    {
        public static SceneRoot Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}