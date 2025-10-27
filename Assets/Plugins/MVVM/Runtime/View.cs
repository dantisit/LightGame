using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVM.Binders;
using MVVM.Utils;
using R3;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Binder = MVVM.Binders.Binder;

namespace MVVM
{
    /// <summary>
    /// Represents a View component in the MVVM architecture, handling the binding between Unity GameObjects and ViewModels.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class View : MonoBehaviour
    {
        private static readonly MethodInfo _castMethod = typeof(ObservableExtensions)
            .GetMethod(nameof(ObservableExtensions.Cast));
        
        [SerializeField, ViewModelType] private string _viewModelTypeFullName;
        [SerializeField] private string _viewModelPropertyName;
        [SerializeField] private int _viewModelIndex;
        [SerializeField] private bool _disableWithoutViewModel;
        [SerializeField] private bool _isParentView;
        [SerializeField] private List<View> _subViews = new();
        [SerializeField] private List<Binder> _childBinders = new();

        public ViewModel? ViewModel => _viewModel;
        public bool DisposeOnDestroy { get; set; } = true;

        public string ViewModelTypeFullName => _viewModelTypeFullName;

        public string ViewModelPropertyName => _viewModelPropertyName;
        public int ViewModelIndex => _viewModelIndex;

        private ViewModel? _viewModel;

#if UNITY_EDITOR
        private void Start() => HandleParentViewRegistration(shouldRegister: true);
        private void OnDestroy() => HandleParentViewRegistration(shouldRegister: false);
#endif

        public void Bind(ViewModel viewModel, bool asParent = true)
        {
            _isParentView = asParent;
            var targetViewModel = ResolveTargetViewModel(viewModel);
            OnViewModelChanged(targetViewModel);
        }

        /// <summary>
        /// Destroys the view's GameObject.
        /// </summary>
        public void Destroy() => Destroy(gameObject);

        private ViewModel ResolveTargetViewModel(ViewModel viewModel)
        {
            if (_isParentView) return viewModel;
            if (_viewModelPropertyName == "") return null;

            var property = viewModel.GetType().GetProperty(_viewModelPropertyName);
            Assert.IsNotNull(property, $"Property {_viewModelPropertyName} is not found on {viewModel.GetType()} for {gameObject.name}");
            var value = property.GetValue(viewModel);

            if (value == null) return null;

            if (ReactivePropertyUtils.IsReactiveProperty(value.GetType()))
            {
                return HandleReactivePropertyValueWithCast(value);
            }

            if (value is IEnumerable enumerable)
            {
                return HandleEnumerableValue(enumerable);
            }

            return value as ViewModel;
        }

        private ViewModel HandleReactivePropertyValueWithCast(object reactiveProperty)
        {
            if (reactiveProperty == null) return null;
            
            var reactivePropertyType = reactiveProperty.GetType();
            var genericArgType = reactivePropertyType.GetGenericArguments().FirstOrDefault(x => typeof(ViewModel).IsAssignableFrom(x));

            if (genericArgType == null) return null;
            
            var cast = ObservableExtensions.CastMethodInfo.MakeGenericMethod(genericArgType, typeof(ViewModel));
            var castedObservable = (Observable<ViewModel>)cast.Invoke(null, new[] { reactiveProperty });
        
            return SubscribeAndGetCurrentValue(castedObservable);

        }

        private ViewModel SubscribeAndGetCurrentValue(Observable<ViewModel> observable)
        {
            ViewModel currentValue = null;
    
            observable.Subscribe(vm => 
            {
                currentValue = vm;
                OnViewModelChanged(vm);
            }).AddTo(gameObject); // or whatever disposal mechanism you're using

            return currentValue;
        }

        private void OnViewModelChanged(ViewModel newViewModel)
        {
            //_viewModel?.Disposables.Dispose(); currently card info  will not work with this
            _viewModel = newViewModel;
            if(DisposeOnDestroy) _viewModel?.Disposables.AddTo(this);

            if (_viewModel == null)
            {
                if(_disableWithoutViewModel) gameObject.SetActive(false);
                return;
            }
            if(_disableWithoutViewModel) gameObject.SetActive(true);
            BindSubViews(newViewModel);
            BindChildBinders(newViewModel);
        }

        private ViewModel HandleEnumerableValue(IEnumerable enumerable)
        {
            var item = enumerable.Cast<object>().ElementAtOrDefault(_viewModelIndex);
            return item as ViewModel;
        }

        private void BindSubViews(ViewModel targetViewModel)
        {
            foreach (var subView in _subViews.Where(sv => !sv._isParentView))
            {
                subView.Bind(targetViewModel, false);
            }
        }

        private void BindChildBinders(ViewModel targetViewModel)
        {
            foreach (var binder in _childBinders)
            {
                Assert.IsNotNull(binder, "Binder is null at object: " + gameObject.name);
                Assert.IsNotNull(targetViewModel, "Target view model is null at object: " + gameObject.name);
                binder.Bind(targetViewModel);
                binder.AfterBind();
            }
        }


#if UNITY_EDITOR
        public void RegisterBinder(Binder binder)
        {
            if (!_childBinders.Contains(binder))
            {
                _childBinders.Add(binder);
            }
        }

        public void RemoveBinder(Binder binder) => _childBinders.Remove(binder);

        public bool IsValidSetup() => !_childBinders.Contains(null) && !_subViews.Contains(null);

        [ContextMenu("Force Fix")]
        public void Fix()
        {
            RefreshChildBinders();
            RefreshSubViews();
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Index To Sibling Index")]
        public void SetIndexToSiblingIndex()
        {
            _viewModelIndex = transform.GetSiblingIndex();
            EditorUtility.SetDirty(this);
        }

        private void RefreshChildBinders()
        {
            _childBinders.Clear();
            var allFoundChildBinders = gameObject.GetComponentsInChildren<Binder>(true);
            foreach (var foundChildBinder in allFoundChildBinders)
            {
                if (foundChildBinder.ViewModelTypeFullName == ViewModelTypeFullName)
                {
                    RegisterBinder(foundChildBinder);
                }
            }
        }

        private void RefreshSubViews()
        {
            _subViews.Clear();
            var allFoundSubViews = gameObject.GetComponentsInChildren<View>(true);
            foreach (var foundSubView in allFoundSubViews)
            {
                var parentView = foundSubView.GetComponentsInParent<View>(true)
                    .FirstOrDefault(c => !ReferenceEquals(c, foundSubView));

                if (parentView == this)
                {
                    RegisterView(foundSubView);
                }
            }
        }

        private void RegisterView(View view)
        {
            if (!_subViews.Contains(view))
            {
                _subViews.Add(view);
            }
        }

        private void RemoveView(View view) => _subViews.Remove(view);

        private void HandleParentViewRegistration(bool shouldRegister)
        {
            var parentTransform = transform.parent;
            if (!parentTransform) return;

            var parentView = parentTransform.GetComponentInParent<View>(true);
            if (parentView == null) return;

            if (shouldRegister)
                parentView.RegisterView(this);
            else
                parentView.RemoveView(this);
        }

        private void OnValidate()
        {
            if(ClearNulls()) EditorUtility.SetDirty(this); 
        }

        public bool ClearNulls()
        {
            var anyNull = _childBinders.Any(x => x == null) || _subViews.Any(x => x == null);
            _childBinders.RemoveAll(b => b == null);
            _subViews.RemoveAll(b => b == null);
            return anyNull;
        }
#endif
    }
}
