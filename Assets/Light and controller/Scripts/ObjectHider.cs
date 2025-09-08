using System.Collections.Generic;
using System.Linq;
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
        var filter = new ContactFilter2D();
        filter.SetLayerMask(mask);
        var results = new List<RaycastHit2D>();
        Physics2D.Raycast(transform.position, direction.normalized, filter, results, direction.magnitude);
        var hit = results.FirstOrDefault(x => !x.transform.CompareTag("PassLight"));
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
