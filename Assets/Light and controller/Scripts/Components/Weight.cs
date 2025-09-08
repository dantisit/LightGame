using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class Weight : MonoBehaviour, IWeight
    {
        [SerializeField] private float weight;
        
        public float Get()
        {
            return weight;
        }
    }
}