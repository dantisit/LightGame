using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(Binders.EnumToBoolMapping))]
    public class EnumToBoolMappingDrawer : EnumMappingDrawerBase
    {
        protected override Type GetBinderType() => typeof(Binders.EnumToBoolMapperUnityEventBinder);
        protected override string GetMappedPropertyName() => "_boolValue";
    }
}
