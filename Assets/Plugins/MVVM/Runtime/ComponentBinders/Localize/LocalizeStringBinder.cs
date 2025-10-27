using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace MVVM.Binders
{
    [RequireComponent(typeof(LocalizeStringEvent))]
    public class LocalizeStringBinder : ObservableBinder<LocalizedString>
    {
        protected LocalizeStringEvent? _localizeStringEvent;
        
        public override void OnPropertyChanged(LocalizedString newValue)
        {
            _localizeStringEvent ??= GetComponent<LocalizeStringEvent>();
            _localizeStringEvent.StringReference = newValue;
        }
    }
}