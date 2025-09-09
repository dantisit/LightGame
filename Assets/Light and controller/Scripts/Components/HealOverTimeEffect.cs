using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class HealOverTimeEffect : MonoBehaviourEffect<HealOverTimeEffect.EffectData>
    {
        [Serializable]
        public class EffectData : Components.EffectData
        {
            public int Amount;
        }
        
        protected override void Tick()
        {
            ExecuteEvents.Execute<IHealable>(gameObject, null, (x, _) => x.Heal(Data.Amount));
        }
    }
}