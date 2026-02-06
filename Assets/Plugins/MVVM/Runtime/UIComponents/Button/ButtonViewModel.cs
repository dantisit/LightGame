using Core.Client.UI.Components;
using MVVM;
using R3;

namespace Plugins.MVVM.Runtime.UIComponents.Button
{
    public class ButtonViewModel : ViewModel
    {
        public ReactiveProperty<bool> IsEnabled { get; } = new(true);
        public ReactiveProperty<SelectionState> State { get; } = new();
        public Subject<Unit> OnPressed { get; } = new();
    }
}
