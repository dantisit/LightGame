using System;
using UnityEngine;
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
        }
    }
}