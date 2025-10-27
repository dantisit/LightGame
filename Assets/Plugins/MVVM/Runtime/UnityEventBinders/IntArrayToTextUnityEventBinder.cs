using MVVM.Binders;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntArrayToTextUnityEventBinder : ObservableBinder<int[]>
    {
        [SerializeField] private int _index;
        [SerializeField] private string _format = "0"; // Optional number format string
        [SerializeField] private string _prefix = ""; // Optional text to add before the number
        [SerializeField] private UnityEvent<string> _event;

        public override void OnPropertyChanged(int[] newValue)
        {
            if (newValue == null || _index < 0 || _index >= newValue.Length)
            {
                _event.Invoke(string.Empty);
                return;
            }

            _event.Invoke(_prefix + newValue[_index].ToString(_format));
        }
    }
}
