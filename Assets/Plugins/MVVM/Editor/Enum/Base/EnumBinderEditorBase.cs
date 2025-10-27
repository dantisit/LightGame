using System;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    public abstract class EnumBinderEditorBase : ObservableBinderBase
    {
        protected SerializedProperty _propertyNameProperty;
        protected string _lastPropertyName;

        protected override SerializedProperty _propertyName
        {
            get => _propertyNameProperty;
            set => _propertyNameProperty = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _propertyNameProperty = serializedObject.FindProperty("_propertyName");
            _lastPropertyName = _propertyNameProperty.stringValue;
        }

        protected Type GetEnumTypeFromProperty()
        {
            var viewModelType = GetViewModelType(ViewModelTypeFullName.stringValue);
            if (viewModelType == null) return null;

            var propertyInfo = viewModelType.GetProperty(_propertyNameProperty.stringValue);
            if (propertyInfo == null) return null;

            var enumType = propertyInfo.PropertyType.GetGenericArguments()[0];
            if (!enumType.IsEnum) return null;

            return enumType;
        }

        public static int GetEnumIndex(Array enumValues, int currentValue)
        {
            for (int i = 0; i < enumValues.Length; i++)
            {
                if (Convert.ToInt32(enumValues.GetValue(i)) == currentValue)
                {
                    return i;
                }
            }
            return 0;
        }

        protected override bool IsValidProperty(Type propertyType)
        {
            if (!propertyType.IsGenericType)
                return false;

            var requiredType = typeof(R3.Observable<>);
            var genericArgument = propertyType.GetGenericArguments()[0];

            return genericArgument.IsEnum && IsValidProperty(propertyType, requiredType, genericArgument);
        }
    }
}
