using System.Collections.Generic;
using UnityEngine;

namespace MVVM.Binders
{
    public class VMCollectionToGameObjectCreationBinder : BaseVMCollectionToGameObjectCreationBinder
    {
        [SerializeField] protected View _prefabView;

        public override void OnItemAdded(ViewModel value)
        {
            if (_createdViews.TryGetValue(value, out var added)) return;

            var createdView = Instantiate(_prefabView, transform);

            _createdViews.Add(value, createdView);
            createdView.Bind(value);
        }
    }
}
