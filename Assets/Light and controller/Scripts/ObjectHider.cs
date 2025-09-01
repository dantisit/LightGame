using UnityEngine;

public class ObjectHider : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;
    public bool LightBlockDetected;

    public void LightBlockCheck(Vector3 targetPosition) 
    {
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
        GetComponent<Collider>().enabled = false;
        m_Renderer.enabled = (false);
        Debug.Log(false);
    }

    public void ShowColider() 
    {
        GetComponent<Collider>().enabled = true;
        m_Renderer.enabled = (true);
        Debug.Log(true);
    }
}
