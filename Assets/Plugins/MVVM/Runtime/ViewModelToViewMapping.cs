using System;
using UnityEngine.Serialization;

namespace MVVM
{
    [Serializable]
    public class ViewModelToViewMapping
    {
        public string ViewModelTypeFullName;
        [FormerlySerializedAs("prefabView")] [FormerlySerializedAs("PrefabView")] public BinderView prefabBinderView;
    }
}