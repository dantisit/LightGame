using MVVM.Binders;
using UnityEngine;

namespace Plugins.MVVM.Runtime
{
    public class IntToActiveChildBinder : ObservableBinder<int>
    {
        public override void OnPropertyChanged(int newValue)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            
            transform.GetChild(newValue).gameObject.SetActive(true);
        }
    }
}