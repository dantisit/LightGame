using Light_and_controller.Scripts.Systems;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class Weight : MonoBehaviour, IWeight
    {
        [SerializeField] private float weight;

        private void OnEnable()
        {
            EventBus.Subscribe<WeightRequestEvent>(gameObject, OnWeightRequest);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeightRequestEvent>(gameObject, OnWeightRequest);
        }

        private void OnWeightRequest(WeightRequestEvent evt)
        {
            evt.Weight = weight;
        }
        
        public float Get()
        {
            return weight;
        }
    }
}