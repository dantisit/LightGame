using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightRayScaller : MonoBehaviour
{
    [SerializeField] private Light2D Light;
    [SerializeField] private Vector3[] Shape;
    public float speed;
    void Update()
    {
        Debug.Log(Time.deltaTime * speed * Shape[0].x);
        Shape[0].x += Time.deltaTime * speed;
        Light.SetShapePath(Shape);
    }
}
