namespace Light_and_controller.Scripts.Components
{
    public interface ILightable
    {
        public void OnInLightChange(bool isInLight);
    }

    public class LightChangeEvent
    {
        public bool IsInLight { get; set; }
    }
}