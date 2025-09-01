using UnityEngine;
public class CheckShadow : MonoBehaviour
{
    public Light directionalLight; // Assign your main directional light in the inspector
    public bool isInShadow;

    void Update()
    {
        if (directionalLight == null || directionalLight.type != LightType.Directional)
            return;

        // Calculate direction FROM the light TO the object (reverse of light's direction)
        Vector3 lightDirection = -directionalLight.transform.forward;

        // Raycast from the object's position back towards the light source
        if (Physics.Raycast(transform.position, lightDirection, out RaycastHit hitInfo))
        {
            // If the ray hit something, we are in that thing's shadow
            isInShadow = true;
            Debug.Log("Is in shadow!");
        }
        else
        {
            // If the ray did not hit anything, we are directly illuminated
            isInShadow = false;
            Debug.Log("Isnt in shadow!");
        }
    }

    // Optional: Visualize the ray in the Scene view
    void OnDrawGizmos()
    {
        if (directionalLight != null && directionalLight.type == LightType.Directional)
        {
            Gizmos.color = isInShadow ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, -directionalLight.transform.forward * 10f);
        }
    }
}