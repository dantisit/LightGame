using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class HealOverTimeEffect : MonoBehaviourEffect
    {
        [SerializeField] private float amount;
        
        protected override void Tick()
        {
            ExecuteEvents.Execute<IHealable>(gameObject, null, (x, _) => x.Heal(amount));
        }
    }
}