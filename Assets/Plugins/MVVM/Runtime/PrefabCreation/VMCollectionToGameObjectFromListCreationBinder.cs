using System.Collections.Generic;
using UnityEngine;

namespace MVVM.Binders
{
    public class VMCollectionToGameObjectFromListCreationBinder : ObservableCollectionBinder<ViewModel>
    {
        [SerializeField] private ViewModelToViewMapper _mapper;

        private readonly Dictionary<ViewModel, BinderView> _createdViews = new();

        public override void OnItemAdded(ViewModel viewModel)
        {
            if (_createdViews.ContainsKey(viewModel))
            {
                return;
            }

            var prefab = _mapper.GetPrefab(viewModel.GetType().FullName);
            var createdView = Instantiate(prefab, transform);

            _createdViews.Add(viewModel, createdView);
            createdView.BindViewModel(viewModel);

            return;
        }
        public override void OnCollectionSort(IList<ViewModel> observableList)
        {
            foreach (var pair in _createdViews)
            {
                var index = observableList.IndexOf(pair.Key);
                pair.Value.transform.SetSiblingIndex(index);
            }
        }

        public override void OnItemRemoved(ViewModel viewModel)
        {
            if (_createdViews.TryGetValue(viewModel, out var view))
            {
                view.Destroy();
                _createdViews.Remove(viewModel);
            }
        }

        public override void OnCollectionClear()
        {
            foreach (var createdView in _createdViews)
            {
                createdView.Value.Destroy();
            }
            _createdViews.Clear();
        }
    }
}
