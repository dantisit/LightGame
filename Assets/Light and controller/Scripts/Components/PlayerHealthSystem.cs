using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class PlayerHealthSystem : HealthSystem
    {
        public override void Die()
        {
            transform.position = Checkpoint.Active.transform.position;
            Heal(MaxHealth - Health);
        }
    }
}