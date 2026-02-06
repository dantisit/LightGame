using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVM.Binders;
using MVVM.Utils;
using ObservableCollections;
using Plugins.MVVM.Runtime;
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
    public class BinderView : MonoBehaviour, IView
    {
        public ViewModel ViewModel { get; protected set; }
        public bool DisposeOnDestroy { get; set; } = true;
        
        private static readonly MethodInfo _castMethod = typeof(ObservableExtensions)
            .GetMethod(nameof(ObservableExtensions.Cast));
        
        [SerializeField, ViewModelType, HideInInspector] private string _viewModelTypeFullName;
        [SerializeField, HideInInspector] private string _viewModelPropertyName;
        [SerializeField, HideInInspector] private int _viewModelIndex;
        [SerializeField, HideInInspector] private bool _disableWithoutViewModel;
        [SerializeField, HideInInspector] private bool _isParentView;
        [SerializeField, HideInInspector] private List<BinderView> _subViews = new();
        [SerializeField, HideInInspector] private List<Binder> _childBinders = new();

        public string ViewModelTypeFullName => _viewModelTypeFullName;

        public string ViewModelPropertyName => _viewModelPropertyName;
        public int ViewModelIndex => _viewModelIndex;
        
        public virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(ClearNulls()) EditorUtility.SetDirty(this);

            // Refresh parent view registration when component is modified in editor
            if (Application.isPlaying) return; // Only in edit mode
            
            HandleParentViewRegistration(shouldRegister: true);
            RefreshChildBinders();
            RefreshSubViews(); 
#endif            
        }

        public virtual void BindViewModel(ViewModel viewModel, bool asParent = true)
        {
            _isParentView = asParent;
            var targetViewModel = ResolveTargetViewModel(viewModel);
            OnViewModelChanged(targetViewModel);
            OnBindViewModel(targetViewModel);
        }
        
        protected virtual void OnBindViewModel(ViewModel viewModel) { }

        protected virtual void OnViewModelChanged(ViewModel newViewModel)
        {
            // Dispose previous ViewModel if it exists
            if (ViewModel != null && ViewModel.Disposables != null && DisposeOnDestroy)
            {
                ViewModel.Disposables.Dispose();
            }
            
            ViewModel = newViewModel;

            if (ViewModel == null)
            {
                if(_disableWithoutViewModel) gameObject.SetActive(false);
                return;
            }
            if(_disableWithoutViewModel) gameObject.SetActive(true);
            BindSubViews(newViewModel);
            BindChildBinders(newViewModel);
        }

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


        private ViewModel HandleEnumerableValue(IEnumerable enumerable)
        {
            var item = enumerable.Cast<object>().ElementAtOrDefault(_viewModelIndex);
            return item as ViewModel;
        }

        private void BindSubViews(ViewModel targetViewModel)
        {
            foreach (var subView in _subViews.Where(sv => !sv._isParentView))
            {
                subView.BindViewModel(targetViewModel, false);
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


        protected virtual void OnDestroy()
        {
            if (ViewModel != null && DisposeOnDestroy)
            {
                ViewModel.Disposables.Dispose();
            }
        }
        
        public void Destroy() => Destroy(gameObject);
        
        protected BindBuilder<TValue> Bind<TValue>(Observable<TValue> source)
        {
            return new BindBuilder<TValue>(source, gameObject);
        }
        
        protected BindBuilder<(T1, T2)> Bind<T1, T2>(
            Observable<T1> source1, 
            Observable<T2> source2)
        {
            var combined = source1.CombineLatest(source2, (x, y) => (x, y));
            return new BindBuilder<(T1, T2)>(combined, gameObject);
        }
        
        protected BindBuilder<TResult> Bind<T1, T2, TResult>(
            Observable<T1> source1, 
            Observable<T2> source2,
            Func<T1, T2, TResult> combiner)
        {
            var combined = source1.CombineLatest(source2, combiner);
            return new BindBuilder<TResult>(combined, gameObject);
        }
        
        protected BindBuilder<(T1, T2, T3)> Bind<T1, T2, T3>(
            Observable<T1> source1, 
            Observable<T2> source2, 
            Observable<T3> source3)
        {
            var combined = source1.CombineLatest(source2, source3, (x, y, z) => (x, y, z));
            return new BindBuilder<(T1, T2, T3)>(combined, gameObject);
        }

        /// <summary>
        /// Start fluent virtual list builder with a single item
        /// Chain with .Bind() or .BindCollection() and end with .ToScroller() or other terminal methods
        /// Size is auto-calculated from prefab's RectTransform
        /// </summary>
        protected BindVirtualList Bind(object viewModel, GameObject prefab)
        {
            var list = new BindVirtualList(gameObject);
            list.Bind(viewModel, prefab);
            return list;
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
            var allFoundSubViews = gameObject.GetComponentsInChildren<BinderView>(true);
            foreach (var foundSubView in allFoundSubViews)
            {
                var parentView = foundSubView.GetComponentsInParent<BinderView>(true)
                    .FirstOrDefault(c => !ReferenceEquals(c, foundSubView));

                if (parentView == this)
                {
                    RegisterView(foundSubView);
                }
            }
        }

        private void RegisterView(BinderView binderView)
        {
            if (!_subViews.Contains(binderView))
            {
                _subViews.Add(binderView);
            }
        }

        private void RemoveView(BinderView binderView) => _subViews.Remove(binderView);

        private void HandleParentViewRegistration(bool shouldRegister)
        {
            var parentTransform = transform.parent;
            if (!parentTransform) return;

            var parentView = parentTransform.GetComponentInParent<BinderView>(true);
            if (parentView == null) return;

            if (shouldRegister)
                parentView.RegisterView(this);
            else
                parentView.RemoveView(this);
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
