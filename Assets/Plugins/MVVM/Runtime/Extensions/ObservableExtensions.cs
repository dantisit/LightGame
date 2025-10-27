using System;
using System.Reflection;
using MVVM.Binders;
using ObservableCollections;
using Plugins.MVVM.Runtime;
using R3;

namespace MVVM
{
    public static class ObservableExtensions
    {
        public static readonly MethodInfo CastMethodInfo = typeof(ObservableExtensions)
            .GetMethod(nameof(Cast), BindingFlags.Public | BindingFlags.Static);

        
        public static IDisposable SubscribeByBinder<T>(this Observable<T> observable, ObservableBinder<object> binder)
        {
            return observable.Subscribe(x => binder.OnPropertyChanged(x));
        }
        
        public static Observable<TTo> Cast<TFrom, TTo>(this Observable<TFrom> observable) where TTo : class 
        {
            return observable.Select(x => x as TTo);
        }

        public static void SubscribeByBinderList<T>(this ObservableList<T> observableList, IObservableCollectionBinder binder, CompositeDisposable compositeDisposable)
        {
            observableList.ObserveAdd().Subscribe(x => binder.AddItem(x.Value)).AddTo(compositeDisposable);
            observableList.ObserveRemove().Subscribe(x => binder.RemoveItem(x.Value)).AddTo(compositeDisposable);
            observableList.ObserveSort().Subscribe(_ => binder.Sort(observableList)).AddTo(compositeDisposable);
            observableList.ObserveClear().Subscribe(_ => binder.Clear()).AddTo(compositeDisposable);

            foreach (var item in observableList) binder.AddItem(item);
        }
    }
}
