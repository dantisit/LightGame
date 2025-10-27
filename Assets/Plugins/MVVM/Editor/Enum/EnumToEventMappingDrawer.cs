using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(Binders.EnumToUltEventBinder.EnumToEventMapping))]
    public class EnumToEventMappingDrawer : PropertyDrawer
    {
        private Type GetEnumType(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject as Binders.EnumToUltEventBinder;
            if (target == null) return null;

            var viewModelType = TypeCache.GetTypesDerivedFrom<ViewModel>()
                .FirstOrDefault(t => t.FullName == target.ViewModelTypeFullName);
                
            if (viewModelType == null) return null;

            var propertyInfo = viewModelType.GetProperty(target.PropertyName);
            var enumType = propertyInfo?.PropertyType.GetGenericArguments()[0];

            if (enumType?.IsEnum != true) return null;

            return enumType;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var enumType = GetEnumType(property);
            if (enumType == null)
                return EditorGUIUtility.singleLineHeight;

            var eventProp = property.FindPropertyRelative("_event");
            return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(eventProp, true) + EditorGUIUtility.standardVerticalSpacing;
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

            var valueProp = property.FindPropertyRelative("_value");
            var eventProp = property.FindPropertyRelative("_event");

            // Enum popup on first line
            var enumRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            var enumValues = Enum.GetValues(enumType);
            var currentIndex = EnumBinderEditorBase.GetEnumIndex(enumValues, valueProp.intValue);
            var enumNames = Enum.GetNames(enumType);
            
            var newIndex = EditorGUI.Popup(enumRect, "Enum Value", currentIndex, enumNames);
            if (newIndex != currentIndex)
            {
                valueProp.intValue = Convert.ToInt32(enumValues.GetValue(newIndex));
                property.serializedObject.ApplyModifiedProperties();
            }

            // UltEvent field below
            var eventRect = new Rect(
                position.x, 
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, 
                position.width, 
                EditorGUI.GetPropertyHeight(eventProp, true)
            );
            
            EditorGUI.PropertyField(eventRect, eventProp, new GUIContent("Event"), true);

            EditorGUI.EndProperty();
        }
    }
}