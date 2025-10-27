using MVVM.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IsNullUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private bool _inverse;
        [SerializeField] private UnityEvent<bool> _event;

        public override void OnPropertyChanged(object? newValue)
        {
            var isNull = newValue == null;
            _event.Invoke(_inverse ? !isNull : isNull);
        }
    }
}
