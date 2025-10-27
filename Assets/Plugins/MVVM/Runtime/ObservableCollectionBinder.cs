using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObservableCollections;
using Plugins.MVVM.Runtime;
using R3;

namespace MVVM.Binders
{
    public abstract class ObservableCollectionBinder : Binder
    {
        public abstract Type ArgumentType { get; }
    }

    public abstract class ObservableCollectionBinder<T> : ObservableCollectionBinder, IObservableCollectionBinder 
    {
        private static readonly MethodInfo _genericMethodSubscribeByBinder = typeof(ObservableExtensions).GetMethod(nameof(ObservableExtensions.SubscribeByBinderList));
        private static readonly Type _observableListType = typeof(ObservableList<>);

        public override Type ArgumentType => typeof(T);

        private object _observableListObject;

        protected sealed override void BindInternal(ViewModel viewModel)
        {
            OnCollectionClear();
            
            var propertyInfo = viewModel.GetType().GetProperty(PropertyName);
            _observableListObject = propertyInfo.GetValue(viewModel);

            Type objType = _observableListObject.GetType();
            var compositeDisposable = new CompositeDisposable();

            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == _observableListType)
            {
                Type genericArg = objType.GetGenericArguments()[0];
                var genericMethod = _genericMethodSubscribeByBinder.MakeGenericMethod(genericArg);
                genericMethod.Invoke(null, new [] {_observableListObject, this, compositeDisposable}); // expensive call
            }

            compositeDisposable.AddTo(this);
        }

        public abstract void OnItemAdded(T value);
        public abstract void OnItemRemoved(T value);
        public abstract void OnCollectionSort(IList<T> observableList);
        public abstract void OnCollectionClear();
        
        public void AddItem(object value) => OnItemAdded(value is T casted ? casted : default);
        public void RemoveItem(object value) => OnItemRemoved(value is T casted ? casted : default);
        public void Sort<TSort>(ObservableList<TSort> observableList) => OnCollectionSort(observableList.OfType<T>().ToList());
        public void Clear() => OnCollectionClear();
    }
}
