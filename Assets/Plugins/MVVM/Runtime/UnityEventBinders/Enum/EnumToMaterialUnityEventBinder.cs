using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class EnumToMaterialUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private List<EnumToMaterialMapping> _mappings = new();
        [SerializeField] private Material _materialByDefault;
        [SerializeField] private UnityEvent<Material> _event;
        [SerializeField] private UnityEvent<bool> _visibilityEvent;

        private Dictionary<int, Material>? _materialsMap;

        public override void OnPropertyChanged(object newValue)
        {
            if (_materialsMap == null)
            {
                _materialsMap = new Dictionary<int, Material>();

                foreach (var mapping in _mappings)
                {
                    _materialsMap[mapping.Value] = mapping.Material;
                }
            }

            bool hasMaterial = false;
            Material materialToUse = _materialByDefault;

            if (newValue != null)
            {
                var intValue = Convert.ToInt32(newValue);
                if (_materialsMap.TryGetValue(intValue, out var material))
                {
                    materialToUse = material;
                }
            }

            if (materialToUse != null) hasMaterial = true;

            _event.Invoke(materialToUse);
            _visibilityEvent.Invoke(hasMaterial);
        }

        [Serializable]
        public class EnumToMaterialMapping
        {
            [SerializeField] private int _value;
            [SerializeField] private Material _material;

            public int Value => _value;
            public Material Material => _material;
        }
    }
}