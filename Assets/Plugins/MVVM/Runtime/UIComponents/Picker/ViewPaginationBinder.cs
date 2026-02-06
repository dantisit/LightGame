using System.Collections.Generic;
using MVVM;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    public class ViewPaginationBinder : PaginationBinder<ViewModel>
    {
        [FormerlySerializedAs("prefabView")] [FormerlySerializedAs("_prefabView")] [SerializeField] private BinderView prefabBinderView;
        protected readonly Dictionary<ViewModel, BinderView> _viewModelToView = new();

        public override void OnItemAdded(ViewModel value)
        {
            base.OnItemAdded(value);
            value.AddTo(this);
        }

        public override void AddPaginationItem(ViewModel item, int offset)
        {
            if (_viewModelToView.TryGetValue(item, out _)) return;
            
            var createdView = Instantiate(prefabBinderView, transform);
            createdView.transform.SetSiblingIndex(WindowSize + offset);

            _viewModelToView.Add(item, createdView);
            createdView.DisposeOnDestroy = false;
            createdView.BindViewModel(item);
        }
        
        public override void RemovePaginationItem(ViewModel item, int offset)
        {
            if (!_viewModelToView.TryGetValue(item, out var view)) return;
            
            DestroyImmediate(view.gameObject);
            _viewModelToView.Remove(item);
        }
    }
}