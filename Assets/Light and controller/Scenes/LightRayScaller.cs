using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightRayScaller : MonoBehaviour
{
    [SerializeField] private Collider2D collider;
    [SerializeField] private Light2D light;
    [SerializeField] private Vector3[] shape;
    public float speed;
    
    void Update()
    {
        // Изменяем форму света - обе точки по X
        shape[0].x += Time.deltaTime * speed;
        if (shape.Length > 1)
        {
            shape[1].x += Time.deltaTime * speed;
        }
        light.SetShapePath(shape);

        // Обновляем коллайдер
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        if (collider is BoxCollider2D boxCollider)
        {
            // Вычисляем границы формы света
            Vector2 min = shape[0];
            Vector2 max = shape[0];
            
            for (int i = 1; i < shape.Length; i++)
            {
                min = Vector2.Min(min, shape[i]);
                max = Vector2.Max(max, shape[i]);
            }

            // Устанавливаем размер и центр коллайдера
            Vector2 size = max - min;
            boxCollider.size = size;
            boxCollider.offset = min + size / 2f;
        }
    }

    // Для инициализации в редакторе
    private void OnValidate()
    {
        if (shape != null && shape.Length > 0 && light != null)
        {
            UpdateCollider();
        }
    }
}