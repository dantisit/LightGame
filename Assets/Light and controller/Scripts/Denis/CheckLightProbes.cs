using UnityEngine;
using UnityEngine.Rendering;

// Attach this to your object that needs to check its own illumination
public class CheckLightProbes : MonoBehaviour
{
    public float currentIntensity;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Get the light probe data at this object's position
        SphericalHarmonicsL2 lightProbe;
        LightProbes.GetInterpolatedProbe(transform.position, _renderer, out lightProbe);

        // Evaluate the light probe's effect on a neutral color (white)
        // This gives an approximation of how bright the light is at this point
        Vector3[] directions = new Vector3[] { Vector3.up };
        Color[] results = new Color[1];
        lightProbe.Evaluate(directions, results);

        // The 'r' channel of the result is a good proxy for overall intensity
        currentIntensity = results[0].r;

        // Now you can set a threshold:
        if (currentIntensity > 0.5f)
        {
            Debug.Log("Object in light!");
        }
        else
        {
            Debug.Log("Object in shadow!");
            // Object is in shadow/darkness
        }
    }
}