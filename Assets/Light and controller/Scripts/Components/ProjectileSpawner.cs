using System.Collections;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private float rate;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform directionPoint;
        [SerializeField] private float speed;
        
        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(rate);
                
                var instantiate = Instantiate(projectilePrefab, transform);
                var projectile = instantiate.GetComponent<Projectile>();
                projectile.transform.SetParent(SceneRoot.Instance.transform, true);
                projectile.Direction = (directionPoint.position - transform.position).normalized;
                projectile.Speed = speed;
            }
        }
    }
}