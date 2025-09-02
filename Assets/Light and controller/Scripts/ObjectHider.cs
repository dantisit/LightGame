using Unity.VisualScripting;
using UnityEngine;

public class ObjectHider : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;
    [SerializeField] private Collider m_Collider;
    [SerializeField] private Collider2D m_Collider2D;

    [HideInInspector] public bool LightBlockDetected;

    public void LightBlockCheck(Vector3 targetPosition) 
    {
        Debug.Log(1);

        Vector3 direction = targetPosition - transform.position;

        RaycastHit hit;
        LightBlockDetected = Physics.Raycast(transform.position, direction.normalized, out hit);
        Debug.DrawRay(transform.position, direction);

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
        if (m_Collider != null) 
            m_Collider.enabled = false;
        else
            m_Collider2D.enabled = false;

        m_Renderer.enabled = false;
        Debug.Log(false);
    }

    public void ShowColider() 
    {
        if (m_Collider != null)
            m_Collider.enabled = true;
        else
            m_Collider2D.enabled = true;

        m_Renderer.enabled = true;
        Debug.Log(true);
    }
}
