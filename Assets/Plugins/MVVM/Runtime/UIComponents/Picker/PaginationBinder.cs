using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    public abstract class PaginationBinder<T> : ObservablePickerBinder<T>
    {
        [SerializeField] private int windowSize;
        
        public int WindowSize => windowSize;
        
        private int _previousIndex = -1;
        protected override void OnValueChange(T value)
        {
            if (value == null) return;
            if (Values.Count < windowSize * 2 + 1) return;
            
            var oldWindow = _previousIndex != -1 ? GetWindowIndices(_previousIndex) : new List<int>();
            var newWindow = GetWindowIndices(CurrentIndex);
            
            foreach (var idx in oldWindow.Except(newWindow))
            {
                var offset = GetWrappedOffset(idx - CurrentIndex);
                RemovePaginationItem(Values[idx], offset);
            }
            
            foreach (var idx in newWindow.Except(oldWindow))
            {
                var offset = GetWrappedOffset(idx - CurrentIndex);
                AddPaginationItem(Values[idx], offset);
            }
            
            _previousIndex = CurrentIndex;
        }
        
        private List<int> GetWindowIndices(int centerIndex)
        {
            var indices = new List<int>();
            for (int offset = -windowSize; offset <= windowSize; offset++)
            {
                indices.Add(GetWrappedIndex(centerIndex + offset));
            }
            return indices;
        }
        
        public abstract void RemovePaginationItem(T item, int offset);
        public abstract void AddPaginationItem(T item, int offset);
    }
}