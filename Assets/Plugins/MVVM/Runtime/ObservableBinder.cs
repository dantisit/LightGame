using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Object = System.Object;

namespace MVVM.Binders
{
    public abstract class ObservableBinder : Binder
    {
        public abstract Type ArgumentType { get; }
    }

    public abstract class ObservableBinder<T> : ObservableBinder
    {
        protected string AssertionContext => $"at ObservableBinder<{typeof(T)}> of {gameObject.name}";

        public override Type ArgumentType => typeof(T);

        protected T _valueWhileDeactivated = default(T);
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                _subject?.OnNext(value);
            }
        }

        private ISubject<T>? _subject;

        protected sealed override void BindInternal(ViewModel viewModel)
        {
            Assert.IsFalse(PropertyName.Length == 0, $"PropertyName is empty on {GetPathToRoot()}");
            Assert.IsNotNull(viewModel, $"ViewModel is null on {GetPathToRoot()}");
            var property = viewModel.GetType().GetProperty(PropertyName);
            Assert.IsNotNull(property, $"Property {PropertyName} not found in {viewModel.GetType().FullName} on {GetPathToRoot()}");
            var observableObject = property.GetValue(viewModel);

            if (observableObject == null) return;
            
            if (observableObject is not Observable<T> directObservable)
            {
                // Handle case when generic parameter of observableObject is derived class of T
                var observableType = observableObject.GetType();
                var observableResultType = GetObservableResultType(observableType);

                var cast = ObservableExtensions.CastMethodInfo.MakeGenericMethod(observableResultType, typeof(T));
                directObservable = (Observable<T>)cast.Invoke(null, new[] { observableObject });
            }

            switch (directObservable)
            {
                case ReactiveProperty<T> { IsCompletedOrDisposed: true }:
                    Debug.Log($"[MVVM] Can't subscribe to completed or disposed reactive property: {PropertyName}");
                    return;
                case Subject<T> { IsDisposed: true }:
                    Debug.Log($"[MVVM] Can't subscribe to completed or disposed subject property: {PropertyName}");
                    return;
            }

            directObservable.Subscribe(ChangeProperty).AddTo(ViewModel.Disposables);
            if (directObservable is ISubject<T> subject) _subject = subject;
        }
        
        private static Type GetObservableResultType(Type observableType)
        {
            while (observableType != null && observableType != typeof(object))
            {
                if (observableType.IsGenericType &&
                    observableType.GetGenericTypeDefinition() == typeof(Observable<>))
                {
                    return observableType.GetGenericArguments()[0]; // TResult
                }

                observableType = observableType.BaseType;
            }

            throw new InvalidOperationException("Type does not derive from Observable<T>");
        }

        private void ChangeProperty(T newValue)
        {
            if(!enabled) return;
            _value = newValue;
            if(!gameObject.activeInHierarchy) _valueWhileDeactivated = newValue;
            OnPropertyChanged(newValue);
        }
        
        public virtual void OnPropertyChanged(T newValue)
        {
            
        }

        private void OnEnable()
        {
            if (EqualityComparer<T>.Default.Equals(_valueWhileDeactivated, default)) return;
            
            OnPropertyChanged(_valueWhileDeactivated);
            _valueWhileDeactivated = default;
        }

        private string GetPathToRoot()
        {
            var path = name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
