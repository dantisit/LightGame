using System;
using System.Collections;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;

        public Vector2 Direction { get; set; }
        public float Speed { get; set; }

        private void FixedUpdate()
        {
            rb.MovePosition(rb.position + Direction * (Speed * Time.fixedDeltaTime));
        }

        private IEnumerator OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger) yield break;
            yield return new WaitForFixedUpdate();
            Destroy(gameObject);
        }
    }
}