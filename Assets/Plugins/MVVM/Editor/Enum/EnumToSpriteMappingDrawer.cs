using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(Binders.EnumToSpriteUnityEventBinder.EnumToSpriteMapping))]
    public class EnumToSpriteMappingDrawer : EnumMappingDrawerBase
    {
        protected override Type GetBinderType() => typeof(Binders.EnumToSpriteUnityEventBinder);
        protected override string GetMappedPropertyName() => "_sprite";
    }
}
