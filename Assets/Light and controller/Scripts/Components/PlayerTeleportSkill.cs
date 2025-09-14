using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngineInternal;

public class PlayerTeleportSkill : MonoBehaviourWithData<PlayerTeleportSkill.TeleportData>
{
    [Serializable]
    public class TeleportData
    {
        public float Radius = 25f;
        public GameObject TeleportPoint;
    }

    public ContactFilter2D contactFilter;

    private bool StartLightFiltr;
    private bool IsTeleported;
    
    private Vector2? TargetPosition;


    public void Start()
    { 
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = true;
    }
    public void Update()
    {
        // Get the center of the screen in world coordinates
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // Get the mouse position in world coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Calculate the direction vector
        Vector2 direction = (mouseScreenPosition - screenCenter).normalized;

        // Optional: Visualize the direction
        Debug.DrawRay(transform.position, direction * Data.Radius, Color.green);

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction * Data.Radius);
        foreach (RaycastHit2D hit in hits)
        {
            if (StartLightFiltr && hit.collider?.tag == "Teleport")
            {
                TargetPosition = hit.point;

                Data.TeleportPoint.SetActive(true);
                Data.TeleportPoint.transform.position = TargetPosition.Value;

                IsTeleported = true;
            }

            if (hit.collider?.tag == "Teleport")
                StartLightFiltr = true;
        }

        if (IsTeleported == false)
        {
            TargetPosition = null;

            Data.TeleportPoint.SetActive(false);
        }
        else IsTeleported = false;
        
        StartLightFiltr = false;

        if (Input.GetMouseButton(1)) 
        {
            Teleportation();
        }
    }

    private void Teleportation()
    {
        if (TargetPosition == null) return;

        transform.position = TargetPosition.Value;

        Data.TeleportPoint.SetActive(false);
        TargetPosition = null;
    }
}