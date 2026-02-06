using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Plugins.MVVM.Runtime.Operators;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM
{
    public static class ObservableListExtensions
    {
        public static SyncToGameObjectOperator<TViewModel, TView> SyncToGameObjects<TViewModel, TView>(
            this ObservableList<TViewModel> collection,
            TView prefab,
            Transform parent) where TView : MonoBehaviour
        {
            return new SyncToGameObjectOperator<TViewModel, TView>(collection, prefab, parent);
        }
    
        public static SyncOperator<T1, T2> Sync<T1, T2>(
            this ObservableList<T1> source, 
            ObservableList<T2> target, 
            Func<T1, T2> selector) where T2 : IDisposable
        {
            return new SyncOperator<T1, T2>(source, target, selector);
        }
    }
}