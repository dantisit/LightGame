using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Transform targetLightPoint;
    [SerializeField] private Collider2D collider2D;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Handle(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Handle(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;
        var contacts = new List<Collider2D>();
        collider2D.GetContacts(contacts);
        if(contacts.Any(x => x.gameObject == other.gameObject)) return;

        other.GetComponent<ObjectHider>().lightSprings.Remove(gameObject);
    }

    private void Handle(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;

        var component = other.GetComponent<ObjectHider>();
        var is_lighted = component.LightBlockCheck(targetLightPoint.position);
        if (is_lighted) component.lightSprings.Add(gameObject);
        else component.lightSprings.Remove(gameObject);
    }
}
