using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToColorUnityEventBinder))]
    public class EnumToColorUnityEventBinderEditor : EnumMappingBinderEditor<Binders.EnumToColorUnityEventBinder>
    {
        protected override string MappingsPropertyName => "_mappings";
    }
}