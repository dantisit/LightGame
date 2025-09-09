using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public interface ILightable : IEventSystemHandler
    {
        public void OnInLightChange(bool isInLight);
    }
}