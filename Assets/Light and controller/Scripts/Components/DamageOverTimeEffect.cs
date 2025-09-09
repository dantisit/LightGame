using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class DamageOverTimeEffect : MonoBehaviourEffect<DamageOverTimeEffect.EffectData>
    {
        [Serializable]
        public class EffectData : Components.EffectData
        {
            public int Amount;
        }
        
        protected override void Tick()
        {
            ExecuteEvents.Execute<IDamageable>(gameObject, null, (x, _) => x.TakeDamage(Data.Amount));
        }
    }
}