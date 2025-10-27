using R3;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class UnitUnityEventBinder : ObservableBinder<Unit>
    {
        [SerializeField] private UltEvent _ultEvent;
        [SerializeField] private UnityEvent _event;
        public override void OnPropertyChanged(Unit newValue)
        {
            _event.Invoke();
            _ultEvent.Invoke();
        }
    }
}