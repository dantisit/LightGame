using System;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class Checkpoint : MonoBehaviour
    {
        public static Checkpoint Active;

        private void Awake()
        {
            Active = this;
        }
    }
}                     