using System;
using System.Collections.Generic;
using UnityEngine;
using UltEvents;

namespace MVVM.Binders
{
    /// <summary>
    /// Binds an enum observable to UltEvents - invokes the corresponding event when enum value changes
    /// </summary>
    public class EnumToUltEventBinder : ObservableBinder<object>
    {
        [SerializeField] private List<EnumToEventMapping> _mappings = new();
        [SerializeField] private UltEvent _defaultEvent;

        private Dictionary<int, UltEvent>? _eventsMap;

        public override void OnPropertyChanged(object newValue)
        {
            if (_eventsMap == null)
            {
                _eventsMap = new Dictionary<int, UltEvent>();

                foreach (var mapping in _mappings)
                {
                    _eventsMap[mapping.Value] = mapping.Event;
                }
            }

            if (newValue != null)
            {
                var intValue = Convert.ToInt32(newValue);
                if (_eventsMap.TryGetValue(intValue, out var ultEvent))
                {
                    ultEvent?.Invoke();
                    return;
                }
            }

            _defaultEvent?.Invoke();
        }

        [Serializable]
        public class EnumToEventMapping
        {
            [SerializeField] private int _value;
            [SerializeField] private UltEvent _event;

            public int Value => _value;
            public UltEvent Event => _event;
        }
    }
}