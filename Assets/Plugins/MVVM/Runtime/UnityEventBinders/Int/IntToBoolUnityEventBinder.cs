using MVVM.Utils;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntToBoolUnityEventBinder : ObservableBinder<int>
    {
        [SerializeField] private CompareOperation _compareOperation;
        [SerializeField] private int _comparingValue;

        [SerializeField] private UnityEvent<bool> _event;
        [SerializeField] private UltEvent<bool> _ultEvent;

        public override void OnPropertyChanged(int newValue)
        {
            var result = _compareOperation.Compare(newValue, _comparingValue);

            _event.Invoke(result);
            _ultEvent.Invoke(result);
        }
    }
}
