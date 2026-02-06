using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVM.Binders
{
    public abstract class Binder : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string _viewModelTypeFullName;
        [SerializeField, HideInInspector] private string _propertyName;

        public string ViewModelTypeFullName => _viewModelTypeFullName;
        public string PropertyName => _propertyName;

        protected ViewModel ViewModel { get; set; }

        private void Start() => OnStart();

        protected virtual void OnStart() { }

        private void OnDestroy() =>  OnDestroyed();

        protected virtual void OnDestroyed() { }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            var parentView = GetComponentInParent<BinderView>(true);
            if(parentView == null) return;
            parentView.RegisterBinder(this);
        }
#endif

        public virtual void Bind(ViewModel viewModel)
        {
            ViewModel = viewModel;
            BindInternal(viewModel);
        }

        public virtual void AfterBind() {}

        protected abstract void BindInternal(ViewModel viewModel);
    }
}
