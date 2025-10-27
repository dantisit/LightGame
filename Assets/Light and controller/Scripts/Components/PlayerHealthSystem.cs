using System;
using Light_and_controller.Scripts.Systems;
using R3;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class PlayerHealthSystem : HealthSystem
    {
        private Rigidbody2D rigidbody = null;
        public override void Die()
        {
            rigidbody ??= GetComponent<Rigidbody2D>();
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.position = Checkpoint.Active.transform.position;
            Heal(MaxHealth - Health);
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            EventBus.Publish(new TakeDamageEvent { Amount = amount });
        }

        public class TakeDamageEvent
        {
            public int Amount;
        }
    }
}