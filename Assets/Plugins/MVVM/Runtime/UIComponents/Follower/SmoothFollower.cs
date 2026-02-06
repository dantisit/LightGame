using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents
{
    public class SmoothFollower : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField, Tooltip("Transform that will follow the target (if null, uses this transform)")]
        private Transform followerTransform;

        [SerializeField, Range(0.01f, 1f), Tooltip("Speed of position interpolation")]
        private float positionLerpSpeed = 0.08f;
        
        [SerializeField, Tooltip("Enable rotation based on movement delta")]
        private bool enableRotation = true;
        
        [SerializeField, Range(0f, 100f), Tooltip("Multiplier for rotation based on X delta")]
        private float rotationMultiplier = 35f;
        
        [Header("Offset Settings")]
        [SerializeField, Tooltip("Transform to apply offset to (optional)")]
        private Transform offsetTransform;
        
        [SerializeField, Range(0f, 100f), Tooltip("Maximum offset distance")]
        private float maxOffsetDistance = 20f;
        
        [SerializeField, Range(0f, 100f), Tooltip("Multiplier for offset based on delta")]
        private float offsetMultiplier = 35f;
        
        public Transform FollowerTransform => followerTransform ?? transform;
        
        public float FollowerLerpSpeed
        {
            get => positionLerpSpeed;
            set => positionLerpSpeed = value;
        }
        public float RotationMultiplier
        {
            get => rotationMultiplier;
            set => rotationMultiplier = value;
        }
        
        public Transform FollowTarget { get; set; }
        public Vector3? ManualTarget { get; set; }
        public bool IsFollowing => FollowTarget != null || ManualTarget.HasValue;

        public void StartFollowing(Transform target)
        {
            FollowTarget = target;
        }
        
        public void StopFollowing()
        {
            FollowTarget = null;
            ManualTarget = null;
        }

        private void FollowPosition(Vector3 targetPosition, float? customLerpSpeed = null)
        {
            var lerpSpeed = customLerpSpeed ?? positionLerpSpeed;
            
            var transformToMove = FollowerTransform;

            // Smooth position interpolation
            transformToMove.position = Vector3.Lerp(transformToMove.position, targetPosition, lerpSpeed);
            
            // Calculate delta for rotation/offset
            var deltaX = transformToMove.position.x - targetPosition.x;

            // Apply rotation if enabled
            if (enableRotation)
            {
                var rotationZ = deltaX * rotationMultiplier;
                transformToMove.eulerAngles = new Vector3(
                    transformToMove.eulerAngles.x,
                    transformToMove.eulerAngles.y,
                    rotationZ
                );
            }
            
            // Apply offset if transform is assigned
            if (offsetTransform != null)
            {
                var offsetX = Mathf.Clamp(
                    deltaX * offsetMultiplier,
                    -maxOffsetDistance,
                    maxOffsetDistance
                );
                offsetTransform.localPosition = new Vector3(offsetX, 0, 0);
            }
        }
        
        private void Update()
        {
            if (ManualTarget.HasValue)
            {
                FollowPosition(ManualTarget.Value);
                return;
            }
            
            if (!FollowTarget)
                return;
            
            FollowPosition(FollowTarget.position);
        }
    }
}
