using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.MVVM.Runtime.UIComponents
{
    public class LinearLayoutComponent : MonoBehaviour, IReorderCalculator
    {
        [Header("Spacing Settings")]
        [SerializeField] private float minSpacing = 5f;
        [SerializeField] private float maxSpacing = 300f;
        
        [Header("Direction")]
        [SerializeField] private Vector2 direction = Vector2.right;
        
        [Header("Animation")]
        [SerializeField] private float moveDuration = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool alwaysRecalculate = false;
        
        private RectTransform _rectTransform;
        
        public readonly List<int> MoveExcludedIndices = new();

        public event Action<int, Vector3> OnChildPositionCalculated;
        
        public float CurrentSpacing { get; private set; }
        
        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }
        
        private void Update()
        {
            if (alwaysRecalculate)
            {
                Recalculate();
            }
        }
        
        [ContextMenu("Recalculate")]
        public void Recalculate()
        {
            var children = GetLayoutChildren();
            if (children.Count == 0) return;
            
            var positions = CalculatePositions(children);
            ApplyPositions(children, positions);
        }
        
        public List<Vector3> CalculatePositions(List<RectTransform> children)
        {
            var count = children.Count;
            if (count == 0) return new List<Vector3>();
            
            var availableWidth = _rectTransform.rect.width;
            CurrentSpacing = CalculateSpacing(children, availableWidth);
            
            var normalizedDirection = direction.normalized;
            var totalChildrenWidth = children.Sum(child => child.rect.width);
            var totalSpacingWidth = CurrentSpacing * (count - 1);
            var totalContentWidth = totalChildrenWidth + totalSpacingWidth;
            var startOffset = -totalContentWidth / 2f;
            
            var positions = new List<Vector3>();
            var currentOffset = startOffset;
            
            for (var i = 0; i < count; i++)
            {
                var child = children[i];
                var childCenterOffset = currentOffset + child.rect.width / 2f;
                var position = normalizedDirection * childCenterOffset;
                
                positions.Add(position);
                currentOffset += child.rect.width + CurrentSpacing;
            }
            
            return positions;
        }
        

        private void ApplyPositions(List<RectTransform> children, List<Vector3> positions)
        {
            for (var i = 0; i < children.Count && i < positions.Count; i++)
            {
                OnChildPositionCalculated?.Invoke(i, positions[i]);
                
                // Skip excluded child (e.g., currently being dragged)
                if (MoveExcludedIndices.Contains(i)) continue;
                
                var child = children[i];
                var position = positions[i];
                
                var animatedMovement = child.GetComponent<IAnimatedMovement>();
                if (animatedMovement != null)
                {
                    animatedMovement.MoveToAnchored(position, moveDuration);
                }
                else
                {
                    child.anchoredPosition = position;
                }
            }
        }

        public List<RectTransform> GetLayoutChildren()
        {
            return transform.Cast<Transform>()
                .Select(t => t.GetComponent<RectTransform>())
                .Where(rt => rt != null && rt.gameObject.activeInHierarchy)
                .ToList();
        }
        
        private float CalculateSpacing(List<RectTransform> children, float availableWidth)
        {
            var childCount = children.Count;
            if (childCount <= 1) return 0;
            
            var totalChildrenWidth = children.Sum(child => child.rect.width);
            var remainingWidth = availableWidth - totalChildrenWidth;
            var calculatedSpacing = remainingWidth / (childCount - 1);
            
            return Mathf.Clamp(calculatedSpacing, minSpacing, maxSpacing);
        }
        
        // Returns left edge of the slot at the given index.
        public float GetSlotPosition(int index, int? excludeIndex = null)
        {
            var children = GetLayoutChildren();
            var childCount = children.Count;
            
            if (childCount == 0) return 0;
            
            var spacing = CurrentSpacing > 0 ? CurrentSpacing : CalculateSpacing(children, _rectTransform.rect.width);
            var normalizedDirection = direction.normalized;
            
            var totalChildrenWidth = children.Sum(child => child.rect.width);
            var totalSpacingWidth = spacing * (childCount - 1);
            var totalContentWidth = totalChildrenWidth + totalSpacingWidth;
            var startOffset = -totalContentWidth / 2f;
            
            if (index == 0)
            {
                return startOffset * (normalizedDirection.x != 0 ? normalizedDirection.x : normalizedDirection.y);
            }
            
            var currentOffset = startOffset;
            for (var i = 0; i < index && i < childCount; i++)
            {
                var child = children[i];
                currentOffset += child.rect.width + spacing;
                
                if (excludeIndex.HasValue && i == excludeIndex.Value)
                {
                    currentOffset -= (child.rect.width + spacing);
                }
            }
            
            return currentOffset * (normalizedDirection.x != 0 ? normalizedDirection.x : normalizedDirection.y);
        }
        
        public int CalculateInsertionIndex(
            PointerEventData eventData,
            RectTransform containerRect,
            int draggingIndex)
        {
            var childCount = transform.childCount;
            if (childCount <= 1) return 0;
            
            // Convert pointer position to local space
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
            
            // Find closest insertion slot
            float minDistance = float.MaxValue;
            int closestIndex = 0;
            var children = GetLayoutChildren();
            
            // Use X or Y based on primary direction
            float pointerCoord = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? localPoint.x
                : localPoint.y;
            
            var itemWidth = children[0].rect.width;
            
            for (var i = 0; i <= childCount + 1; i++)
            {
                float itemLeftEdge = GetSlotPosition(i);
                float itemCenter = itemLeftEdge - itemWidth / 2f;
                Debug.Log($"[LinearLayoutComponent] Item {i}: LeftEdge={itemLeftEdge}, Width={itemWidth}, Center={itemCenter}, PointerCoord={pointerCoord}");

                if (!(pointerCoord > itemCenter)) continue;

                if (i == draggingIndex + 1) return draggingIndex;
                if (i > draggingIndex + 1) return i - 1;
                
                return i;
            }
            
            return childCount - 1;
        }
    }
    
    public interface IAnimatedMovement
    {
        void MoveToAnchored(Vector3 position, float duration);
    }
}
