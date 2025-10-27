using System;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    /// <summary>
    /// Base editor for enum binders that have a single selected value (not mappings)
    /// </summary>
    public abstract class EnumValueSelectorBinderEditor<TBinder> : EnumBinderEditorBase
        where TBinder : UnityEngine.Object
    {
        private SerializedProperty _selectedValueProperty;
        protected abstract string SelectedValuePropertyName { get; }
        protected virtual string SelectedValueLabel => "Selected Value";

        protected override void OnEnable()
        {
            base.OnEnable();
            _selectedValueProperty = serializedObject.FindProperty(SelectedValuePropertyName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            var enumType = GetEnumTypeFromProperty();
            if (enumType == null) return;

            var enumValues = Enum.GetValues(enumType);
            var enumNames = Enum.GetNames(enumType);

            var currentIndex = GetEnumIndex(enumValues, _selectedValueProperty.intValue);

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup(SelectedValueLabel, currentIndex, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedValueProperty.intValue = Convert.ToInt32(enumValues.GetValue(newIndex));
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}