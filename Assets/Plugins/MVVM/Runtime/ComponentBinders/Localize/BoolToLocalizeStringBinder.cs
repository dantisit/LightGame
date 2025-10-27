using MVVM.Binders;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class BoolToLocalizeStringBinder : ObservableBinder<bool>
{
    public LocalizedString trueStringReference = new();
    public LocalizedString falseStringReference = new();
        
    private LocalizeStringEvent? _localizeStringEvent;

    public override void OnPropertyChanged(bool newValue)
    {
        _localizeStringEvent ??= GetComponent<LocalizeStringEvent>();
        _localizeStringEvent.StringReference = newValue ? trueStringReference : falseStringReference;
    }
}
