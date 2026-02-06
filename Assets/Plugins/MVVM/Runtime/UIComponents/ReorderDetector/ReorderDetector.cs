using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.MVVM.Runtime.UIComponents
{
    public class ReorderDetector : MonoBehaviour, IPointerMoveHandler
    {
        [Header("Settings")]
        [SerializeField] private bool enableReordering = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        private RectTransform _rectTransform;
        private int _draggingChildIndex = -1;
        private IReorderCalculator _calculator;
        
        public event Action<Transform, int, int> OnReorderRequested;
        
        public bool EnableReordering
        {
            get => enableReordering;
            set => enableReordering = value;
        }
        
        public bool IsDragging => _draggingChildIndex >= 0;
        public int DraggingChildIndex => _draggingChildIndex;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError($"[ReorderDetector] RectTransform not found on {gameObject.name}");
            }
            
            _calculator = GetComponent<IReorderCalculator>();
        }
        

        public void SetCalculator(IReorderCalculator calculator)
        {
            _calculator = calculator;
        }
        

        public void StartDragging(int childIndex)
        {
            if (childIndex < 0 || childIndex >= transform.childCount)
            {
                Debug.LogWarning($"[ReorderDetector] Invalid child index: {childIndex}");
                return;
            }
            
            _draggingChildIndex = childIndex;
            
            if (showDebugInfo)
            {
                Debug.Log($"[ReorderDetector] Started dragging child {childIndex}");
            }
        }
        
        public void StopDragging()
        {
            if (showDebugInfo && _draggingChildIndex >= 0)
            {
                Debug.Log($"[ReorderDetector] Stopped dragging child {_draggingChildIndex}");
            }
            
            _draggingChildIndex = -1;
        }
        
        public void OnPointerMove(PointerEventData eventData)
        {
            if (!enableReordering || _draggingChildIndex < 0 || _calculator == null)
                return;
            
            if (_draggingChildIndex >= transform.childCount)
                return;
            
            var draggedChild = transform.GetChild(_draggingChildIndex);
            var targetIndex = _calculator.CalculateInsertionIndex(
                eventData,
                _rectTransform,
                _draggingChildIndex
            );
            
            // Only trigger reorder if target index is different
            if (targetIndex != _draggingChildIndex)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[ReorderDetector] Reorder requested: {_draggingChildIndex} → {targetIndex}");
                }
                
                OnReorderRequested?.Invoke(draggedChild, _draggingChildIndex, targetIndex);
            }
        }
    }
    
    /// <summary>
    /// Interface for calculating insertion index based on pointer position.
    /// Implement this to provide custom reorder logic for different layouts.
    /// </summary>
    public interface IReorderCalculator
    {
        /// <summary>
        /// Calculate the target insertion index based on pointer position
        /// </summary>
        /// <param name="eventData">Pointer event data</param>
        /// <param name="container">Container RectTransform</param>
        /// <param name="draggingIndex">Current index of dragged child</param>
        /// <returns>Target index for insertion</returns>
        int CalculateInsertionIndex(
            PointerEventData eventData,
            RectTransform container,
            int draggingIndex
        );
    }
}
