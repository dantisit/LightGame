using DG.Tweening;
using MVVM;
using MVVM.Binders;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public class DragMethodBinder : GenericMethodBinder<DragEventData>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float maxOffset = 60f;
        public bool IsDraggable { get; set; }
        private bool IsDragging { get; set; }
        
        private Vector2 _offset;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable) return;
            
            var pointerPosition = eventData.pointerCurrentRaycast.worldPosition;
            _offset = transform.position - new Vector3(pointerPosition.x, pointerPosition.y);
            if (_offset.magnitude > maxOffset) _offset = _offset.normalized * maxOffset;
            
            IsDragging = true;
            DragManager.RegisterDragStart(gameObject);
            
            Perform(new DragEventData(DragEventData.Types.OnBeginDrag));
        }
    
        public virtual void OnDrag(PointerEventData eventData)
        { 
            if (!IsDragging) return;
            if (!IsDraggable)
            {
                CancelDrag(eventData);
                Revert();
                return;
            }
            if (eventData.pointerCurrentRaycast.worldPosition == Vector3.zero) return;
        
            var pointerPosition = eventData.pointerCurrentRaycast.worldPosition;
            transform.position = new Vector2(pointerPosition.x, pointerPosition.y) + _offset;
            Perform(new DragEventData(DragEventData.Types.OnDrag));
        }
    
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            Revert();
        }
        
        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            
            if (!IsDragging) return;
            Revert();
        }
        
        protected virtual void Revert()
        {
            IsDragging = false;
            DragManager.RegisterDragEnd();
            EventSystem.current.SetSelectedGameObject(null);
            Perform(new DragEventData(DragEventData.Types.OnEndDrag));
        }
        
        private void CancelDrag(PointerEventData eventData)
        {
            // Force-end drag interaction
            ExecuteEvents.Execute(
                gameObject,
                eventData,
                ExecuteEvents.endDragHandler
            );

            // Clear dragging reference in EventSystem
            eventData.pointerDrag = null;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}