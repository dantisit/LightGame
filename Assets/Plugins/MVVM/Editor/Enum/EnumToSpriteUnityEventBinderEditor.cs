using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToSpriteUnityEventBinder))]
    public class EnumToSpriteUnityEventBinderEditor : EnumMappingBinderEditor<Binders.EnumToSpriteUnityEventBinder>
    {
        protected override string MappingsPropertyName => "_mappings";
    }
}
