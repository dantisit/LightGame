using System;
using UnityEngine;

namespace Plugins.MVVM.Runtime
{
    public class WindowsManagerComponent : MonoBehaviour
    {
        public void Awake()
        {
            var windows = GetComponentsInChildren<ObserveWindowEvents>(true);
            foreach (var window in windows)
            {
                window.Initialize();
            }
        }
    }
}