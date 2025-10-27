using UnityEngine;

namespace MVVM.Binders
{
    public class VMToGameObjectCreationFromListBinder : ObservableBinder<ViewModel>
    {
        [SerializeField] private ViewModelToViewMapper _mapper;

        private View _createdView;

        public override void OnPropertyChanged(ViewModel newValue)
        {
            var prefabView = _mapper.GetPrefab(newValue.GetType().FullName);
            _createdView = Instantiate(prefabView, transform);

            _createdView.Bind(newValue);
        }
    }
}
