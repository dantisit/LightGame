using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class BoolToColorUnityEventBinder : ObservableBinder<bool>
    {
        [SerializeField] private Color _colorTrue = Color.white;
        [SerializeField] private Color _colorFalse = Color.white;

        [SerializeField] private UnityEvent<Color> _event;

        public override void OnPropertyChanged(bool newValue)
        {
            var color = newValue ? _colorTrue : _colorFalse;

            _event.Invoke(color);
        }
    }
}
