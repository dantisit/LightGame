using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    public abstract class EnumMappingDrawerBase : PropertyDrawer
    {
        protected abstract Type GetBinderType();
        protected abstract string GetMappedPropertyName();

        private Type GetEnumType(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            if (target == null || target.GetType() != GetBinderType()) 
                return null;

            var binderType = target.GetType();
            var viewModelTypeProperty = binderType.GetProperty("ViewModelTypeFullName");
            var propertyNameProperty = binderType.GetProperty("PropertyName");
            
            if (viewModelTypeProperty == null || propertyNameProperty == null) 
                return null;

            var viewModelTypeName = viewModelTypeProperty.GetValue(target) as string;
            var propertyName = propertyNameProperty.GetValue(target) as string;

            if (string.IsNullOrEmpty(viewModelTypeName) || string.IsNullOrEmpty(propertyName))
                return null;

            var viewModelType = TypeCache.GetTypesDerivedFrom<ViewModel>()
                .FirstOrDefault(t => t.FullName == viewModelTypeName);

            if (viewModelType == null) return null;

            var propertyInfo = viewModelType.GetProperty(propertyName);
            var enumType = propertyInfo?.PropertyType.GetGenericArguments()[0];

            if (enumType?.IsEnum != true) return null;

            return enumType;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumType = GetEnumType(property);
            if (enumType == null)
            {
                EditorGUI.HelpBox(position, "Select a valid enum property first", MessageType.Warning);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var valueRect = new Rect(position.x, position.y, position.width * 0.5f - 2, position.height);
            var mappedRect = new Rect(position.x + position.width * 0.5f + 2, position.y, position.width * 0.5f - 2, position.height);

            var valueProp = property.FindPropertyRelative("_value");
            var mappedProp = property.FindPropertyRelative(GetMappedPropertyName());

            var enumValues = Enum.GetValues(enumType);
            var currentIndex = EnumBinderEditorBase.GetEnumIndex(enumValues, valueProp.intValue);

            var enumNames = Enum.GetNames(enumType);
            var newIndex = EditorGUI.Popup(valueRect, currentIndex, enumNames);

            if (newIndex != currentIndex)
            {
                valueProp.intValue = Convert.ToInt32(enumValues.GetValue(newIndex));
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.PropertyField(mappedRect, mappedProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}