using System.Collections.Generic;
using UnityEngine;

namespace Light_and_controller.Scripts
{
    [CreateAssetMenu(fileName = "LevelOrder", menuName = "Game/LevelOrder")]
    public class LevelOrder : ScriptableObject
    {
        public List<SceneName> Value = new();
    }
}