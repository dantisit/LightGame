using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntToVector3UnityEventBinder : ObservableBinder<int>
    {
        [SerializeField] private List<IntToVector3Mapping> _mappings = new();
        [SerializeField] private Vector3 _vector3ByDefault;

        [SerializeField] private UnityEvent<Vector3> _event;

        private Dictionary<int, Vector3>? _vector3Map;


        public override void OnPropertyChanged(int newValue)
        {
            if (_vector3Map == null)
            {
                _vector3Map = new Dictionary<int, Vector3>();

                foreach (var mapping in _mappings)
                {
                    _vector3Map.Add(mapping.Value, mapping.Vector3Value);
                }
            }

            if (_vector3Map.TryGetValue(newValue, out var vector3))
            {
                _event.Invoke(vector3);
                return;
            }

            _event.Invoke(_vector3ByDefault);
        }
    }

    [Serializable]
    public class IntToVector3Mapping
    {
        [SerializeField] private int _value;
        [SerializeField] private Vector3 _vector3Value;

        public int Value => _value;
        public Vector3 Vector3Value => _vector3Value;
    }
}
