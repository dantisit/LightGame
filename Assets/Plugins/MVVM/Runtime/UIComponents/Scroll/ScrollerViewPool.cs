using UIS;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerViewPool
    {
        public Scroller Scroller { get; private set; }
        public ScrollerData Data { get; private set; }
        
        public ScrollerViewPool(Scroller scroller) {
            Scroller = scroller;
            Data = scroller.Data;
        }

        /// <summary>
        /// Create views
        /// </summary>
        /// <param name="isForceCreate">Create views anyway</param>
        /// <param name="type">Scroller type: 0 for vertical, 1 for horizontal</param>
        public void CreateViews(bool isForceCreate, int type) {
            var isVertical = type == 0;
            if (Data.Views != null) return;

            var childs = Data.Content.transform.childCount;
            if (childs > 0 && !isForceCreate) {
                LoadExistingViews(childs);
            } else {
                CreateNewViews(isVertical);
            }

            InitializeViewData();
        }

        private void LoadExistingViews(int count) {
            Data.Views = new GameObject[count];
            for (var i = 0; i < count; i++) {
                Data.Views[i] = Data.Content.transform.GetChild(i).gameObject;
            }
        }

        private void CreateNewViews(bool isVertical) {
            var fillCount = CalculateFillCount(isVertical);
            Data.Views = new GameObject[fillCount];

            for (var i = 0; i < fillCount; i++) {
                // Get prefab from callback if using multi-prefab mode, otherwise use default Prefab
                var prefab = Scroller.UseMultiplePrefabs && Scroller.OnGetPrefab != null 
                    ? Scroller.OnGetPrefab(i) ?? Scroller.Prefab 
                    : Scroller.Prefab;
                    
                var clone = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                clone.transform.SetParent(Data.Content);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;
                
                ConfigureRectTransform(clone.GetComponent<RectTransform>(), isVertical);
                Data.Views[i] = clone;
            }
        }

        private int CalculateFillCount(bool isVertical) {
            var sizes = isVertical ? Data.Heights : Data.Widths;
            var containerSize = isVertical ? Data.Container.height : Data.Container.width;
            
            var avgSize = 0;
            foreach (var size in sizes.Values) {
                avgSize += size + Scroller.ItemSpacing;
            }
            avgSize /= sizes.Count;

            return Mathf.RoundToInt(containerSize / avgSize) + Scroller.AddonViewsCount;
        }

        private void ConfigureRectTransform(RectTransform rect, bool isVertical) {
            if (isVertical) {
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = Vector2.one;
            } else {
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(0f, 1f);
            }
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
        }

        private void InitializeViewData() {
            Data.Indexes = new int[Data.Views.Length];
            Data.Rects = new RectTransform[Data.Views.Length];
            Data.ViewPrefabs = new GameObject[Data.Views.Length];
            
            for (var i = 0; i < Data.Views.Length; i++) {
                Data.Indexes[i] = -1;
                Data.Rects[i] = Data.Views[i].GetComponent<RectTransform>();
                
                // Track which prefab this view was created from
                if (Scroller.UseMultiplePrefabs && Scroller.OnGetPrefab != null) {
                    Data.ViewPrefabs[i] = Scroller.OnGetPrefab(i) ?? Scroller.Prefab;
                } else {
                    Data.ViewPrefabs[i] = Scroller.Prefab;
                }
            }
        }
    }
}