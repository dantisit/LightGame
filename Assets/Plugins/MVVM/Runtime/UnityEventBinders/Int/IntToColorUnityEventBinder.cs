using System;
using System.Collections.Generic;
using MVVM.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntToColorUnityEventBinder : ObservableBinder<int>
    {
        [SerializeField] private List<IntToColorMapping> _mappings = new();
        [SerializeField] private Color _colorByDefault = Color.white;
        [SerializeField] private UnityEvent<Color> _event;

        public override void OnPropertyChanged(int newValue)
        {
            foreach (var mapping in _mappings)
            {
                if (!mapping.CompareOperation.Compare(newValue, mapping.Value)) continue;

                _event.Invoke(mapping.Color);
                return;
            }

            _event.Invoke(_colorByDefault);
        }
    }

    [Serializable]
    public class IntToColorMapping
    {
        [SerializeField] private CompareOperation _compareOperation;
        [SerializeField] private int _value;
        [SerializeField] private Color _color = Color.white;

        public CompareOperation CompareOperation => _compareOperation;
        public int Value => _value;
        public Color Color => _color;
    }
}
