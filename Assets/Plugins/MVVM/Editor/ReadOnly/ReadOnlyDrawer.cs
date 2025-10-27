using UnityEditor;
using UnityEngine;

namespace MVVM.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            
            GUI.enabled = false;
            
            EditorGUI.PropertyField(position, property, label);
            
            GUI.enabled = wasEnabled;
        }
    }
}