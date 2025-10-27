using DG.Tweening;
using MVVM.Data;
using MVVM.Utils;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.Binders
{
    public class DropMethodBinder : GenericMethodBinder<DropEventData>, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ViewModelType] private string dropType;
        
        private View _pendingView;
        
        public void OnDrop(PointerEventData eventData)
        {
            if(!IsEventValid(eventData)) return;
            
            Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDrop));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            eventData.pointerDrag?.TryGetComponent(out _pendingView);
            
            if (!IsDropValid(_pendingView))
            {
                _pendingView = null;
                return;
            }
            
            Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropEnter));
        }

        public void OnPointerExit(PointerEventData _)
        {
            if(_pendingView == null) return;
            
            Perform(new DropEventData(_pendingView.ViewModel, DropEventData.Types.OnDropExit));
            _pendingView = null;
        }

        private bool IsEventValid(PointerEventData eventData) =>
            eventData.pointerDrag != null &&
            _pendingView != null;
        
        private bool IsDropValid(View view) => 
            view != null && 
            view.ViewModel != null &&
            view.ViewModel.IsInstanceOf(dropType);
    }
}