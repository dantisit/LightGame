using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public abstract class MonoBehaviourEffect<T> : MonoBehaviour where T : EffectData
    {
        [SerializeField] private T data;

        public T Data
        {
            get => data;
            set => data = value;
        }
        
        private float _currentTime;
        
        private IEnumerator Start()
        {
            while (_currentTime < data.Duration || data.IsInfinity)
            {
                yield return new WaitForSeconds(data.Rate);
                Tick();
                _currentTime += data.Rate;
            }
            
            Destroy(this);
        }

        protected abstract void Tick();
    }
    
    [Serializable]
    public class EffectData
    {
        public float Duration;
        public float Rate;
        public bool IsInfinity;
    }
}