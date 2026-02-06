using System;
using Core.Client.Events;
using MVVM;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Client.UI.Components
{
    public class ButtonExtended : Button, ISelectable
    {
        public ReactiveProperty<Components.SelectionState> SelectionStateTransition { get; } = new();

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        public void UpdateSelectionState(Components.SelectionState state) => DoStateTransition((Selectable.SelectionState)state, false);
        
        protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (!gameObject.activeInHierarchy)
                return;
            
            if(!Application.isPlaying) return;
            
            SelectionStateTransition.Value = (Components.SelectionState)state;
        }
    }
}