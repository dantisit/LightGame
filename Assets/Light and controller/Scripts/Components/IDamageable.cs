using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public interface IDamageable : IEventSystemHandler
    {
        public void TakeDamage(float amount);
    }
}