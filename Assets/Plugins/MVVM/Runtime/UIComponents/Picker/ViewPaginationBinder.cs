using System.Collections.Generic;
using MVVM;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    public class ViewPaginationBinder : PaginationBinder<ViewModel>
    {
        [SerializeField] private View _prefabView;
        protected readonly Dictionary<ViewModel, View> _viewModelToView = new();

        public override void OnItemAdded(ViewModel value)
        {
            base.OnItemAdded(value);
            value.AddTo(this);
        }

        public override void AddPaginationItem(ViewModel item, int offset)
        {
            if (_viewModelToView.TryGetValue(item, out _)) return;
            
            var createdView = Instantiate(_prefabView, transform);
            createdView.transform.SetSiblingIndex(WindowSize + offset);

            _viewModelToView.Add(item, createdView);
            createdView.DisposeOnDestroy = false;
            createdView.Bind(item);
        }
        
        public override void RemovePaginationItem(ViewModel item, int offset)
        {
            if (!_viewModelToView.TryGetValue(item, out var view)) return;
            
            DestroyImmediate(view.gameObject);
            _viewModelToView.Remove(item);
        }
    }
}