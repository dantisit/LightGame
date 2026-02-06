using UIS;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerUpdateSystem
    {
        public Scroller Scroller { get; private set; }
        public ScrollerData Data { get; private set; }
        
        public ScrollerUpdateSystem(Scroller scroller) {
            Scroller = scroller;
            Data = scroller.Data;
        }

        /// <summary>
        /// Main loop to check items positions and heights
        /// </summary>
        /// <param name="type">Scroller type: 0 for vertical, 1 for horizontal</param>
        public void Update(int type) {
            var isVertical = type == 0;
            if (Data.Count == 0 || !Data.IsInited) return;

            if (Data.IsComplexList) {
                UpdateComplexList(isVertical);
            } else {
                UpdateSimpleList(isVertical);
            }
        }

        private void UpdateComplexList(bool isVertical) {
            var scrollPosition = GetScrollPosition(isVertical);
            var padding = isVertical ? Scroller.TopPadding : Scroller.LeftPadding;
            var rectPosition = isVertical ? Data.Rects[0].anchoredPosition.y : Data.Rects[0].anchoredPosition.x;
            
            if (scrollPosition <= 0f && rectPosition < -padding) {
                Scroller.InitData(Data.Count);
                return;
            }
            
            if (scrollPosition < 0f) {
                return;
            }

            var sizes = isVertical ? Data.Heights : Data.Widths;
            if (!Data.Positions.ContainsKey(Data.PreviousPosition) || !sizes.ContainsKey(Data.PreviousPosition)) {
                return;
            }

            var itemPosition = Mathf.Abs(Data.Positions[Data.PreviousPosition]) + sizes[Data.PreviousPosition];
            var position = (scrollPosition > itemPosition) ? Data.PreviousPosition + 1 : Data.PreviousPosition - 1;
            
            if (position < 0 || position == Data.PreviousPosition) {
                return;
            }

            if (position > Data.PreviousPosition) {
                HandleForwardScroll(position, isVertical, sizes);
            } else {
                HandleBackwardScroll(position, isVertical, sizes);
            }
            
            Data.PreviousPosition = position;
        }

        private void UpdateSimpleList(bool isVertical) {
            var scrollPosition = GetScrollPosition(isVertical);
            var offset = Mathf.FloorToInt(scrollPosition / (Data.OffsetData + Scroller.ItemSpacing));
            var sizes = isVertical ? Data.Heights : Data.Widths;
            
            for (var i = offset; i < offset + Data.Views.Length; i++) {
                var index = i % Data.Views.Length;
                if (i < 0 || i > Data.Count - 1 || Data.Rects == null || !Data.IsInited) {
                    continue;
                }

                UpdateViewPosition(index, i, isVertical);
                
                if (Data.Indexes[index] != i) {
                    Data.Indexes[index] = i;
                    UpdateViewSize(index, i, isVertical, sizes);
                    Data.Views[index].name = i.ToString();
                    Scroller.OnFill(i, Data.Views[index]);
                }
            }
        }

        private float GetScrollPosition(bool isVertical) {
            return isVertical 
                ? Data.Content.anchoredPosition.y - Scroller.ItemSpacing
                : Data.Content.anchoredPosition.x * -1f - Scroller.ItemSpacing;
        }

        private void HandleForwardScroll(int position, bool isVertical, System.Collections.Generic.Dictionary<int, int> sizes) {
            if (position - Data.PreviousPosition > 1) {
                position = Data.PreviousPosition + 1;
            }
            
            var newPosition = position % Data.Views.Length;
            newPosition--;
            if (newPosition < 0) {
                newPosition = Data.Views.Length - 1;
            }
            
            var index = position + Data.Views.Length - 1;
            
            if (index < Data.Count) {
                RecreateViewIfNeeded(newPosition, index, isVertical);
                UpdateViewPosition(newPosition, index, isVertical);
                UpdateViewSize(newPosition, index, isVertical, sizes);
                Data.Views[newPosition].name = index.ToString();
                Scroller.OnFill(index, Data.Views[newPosition]);
            }
        }

        private void HandleBackwardScroll(int position, bool isVertical, System.Collections.Generic.Dictionary<int, int> sizes) {
            if (Data.PreviousPosition - position > 1) {
                position = Data.PreviousPosition - 1;
            }
            
            var newIndex = position % Data.Views.Length;
            
            RecreateViewIfNeeded(newIndex, position, isVertical);
            UpdateViewPosition(newIndex, position, isVertical);
            UpdateViewSize(newIndex, position, isVertical, sizes);
            Data.Views[newIndex].name = position.ToString();
            Scroller.OnFill(position, Data.Views[newIndex]);
        }

        private void RecreateViewIfNeeded(int viewIndex, int dataIndex, bool isVertical) {
            if (!Scroller.UseMultiplePrefabs || Scroller.OnGetPrefab == null) return;
            
            var requiredPrefab = Scroller.OnGetPrefab(dataIndex) ?? Scroller.Prefab;
            var currentPrefab = Data.ViewPrefabs[viewIndex];
            
            // If prefab type hasn't changed, no need to recreate
            if (requiredPrefab == currentPrefab) return;
            
            // Destroy old view
            var oldView = Data.Views[viewIndex];
            var parent = oldView.transform.parent;
            var siblingIndex = oldView.transform.GetSiblingIndex();
            UnityEngine.Object.Destroy(oldView);
            
            // Create new view with correct prefab
            var newView = UnityEngine.Object.Instantiate(requiredPrefab, Vector3.zero, Quaternion.identity);
            newView.transform.SetParent(parent);
            newView.transform.SetSiblingIndex(siblingIndex);
            newView.transform.localScale = Vector3.one;
            newView.transform.localPosition = Vector3.zero;
            
            // Configure RectTransform
            var rect = newView.GetComponent<RectTransform>();
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
            
            // Update references
            Data.Views[viewIndex] = newView;
            Data.Rects[viewIndex] = rect;
            Data.ViewPrefabs[viewIndex] = requiredPrefab;
        }

        private void UpdateViewPosition(int viewIndex, int dataIndex, bool isVertical) {
            var pos = Data.Rects[viewIndex].anchoredPosition;
            if (isVertical) {
                pos.y = Data.Positions[dataIndex];
            } else {
                pos.x = Data.Positions[dataIndex];
            }
            Data.Rects[viewIndex].anchoredPosition = pos;
        }

        private void UpdateViewSize(int viewIndex, int dataIndex, bool isVertical, System.Collections.Generic.Dictionary<int, int> sizes) {
            var size = Data.Rects[viewIndex].sizeDelta;
            if (isVertical) {
                size.y = sizes[dataIndex];
            } else {
                size.x = sizes[dataIndex];
            }
            Data.Rects[viewIndex].sizeDelta = size;
        }
    }
}