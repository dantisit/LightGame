using Light_and_controller.Scripts.Components;
using UnityEngine;

[RequireComponent(typeof(LightDetector))]
public class PlayerTeleportSystem : MonoBehaviour, ILightable
{
    private MonoBehaviour TeleportComponent;
    [SerializeField] private PlayerTeleportSkill.TeleportData data;

    public void OnInLightChange(bool IsInLight)
    {
        if (IsInLight) TeleportOn();
        else TeleportOff();
    }

    public void TeleportOn()
    {
        TeleportComponent = gameObject.AddComponent<PlayerTeleportSkill>();
    }

    public void TeleportOff()
    {
        Destroy(TeleportComponent);
    }
}
