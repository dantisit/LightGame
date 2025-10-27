using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class EnumToColorUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private List<EnumToColorMapping> _mappings = new();
        [SerializeField] private Color _colorByDefault = Color.white;
        [SerializeField] private UnityEvent<Color> _event;

        private Dictionary<int, Color>? _colorsMap;

        public override void OnPropertyChanged(object newValue)
        {
            if (_colorsMap == null)
            {
                _colorsMap = new Dictionary<int, Color>();

                foreach (var mapping in _mappings)
                {
                    _colorsMap[mapping.Value] = mapping.Color;
                }
            }

            if (newValue == null)
            {
                _event.Invoke(_colorByDefault);
                return;
            }

            var intValue = Convert.ToInt32(newValue);
            if (_colorsMap.TryGetValue(intValue, out var color))
            {
                _event.Invoke(color);
                return;
            }

            _event.Invoke(_colorByDefault);
        }

        [Serializable]
        public class EnumToColorMapping
        {
            [SerializeField] private int _value;
            [SerializeField] private Color _color = Color.white;

            public int Value => _value;
            public Color Color => _color;
        }
    }
}
