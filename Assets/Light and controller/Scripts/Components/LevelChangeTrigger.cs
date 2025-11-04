using Light_and_controller.Scripts;
using Light_and_controller.Scripts.Components;
using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using UnityEngine;

[RequireComponent(typeof(LightDetector))]
public class LevelChangeTrigger : MonoBehaviour, ILightable
{
    [SerializeField] private SceneName targetScene;
    
    private LightDetector _lightDetector;
    private bool _wasInLevelChangeLight = false;

    private void Awake()
    {
        _lightDetector = GetComponent<LightDetector>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<LightChangeEvent>(gameObject, OnLightChangeEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<LightChangeEvent>(gameObject, OnLightChangeEvent);
    }

    private void OnLightChangeEvent(LightChangeEvent evt)
    {
        // Only respond to LevelChange light type
        if (!evt.LightType.HasValue || evt.LightType.Value != LightType.LevelChange)
            return;

        OnInLightChange(evt.IsInLight);
    }

    public void OnInLightChange(bool isInLight)
    {
        if (isInLight && !_wasInLevelChangeLight)
        {
            // Entered LevelChange light - trigger scene change
            TriggerLevelChange();
        }
        
        _wasInLevelChangeLight = isInLight;
    }

    private void TriggerLevelChange()
    {
        // Publish event that LevelChangeView can listen to
        EventBus.Publish(new RequestLevelChangeEvent(targetScene));
    }

    /// <summary>
    /// Set the target scene dynamically
    /// </summary>
    public void SetTargetScene(SceneName scene)
    {
        targetScene = scene;
    }
}
