using UnityEngine;
using UnityEngine.Serialization;

namespace MVVM.Binders
{
    public class VMToGameObjectCreationBinder : ObservableBinder<ViewModel>
    {
        [FormerlySerializedAs("prefabView")] [FormerlySerializedAs("_prefabView")] [SerializeField] protected BinderView prefabBinderView;
        protected BinderView CreatedBinderView;
        
        public override void OnPropertyChanged(ViewModel newValue)
        {
            DestroyCreatedView();
            if(newValue == null) return;
            CreatedBinderView = Instantiate(prefabBinderView, transform);

            CreatedBinderView.BindViewModel(newValue);
        }

        public void DestroyCreatedView()
        {
            if(CreatedBinderView) Destroy(CreatedBinderView.gameObject);
            _valueWhileDeactivated = null; // TODO: Hack
        }
    }
}
