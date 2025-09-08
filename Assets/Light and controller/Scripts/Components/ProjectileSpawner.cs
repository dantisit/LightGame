using System.Collections;
using UnityEngine;

namespace Light_and_controller.Scripts.Components
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private float duration;
        [SerializeField] private float rate;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject spawnPoint;

        private float _currentTime;
        
        private IEnumerator Start()
        {
            while (_currentTime < duration)
            {
                var projectile = Instantiate(projectilePrefab, spawnPoint.transform);
                projectile.transform.SetParent(SceneRoot.Instance.transform);
                yield return new WaitForSeconds(rate);
                duration += rate;
            }
            
            Destroy(this);
        }
    }
}