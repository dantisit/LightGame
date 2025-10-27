using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class BoolToStringUnityEventBinder : ObservableBinder<bool>
    {
        [SerializeField] private string _stringTrue;
        [SerializeField] private string _stringFalse;

        [SerializeField] private UnityEvent<string> _event;

        public override void OnPropertyChanged(bool newValue)
        {
            var sprite = newValue ? _stringTrue : _stringFalse;

            _event.Invoke(sprite);
        }
    }
}
