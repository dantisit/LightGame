using System;
using System.Collections.Generic;
using Light_and_controller.Scripts.Systems;
using UnityEngine;
using UnityEngine.Events;

namespace Light_and_controller.Scripts.Components
{
    public class WeightTrigger : MonoBehaviour
    {
        [SerializeField] private float weightThreshold;
        [SerializeField] private List<Togglable> togglables = new List<Togglable>();
        public UnityEvent onActivate = new UnityEvent();
        public UnityEvent onDeactivate = new UnityEvent();
        
        private float _currentWeight;
        private bool _active;

        private HashSet<GameObject> AddedWeights { get; } = new();

        private void Start()
        {
            DeactivateTogglables(); // Initialize
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!AddedWeights.Add(other.gameObject)) return;
            var weightRequest = new WeightRequestEvent();
            EventBus.Publish(other.gameObject, weightRequest);
            if (weightRequest.Weight > 0)
            {
                AddWeight(weightRequest.Weight);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            
            if(!AddedWeights.Remove(other.gameObject)) return;
            var weightRequest = new WeightRequestEvent();
            EventBus.Publish(other.gameObject, weightRequest);
            if (weightRequest.Weight > 0)
            {
                RemoveWeight(weightRequest.Weight);
            }
        }

        private void AddWeight(float value)
        {
            _currentWeight += value;
            if (!(_currentWeight >= weightThreshold) || _active) return;
            
            onActivate.Invoke();
            ActivateTogglables();
            _active = true;
        }

        private void RemoveWeight(float value)
        {
            _currentWeight -= value;
            if (!(_currentWeight < weightThreshold) || !_active) return;
            
            onDeactivate.Invoke();
            DeactivateTogglables();
            _active = false;
        }

        private void ActivateTogglables()
        {
            foreach (var togglable in togglables)
            {
                if (togglable != null)
                {
                    togglable.Enable();
                }
            }
        }

        private void DeactivateTogglables()
        {
            foreach (var togglable in togglables)
            {
                if (togglable != null)
                {
                    togglable.Disable();
                }
            }
        }
    }
}