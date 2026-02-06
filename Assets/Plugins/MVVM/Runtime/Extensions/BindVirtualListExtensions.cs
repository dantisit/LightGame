using System;
using ObservableCollections;
using Plugins.MVVM.Runtime.UIComponents.Scroll;
using R3;
using UIS;
using UnityEngine;

namespace MVVM
{
    public static class ScrollerExtensions
    {
        /// <summary>
        /// Convert BindVirtualList to Scroller with virtual scrolling
        /// Terminal method that builds and disposes automatically
        /// Sizes are auto-calculated from prefab RectTransforms
        /// </summary>
        public static SyncToScrollerOperator ToScroller(this BindVirtualList list, Scroller scroller)
        {
            var op = new SyncToScrollerOperator(scroller);
            
            // Apply all accumulated bindings
            foreach (var entry in list.Entries)
            {
                if (entry is ItemBindingEntry itemEntry)
                {
                    op.BindItem(itemEntry.ViewModel, itemEntry.Prefab);
                }
                else if (entry is CollectionBindingEntry collectionEntry)
                {
                    var collection = collectionEntry.Collection;
                    var collectionType = collection.GetType();
                    
                    // Use reflection to call the generic BindCollection method
                    var itemType = collectionType.GetGenericArguments()[0];
                    var bindMethod = typeof(SyncToScrollerOperator).GetMethod(nameof(SyncToScrollerOperator.BindCollection));
                    var genericMethod = bindMethod.MakeGenericMethod(itemType);
                    
                    genericMethod.Invoke(op, new object[] { collection, collectionEntry.Prefab });
                }
            }
            
            // Apply OnChanged callback if set
            if (list.OnChangedCallback != null)
            {
                op.OnChanged(list.OnChangedCallback);
            }
            
            op.Build();
            Disposable.Create(() => op.Dispose()).AddTo(list.DisposeTarget);
            return op;
        }
    }
}
