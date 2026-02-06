using System.Collections.Generic;
using System.Linq;
using MVVM;
using MVVM.Binders;
using MVVM.Data;
using MVVM.Utils;
using Plugins.MVVM.Runtime;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.Components
{
    public class GroupDropComponent : MonoBehaviour, IDropHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private List<DropMethodBinder> drops = new();
        [SerializeField, ViewModelType] private string dropType;
        
        [Header("Snap Settings")]
        [SerializeField] private float snapInThreshold = 1.7f;
        [SerializeField] private float snapOutThreshold = 2f;

        private IView _pendingBinderView;
        private DropMethodBinder _lastNearestDrop;
        private bool _isSnapped;
        
        public void OnDrop(PointerEventData eventData)
        {
            if (_pendingBinderView == null || eventData.used) return;
            
            // Call the bound method on the drop target
            _lastNearestDrop?.Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDrop));
            
            // Fire global drop event for any listeners
            EventBus.Emit(new DropEvent
            {
                DroppedView = _pendingBinderView,
                DropZone = this
            });
            
            _lastNearestDrop = null;
            _pendingBinderView = null;
            _isSnapped = false;
            DragManager.RegisterDropZoneExit(this);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _pendingBinderView = IsDropValid(DragManager.CurrentDragView?.DraggingView) 
                ? DragManager.CurrentDragView?.DraggingView 
                : null;
            
            if (_pendingBinderView == null) return;
            
            EvaluateSnap(eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (_pendingBinderView == null) return;
            
            ClearSnap();
            _pendingBinderView = null;
        }
        
        public void OnPointerMove(PointerEventData eventData)
        {
            if (_pendingBinderView == null) return;
            
            EvaluateSnap(eventData.pointerCurrentRaycast.worldPosition);
        }
        
        private void EvaluateSnap(Vector3 pointerPosition)
        {
            var nearestDrop = GetNearestDrop(pointerPosition);
            if (nearestDrop == null)
            {
                ClearSnap();
                return;
            }
            
            var distance = Vector3.Distance(nearestDrop.transform.position, pointerPosition);
            
            // Hysteresis: use different thresholds based on current snap state
            var threshold = _isSnapped && nearestDrop == _lastNearestDrop 
                ? snapOutThreshold 
                : snapInThreshold;
            
            if (distance < threshold)
            {
                if (nearestDrop != _lastNearestDrop)
                {
                    // Exit previous drop target
                    _lastNearestDrop?.Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDropExit));
                    
                    // Enter new drop target
                    _lastNearestDrop = nearestDrop;
                    _lastNearestDrop.Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDropEnter));
                }
                
                if (!_isSnapped)
                {
                    _isSnapped = true;
                    // First snap - notify enter
                    _lastNearestDrop?.Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDropEnter));
                }
                
                DragManager.RegisterDropZoneEnter(this, nearestDrop, nearestDrop.transform.position);
            }
            else if (_isSnapped)
            {
                ClearSnap();
            }
        }
        
        private void ClearSnap()
        {
            // Notify exit before clearing
            if (_isSnapped && _lastNearestDrop != null && _pendingBinderView != null)
            {
                _lastNearestDrop.Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDropExit));
            }
            
            _isSnapped = false;
            _lastNearestDrop = null;
            DragManager.RegisterDropZoneExit(this);
        }
        
        private bool IsDropValid(IView binderView) => 
            binderView != null && 
            binderView.ViewModel != null &&
            binderView.ViewModel.IsInstanceOf(dropType);
        
        private DropMethodBinder GetNearestDrop(Vector3 position)
        {
            return drops
                .Where(x => x.enabled)
                .OrderBy(slot => (slot.transform.position - position).sqrMagnitude)
                .FirstOrDefault();
        }
    }
}