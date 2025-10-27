using System.Collections.Generic;
using System.Linq;
using MVVM;
using MVVM.Binders;
using MVVM.Data;
using MVVM.Utils;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace MVVM.Components
{
    public class GroupDropComponent : MonoBehaviour, IDropHandler, IPointerMoveHandler,  IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private List<DropMethodBinder> drops = new();
        [SerializeField, ViewModelType] private string dropType;

        private View _pendingView;
        private DropMethodBinder _lastNearestDrop;
        
        public void OnDrop(PointerEventData eventData)
        {
            if(_pendingView == null || eventData.used) return;
            
            _lastNearestDrop?.Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDrop));
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _lastNearestDrop = GetNearestDrop(eventData.pointerCurrentRaycast.worldPosition);
            eventData.pointerDrag?.TryGetComponent(out _pendingView);
            
            if (!IsDropValid(_pendingView))
            {
                _pendingView = null;
                return;
            }
            
            _lastNearestDrop?.Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropEnter));
        }

        public void OnPointerExit(PointerEventData _)
        {
            if(_pendingView == null) return;
            
            _lastNearestDrop?.Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropExit));
            _pendingView = null;
        }
        
        public void OnPointerMove(PointerEventData eventData)
        {
            if(_pendingView == null) return;
            
            var nearestDrop = GetNearestDrop(eventData.pointerCurrentRaycast.worldPosition);
            
            if(nearestDrop == _lastNearestDrop) return;
            
            _lastNearestDrop?.Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropExit));
            nearestDrop?.Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropEnter));
            _lastNearestDrop = nearestDrop;
        }

        private bool IsDropValid(View view) => 
            view != null && 
            view.ViewModel != null &&
            view.ViewModel.IsInstanceOf(dropType);
        
        private DropMethodBinder GetNearestDrop(Vector3 position)
        {
            return drops
                .Where(x => x.enabled)
                .OrderBy(slot => (slot.transform.position - position).sqrMagnitude)
                .FirstOrDefault();
        }
    }
}