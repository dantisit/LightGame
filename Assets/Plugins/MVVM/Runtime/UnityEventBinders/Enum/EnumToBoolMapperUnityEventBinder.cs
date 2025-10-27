using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MVVM.Binders
{
    public class EnumToBoolMapperUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private List<EnumToBoolMapping> _mappings = new();
        [SerializeField] private bool _defaultValue = false;
        [SerializeField] private UnityEvent<bool> _event;

        private Dictionary<int, bool>? _boolMap;

        private bool _additionalCondition = true;
        public bool AdditionalCondition
        {
            get => _additionalCondition;
            set
            {
                _additionalCondition = value;
                OnPropertyChanged(Value);
            }
        }

        public override void OnPropertyChanged(object newValue)
        {
            if (_boolMap == null)
            {
                _boolMap = new Dictionary<int, bool>();

                foreach (var mapping in _mappings)
                {
                    _boolMap.Add(mapping.EnumValue, mapping.BoolValue);
                }
            }

            if (newValue == null)
            {
                _event.Invoke(_defaultValue && AdditionalCondition);
                return;
            }

            var intValue = Convert.ToInt32(newValue);
            if (_boolMap.TryGetValue(intValue, out var boolValue))
            {
                _event.Invoke(boolValue && AdditionalCondition);
                return;
            }

            _event.Invoke(_defaultValue && AdditionalCondition);
        }
    }

    [Serializable]
    public class EnumToBoolMapping
    {
        [SerializeField]
        [FormerlySerializedAs("_enumValue")]  // ✅ Tells Unity the old name
        private int _value;
        [SerializeField] private bool _boolValue;

        public int EnumValue => _value;
        public bool BoolValue => _boolValue;
    }
}
