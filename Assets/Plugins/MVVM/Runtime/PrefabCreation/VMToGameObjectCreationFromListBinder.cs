using UnityEngine;

namespace MVVM.Binders
{
    public class VMToGameObjectCreationFromListBinder : ObservableBinder<ViewModel>
    {
        [SerializeField] private ViewModelToViewMapper _mapper;

        private BinderView _createdBinderView;

        public override void OnPropertyChanged(ViewModel newValue)
        {
            var prefabView = _mapper.GetPrefab(newValue.GetType().FullName);
            _createdBinderView = Instantiate(prefabView, transform);

            _createdBinderView.BindViewModel(newValue);
        }
    }
}
