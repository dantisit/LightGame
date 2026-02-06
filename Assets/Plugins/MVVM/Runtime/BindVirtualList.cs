using System;
using System.Collections.Generic;
using ObservableCollections;
using UnityEngine;

namespace MVVM
{
    /// <summary>
    /// Virtual list builder for accumulating ViewModel+Prefab bindings
    /// Can be converted to different container types: ToScroller(), ToGridView(), etc.
    /// </summary>
    public class BindVirtualList
    {
        private readonly List<BindingEntry> _entries = new List<BindingEntry>();
        private readonly GameObject _disposeTarget;
        private Action _onChanged;

        internal List<BindingEntry> Entries => _entries;
        internal GameObject DisposeTarget => _disposeTarget;
        internal Action OnChangedCallback => _onChanged;

        public BindVirtualList(GameObject disposeTarget)
        {
            _disposeTarget = disposeTarget;
        }

        /// <summary>
        /// Set callback when content changes
        /// </summary>
        public BindVirtualList OnChanged(Action callback)
        {
            _onChanged = callback;
            return this;
        }

        /// <summary>
        /// Bind a single item (e.g., section header)
        /// Size is auto-calculated from prefab's RectTransform
        /// </summary>
        public BindVirtualList Bind(object viewModel, GameObject prefab)
        {
            _entries.Add(new ItemBindingEntry
            {
                ViewModel = viewModel,
                Prefab = prefab
            });
            return this;
        }

        /// <summary>
        /// Bind an observable collection
        /// Size is auto-calculated from prefab's RectTransform
        /// </summary>
        public BindVirtualList BindCollection<TViewModel>(
            ObservableList<TViewModel> collection,
            GameObject prefab)
        {
            _entries.Add(new CollectionBindingEntry
            {
                Collection = collection,
                Prefab = prefab
            });
            return this;
        }
    }

    /// <summary>
    /// Base class for binding entries
    /// </summary>
    public abstract class BindingEntry
    {
        public GameObject Prefab { get; set; }
    }

    /// <summary>
    /// Single item binding entry
    /// </summary>
    public class ItemBindingEntry : BindingEntry
    {
        public object ViewModel { get; set; }
    }

    /// <summary>
    /// Collection binding entry
    /// </summary>
    public class CollectionBindingEntry : BindingEntry
    {
        public object Collection { get; set; }
    }
}
