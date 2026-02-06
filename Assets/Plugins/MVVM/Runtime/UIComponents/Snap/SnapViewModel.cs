using System;
using MVVM;
using R3;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Behaviors
{
    public class SnapViewModel : ViewModel
    {
        public ReactiveProperty<bool> EnableSnapping { get; } = new(true);
        public ReactiveProperty<bool> IsSnapped { get; } = new();
        public ReactiveProperty<Vector3?> SnapTarget { get; } = new();
        
        [Header("Snap Thresholds")]
        public ReactiveProperty<float> SnapInThreshold { get; } = new(1.5f);
        public ReactiveProperty<float> SnapOutThreshold { get; } = new(2f);
        
        
        public ReactiveProperty<Vector3?> SnapTargetSource { get; set; }
        
        
        public void EvaluateSnapDuringDrag(Vector3 currentPosition)
        {
            if (SnapTargetSource != null)
            {
                EvaluateSnap(currentPosition, SnapTargetSource.Value);
            }
        }
        
        public void EvaluateSnap(Vector2 objectPosition, Vector2? targetPosition)
        {
            Debug.Log($"EvaluateSnap: {objectPosition} {targetPosition}");
            
            if (!EnableSnapping.Value)
            {
                UpdateSnapState(false, null);
                return;
            }
            
            if (!targetPosition.HasValue)
            {
                UpdateSnapState(false, null);
                return;
            }
            
            var snapPos = targetPosition.Value;
            var objPos2D = new Vector2(objectPosition.x, objectPosition.y);
            var snapPos2D = new Vector2(snapPos.x, snapPos.y);
            var distance = Vector2.Distance(objPos2D, snapPos2D);
            
            // Use hysteresis to prevent flickering
            var threshold = IsSnapped.Value ? SnapOutThreshold.Value : SnapInThreshold.Value;
            var shouldSnap = distance < threshold;
            
            UpdateSnapState(shouldSnap, shouldSnap ? snapPos : null);
        }
        
        public void SetSnapState(bool isSnapped, Vector3? snapPosition = null)
        {
            UpdateSnapState(isSnapped, snapPosition);
        }
        
        public void ClearSnap()
        {
            UpdateSnapState(false, null);
        }
        
        private void UpdateSnapState(bool isSnapped, Vector3? snapPosition)
        {
            IsSnapped.Value = isSnapped;
            SnapTarget.Value = snapPosition;
        }
    }
}
