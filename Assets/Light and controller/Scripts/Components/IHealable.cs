using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public interface IHealable : IEventSystemHandler
    {
        public void Heal(float amount);
    }
}