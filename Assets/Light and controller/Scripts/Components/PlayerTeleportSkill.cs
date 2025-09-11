using System;
using UnityEngine;

public class PlayerTeleportSkill : MonoBehaviour
{
    [Serializable]
    public class TeleportData
    {
        public float Radius = 10f;
    }

    public TeleportData teleportData = new TeleportData();

    void Update()
    {
        // Get the center of the screen in world coordinates
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // Get the mouse position in world coordinates
        Vector3 mouseWorldPos = Input.mousePosition;

        // Calculate the direction vector
        Vector2 direction = (mouseWorldPos - screenCenter).normalized;

        // Optional: Visualize the direction
        Debug.DrawRay(transform.position, direction * 5f, Color.green);

        RaycastHit hit;
        //if (Physics2D.Raycast(ray, out hit, teleportData.Radius))
        //{
        //    Debug.Log("Попал в: " + hit.collider.name);
        //}
    }
}