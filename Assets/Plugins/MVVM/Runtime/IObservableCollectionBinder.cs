using System.Collections;
using System.Collections.Generic;
using ObservableCollections;

namespace Plugins.MVVM.Runtime
{
    public interface IObservableCollectionBinder
    {
        public void AddItem(object value);
        public void RemoveItem(object value);
        public void Sort<T>(ObservableList<T> collection);
        public void Clear();
    }
}