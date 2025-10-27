using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class OnChangedUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private int _skip;
        [SerializeField] private UnityEvent _event;
        [SerializeField] private UltEvent _ultEvent;
        public override void OnPropertyChanged(object _)
        {
            if (_skip > 0)
            {
                _skip--;
                return;
            }
            _event.Invoke();
            _ultEvent.Invoke();
        }
    }
}