using MVVM.Binders;
using UnityEngine.UI;

namespace Plugins.MVVM.Runtime
{
    public class IntToChildButtonInteractableBinder : ObservableBinder<int>
    {
        private Button _lastButton;
        
        public override void OnPropertyChanged(int newValue)
        {
            if (_lastButton) _lastButton.interactable = true;
            _lastButton = transform.GetChild(newValue).GetComponent<Button>();
            _lastButton.interactable = false;
        }
    }
}