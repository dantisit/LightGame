using System;
using ObservableCollections;
using Plugins.MVVM.Runtime.Operators;
using Plugins.MVVM.Runtime.UIComponents.Scroll;
using R3;
using UIS;
using UnityEngine;

namespace MVVM
{
    public class BindBuilder<TValue> 
    {
        private readonly Observable<TValue> _source;
        private readonly GameObject _disposeTarget;
        
        internal Observable<TValue> Source => _source;
        internal GameObject DisposeTarget => _disposeTarget;

        public BindBuilder(Observable<TValue> source, GameObject disposeTarget)
        {
            _source = source;
            _disposeTarget = disposeTarget;
        }
        
        public BindBuilder<TResult> Select<TResult>(Func<TValue, TResult> selector)
        {
            var transformed = _source.Select(selector);
            return new BindBuilder<TResult>(transformed, _disposeTarget);
        }
        
        public void To<TTarget>(TTarget target, Action<TTarget, TValue> setter)
        {
            if(target == null) return;
            _source.Subscribe(v => setter(target, v)).AddTo(_disposeTarget);
        }
        
        public void To(Action<TValue> setter)
        {
            _source.Subscribe(setter).AddTo(_disposeTarget);
        }
    }
    
    public static class BindBuilderExtensions
    {
        public static IDisposable ToView<TViewModel, TView>(this BindBuilder<TViewModel> builder, 
            TView prefab,
            Transform parent,
            System.Action<ViewModel, TView> onCreated = null,
            System.Action<ViewModel, TView> onDestroyed = null)
            where TView : BinderView
            where TViewModel : ViewModel
        {
            TView currentChild = null;
            
            return builder.Source.Subscribe(newViewModel =>
            {
                // Destroy previous child if it exists
                if (currentChild != null)
                {
                    var oldViewModel = currentChild.ViewModel;
                    onDestroyed?.Invoke(oldViewModel, currentChild);
                    
                    if (currentChild.gameObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(currentChild.gameObject);
                    }
                    currentChild = null;
                }
                
                // Create new child if ViewModel is not null
                if (newViewModel != null)
                {
                    currentChild = UnityEngine.Object.Instantiate(prefab, parent);
                    currentChild.BindViewModel(newViewModel);
                    onCreated?.Invoke(newViewModel, currentChild);
                }
            }).AddTo(builder.DisposeTarget);
        }
        
        public static BindBuilder<TResult> Select<T1, T2, TResult>(this BindBuilder<(T1, T2)> builder, Func<T1, T2, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, TResult>(this BindBuilder<(T1, T2, T3)> builder, Func<T1, T2, T3, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, TResult>(this BindBuilder<(T1, T2, T3, T4)> builder, Func<T1, T2, T3, T4, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, TResult>(this BindBuilder<(T1, T2, T3, T4, T5)> builder, Func<T1, T2, T3, T4, T5, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, T6, TResult>(this BindBuilder<(T1, T2, T3, T4, T5, T6)> builder, Func<T1, T2, T3, T4, T5, T6, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, T6, T7, TResult>(this BindBuilder<(T1, T2, T3, T4, T5, T6, T7)> builder, Func<T1, T2, T3, T4, T5, T6, T7, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this BindBuilder<(T1, T2, T3, T4, T5, T6, T7, T8)> builder, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this BindBuilder<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> builder, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9));
        }
        
        public static BindBuilder<TResult> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this BindBuilder<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> builder, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> selector)
        {
            return builder.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10));
        }
        
        // To extensions for tuple BindBuilders - allows passing separate parameters instead of tuple
        public static void To<T1, T2>(this BindBuilder<(T1, T2)> builder, Action<T1, T2> setter)
        {
            builder.To(t => setter(t.Item1, t.Item2));
        }

        public static void To<T1, T2, T3>(this BindBuilder<(T1, T2, T3)> builder, Action<T1, T2, T3> setter)
        {
            builder.To(t => setter(t.Item1, t.Item2, t.Item3));
        }

        public static void To<T1, T2, T3, T4>(this BindBuilder<(T1, T2, T3, T4)> builder, Action<T1, T2, T3, T4> setter)
        {
            builder.To(t => setter(t.Item1, t.Item2, t.Item3, t.Item4));
        }

        public static void To<T1, T2, T3, T4, T5>(this BindBuilder<(T1, T2, T3, T4, T5)> builder, Action<T1, T2, T3, T4, T5> setter)
        {
            builder.To(t => setter(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
    }
    
    public class CollectionBindBuilder<T> where  T : ViewModel
    {
        private readonly ObservableList<T> _collection;
        private readonly BinderView _binderView;
        
        public CollectionBindBuilder(ObservableList<T> collection, BinderView binderView)
        {
            _collection = collection;
            _binderView = binderView;
        }
        
        public TypedCollectionBindBuilder<T, TView> ToViews<TView>(TView prefab, Transform parent) where TView : BinderView 
        {
            var op = _collection.SyncToGameObjects(prefab, parent);
            op.OnAdd((vm, v) => v.BindViewModel(vm));
            var subscription = op.AddTo(_binderView);
            return new TypedCollectionBindBuilder<T, TView>(op, subscription);
        }
    }
    
    public class TypedCollectionBindBuilder<T, TView> : IDisposable where TView : BinderView
    {
        private readonly SyncToGameObjectOperator<T, TView> _operator;
        private readonly IDisposable _subscription;
        
        public TypedCollectionBindBuilder(SyncToGameObjectOperator<T, TView> op, IDisposable subscription)
        {
            _operator = op;
            _subscription = subscription;
        }
        
        public TypedCollectionBindBuilder<T, TView> OnAdd(Action<T, TView> callback)
        {
            _operator.OnAdd(callback);
            return this;
        }
        
        public TypedCollectionBindBuilder<T, TView> OnRemove(Action<T, TView> callback)
        {
            _operator.OnRemove(callback);
            return this;
        }
        
        public TypedCollectionBindBuilder<T, TView> OnChanged(System.Action callback)
        {
            _operator.OnChanged(callback);
            return this;
        }
        
        public TypedCollectionBindBuilder<T, TView> OnSort(System.Action callback)
        {
            _operator.OnSort(callback);
            return this;
        }
        
        public TypedCollectionBindBuilder<T, TView> OnClear(System.Action callback)
        {
            _operator.OnClear(callback);
            return this;
        }
        
        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
