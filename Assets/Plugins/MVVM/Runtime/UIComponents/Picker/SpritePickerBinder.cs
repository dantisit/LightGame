using UltEvents;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    public class SpritePickerBinder : ObservablePickerBinder<Sprite>
    {
        [SerializeField] private UltEvent<Sprite> onValueChange;
        protected override void OnValueChange(Sprite value)
        {
            onValueChange.Invoke(value);
        }
    }
}