using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class BoolUnityEventBinder :  ObservableBinder<bool>
    {
        [SerializeField] private bool _inverse;
        [SerializeField] private UnityEvent<bool> _event;
        [SerializeField] private UltEvent<bool> _ultEvent;

        public override void OnPropertyChanged(bool newValue)
        {
            _event.Invoke(_inverse ? !newValue : newValue);
            _ultEvent.Invoke(_inverse ? !newValue : newValue);
        }
    }
}
