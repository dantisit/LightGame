using System;
using UnityEngine;

namespace Plugins.MVVM.Runtime
{
    [Serializable]
    public class CanvasSorting
    {
        [HideInInspector] public bool Enabled;
        public string Key;
        public bool OverrideSorting = true;
        [SortingLayer] public int SortingLayerID;
        public int SortingOrder;

        public CanvasSorting(int sortingLayerID = 0, int sortingOrder = 0)
        {
            SortingLayerID = sortingLayerID;
            SortingOrder = sortingOrder;
        }
    }
}