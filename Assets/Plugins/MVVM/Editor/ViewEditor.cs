using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVM.Utils;
using MVVM.Binders;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using Binder = MVVM.Binders.Binder;

namespace MVVM.Editor
{
    [CustomEditor(typeof(View))]
    public class ViewEditor : UnityEditor.Editor
    {
        #region Fields

        private SerializedProperty _viewModelTypeFullName;
        private SerializedProperty _viewModelPropertyName;
        private SerializedProperty _viewModelIndex;
        private SerializedProperty _isParentView;
        private SerializedProperty _disableWithoutViewModel;
        private SerializedProperty _childBinders;
        private SerializedProperty _subViews;
        private View _view;
        private TypeCache.TypeCollection _cachedViewModelTypes;
        private readonly Dictionary<string, string> _viewModelNames = new();
        private readonly List<string> _viewModelPropertyNames = new();
        private bool _childBindersFoldout = true;
        private bool _subViewsFoldout = true;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            _view = (View)target;
            _viewModelTypeFullName = serializedObject.FindProperty(nameof(_viewModelTypeFullName));
            _viewModelPropertyName = serializedObject.FindProperty(nameof(_viewModelPropertyName));
            _viewModelIndex = serializedObject.FindProperty(nameof(_viewModelIndex));
            _isParentView = serializedObject.FindProperty(nameof(_isParentView));
            _disableWithoutViewModel = serializedObject.FindProperty(nameof(_disableWithoutViewModel));
            _childBinders = serializedObject.FindProperty(nameof(_childBinders));
            _subViews = serializedObject.FindProperty(nameof(_subViews));
        }

        public override void OnInspectorGUI()
        {
            _cachedViewModelTypes = TypeCache.GetTypesDerivedFrom<ViewModel>();

            DrawScriptTitle();

            var parentView = _view.GetComponentsInParent<View>(true).FirstOrDefault(c => !ReferenceEquals(c, _view));
            var provider = CreateInstance<StringListSearchProvider>();
            var isParentViewExist = parentView != null;
            var parentViewGo = isParentViewExist ? parentView.gameObject : _view.gameObject;



            if (isParentViewExist && !string.IsNullOrEmpty(parentView.ViewModelTypeFullName))
            {
                HandleSubView(provider, parentView, parentViewGo);
            }
            else
            {
                HandleParentView(provider);
            }

            if (!_view.IsValidSetup())
            {
                DrawFixButton();
            }
        }

        #endregion

        #region View Model Type Handling

        protected Type GetViewModelType(string viewModelTypeFullName)
        {
            return _cachedViewModelTypes.FirstOrDefault(t => t.FullName == viewModelTypeFullName);
        }

        private void HandleSubView(StringListSearchProvider provider, View parentView, GameObject parentViewGo)
        {
            SetParentViewBoolean(false);
            DefineAllViewModelPropertyNames(parentView.ViewModelTypeFullName);
            DrawEditorForSubView(provider, parentView.ViewModelTypeFullName);
            DrawDebug();

            var childViewModelType = GetChildViewModelType(parentView.ViewModelTypeFullName, _view.ViewModelPropertyName);
            DrawSubViewModelDebugButtons(parentViewGo, childViewModelType?.FullName);
        }

        private void HandleParentView(StringListSearchProvider provider)
        {
            SetParentViewBoolean(true);
            
            EditorGUILayout.PropertyField(_viewModelTypeFullName, new GUIContent("View Model"));
            
            DrawIndexField();
            DrawDisableWithoutViewModelField();
            DrawDebug();
            DrawOpenViewModelButton(_view.ViewModelTypeFullName);
        }

        #endregion

        #region Property Drawing

        private void DrawEditorForSubView(StringListSearchProvider provider, string parentViewModelTypeFullName)
        {
            var options = _viewModelPropertyNames.ToArray();

            provider.Init(options, result =>
            {
                _viewModelPropertyName.stringValue = result == MVVMConstants.NONE ? null : result;

                if (result != MVVMConstants.NONE)
                {
                    _viewModelTypeFullName.stringValue = GetChildViewModelType(parentViewModelTypeFullName, result)?.FullName;
                }

                serializedObject.ApplyModifiedProperties();
            });

            DrawPropertyNameField(parentViewModelTypeFullName, provider);
            DrawIndexField();
            DrawDisableWithoutViewModelField();
        }

        private void DrawPropertyNameField(string parentViewModelTypeFullName, StringListSearchProvider provider)
        {
            EditorGUILayout.BeginHorizontal();

            var parentViewModelType = GetViewModelType(parentViewModelTypeFullName);
            var property = !string.IsNullOrEmpty(_viewModelPropertyName.stringValue) && parentViewModelType != null
                ? parentViewModelType.GetProperty(_viewModelPropertyName.stringValue)
                : null;
            var childPropertyType = property != null ? GetChildViewModelType(parentViewModelTypeFullName, _viewModelPropertyName.stringValue) : null;
            var propertyTypeName = childPropertyType != null ? $" ({childPropertyType.Name})" : string.Empty;

            EditorGUILayout.LabelField($"Property Name{propertyTypeName}:");

            var displayName = string.IsNullOrEmpty(_viewModelPropertyName.stringValue)
                ? MVVMConstants.NONE
                : _viewModelPropertyName.stringValue;

            if (GUILayout.Button(displayName, EditorStyles.popup))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_viewModelPropertyName.stringValue) && property == null)
            {
                EditorGUILayout.HelpBox(
                    $"Property {_viewModelPropertyName.stringValue} is not found on {parentViewModelTypeFullName}",
                    MessageType.Error);
            }
        }

        private void DrawEditorForParentView(StringListSearchProvider provider)
        {
            var options = _viewModelNames.Keys.ToArray();

            provider.Init(options, result =>
            {
                _viewModelTypeFullName.stringValue = _viewModelNames[result];
                serializedObject.ApplyModifiedProperties();
            });

            DrawViewModelField(provider);
            DrawIndexField();
            DrawDisableWithoutViewModelField();
        }

        private void DrawViewModelField(StringListSearchProvider provider)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(MVVMConstants.VIEW_MODEL);

            var displayName = string.IsNullOrEmpty(_viewModelTypeFullName.stringValue)
                ? MVVMConstants.NONE
                : ViewModelsEditorUtility.ToShortName(_viewModelTypeFullName.stringValue, _cachedViewModelTypes);

            if (GUILayout.Button(displayName, EditorStyles.popup))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawIndexField()
        {
            EditorGUILayout.BeginHorizontal();
            _viewModelIndex.intValue = EditorGUILayout.IntField("Index:", _viewModelIndex.intValue);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDisableWithoutViewModelField()
        {
            EditorGUILayout.PropertyField(_disableWithoutViewModel);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region View Model Property Management

        private void SetParentViewBoolean(bool isParentView)
        {
            if (Application.isPlaying) return;

            _isParentView.boolValue = isParentView;
            serializedObject.ApplyModifiedProperties();
        }

        private void DefineAllViewModelPropertyNames(string parentViewModelTypeFullName)
        {
            _viewModelPropertyNames.Clear();
            _viewModelPropertyNames.Add(MVVMConstants.NONE);

            var parentViewModelType = GetViewModelType(parentViewModelTypeFullName);
            var allViewModelProperties = parentViewModelType.GetProperties();
            var allValidProperties =
                allViewModelProperties.Where(p =>
                    typeof(ViewModel).IsAssignableFrom(p.PropertyType) ||
                    IsIEnumerable(p) ||
                    (ReactivePropertyUtils.IsReactiveProperty(p.PropertyType) &&
                     typeof(ViewModel).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0])));

            foreach (var validProperty in allValidProperties)
            {
                _viewModelPropertyNames.Add(validProperty.Name);
            }
        }

        private Type GetChildViewModelType(string parentViewModelTypeFullName, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            var parentViewModelType = GetViewModelType(parentViewModelTypeFullName);
            if (parentViewModelType == null) return null;

            var property = parentViewModelType.GetProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"Property {propertyName} is not found on {parentViewModelTypeFullName} for {_view.gameObject.name}");
                return null;
            }

            if (IsIEnumerable(property))
            {
                return property.PropertyType.GetGenericArguments()[0];
            }

            if (ReactivePropertyUtils.IsReactiveProperty(property.PropertyType))
            {
                var genericArg = property.PropertyType.GetGenericArguments()[0];
                return typeof(ViewModel).IsAssignableFrom(genericArg) ? genericArg : null;
            }

            return property.PropertyType;
        }

        #endregion

        #region UI Drawing

        private void DrawScriptTitle()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MVVMConstants.SCRIPT, MonoScript.FromMonoBehaviour((View)target), typeof(View), false);
            GUI.enabled = true;
        }

        private void DrawDebug()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_isParentView);
            DrawSubViewsList();
            DrawChildBindersList();
            GUI.enabled = true;
        }

        private void DrawPingParentViewButton(GameObject parentViewGo)
        {
            if (parentViewGo != null && GUILayout.Button(MVVMConstants.HIGHLIGHT_PARENT_VIEW))
            {
                EditorGUIUtility.PingObject(parentViewGo);
            }
        }

        private void DrawOpenViewModelButton(string viewModelTypeFullName)
        {
            if (string.IsNullOrEmpty(viewModelTypeFullName)) return;

            var viewModelType = GetViewModelType(viewModelTypeFullName);
            if (viewModelType == null) return;

            if (GUILayout.Button($"Open {viewModelType.Name}"))
            {
                OpenScript(viewModelType.Name);
            }
        }

        private void DrawSubViewModelDebugButtons(GameObject parentViewGo, string viewModelTypeFullName)
        {
            EditorGUILayout.BeginHorizontal();
            DrawPingParentViewButton(parentViewGo);
            DrawOpenViewModelButton(viewModelTypeFullName);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFixButton()
        {
            EditorGUILayout.HelpBox("Some binders or sub views are missing. Please, fix it", MessageType.Warning);

            if (GUILayout.Button("Fix"))
            {
                _view.Fix();
            }
        }

        private void DrawChildBindersList()
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = true; // allow foldout interaction
            _childBindersFoldout = EditorGUILayout.Foldout(_childBindersFoldout, $"Child Binders ({_childBinders.arraySize})", true);
            GUI.enabled = prevEnabled;

            if (!_childBindersFoldout) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < _childBinders.arraySize; i++)
            {
                var element = _childBinders.GetArrayElementAtIndex(i);
                var binder = element.objectReferenceValue as Binder;

                if (binder == null)
                {
                    EditorGUILayout.ObjectField($"[{i}]", null, typeof(Binder), true);
                    continue;
                }

                // Get the editor for this binder
                var viewType = GetViewModelType(_view.ViewModelTypeFullName);
                var isValid = BinderEditor.IsValidPropertyName(binder.PropertyName, viewType) ||
                              BinderEditor.IsValidMethodName(binder.PropertyName, viewType);

                // Draw UI
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField($"[{i}] {binder.GetType().Name}", binder, typeof(Binder), true);

                if (!isValid)
                {
                    var prev = GUI.color;
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField($"{binder.PropertyName} (invalid)");
                    GUI.color = prev;
                }
                else
                {
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(binder.PropertyName) ? "[no property]" : binder.PropertyName);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawSubViewsList()
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = true;
            _subViewsFoldout = EditorGUILayout.Foldout(_subViewsFoldout, $"Sub Views ({_subViews.arraySize})", true);
            GUI.enabled = prevEnabled;

            if (!_subViewsFoldout) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < _subViews.arraySize; i++)
            {
                var element = _subViews.GetArrayElementAtIndex(i);
                var view = element.objectReferenceValue as View;

                if (view == null)
                {
                    EditorGUILayout.ObjectField($"[{i}]", null, typeof(View), true);
                    continue;
                }

                string propertyName = view.ViewModelPropertyName;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField($"[{i}] {view.GetType().Name}", view, typeof(View), true);
                EditorGUILayout.LabelField(string.IsNullOrEmpty(propertyName) ? "[no property]" : propertyName);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Utility Methods

        private void OpenScript(string typeName)
        {
            var guids = AssetDatabase.FindAssets($"t: script {typeName}");

            if (guids.Length > 0)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorUtility.OpenWithDefaultApp(scriptPath);
            }
            else
            {
                Debug.LogError($"No script found for type: {typeName}");
            }
        }

        private static bool IsIEnumerable(PropertyInfo property)
        {
            return typeof(IEnumerable).IsAssignableFrom(property.PropertyType) ||
                   (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        #endregion
    }
}
