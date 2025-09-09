using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ObjectHider : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;
    [SerializeField] private UnityEvent<bool> onChangeState;

    [HideInInspector] public bool LightBlockDetected;

    public HashSet<GameObject> lightSprings = new();

    private int _originalLayer;
    
    private void Start()
    {
        _originalLayer = gameObject.layer;
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
        var filter = new ContactFilter2D();
        filter.SetLayerMask(mask);
        var results = new List<RaycastHit2D>();
        Physics2D.Raycast(transform.position, direction.normalized, filter, results, direction.magnitude);
        var hit = results.FirstOrDefault(x => !x.collider.transform.CompareTag("PassLight"));
        if(hit.collider == null) return false;
        Debug.DrawLine(transform.position, hit.transform.position);
        return hit.collider.CompareTag("Light");
    }
    void HideColider() 
    { 
        gameObject.layer = LayerMask.NameToLayer("Hidden");
        m_Renderer.enabled = (false);
        onChangeState.Invoke(false);
    }

    
    public void ShowColider() 
    {
        gameObject.layer = _originalLayer;
        m_Renderer.enabled = (true);
        onChangeState.Invoke(true);
    }
}
