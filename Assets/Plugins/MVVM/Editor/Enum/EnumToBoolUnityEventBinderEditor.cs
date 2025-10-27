using System;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToBoolUnityEventBinder))]
    public class EnumToBoolUnityEventBinderEditor : EnumValueSelectorBinderEditor<Binders.EnumToBoolUnityEventBinder>
    {
        protected override string SelectedValuePropertyName => "_selectedValue";
    }
}
