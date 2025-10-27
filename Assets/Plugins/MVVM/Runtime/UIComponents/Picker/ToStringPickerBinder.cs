using UltEvents;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    public class ToStringPickerBinder : ObservablePickerBinder<object>
    {
        [SerializeField] private UltEvent<string> onValueChange;
        protected override void OnValueChange(object value)
        {
            onValueChange.Invoke(value?.ToString() ?? string.Empty);
        }
    }
}