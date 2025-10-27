using R3;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class UnitToBoolUnityEventBinder : ObservableBinder<Unit>
    {
        [SerializeField] private UnityEvent<bool> _event;
        public override void OnPropertyChanged(Unit newValue)
        {
            _event.Invoke(true);
        }
    }
}