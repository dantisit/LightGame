using Core.Client.UI.Components;
using MVVM;
using R3;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Button
{
    public class ButtonView : View<ButtonViewModel>
    {
        [SerializeField] private ButtonExtended button;
        
        protected override void OnBindViewModel(ButtonViewModel viewModel)
        {
            DisposeOnDestroy = false;
            
            Bind(viewModel.IsEnabled).To(x => button.interactable = x);
            
            // Two-way binding for State
            button.SelectionStateTransition
                .Where(state => state != viewModel.State.Value)
                .Subscribe(state => viewModel.State.Value = state)
                .AddTo(gameObject);
            
            viewModel.State
                .Where(state => state != button.SelectionStateTransition.Value)
                .Subscribe(state => button.UpdateSelectionState(state))
                .AddTo(gameObject);
            
            button.onClick.AsObservable()
                .Subscribe(_ => viewModel.OnPressed.OnNext(Unit.Default))
                .AddTo(gameObject);
        }
        
        private void Reset()
        {
            button = GetComponent<ButtonExtended>();
        }
    }
}
