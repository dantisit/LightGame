using UnityEditor;
using UnityEngine;

namespace UIS {

    [CustomEditor(typeof(Scroller))]
    public class ScrollerEditor : Editor {

        /// <summary>
        /// Scroller target
        /// </summary>
        Scroller _target;

        /// <summary>
        /// Serialized target object
        /// </summary>
        SerializedObject _object;

        SerializedProperty _prefab;
        SerializedProperty _useMultiplePrefabs;
        SerializedProperty _autoCalculateSizes;
        SerializedProperty _topPadding;
        SerializedProperty _bottomPadding;
        SerializedProperty _leftPadding;
        SerializedProperty _rightPadding;
        SerializedProperty _itemSpacing;
        SerializedProperty _parentContainer;
        SerializedProperty _addonViewsCount;

        void OnEnable() {
            _target = (Scroller)target;
            _object = new SerializedObject(target);
            _prefab = _object.FindProperty("Prefab");
            _useMultiplePrefabs = _object.FindProperty("UseMultiplePrefabs");
            _autoCalculateSizes = _object.FindProperty("AutoCalculateSizes");
            _topPadding = _object.FindProperty("TopPadding");
            _bottomPadding = _object.FindProperty("BottomPadding");
            _leftPadding = _object.FindProperty("LeftPadding");
            _rightPadding = _object.FindProperty("RightPadding");
            _itemSpacing = _object.FindProperty("ItemSpacing");
            _parentContainer = _object.FindProperty("ParentContainer");
            _addonViewsCount = _object.FindProperty("AddonViewsCount");
        }

        public override void OnInspectorGUI() {
            _object.Update();
            EditorGUI.BeginChangeCheck();
            
            // Orientation selector
            _target.Type = GUILayout.Toolbar(_target.Type, new string[] { "Vertical", "Horizontal" });
            
            EditorGUILayout.Space();
            
            // Item Settings
            EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_prefab);
            EditorGUILayout.PropertyField(_useMultiplePrefabs);
            EditorGUILayout.PropertyField(_autoCalculateSizes);
            
            EditorGUILayout.Space();
            
            // Padding (show relevant padding based on orientation)
            EditorGUILayout.LabelField("Padding", EditorStyles.boldLabel);
            if (_target.IsVertical) {
                EditorGUILayout.PropertyField(_topPadding);
                EditorGUILayout.PropertyField(_bottomPadding);
            } else {
                EditorGUILayout.PropertyField(_leftPadding);
                EditorGUILayout.PropertyField(_rightPadding);
            }
            EditorGUILayout.PropertyField(_itemSpacing);
            
            EditorGUILayout.Space();
            
            // Other Settings
            EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_parentContainer);
            EditorGUILayout.PropertyField(_addonViewsCount);
            
            EditorGUILayout.Space();
            
            // Info box for pull-to-refresh
            EditorGUILayout.HelpBox(
                "Pull-to-refresh is now an extension. Add ScrollerPullToRefreshExtension component to enable it.",
                MessageType.Info
            );
            
            if (EditorGUI.EndChangeCheck()) {
                _object.ApplyModifiedProperties();
            }
        }
    }
}