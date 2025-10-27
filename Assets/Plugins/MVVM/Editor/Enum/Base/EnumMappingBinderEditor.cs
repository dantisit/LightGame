using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    public abstract class EnumMappingBinderEditor<TBinder> : EnumBinderEditorBase
        where TBinder : UnityEngine.Object
    {
        private SerializedProperty _mappingsProperty;
        protected abstract string MappingsPropertyName { get; }

        protected override void OnEnable()
        {
            base.OnEnable();
            _mappingsProperty = serializedObject.FindProperty(MappingsPropertyName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            base.OnInspectorGUI();

            // Check if property name changed
            if (_propertyNameProperty.stringValue != _lastPropertyName)
            {
                PopulateEnumMappings();
                _lastPropertyName = _propertyNameProperty.stringValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PopulateEnumMappings()
        {
            var enumType = GetEnumTypeFromProperty();
            if (enumType == null) return;

            var enumValues = Enum.GetValues(enumType);
            var existingMappings = new HashSet<int>();

            // Collect existing mappings
            for (int i = 0; i < _mappingsProperty.arraySize; i++)
            {
                var mapping = _mappingsProperty.GetArrayElementAtIndex(i);
                var valueProp = mapping.FindPropertyRelative("_value");
                existingMappings.Add(valueProp.intValue);
            }

            // Add missing enum values
            foreach (var enumValue in enumValues)
            {
                var intValue = Convert.ToInt32(enumValue);
                if (!existingMappings.Contains(intValue))
                {
                    _mappingsProperty.arraySize++;
                    var mapping = _mappingsProperty.GetArrayElementAtIndex(_mappingsProperty.arraySize - 1);
                    var valueProp = mapping.FindPropertyRelative("_value");
                    valueProp.intValue = intValue;
                }
            }
        }
    }
}