using System;
using MVVM.Binders;
using R3;
using UnityEditor;

namespace MVVM.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ObservableBinder), true)]
    public class ObservableBinderEditor : ObservableBinderBase
    {
        private ObservableBinder _observableBinder;
        protected override SerializedProperty _propertyName { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            _observableBinder = (ObservableBinder)target;
            _propertyName = serializedObject.FindProperty(nameof(_propertyName));
        }

        protected override bool IsValidProperty(Type propertyType)
        {
            var requiredArgumentType = _observableBinder.ArgumentType;
            var requiredType = typeof(Observable<>);

            return IsValidProperty(propertyType, requiredType, requiredArgumentType);
        }
    }
}