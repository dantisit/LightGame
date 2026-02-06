using UIS;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerInitializer
    {
        public Scroller Scroller { get; private set; }
        public ScrollerData Data { get; private set; }
        
        public ScrollerInitializer(Scroller scroller) {
            Scroller = scroller;
            Data = scroller.Data;
        }

        /// <summary>
        /// Init list (vertical or horizontal based on Scroller.Type)
        /// </summary>
        /// <param name="count">Item count</param>
        /// <param name="isOtherSide">Go to bottom/right on init</param>
        public void Init(int count, bool isOtherSide) {
            var isVertical = Scroller.IsVertical;
            var size = Scroller.CalcSizesPositions(count);
            Scroller.CreateViews();
            Data.PreviousPosition = 0;
            Data.Count = count;
            
            Debug.Log($"[Scroller] Init - Count: {count}, ViewsLength: {Data.Views.Length}, IsComplexList: {Data.IsComplexList}, OffsetData: {Data.OffsetData}");
            
            // Set content size based on orientation
            var contentSize = Data.Content.sizeDelta;
            contentSize[isVertical ? 1 : 0] = size;
            Data.Content.sizeDelta = contentSize;
            
            // Set initial position based on orientation and side
            var pos = Data.Content.anchoredPosition;
            pos[isVertical ? 1 : 0] = isOtherSide ? size : 0f;
            Data.Content.anchoredPosition = pos;
            
            var sizes = isVertical ? Data.Heights : Data.Widths;
            var mainAxis = isVertical ? 1 : 0;
            var crossAxis = isVertical ? 0 : 1;
            
            for (var i = 0; i < Data.Views.Length; i++) {
                var showed = i < count;
                Data.Views[i].SetActive(showed);
                if (i + 1 > Data.Count) {
                    continue;
                }
                var index = i;
                if (isOtherSide) {
                    index = (count >= Data.Views.Length) ? count - Data.Views.Length + i : i;
                }
                
                // Set position on main axis, zero on cross axis
                pos = Data.Rects[i].anchoredPosition;
                pos[mainAxis] = Data.Positions[index];
                pos[crossAxis] = 0f;
                Data.Rects[i].anchoredPosition = pos;
                
                // Set size on main axis
                var rectSize = Data.Rects[i].sizeDelta;
                rectSize[mainAxis] = sizes[index];
                Data.Rects[i].sizeDelta = rectSize;
                
                Data.Views[i].name = i.ToString();
                Scroller.OnFill(index, Data.Views[i]);
            }
        }

        /// <summary>
        /// Update list after load new items
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        public void ApplyDataTo(int count, int newCount, ScrollerDirection direction) {
            var isVertical = Scroller.IsVertical;
            if (Data.Count == 0 || count <= Data.Views.Length) {
                Scroller.InitData(count);
                return;
            }
            
            Data.Count = count;
            var size = Scroller.CalcSizesPositions(count);
            var sizes = isVertical ? Data.Heights : Data.Widths;
            
            if (isVertical) {
                Data.Content.sizeDelta = new Vector2(Data.Content.sizeDelta.x, size);
            } else {
                Data.Content.sizeDelta = new Vector2(size, Data.Content.sizeDelta.y);
            }
            
            var pos = Data.Content.anchoredPosition;
            
            if (isVertical) {
                if (direction == ScrollerDirection.Top) {
                    var y = 0f;
                    for (var i = 0; i < newCount; i++) {
                        y += Data.Heights[i] + Scroller.ItemSpacing;
                    }
                    pos.y = y;
                    Data.PreviousPosition = newCount;
                }
            } else {
                if (direction == ScrollerDirection.Left) {
                    var x = 0f;
                    for (var i = 0; i < newCount; i++) {
                        x -= Data.Widths[i] + Scroller.ItemSpacing;
                    }
                    pos.x = x;
                    Data.PreviousPosition = newCount;
                } else {
                    var w = 0f;
                    for (var i = Data.Widths.Count - 1; i >= Data.Widths.Count - newCount; i--) {
                        w += Data.Widths[i] + Scroller.ItemSpacing;
                    }
                    pos.x = -Data.Content.sizeDelta.x + w + Data.Container.width;
                }
            }
            
            Data.Content.anchoredPosition = pos;
            
            var scrollPosition = isVertical 
                ? Data.Content.anchoredPosition.y - Scroller.ItemSpacing
                : Data.Content.anchoredPosition.x - Scroller.ItemSpacing;
            var itemPosition = Mathf.Abs(Data.Positions[Data.PreviousPosition]) + sizes[Data.PreviousPosition];
            var position = (scrollPosition > itemPosition) ? Data.PreviousPosition + 1 : Data.PreviousPosition - 1;
            
            if (position < 0) {
                Data.PreviousPosition = 0;
                position = 1;
            }
            
            if (!Data.IsComplexList) {
                for (var i = 0; i < Data.Indexes.Length; i++) {
                    Data.Indexes[i] = -1;
                }
            }
            
            for (var i = 0; i < Data.Views.Length; i++) {
                var newIndex = position % Data.Views.Length;
                if (newIndex < 0) {
                    continue;
                }
                Data.Views[newIndex].SetActive(true);
                Data.Views[newIndex].name = position.ToString();
                Scroller.OnFill(position, Data.Views[newIndex]);
                pos = Data.Rects[newIndex].anchoredPosition;
                if (isVertical) {
                    pos.y = Data.Positions[position];
                } else {
                    pos.x = Data.Positions[position];
                }
                Data.Rects[newIndex].anchoredPosition = pos;
                var rectSize = Data.Rects[newIndex].sizeDelta;
                if (isVertical) {
                    rectSize.y = sizes[position];
                } else {
                    rectSize.x = sizes[position];
                }
                Data.Rects[newIndex].sizeDelta = rectSize;
                position++;
                if (position == Data.Count) {
                    break;
                }
            }
        }
    }
}