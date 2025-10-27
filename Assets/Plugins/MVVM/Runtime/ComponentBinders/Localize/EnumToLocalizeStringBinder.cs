using System;
using System.Collections.Generic;
using MVVM.Binders;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class EnumToLocalizedStringBinder : ObservableBinder<object>
{
    [SerializeField] private List<EnumToLocalizedStringMapping> _mappings = new();
    
    private Dictionary<int, LocalizedString>? _map;
    
    private LocalizeStringEvent? _localizeStringEvent;
    
    public override void OnPropertyChanged(object newValue)
    {
        if (_map == null)
        {
            _map = new Dictionary<int, LocalizedString>();

            foreach (var mapping in _mappings)
            {
                _map.Add(mapping.EnumValue, mapping.LocalizedStringValue);
            }
        }
        
        var intValue = Convert.ToInt32(Value);
        if (!_map.TryGetValue(intValue, out var localizedString)) return;
        
        _localizeStringEvent ??= GetComponent<LocalizeStringEvent>();
        _localizeStringEvent.StringReference = localizedString;
    }
}

[Serializable]
public class EnumToLocalizedStringMapping
{
    [SerializeField] private int _enumValue;
    [SerializeField] private LocalizedString _LocalizedString;

    public int EnumValue => _enumValue;
    public LocalizedString LocalizedStringValue => _LocalizedString;
}