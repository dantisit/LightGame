using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public class DamageOverTimeEffect : MonoBehaviourEffect
    {
        [SerializeField] private float amount;

        protected override void Tick()
        {
            ExecuteEvents.Execute<IDamageable>(gameObject, null, (x, _) => x.TakeDamage(amount));
        }
    }
}