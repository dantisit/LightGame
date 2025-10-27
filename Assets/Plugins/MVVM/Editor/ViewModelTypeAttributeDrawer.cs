using System.Collections.Generic;
using System.Linq;
using MVVM.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(ViewModelTypeAttribute))]
    public class ViewModelTypeAttributeDrawer : PropertyDrawer
    {
        private static TypeCache.TypeCollection _cachedViewModelTypes;
        private static readonly Dictionary<string, string> _viewModelNames = new();
        private static bool _initialized = false;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Ensure this is only used on string properties
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "ViewModelType attribute can only be used on string fields");
                return;
            }
            
            InitializeIfNeeded();
            
            EditorGUI.BeginProperty(position, label, property);
            
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 
                position.width - EditorGUIUtility.labelWidth, position.height);
            
            EditorGUI.LabelField(labelRect, label);
            
            var displayName = string.IsNullOrEmpty(property.stringValue)
                ? MVVMConstants.NONE
                : ViewModelsEditorUtility.ToShortName(property.stringValue, _cachedViewModelTypes);
            
            if (GUI.Button(buttonRect, displayName, EditorStyles.popup))
            {
                ShowViewModelSelectionWindow(property);
            }
            
            EditorGUI.EndProperty();
        }
        
        private static void InitializeIfNeeded()
        {
            if (_initialized) return;
            
            _cachedViewModelTypes = TypeCache.GetTypesDerivedFrom<ViewModel>();
            ViewModelsEditorUtility.DefineAllViewModels(_viewModelNames);
            _initialized = true;
        }
        
        private void ShowViewModelSelectionWindow(SerializedProperty property)
        {
            var provider = ScriptableObject.CreateInstance<StringListSearchProvider>();
            var options = _viewModelNames.Keys.ToArray();
            
            provider.Init(options, result =>
            {
                property.stringValue = _viewModelNames[result];
                property.serializedObject.ApplyModifiedProperties();
            });
            
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}