using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MVVM;
using MVVM.Binders;
using Plugins.MVVM.Runtime.UIComponents.Behaviors;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public class DragView : View<DragViewModel>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float maxOffset = 60f;
        [SerializeField] private BinderView draggingView;
        private bool _isDraggable;
        
        public BinderView DraggingView => draggingView;
        public Transform TargetTransform => targetTransform != null ? targetTransform : transform;

        public bool IsDraggable
        {
            get => _isDraggable;
            set
            {
                _isDraggable = value;
                if (!_isDraggable && IsDragging)
                {
                    CancelDrag(null);
                }
            }
        }

        private bool IsDragging { get; set; }
        
        private Vector2 _offset;
        
        protected override void OnBindViewModel(DragViewModel viewModel)
        {
            DisposeOnDestroy = false;
            
            // Bind ViewModel properties to component
            Bind(viewModel.IsDraggable).To(x => IsDraggable = x);
        }

        private void Awake()
        {
            EventBus.On<CancelDragEvent>()
                .Subscribe(_ => CancelDrag(null))
                .AddTo(this);
        }
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable) return;
            
            var pointerPosition = eventData.pointerCurrentRaycast.worldPosition;
            _offset = TargetTransform.position - new Vector3(pointerPosition.x, pointerPosition.y);
            if (_offset.magnitude > maxOffset) _offset = _offset.normalized * maxOffset;
            
            IsDragging = true;
            DragManager.RegisterDragStart(this);
            
            var dragData = new DragEventData(DragEventData.Types.OnBeginDrag);
            
            ViewModel.UpdateDrag(dragData);
            canvasGroup.blocksRaycasts = false;
        }
    
        public virtual void OnDrag(PointerEventData eventData)
        { 
            if (!IsDragging) return;
            if (!IsDraggable)
            {
                CancelDrag(eventData);
                return;
            }
            if (eventData.pointerCurrentRaycast.worldPosition == Vector3.zero) return;
        
            if (_offset.sqrMagnitude > maxOffset * maxOffset) _offset = _offset.normalized * maxOffset;
            
            var pointerPosition = eventData.pointerCurrentRaycast.worldPosition;
            TargetTransform.position = new Vector2(pointerPosition.x, pointerPosition.y) + _offset;
            var dragData = new DragEventData(DragEventData.Types.OnDrag);

            ViewModel.UpdateDrag(dragData);
            ViewModel.OnDrag.OnNext(Unit.Default);
        }
    
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            Revert();
        }
        
        protected void OnDestroy()
        {
            if (!IsDragging) return;
            Revert();
        }
        
        protected virtual void Revert()
        {
            IsDragging = false;
            DragManager.RegisterDragEnd();
            EventSystem.current?.SetSelectedGameObject(null);
            var dragData = new DragEventData(DragEventData.Types.OnEndDrag);
            
            ViewModel.UpdateDrag(dragData);
            canvasGroup.blocksRaycasts = true; 
        }
        
        private void CancelDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            
            if (eventData != null)
            {
                // Force-end drag interaction
                ExecuteEvents.Execute(
                    gameObject,
                    eventData,
                    ExecuteEvents.endDragHandler
                );

                // Clear dragging reference in EventSystem
                eventData.pointerDrag = null;
            }
            
            EventSystem.current?.SetSelectedGameObject(null);
            
            Revert();
        }
    }
}