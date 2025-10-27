using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class BoolToSeparateUnityEventBinder :  ObservableBinder<bool>
    {
        [SerializeField] private UltEvent _ultEventTrue;
        [SerializeField] private UnityEvent _eventTrue;
        [SerializeField] private UltEvent _ultEventFalse;
        [SerializeField] private UnityEvent _eventFalse;
        public override void OnPropertyChanged(bool newValue)
        {
            if (newValue)
            {
                _eventTrue.Invoke();
                _ultEventTrue.Invoke();
            }
            else
            {
                _eventFalse.Invoke();
                _ultEventFalse.Invoke();
            }
        }
    }
}
