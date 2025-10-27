using System;
using UnityEditor;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(Binders.EnumToMaterialUnityEventBinder.EnumToMaterialMapping))]
    public class EnumToMaterialMappingDrawer : EnumMappingDrawerBase
    {
        protected override Type GetBinderType() => typeof(Binders.EnumToMaterialUnityEventBinder);
        protected override string GetMappedPropertyName() => "_material";
    }
}