using Light_and_controller.Scripts.Components;
using UnityEngine;

[RequireComponent(typeof(LightDetector))]
public class PlayerTeleportSystem : MonoBehaviourWithData<PlayerTeleportSkill.TeleportData>, ILightable
{
    private PlayerTeleportSkill TeleportComponent;

    public void OnInLightChange(bool IsInLight)
    {
        if (IsInLight) TeleportOn();
        else TeleportOff();
    }

    public void TeleportOn()
    {
        TeleportComponent = gameObject.AddComponent<PlayerTeleportSkill>();
        TeleportComponent.Data = Data;
    }

    public void TeleportOff()
    {
        Destroy(TeleportComponent);
    }
}
