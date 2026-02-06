using MVVM.Data;
using MVVM.Utils;
using Plugins.MVVM.Runtime;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.Binders
{
    /// <summary>
    /// Handles drop events for a single drop target.
    /// When used standalone, handles its own pointer events.
    /// When used with GroupDropComponent, the group handles pointer events and calls Perform directly.
    /// </summary>
    public class DropMethodBinder : GenericMethodBinder<DropEventData>, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ViewModelType] private string dropType;
        
        private IView _pendingBinderView;
        private CompositeDisposable _disposables;
        
        /// <summary>
        /// Whether this drop target is currently being hovered by a valid draggable.
        /// Bound to DragManager.ActiveDropTargetProperty for reactive updates.
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsTargeted { get; private set; }
        
        private void Awake()
        {
            _disposables = new CompositeDisposable();
            
            // Create reactive property that tracks if this binder is the active drop target
            IsTargeted = DragManager.ActiveDropTargetProperty
                .Select(target => target == this)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
        
        // Standalone drop handling (when not part of a GroupDropComponent)
        public void OnDrop(PointerEventData eventData)
        {
            if (!IsEventValid(eventData)) return;
            
            Perform(new DropEventData(_pendingBinderView.ViewModel, DropEventData.Types.OnDrop));
            _pendingBinderView = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pendingBinderView = IsDropValid(DragManager.CurrentDragView?.DraggingView) 
                ? DragManager.CurrentDragView?.DraggingView 
                : null;
            
            if (_pendingBinderView == null) return;

            // Register with DragManager for snap support
            DragManager.RegisterDropZoneEnter(null, this, transform.position);
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (_pendingBinderView == null) return;
            
            DragManager.RegisterDropZoneExit(null);
            _pendingBinderView = null;
        }

        private bool IsEventValid(PointerEventData eventData) =>
            eventData.pointerDrag != null &&
            _pendingBinderView != null;
        
        private bool IsDropValid(IView binderView) => 
            binderView != null && 
            binderView.ViewModel != null &&
            binderView.ViewModel.IsInstanceOf(dropType);
    }
}