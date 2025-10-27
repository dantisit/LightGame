using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins.MVVM.Runtime.Sorting
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasSortingPriorityList : MonoBehaviour
    {
        [SerializeField] public List<CanvasSorting> _sortings = new();
        [SerializeField]  private Canvas _canvas;
        [SerializeField] private string _currentKey;

        private void OnValidate()
        {
            if(_canvas == null) _canvas = GetComponent<Canvas>();
        }

        public void SetSortingOrder(string key, int sortingOrder)
        {
            _sortings.First(x => x.Key == key).SortingOrder = sortingOrder;
            UpdateSorting();
        }
        
        public void Set(string key, bool value)
        {
            if(value) Enable(key);
            else Disable(key);
        }
        
        public void Enable(string key)
        {
            _sortings.First(x => x.Key == key).Enabled = true;
            UpdateSorting();
        }

        public void Disable(string key)
        {
            _sortings.First(x => x.Key == key).Enabled = false;
            UpdateSorting();
        }

        private void UpdateSorting()
        {
            var sorting = _sortings.FirstOrDefault(x => x.Enabled);
            _canvas.overrideSorting = sorting != null;
            if (sorting == null) return;
            _currentKey = sorting.Key;
            _canvas.overrideSorting = sorting.OverrideSorting;
            _canvas.sortingOrder = sorting.SortingOrder;
            _canvas.sortingLayerID = sorting.SortingLayerID;
            Debug.Log($"[Sorting] Sorting Key: {sorting.Key}  Sorting Layer: {_canvas.sortingLayerName} Sorting Order: {sorting.SortingOrder}]");
        }
    }
}