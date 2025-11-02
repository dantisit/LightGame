using System.Collections;
using UnityEngine;

namespace Light_and_controller.Scripts.Components.Enemy
{
    /// <summary>
    /// Enemy projectile with cannon-like physics (strong gravity effect)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float gravityScale = 2f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private float rotationSpeed = 180f;
        
        private Vector2 initialVelocity;
        
        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
        }
        
        private void Start()
        {
            // Set gravity scale for cannon-like arc
            rb.gravityScale = gravityScale;
            
            // Destroy after lifetime
            Destroy(gameObject, lifetime);
        }
        
        public void Initialize(Vector2 direction, float speed, float rotSpeed)
        {
            initialVelocity = direction.normalized * speed;
            rb.linearVelocity = initialVelocity;
            rotationSpeed = rotSpeed;
        }
        
        private void FixedUpdate()
        {
            // Apply rotation
            rb.MoveRotation(rb.rotation + rotationSpeed * Time.fixedDeltaTime);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Don't destroy on trigger colliders
            if (other.isTrigger) return;
            
            // Destroy on contact with solid objects
            Destroy(gameObject);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Destroy on collision
            Destroy(gameObject);
        }
    }
}
