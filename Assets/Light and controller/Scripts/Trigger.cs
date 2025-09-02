using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Transform targetLightPoint;

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

        other.GetComponent<ObjectHider>().lightSprings.Remove(gameObject);
    }

    private void Handle(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;

        var component = other.GetComponent<ObjectHider>();
        var is_lighted = component.LightBlockCheck(targetLightPoint.position);
        if (is_lighted) component.lightSprings.TryAdd(gameObject, true);
        else component.lightSprings.Remove(gameObject);
    }
}
