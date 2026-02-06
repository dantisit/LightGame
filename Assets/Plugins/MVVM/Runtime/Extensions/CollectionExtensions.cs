using System;
using System.Collections.Generic;
using System.Linq;

namespace MVVM
{
    public static class CollectionExtensions
    {
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T> standardList)
            {
                standardList.Sort(comparison);
            }
            else if (list is ObservableCollections.ObservableList<T> observableList)
            {
                observableList.Sort(Comparer<T>.Create(comparison));
            }
            else
            {
                // Fallback for other IList implementations
                var items = list.ToArray();
                Array.Sort(items, comparison);
                list.Clear();
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }
        }
    }
}
