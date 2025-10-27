using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToBoolMapperUnityEventBinder))]
    public class EnumToBoolMapperUnityEventBinderEditor : EnumMappingBinderEditor<Binders.EnumToBoolMapperUnityEventBinder>
    {
        protected override string MappingsPropertyName => "_mappings";
    }
}
