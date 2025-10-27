using System;
using System.Linq;
using MVVM.Binders;
using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binder), true)]
    public abstract class BinderEditor : UnityEditor.Editor
    {
        private Binder _binder;
        private View _parentView;
        private SerializedProperty _viewModelTypeFullName;

        protected SerializedProperty ViewModelTypeFullName => _viewModelTypeFullName;
        private TypeCache.TypeCollection _cachedViewModelTypes;

        protected virtual void OnEnable()
        {
            _binder = target as Binder;
            _parentView = _binder?.GetComponentInParent<View>(true);

            _viewModelTypeFullName = serializedObject.FindProperty(nameof(_viewModelTypeFullName));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target == null)
            {
                OnBinderComponentRemoved();
                return;
            }

            _cachedViewModelTypes = TypeCache.GetTypesDerivedFrom<ViewModel>();

            serializedObject.Update();

            if (_parentView == null) return;
            _viewModelTypeFullName.stringValue = _parentView.ViewModelTypeFullName;
            serializedObject.ApplyModifiedProperties();

            if (string.IsNullOrWhiteSpace(_viewModelTypeFullName.stringValue))
            {
                EditorGUILayout.HelpBox("There is no view model setup on the View. Please check View setup.", MessageType.Warning);
                return;
            }

            DrawProperties();
            DrawViewModelDebug(_viewModelTypeFullName.stringValue);
        }

        private void OnBinderComponentRemoved()
        {
            if(!_binder) return;
            
            _parentView.RemoveBinder(_binder);
            EditorUtility.SetDirty(_parentView);
            _binder = null;
        }

        private void DrawViewModelDebug(string viewModelFullName)
        {
            var viewModelType = GetViewModelType(viewModelFullName);

            if (viewModelType != null)
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField("ViewModel: " + viewModelType.Name);
                GUI.enabled = true;
            }
        }

        protected abstract void DrawProperties();

        protected static bool IsValidProperty(Type propertyType, Type requiredType, Type requiredArgumentType)
        {
            var genericArgument = propertyType.GetGenericArguments().First();
            if (!requiredArgumentType.IsAssignableFrom(genericArgument))
            {
                return false;
            }

            var currentType = propertyType;
            while (currentType != null)
            {
                if (currentType.IsGenericType)
                {
                    var genericTypeDef = currentType.GetGenericTypeDefinition();
                    if (requiredType.IsAssignableFrom(genericTypeDef))
                    {
                        return true;
                    }
                }

                var interfaces = currentType.GetInterfaces().Where(i => i.IsGenericType);
                if (interfaces.Any(interfaceType => requiredType.IsAssignableFrom(interfaceType.GetGenericTypeDefinition())))
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        public static bool IsValidPropertyName(string propertyName, Type viewModelType)
        {
            var property = viewModelType.GetProperty(propertyName);

            return property != null;
        }

        public static bool IsValidMethodName(string methodName, Type viewModelType)
        {
            var method = viewModelType.GetMethod(methodName);

            return method != null;
        }

        public Type GetViewModelType(string viewModelFullName)
        {
            var viewModelType = _cachedViewModelTypes.FirstOrDefault(t => t.FullName == viewModelFullName);

            return viewModelType;
        }
    }
}
