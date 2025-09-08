using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public abstract class MonoBehaviourEffect : MonoBehaviour
    {
        [SerializeField] private float duration;
        [SerializeField] private float rate;
        [SerializeField] private float amount;

        private float _currentTime;
        
        private IEnumerator Start()
        {
            while (_currentTime < duration)
            {
                Tick();
                yield return new WaitForSeconds(rate);
                duration += rate;
            }
            
            Destroy(this);
        }

        protected abstract void Tick();
    }
}