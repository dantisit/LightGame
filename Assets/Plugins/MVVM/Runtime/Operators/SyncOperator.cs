using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace Plugins.MVVM.Runtime.Operators
{
    public class SyncOperator<T1, T2> : IDisposable
    {
        private readonly ObservableList<T1> _source;
        private readonly ObservableList<T2> _target;
        private readonly Func<T1, T2> _selector;
        private readonly Dictionary<T1, T2> _t1ToT2 = new();
        private readonly CompositeDisposable _disposables = new();
        
        private Action<T1, T2> _onAdd;
        private Action<T1, T2> _onRemove;
        private Action _onChanged;
        private Action _onSort;
        private Action _onClear;
        
        public SyncOperator(ObservableList<T1> source, ObservableList<T2> target, Func<T1, T2> selector)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            
            // Add
            source.ObserveAdd().Subscribe(x =>
            {
                var mappedValue = selector(x.Value);
                target.Insert(x.Index, mappedValue);
                _t1ToT2[x.Value] = mappedValue;
                _onAdd?.Invoke(x.Value, mappedValue);
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            // Remove
            source.ObserveRemove().Subscribe(x =>
            {
                if (_t1ToT2.TryGetValue(x.Value, out var value))
                {
                    _onRemove?.Invoke(x.Value, value);
                    target.Remove(value);
                    _t1ToT2.Remove(x.Value);
                    _onChanged?.Invoke();
                }
            }).AddTo(_disposables);
            
            // Replace
            source.ObserveReplace().Subscribe(x =>
            {
                if (_t1ToT2.TryGetValue(x.OldValue, out var oldMapped))
                {
                    _onRemove?.Invoke(x.OldValue, oldMapped);
                    _target.RemoveAt(x.Index);
                    _t1ToT2.Remove(x.OldValue);
                }
                
                var newMapped = _selector(x.NewValue);
                _target.Insert(x.Index, newMapped);
                _t1ToT2[x.NewValue] = newMapped;
                _onAdd?.Invoke(x.NewValue, newMapped);
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            // Clear
            source.ObserveClear().Subscribe(_ =>
            {
                foreach (var pair in _t1ToT2)
                {
                    _onRemove?.Invoke(pair.Key, pair.Value);
                }
                
                target.Clear();
                _t1ToT2.Clear();
                _onClear?.Invoke();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            // Move
            source.ObserveMove().Subscribe(x =>
            {
                if (_t1ToT2.TryGetValue(x.Value, out var mapped))
                {
                    var currentIndex = _target.IndexOf(mapped);
                    if (currentIndex != -1 && currentIndex != x.NewIndex)
                        _target.Move(currentIndex, x.NewIndex);
                }
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            source.ObserveReverse().Subscribe(_ =>
            {
                ReorderTarget();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            source.ObserveSort().Subscribe(_ =>
            {
                ReorderTarget();
                _onSort?.Invoke();
                _onChanged?.Invoke();
            }).AddTo(_disposables);
            
            SyncExistingValues();
        }
        
        /// <summary>
        /// Reorders target collection to match the current source order.
        /// </summary>
        private void ReorderTarget()
        {
            var orderedList = new List<T2>(_source.Count);
            foreach (var item in _source)
            {
                if (_t1ToT2.TryGetValue(item, out var mapped))
                    orderedList.Add(mapped);
            }
            
            _target.Clear();
            foreach (var item in orderedList)
                _target.Add(item);
        }
        
        /// <summary>
        /// Sets the callback invoked when items are added.
        /// Note: This is applied retroactively to already-synced items.
        /// </summary>
        public SyncOperator<T1, T2> OnAdd(Action<T1, T2> callback)
        {
            _onAdd += callback;
            
            foreach (var pair in _t1ToT2)
            {
                callback(pair.Key, pair.Value);
            }
            
            return this;
        }
        
        public SyncOperator<T1, T2> OnRemove(Action<T1, T2> callback)
        {
            _onRemove += callback;
            return this;
        }
        
        public SyncOperator<T1, T2> OnChanged(Action callback)
        {
            _onChanged += callback;
            return this;
        }
        
        public SyncOperator<T1, T2> OnSort(Action callback)
        {
            _onSort += callback;
            return this;
        }
        
        public SyncOperator<T1, T2> OnClear(Action callback)
        {
            _onClear += callback;
            return this;
        }
        
        private void SyncExistingValues()
        {
            _target.Clear();
            _t1ToT2.Clear();
            
            foreach (var item in _source)
            {
                var mapped = _selector(item);
                _target.Add(mapped);
                _t1ToT2[item] = mapped;
                _onAdd?.Invoke(item, mapped);
            }
        }
        
        public void Dispose()
        {
            _t1ToT2.Clear();
            _disposables?.Dispose();
        }
    }
}