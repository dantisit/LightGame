using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class DoubleToTextUnityEventBinder : ObservableBinder<double>
    {
        [SerializeField] private string _format = "0.00";
        [SerializeField] private UnityEvent<string> _event;

        public override void OnPropertyChanged(double newValue)
        {
            _event.Invoke(newValue.ToString(_format));
        }
    }
}
