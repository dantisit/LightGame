using System;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class EnumToBoolUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField, HideInInspector] private int _selectedValue;
        [SerializeField] private UnityEvent<bool> _event;
        [SerializeField] private UltEvent<bool> _ultEvent;
        [SerializeField] private bool _reverse;

        public override void OnPropertyChanged(object newValue)
        {
            if (newValue == null)
            {
                _event.Invoke(false);
                _ultEvent.Invoke(false);
                return;
            }

            var intValue = Convert.ToInt32(newValue);
            var value = _reverse ? intValue != _selectedValue : intValue == _selectedValue;
            _event.Invoke(value);
            _ultEvent.Invoke(value);
        }
    }
}
