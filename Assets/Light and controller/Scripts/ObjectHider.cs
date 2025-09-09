using System.Collections.Generic;
using System.Linq;
using Light_and_controller.Scripts.Components;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LightDetector))]
public class ObjectHider : MonoBehaviour, ILightable
{
    [SerializeField] private MeshRenderer m_Renderer;

    private int _originalLayer;
    
    private void Start()
    {
        _originalLayer = gameObject.layer;
    }

    void HideColider() 
    { 
        gameObject.layer = LayerMask.NameToLayer("Hidden");
        m_Renderer.enabled = (false);
    }

    
    public void ShowColider() 
    {
        gameObject.layer = _originalLayer;
        m_Renderer.enabled = (true);
    }

    public void OnInLightChange(bool isInLight)
    {
        if(isInLight) HideColider();
        else ShowColider();
    }
}
