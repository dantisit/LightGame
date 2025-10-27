using Light_and_controller.Scripts.Components;
using UnityEngine;

[RequireComponent(typeof(LightDetector))]
public class PlayerTeleportSystem : MonoBehaviourWithData<PlayerTeleportSkill.TeleportData>, ILightable
{
    private PlayerTeleportSkill TeleportComponent;

    private void Awake()
    {
        // Pre-create the component but keep it disabled
        TeleportComponent = gameObject.GetComponent<PlayerTeleportSkill>();
        if (TeleportComponent == null)
        {
            TeleportComponent = gameObject.AddComponent<PlayerTeleportSkill>();
        }
        TeleportComponent.Data = Data;
        TeleportComponent.enabled = false;
    }

    public void OnInLightChange(bool IsInLight)
    {
        if (IsInLight) TeleportOn();
        else TeleportOff();
    }

    public void TeleportOn()
    {
        if (TeleportComponent != null)
        {
            TeleportComponent.enabled = true;
        }
    }

    public void TeleportOff()
    {
        if (TeleportComponent != null)
        {
            TeleportComponent.enabled = false;
        }
    }
}
