using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVM.Binders
{
    public class BaseVMCollectionToGameObjectCreationBinder : ObservableCollectionBinder<ViewModel>
    {
        protected readonly Dictionary<ViewModel, BinderView> _createdViews = new();

        public override void OnItemAdded(ViewModel value)
        {
            throw new System.NotImplementedException();
        }

        public override void OnItemRemoved(ViewModel value)
        {
            if (_createdViews.TryGetValue(value, out var view))
            {
                if(view == null) return;
                DestroyImmediate(view.gameObject);
                _createdViews.Remove(value);
            }
        }

        public override void OnCollectionSort(IList<ViewModel> observableList)
        {
            for (var i = 0; i < observableList.Count; i++)
            {
                if (_createdViews.TryGetValue(observableList[i], out var view))
                {
                    view.transform.SetSiblingIndex(i);
                }
            }
        }

        public override void OnCollectionClear()
        {
            foreach (var createdView in _createdViews)
            {
                if(createdView.Value == null) continue;
                DestroyImmediate(createdView.Value?.gameObject);
            }
            _createdViews.Clear();
        }
    }
}
