using System;
using UIS;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    [RequireComponent(typeof(Scroller))]
    public class ScrollerSnapping : MonoBehaviour
    {
        private Scroller _scroller;
        private ScrollerData _data;
        
        [Header("Snapping Settings")]
        [Tooltip("Enable snapping to items when scrolling stops")]
        public bool EnableSnapping = true;
        
        [Tooltip("Speed of snapping animation (higher = faster)")]
        public float SnapSpeed = 10f;
        
        [Tooltip("Velocity threshold to trigger snapping")]
        public float SnapThreshold = 0.5f;
        
        public Action<int> OnSnapToIndex;
        
        private bool _isSnapping = false;
        private int _targetSnapIndex = -1;
        private float _snapVelocity = 0f;
        private bool _isDragging = false;
        private float _lowVelocityTimer = 0f;
        private const float LOW_VELOCITY_TIMEOUT = 0.2f; // Snap after 0.2s of low velocity
        
        public int CurrentCenterIndex { get; private set; } = -1;
        
        private void Awake()
        {
            _scroller = GetComponent<Scroller>();
            _data = _scroller.Data;
        }
        
        public void Update()
        {
            if (!EnableSnapping || !_data.IsInited || _data.Count == 0) return;
            
            CheckDragState();
            
            if (_isSnapping)
            {
                PerformSnap();
            }
            else if (!_isDragging && ShouldStartSnapping())
            {
                StartSnapping();
            }
            
            UpdateCenterIndex();
            
        }
        
        private void CheckDragState()
        {
            var scrollRect = _data.Scroll;
            var velocityMag = scrollRect.velocity.magnitude;
            _isDragging = velocityMag > SnapThreshold;
            
            if (_isDragging)
            {
                _isSnapping = false;
                _lowVelocityTimer = 0f;
            }
            else
            {
                // Velocity is low, accumulate time
                _lowVelocityTimer += Time.deltaTime;
            }
        }
        
        private bool ShouldStartSnapping()
        {
            // Start snapping if velocity is below threshold AND we've waited long enough
            return _lowVelocityTimer >= LOW_VELOCITY_TIMEOUT;
        }
        
        private void StartSnapping()
        {
            _targetSnapIndex = GetNearestItemIndex();
            _lowVelocityTimer = 0f; // Reset timer
            Debug.Log($"[Snapping] StartSnapping - Target index: {_targetSnapIndex}, Count: {_data.Count}");
            if (_targetSnapIndex >= 0 && _targetSnapIndex < _data.Count)
            {
                _isSnapping = true;
                _snapVelocity = 0f;
            }
        }
        
        private void PerformSnap()
        {
            if (_targetSnapIndex < 0 || _targetSnapIndex >= _data.Count)
            {
                _isSnapping = false;
                return;
            }
            
            var targetPosition = GetTargetPositionForIndex(_targetSnapIndex);
            var currentPosition = _scroller.IsVertical 
                ? _data.Content.anchoredPosition.y 
                : -_data.Content.anchoredPosition.x;
            
            var distance = Mathf.Abs(targetPosition - currentPosition);
            
            if (distance < 5f)
            {
                SetScrollPosition(targetPosition);
                _isSnapping = false;
                _lowVelocityTimer = 0f; // Reset timer to prevent immediate re-snap
                _data.Scroll.velocity = Vector2.zero;
                Debug.Log($"[Snapping] Snap complete to index {_targetSnapIndex}");
                OnSnapToIndex?.Invoke(_targetSnapIndex);
                return;
            }
            
            var smoothPosition = Mathf.SmoothDamp(
                currentPosition, 
                targetPosition, 
                ref _snapVelocity, 
                1f / SnapSpeed
            );
            
            SetScrollPosition(smoothPosition);
        }
        
        private int GetNearestItemIndex()
        {
            var scrollPosition = _scroller.IsVertical 
                ? _data.Content.anchoredPosition.y 
                : -_data.Content.anchoredPosition.x;
            
            var viewportCenter = scrollPosition + GetViewportSize() / 2f;
            
            var nearestIndex = 0;
            var minDistance = float.MaxValue;
            
            // Only check visible views, not all items in the data
            if (_data.Views != null)
            {
                for (int i = 0; i < _data.Views.Length; i++)
                {
                    if (!_data.Views[i].activeSelf) continue;
                    
                    // Get the actual data index from the view name
                    if (!int.TryParse(_data.Views[i].name, out int dataIndex)) continue;
                    if (!_data.Positions.ContainsKey(dataIndex)) continue;
                    
                    var itemCenter = GetItemCenterPosition(dataIndex);
                    var distance = Mathf.Abs(itemCenter - viewportCenter);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestIndex = dataIndex;
                    }
                }
            }
            
            return nearestIndex;
        }
        
        private float GetItemCenterPosition(int index)
        {
            if (!_data.Positions.ContainsKey(index)) return 0f;
            
            var itemPosition = _data.Positions[index];
            var itemSize = _scroller.IsVertical 
                ? (_data.Heights.ContainsKey(index) ? _data.Heights[index] : 0)
                : (_data.Widths.ContainsKey(index) ? _data.Widths[index] : 0);
            
            return Mathf.Abs(itemPosition) + itemSize / 2f;
        }
        
        private float GetTargetPositionForIndex(int index)
        {
            if (!_data.Positions.ContainsKey(index)) return 0f;
            
            var itemPosition = Mathf.Abs(_data.Positions[index]);
            var itemSize = _scroller.IsVertical 
                ? (_data.Heights.ContainsKey(index) ? _data.Heights[index] : 0)
                : (_data.Widths.ContainsKey(index) ? _data.Widths[index] : 0);
            
            var viewportSize = GetViewportSize();
            
            var targetScrollPosition = itemPosition - (viewportSize / 2f) + (itemSize / 2f);
            
            return targetScrollPosition;
        }
        
        private float GetViewportSize()
        {
            return _scroller.IsVertical 
                ? _data.Container.height 
                : _data.Container.width;
        }
        
        private void SetScrollPosition(float position)
        {
            var anchoredPos = _data.Content.anchoredPosition;
            if (_scroller.IsVertical)
            {
                anchoredPos.y = position;
            }
            else
            {
                anchoredPos.x = -position;
            }
            _data.Content.anchoredPosition = anchoredPos;
        }
        
        private void UpdateCenterIndex()
        {
            var newCenterIndex = GetNearestItemIndex();
            if (newCenterIndex != CurrentCenterIndex)
            {
                CurrentCenterIndex = newCenterIndex;
                Debug.Log($"[Snapping] Center changed to item {newCenterIndex}");
            }
        }
        
        public void SnapToIndex(int index, bool immediate = false)
        {
            if (index < 0 || index >= _data.Count) return;
            
            if (immediate)
            {
                var targetPosition = GetTargetPositionForIndex(index);
                SetScrollPosition(targetPosition);
                _data.Scroll.velocity = Vector2.zero;
                CurrentCenterIndex = index;
                OnSnapToIndex?.Invoke(index);
            }
            else
            {
                _targetSnapIndex = index;
                _isSnapping = true;
                _snapVelocity = 0f;
            }
        }
    }
}
