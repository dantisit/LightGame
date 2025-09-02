using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Transform targetLightPoint;
    /**
    
    private void OnTriggerStay (Collider other)
    {
        if (!other.CompareTag("HideObject")) return; 
       
        other.GetComponent<ObjectHider>().LightBlockCheck(targetLightPoint.position);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("HideObject")) return;
        
        other.GetComponent<ObjectHider>().ShowColider();
    }

   **/

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;

        other.GetComponent<ObjectHider>().LightBlockCheck(targetLightPoint.position);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;

        other.GetComponent<ObjectHider>().LightBlockCheck(targetLightPoint.position);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("HideObject")) return;

        other.GetComponent<ObjectHider>().ShowColider();
    }
}
