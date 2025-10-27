using UnityEngine;

namespace MVVM.Binders
{
    public class VMToGameObjectCreationBinder : ObservableBinder<ViewModel>
    {
        [SerializeField] protected View _prefabView;
        protected View _createdView;
        
        public override void OnPropertyChanged(ViewModel newValue)
        {
            DestroyCreatedView();
            if(newValue == null) return;
            _createdView = Instantiate(_prefabView, transform);

            _createdView.Bind(newValue);
        }

        public void DestroyCreatedView()
        {
            if(_createdView) Destroy(_createdView.gameObject);
            _valueWhileDeactivated = null; // TODO: Hack
        }
    }
}
