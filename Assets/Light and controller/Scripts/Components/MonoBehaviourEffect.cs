using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public abstract class MonoBehaviourEffect<T> : MonoBehaviourWithData<T> where T : EffectData
    {
        private float _currentTime;
        
        private IEnumerator Start()
        {
            while (_currentTime < Data.Duration || Data.IsInfinity)
            {
                yield return new WaitForSeconds(Data.Rate);
                Tick();
                _currentTime += Data.Rate;
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