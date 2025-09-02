using System.Collections.Generic;
using UnityEngine;

public class ObjectHider : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;

    [HideInInspector] public bool LightBlockDetected;

    public Dictionary<GameObject, bool> lightSprings = new Dictionary<GameObject, bool>();

    private void Start()
    {
       //var LightSprings = new List<GameObject>();
    }

    private void Update()
    {
        if (lightSprings.Count > 0)
        {
            HideColider();
        }
        else
        {
            ShowColider();
        }
    }
    public bool LightBlockCheck(Vector3 targetPosition) 
    {
        Vector3 direction = targetPosition - transform.position;

        var mask = LayerMask.GetMask("LightLayer", "Ground");
        var hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, mask);
        if(hit.collider == null) return false;
        Debug.DrawLine(transform.position, hit.transform.position);
        return hit.collider.CompareTag("Light");
    }
    void HideColider() 
    { 
        gameObject.layer = LayerMask.NameToLayer("Hidden");
        m_Renderer.enabled = (false);
    }

    
    public void ShowColider() 
    {
        gameObject.layer = LayerMask.NameToLayer("HiddenGround");
        m_Renderer.enabled = (true);
    }
}
