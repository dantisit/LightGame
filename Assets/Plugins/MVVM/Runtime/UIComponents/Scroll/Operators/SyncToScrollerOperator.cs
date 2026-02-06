using System;
using System.Collections.Generic;
using System.Linq;
using MVVM;
using ObservableCollections;
using R3;
using UIS;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    /// <summary>
    /// Reactive operator for syncing ViewModels to Scroller with support for sections
    /// Follows the pattern of SyncToGameObjectOperator but optimized for virtual scrolling
    /// </summary>
    public class SyncToScrollerOperator : IDisposable
    {
        private Scroller _scroller;
        private readonly List<ScrollerSection> _sections = new List<ScrollerSection>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private List<ScrollerFlatItem> _flatItems = new List<ScrollerFlatItem>();
        
        private Action _onChanged;
        private bool _isBuilt = false;

        public SyncToScrollerOperator(Scroller scroller = null)
        {
            _scroller = scroller;
        }

        /// <summary>
        /// Set the scroller (used by fluent builder pattern)
        /// </summary>
        internal void SetScroller(Scroller scroller)
        {
            _scroller = scroller;
        }

        /// <summary>
        /// Bind a single static item (e.g., section header)
        /// Size is auto-calculated from prefab's RectTransform
        /// </summary>
        public SyncToScrollerOperator BindItem(object viewModel, GameObject prefab)
        {
            var section = new ScrollerStaticSection
            {
                ViewModel = viewModel,
                Prefab = prefab,
                Height = Scroller.GetPrefabHeight(prefab),
                Width = Scroller.GetPrefabWidth(prefab)
            };
            
            _sections.Add(section);
            return this;
        }

        /// <summary>
        /// Bind an observable collection with automatic reactivity
        /// Size is auto-calculated from prefab's RectTransform
        /// </summary>
        public SyncToScrollerOperator BindCollection<TViewModel>(
            ObservableList<TViewModel> collection,
            GameObject prefab)
        {
            var section = new ScrollerObservableSection<TViewModel>
            {
                Collection = collection,
                Prefab = prefab,
                FixedHeight = Scroller.GetPrefabHeight(prefab),
                FixedWidth = Scroller.GetPrefabWidth(prefab)
            };

            // Subscribe to collection changes
            collection.ObserveAdd().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveRemove().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveClear().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveReplace().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveMove().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveReverse().Subscribe(_ => RebuildScroller()).AddTo(_disposables);
            collection.ObserveSort().Subscribe(_ => RebuildScroller()).AddTo(_disposables);

            _sections.Add(section);
            return this;
        }

        /// <summary>
        /// Callback when scroller content changes
        /// </summary>
        public SyncToScrollerOperator OnChanged(Action callback)
        {
            _onChanged += callback;
            return this;
        }

        /// <summary>
        /// Build and initialize the scroller with all bindings
        /// </summary>
        public SyncToScrollerOperator Build()
        {
            RebuildScroller();
            _isBuilt = true;
            return this;
        }

        private void RebuildScroller()
        {
            // Flatten all sections into a single list
            _flatItems.Clear();
            
            foreach (var section in _sections)
            {
                int count = section.GetCount();
                for (int i = 0; i < count; i++)
                {
                    _flatItems.Add(new ScrollerFlatItem
                    {
                        Section = section,
                        IndexInSection = i
                    });
                }
            }

            // Set up scroller callbacks
            _scroller.OnGetPrefab = GetPrefabForIndex;
            _scroller.OnHeight = GetHeightForIndex;
            _scroller.OnWidth = GetWidthForIndex;
            _scroller.OnFill = FillItemAtIndex;
            _scroller.UseMultiplePrefabs = true;

            // Initialize or refresh scroller
            if (_isBuilt)
            {
                _scroller.InitData(_flatItems.Count);
            }
            else
            {
                _scroller.InitData(_flatItems.Count);
            }

            _onChanged?.Invoke();
        }

        private GameObject GetPrefabForIndex(int index)
        {
            if (index < 0 || index >= _flatItems.Count)
                return _scroller.Prefab;
            return _flatItems[index].Prefab ?? _scroller.Prefab;
        }

        private int GetHeightForIndex(int index)
        {
            if (index < 0 || index >= _flatItems.Count)
                return 80;
            return _flatItems[index].Height;
        }

        private int GetWidthForIndex(int index)
        {
            if (index < 0 || index >= _flatItems.Count)
                return 100;
            return _flatItems[index].Width;
        }

        private void FillItemAtIndex(int index, GameObject view)
        {
            if (index < 0 || index >= _flatItems.Count)
                return;

            var flatItem = _flatItems[index];
            var data = flatItem.Data;

            // Bind ViewModel to View using BinderView
            if (data is ViewModel viewModel)
            {
                var binderView = view.GetComponent<BinderView>();
                if (binderView != null)
                {
                    binderView.BindViewModel(viewModel);
                }
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }

    /// <summary>
    /// Base class for scroller sections
    /// </summary>
    internal abstract class ScrollerSection
    {
        public GameObject Prefab { get; set; }
        public abstract int GetCount();
        public abstract object GetDataAt(int index);
        public abstract int GetHeight(int index);
        public abstract int GetWidth(int index);
    }

    /// <summary>
    /// Static single item section (e.g., header)
    /// </summary>
    internal class ScrollerStaticSection : ScrollerSection
    {
        public object ViewModel { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public override int GetCount() => 1;
        public override object GetDataAt(int index) => ViewModel;
        public override int GetHeight(int index) => Height;
        public override int GetWidth(int index) => Width;
    }

    /// <summary>
    /// Observable collection section
    /// </summary>
    internal class ScrollerObservableSection<TViewModel> : ScrollerSection
    {
        public ObservableList<TViewModel> Collection { get; set; }
        public Func<object, int> HeightSelector { get; set; }
        public Func<object, int> WidthSelector { get; set; }
        public int FixedHeight { get; set; }
        public int FixedWidth { get; set; }

        public override int GetCount() => Collection?.Count ?? 0;

        public override object GetDataAt(int index)
        {
            if (Collection == null || index < 0 || index >= Collection.Count)
                return null;
            return Collection[index];
        }

        public override int GetHeight(int index)
        {
            if (HeightSelector != null && Collection != null && index >= 0 && index < Collection.Count)
                return HeightSelector(Collection[index]);
            return FixedHeight;
        }

        public override int GetWidth(int index)
        {
            if (WidthSelector != null && Collection != null && index >= 0 && index < Collection.Count)
                return WidthSelector(Collection[index]);
            return FixedWidth;
        }
    }

    /// <summary>
    /// Internal flat item representation
    /// </summary>
    internal class ScrollerFlatItem
    {
        public ScrollerSection Section { get; set; }
        public int IndexInSection { get; set; }
        public GameObject Prefab => Section.Prefab;
        public object Data => Section.GetDataAt(IndexInSection);
        public int Height => Section.GetHeight(IndexInSection);
        public int Width => Section.GetWidth(IndexInSection);
    }
}
