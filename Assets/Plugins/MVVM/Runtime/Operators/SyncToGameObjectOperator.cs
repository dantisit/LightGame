using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.MVVM.Runtime.Operators
{
    public class SyncToGameObjectOperator<TViewModel, TView> : IDisposable where TView : MonoBehaviour
    {
        private readonly ObservableList<TViewModel> _collection;
        private readonly TView _prefab;
        private readonly Transform _parent;
        private readonly Dictionary<TViewModel, TView> _viewModelToView = new();
        private readonly CompositeDisposable _disposables = new();

        private Action<TViewModel, TView> _onAdd;
        private Action<TViewModel, TView> _onRemove;
        private Action _onChanged; 
        private Action _onSort;
        private Action _onClear;
        
        public SyncToGameObjectOperator(ObservableList<TViewModel> collection, TView prefab, Transform parent)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            
            _collection = collection;
            _prefab = prefab;
            _parent = parent;
            
            _collection.ObserveAdd().Subscribe(x =>
            {
                AddViewModel(x.Value, x.Index);
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            _collection.ObserveRemove().Subscribe(x =>
            {
                RemoveViewModel(x.Value);
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            _collection.ObserveClear().Subscribe(_ =>
            {
                ClearAllViewModels();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            _collection.ObserveReplace().Subscribe(x =>
            {
                // Check if this is a reorder (both VMs already exist) vs true replacement
                bool oldExists = _viewModelToView.ContainsKey(x.OldValue);
                bool newExists = _viewModelToView.ContainsKey(x.NewValue);
    
                if (oldExists && newExists)
                {
                    // This is a reorder during Sort/Move - just update sibling indices
                    ReorderViews();
                }
                else
                {
                    // True replacement - one VM is being swapped for another
                    RemoveViewModel(x.OldValue);
                    AddViewModel(x.NewValue, x.Index);
                    _onChanged?.Invoke();
                }
            }).AddTo(_disposables);
            
            // Specific reorder events
            _collection.ObserveMove().Subscribe(_ =>
            {
                ReorderViews();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            _collection.ObserveReverse().Subscribe(_ =>
            {
                ReorderViews();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            _collection.ObserveSort().Subscribe(_ =>
            {
                ReorderViews();
                _onSort?.Invoke();
                _onChanged?.Invoke();
            }).AddTo(_disposables);

            // Auto-sync existing values
            SyncExistingValues();
        }
        
        /// <summary>
        /// Adds a ViewModel and creates its corresponding view.
        /// If the ViewModel already exists, logs a warning and replaces the old view.
        /// </summary>
        private void AddViewModel(TViewModel viewModel, int index)
        {
            // Check for duplicate
            if (_viewModelToView.ContainsKey(viewModel))
            {
                Debug.LogWarning($"[SyncToGameObject] Duplicate ViewModel detected at index {index}. " +
                                $"The previous view will be destroyed and replaced. " +
                                $"This may indicate a bug - ViewModels should be unique instances.");
                
                // Remove old view to prevent memory leak
                RemoveViewModel(viewModel);
            }
            
            var view = Object.Instantiate(_prefab, _parent);
            view.transform.SetSiblingIndex(index);
            _viewModelToView[viewModel] = view;
            _onAdd?.Invoke(viewModel, view);
        }
        
        /// <summary>
        /// Removes a ViewModel and destroys its corresponding view.
        /// </summary>
        private void RemoveViewModel(TViewModel viewModel)
        {
            if (_viewModelToView.TryGetValue(viewModel, out var view))
            {
                _onRemove?.Invoke(viewModel, view);
                _viewModelToView.Remove(viewModel);
                Object.DestroyImmediate(view.gameObject);
            }
        }
        
        /// <summary>
        /// Reorders views to match the current collection order by updating sibling indices.
        /// </summary>
        private void ReorderViews()
        {
            for (int i = 0; i < _collection.Count; i++)
            {
                if (_viewModelToView.TryGetValue(_collection[i], out var view))
                {
                    view.transform.SetSiblingIndex(i);
                }
            }
        }
        
        /// <summary>
        /// Clears all ViewModels and destroys all views.
        /// </summary>
        /// <param name="invokeClearCallback">Whether to invoke the OnClear callback.</param>
        private void ClearAllViewModels(bool invokeClearCallback = true)
        {
            foreach (var pair in _viewModelToView)
            {
                _onRemove?.Invoke(pair.Key, pair.Value);
                Object.DestroyImmediate(pair.Value.gameObject);
            }
            _viewModelToView.Clear();
            
            if (invokeClearCallback) _onClear?.Invoke();
        }
        
        /// <summary>
        /// Sets the callback invoked when items are added.
        /// Note: This is applied retroactively to already-synced items.
        /// </summary>
        public SyncToGameObjectOperator<TViewModel, TView> OnAdd(Action<TViewModel, TView> callback)
        {
            _onAdd += callback;
            
            // Apply callback to already-instantiated views
            foreach (var pair in _viewModelToView)
            {
                callback(pair.Key, pair.Value);
            }
            
            return this;
        }
        
        public SyncToGameObjectOperator<TViewModel, TView> OnRemove(Action<TViewModel, TView> callback)
        {
            _onRemove += callback;
            return this;
        }
        
        public SyncToGameObjectOperator<TViewModel, TView> OnChanged(Action callback)
        {
            _onChanged += callback;
            return this;
        }
        
        public SyncToGameObjectOperator<TViewModel, TView> OnSort(Action callback)
        {
            _onSort += callback;
            return this;
        }
        
        public SyncToGameObjectOperator<TViewModel, TView> OnClear(Action callback)
        {
            _onClear += callback;
            return this;
        }
        
        private void SyncExistingValues()
        {
            // Destroy old views (but don't call OnRemove - this is a re-sync, not a remove)
            foreach (var pair in _viewModelToView)
            {
                Object.DestroyImmediate(pair.Value.gameObject);
            }
            _viewModelToView.Clear();
            
            // Re-create views
            for (int i = 0; i < _collection.Count; i++)
            {
                var vm = _collection[i];
                
                // Check for duplicates during initial sync
                if (_viewModelToView.ContainsKey(vm))
                {
                    Debug.LogError($"[SyncToGameObject] Duplicate ViewModel found at index {i} during initial sync. " +
                                  $"Each ViewModel instance must be unique. Skipping duplicate.");
                    continue;
                }
                
                var view = Object.Instantiate(_prefab, _parent);
                view.transform.SetSiblingIndex(i);
                _viewModelToView[vm] = view;
                _onAdd?.Invoke(vm, view);
            }
        }
        
        public void Dispose()
        {
            ClearAllViewModels(invokeClearCallback: false);
            _disposables?.Dispose();
        }
    }
}