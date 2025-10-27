using System;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    /// <summary>
    /// Binder that maps an integer value to a RectTransform target, with the ability to specify
    /// movement along X or Y axis. Useful for positioning UI elements like tooltip arrows.
    /// </summary>
    public class IntToTargetPositionBinder : ObservableBinder<int>
    {
        public enum MovementAxis
        {
            Both,
            OnlyX,
            OnlyY
        }

        [SerializeField] private List<IntToTargetMapping> _mappings = new();
        [SerializeField] private RectTransform _defaultTarget;
        [SerializeField] private MovementAxis _movementAxis = MovementAxis.Both;

        [SerializeField] private UnityEvent<Vector3> _event;

        private Dictionary<int, RectTransform>? _targetsMap;

        private RectTransform _currentTarget;
        private Vector3 _currentTargetPosition;

        private void Update()
        {
            if (_currentTarget && _currentTarget.position != _currentTargetPosition)
            {
                _currentTargetPosition = _currentTarget.position;
                UpdatePosition(_currentTarget.position);
            }
        }

        public override void OnPropertyChanged(int newValue)
        {
            if (_targetsMap == null)
            {
                _targetsMap = new Dictionary<int, RectTransform>();
                foreach (var mapping in _mappings)
                {
                    if (mapping.Target != null)
                    {
                        _targetsMap.Add(mapping.Value, mapping.Target);
                    }
                }
            }

            RectTransform target;
            if (_targetsMap.TryGetValue(newValue, out var mappedTarget) && mappedTarget != null)
            {
                target = mappedTarget;
            }
            else
            {
                target = _defaultTarget;
            }

            _currentTarget = target;
        }

        private void UpdatePosition(Vector3 targetPosition)
        {
            if (_movementAxis != MovementAxis.Both)
            {
                var rectTransform = transform as RectTransform;
                if (rectTransform != null)
                {
                    Vector3 currentPosition = rectTransform.position;

                    if (_movementAxis == MovementAxis.OnlyX)
                    {
                        targetPosition.y = currentPosition.y;
                    }
                    else if (_movementAxis == MovementAxis.OnlyY)
                    {
                        targetPosition.x = currentPosition.x;
                    }
                }
            }

            _event.Invoke(targetPosition);
        }
    }

    [Serializable]
    public class IntToTargetMapping
    {
        [SerializeField] private int _value;
        [SerializeField] private RectTransform _target;

        public int Value => _value;
        public RectTransform Target => _target;
    }
}
