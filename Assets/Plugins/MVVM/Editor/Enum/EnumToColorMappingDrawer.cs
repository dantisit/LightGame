using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(Binders.EnumToColorUnityEventBinder.EnumToColorMapping))]
    public class EnumToColorMappingDrawer : EnumMappingDrawerBase
    {
        protected override Type GetBinderType() => typeof(Binders.EnumToColorUnityEventBinder);
        protected override string GetMappedPropertyName() => "_color";
    }
}