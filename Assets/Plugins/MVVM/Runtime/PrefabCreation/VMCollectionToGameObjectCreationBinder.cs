using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MVVM.Binders
{
    public class VMCollectionToGameObjectCreationBinder : BaseVMCollectionToGameObjectCreationBinder
    {
        [FormerlySerializedAs("prefabView")] [FormerlySerializedAs("_prefabView")] [SerializeField] protected BinderView prefabBinderView;

        public override void OnItemAdded(ViewModel value)
        {
            if (_createdViews.TryGetValue(value, out var added)) return;

            var createdView = Instantiate(prefabBinderView, transform);

            _createdViews.Add(value, createdView);
            createdView.BindViewModel(value);
        }
    }
}
