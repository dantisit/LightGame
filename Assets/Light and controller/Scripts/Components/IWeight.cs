using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    public interface IWeight : IEventSystemHandler
    {
        public float Get();
    }
}