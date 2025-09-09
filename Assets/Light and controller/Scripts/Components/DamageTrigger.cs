using System;
using System.Collections.Generic;
using Light_and_controller.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private int damage;
    
    private HashSet<GameObject> ObjectsInTrigger { get; } = new();
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!ObjectsInTrigger.Add(other.gameObject)) return;
        ExecuteEvents.Execute<IDamageable>(other.gameObject, null, (x, _) => x.TakeDamage(damage));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ObjectsInTrigger.Remove(other.gameObject);
    }
}
