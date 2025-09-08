using System;
using Light_and_controller.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private float damage;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        ExecuteEvents.Execute<IDamageable>(other.gameObject, null, (x, _) => x.TakeDamage(damage));
    }
}
