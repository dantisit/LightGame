using MVVM.Utils;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class LongToBoolUnityEventBinder : ObservableBinder<long>
    {
        [SerializeField] private CompareOperation _compareOperation;
        [SerializeField] private int _comparingValue;

        [SerializeField] private UltEvent<bool> _event;

        public override void OnPropertyChanged(long newValue)
        {
            var result = _compareOperation.Compare(newValue, _comparingValue);

            _event.Invoke(result);
        }
    }
}