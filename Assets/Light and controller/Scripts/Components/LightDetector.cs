using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Light_and_controller.Scripts.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class LightDetector : MonoBehaviour
   {
       [SerializeField] private UnityEvent<bool> onChangeState = new UnityEvent<bool>();
       [SerializeField] private UnityEvent<bool> onChangeStateInverse = new UnityEvent<bool>();

       public HashSet<GameObject> lightSprings = new();

       // Public accessors for events
       public UnityEvent<bool> OnChangeState => onChangeState;
       public UnityEvent<bool> OnChangeStateInverse => onChangeStateInverse;

       private bool _lastStateIsInLight = true;
       private bool _isInLight;
   
       private void Update()
       {
           _isInLight = lightSprings.Count > 0;
           if(_isInLight == _lastStateIsInLight) return;
           ExecuteEvents.Execute<ILightable>(gameObject, null, (x, _) => x.OnInLightChange(_isInLight));
           onChangeState?.Invoke(_isInLight);
           onChangeStateInverse?.Invoke(!_isInLight);
           _lastStateIsInLight = _isInLight;
       }
       
       public bool LightBlockCheck(Vector3 targetPosition, Rigidbody2D lightRigidbody) 
       {
           Debug.Log(lightRigidbody);
           
           Vector3 direction = targetPosition - transform.position;
   
           var mask = LayerMask.GetMask("LightLayer", "Ground");
           var filter = new ContactFilter2D();
           filter.SetLayerMask(mask);
           filter.useTriggers = false;
           var results = new List<RaycastHit2D>();
           Physics2D.Raycast(transform.position, direction.normalized, filter, results, direction.magnitude);
           var hit = results.FirstOrDefault(x => !x.collider.transform.CompareTag("PassLight"));
           if(hit.collider == null && hit.rigidbody != lightRigidbody) return false;
           Debug.DrawLine(transform.position, hit.transform.position);
           return hit.collider.CompareTag("Light");
       }
   }
}