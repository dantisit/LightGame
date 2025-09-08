using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class WeightTrigger : MonoBehaviour
    {
        [SerializeField] private float weightThreshold;
        [SerializeField] private UnityEvent onActivate;
        [SerializeField] private UnityEvent onDeactivate;
        
        private float _currentWeight;
        private bool _active;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExecuteEvents.Execute<IWeight>(other.gameObject, null, (x, _) => AddWeight(x.Get()));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            ExecuteEvents.Execute<IWeight>(other.gameObject, null, (x, _) => RemoveWeight(x.Get()));
        }

        private void AddWeight(float value)
        {
            _currentWeight += value;
            if (!(_currentWeight >= weightThreshold) || _active) return;
            
            onActivate.Invoke();
            _active = true;
        }

        private void RemoveWeight(float value)
        {
            _currentWeight -= value;
            if (!(_currentWeight < weightThreshold) || !_active) return;
            
            onDeactivate.Invoke();
            _active = false;
        }
    }
}