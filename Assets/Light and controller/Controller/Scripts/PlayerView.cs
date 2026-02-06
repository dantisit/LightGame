using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMain playerMain;
    
    [Header("Landing Effect Settings")]
    [SerializeField] private GameObject landingEffectPrefab;
    [SerializeField] private Transform landingSpawnPoint;
    
    [Header("Velocity Scaling")]
    [SerializeField] private bool scaleEffectByVelocity = true;
    [SerializeField] private float minVelocityForEffect = 2f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float velocityScaleMultiplier = 0.1f;

    private PlayerLandState LandState => playerMain?.LandState as PlayerLandState;
    
    private void Awake()
    {
        if (playerMain == null)
            playerMain = GetComponent<PlayerMain>();
    }

    private void OnEnable()
    {
        if (playerMain != null && LandState != null)
        {
            LandState.OnLanded += OnPlayerLanded;
        }
    }

    private void OnDisable()
    {
        if (playerMain != null && LandState != null)
        {
            LandState.OnLanded -= OnPlayerLanded;
        }
    }

    private void OnPlayerLanded(float velocity)
    {
        if (landingEffectPrefab == null)
            return;

        if (velocity < minVelocityForEffect)
            return;

        Vector3 spawnPosition = GetLandingSpawnPosition();
        GameObject effect = Instantiate(landingEffectPrefab, spawnPosition, Quaternion.identity);

        if (scaleEffectByVelocity)
        {
            float scale = CalculateScaleFromVelocity(velocity);
            effect.transform.localScale = Vector3.one * scale;
        }
    }

    private Vector3 GetLandingSpawnPosition()
    {
        if (landingSpawnPoint != null)
        {
            return landingSpawnPoint.position;
        }

        return transform.position;
    }

    private float CalculateScaleFromVelocity(float velocity)
    {
        float normalizedVelocity = velocity * velocityScaleMultiplier;
        return Mathf.Clamp(normalizedVelocity, minScale, maxScale);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 spawnPos = GetLandingSpawnPosition();
        Gizmos.DrawWireSphere(spawnPos, 0.2f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
#endif
}
