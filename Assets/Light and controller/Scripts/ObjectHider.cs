using UnityEngine;

public class ObjectHider : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;
    [HideInInspector] public bool LightBlockDetected;

    public void LightBlockCheck(Vector3 targetPosition) 
    {
        Debug.Log(1);

        Vector3 direction = targetPosition - transform.position;

        var mask = LayerMask.GetMask("LightLayer", "Ground");
        var hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, mask);
        if(hit.collider == null) return;
        Debug.DrawLine(transform.position, hit.transform.position);
        Debug.Log($"{hit.transform.gameObject.name} on layer {hit.transform.gameObject.layer}");
        if (hit.collider.CompareTag("Light"))
        {
            HideColider();
        }

        else 
        { 
            ShowColider();
        }
    }
    void HideColider() 
    { 
        gameObject.layer = LayerMask.NameToLayer("Hidden");
        m_Renderer.enabled = (false);
        Debug.Log(false);
    }

    
    public void ShowColider() 
    {
        gameObject.layer = LayerMask.NameToLayer("HiddenGround");
        m_Renderer.enabled = (true);
        Debug.Log(true);
    }
}
