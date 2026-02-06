using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.MVVM.Runtime.UIComponents
{
    /// <summary>
    /// Forwards pointer and drag events to specified target GameObjects.
    /// Unlike PassthroughPointer, this doesn't raycast - it sends directly to configured targets.
    /// </summary>
    public class InputForwarder : MonoBehaviour, 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerDownHandler, 
        IPointerUpHandler, 
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private List<GameObject> targets = new();
        [SerializeField] private bool forwardEnabled = true;

        public bool ForwardEnabled
        {
            get => forwardEnabled;
            set => forwardEnabled = value;
        }
        
        public List<GameObject> Targets => targets;

        public void OnPointerEnter(PointerEventData eventData) => Forward(eventData, ExecuteEvents.pointerEnterHandler);
        public void OnPointerExit(PointerEventData eventData) => Forward(eventData, ExecuteEvents.pointerExitHandler);
        public void OnPointerDown(PointerEventData eventData) => Forward(eventData, ExecuteEvents.pointerDownHandler);
        public void OnPointerUp(PointerEventData eventData) => Forward(eventData, ExecuteEvents.pointerUpHandler);
        public void OnPointerClick(PointerEventData eventData) => Forward(eventData, ExecuteEvents.pointerClickHandler);
        public void OnBeginDrag(PointerEventData eventData) => Forward(eventData, ExecuteEvents.beginDragHandler);
        public void OnDrag(PointerEventData eventData) => Forward(eventData, ExecuteEvents.dragHandler);
        public void OnEndDrag(PointerEventData eventData) => Forward(eventData, ExecuteEvents.endDragHandler);

        private void Forward<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
        {
            if (!forwardEnabled) return;
            
            foreach (var target in targets)
            {
                if (target != null && target.activeInHierarchy)
                {
                    ExecuteEvents.Execute(target, eventData, eventFunction);
                }
            }
        }
    }
}
