using UIS;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerNavigation
    {
        public Scroller Scroller { get; private set; }
        public ScrollerData Data { get; private set; }
        
        public ScrollerNavigation(Scroller scroller) {
            Scroller = scroller;
            Data = scroller.Data;
        }
        
        /// <summary>
        /// Scroll to show item by index
        /// </summary>
        /// <param name="index">Item index</param>
        public void ScrollTo(int index) {
            var gap = 2;
            if (index > Data.Count) {
                index = Data.Count;
            } else if (index < 0) {
                index = 0;
            }
            if (index + Data.Views.Length >= Data.Count) {
                index = Data.Count - Data.Views.Length + Scroller.AddonViewsCount;
            }
            if (index < 0) {
                index = 0;
            }            
            for (var i = 0; i < Data.Views.Length; i++) {
                var position = (index < gap) ? index : index + i - gap;
                if (i + 1 > Data.Count || position >= Data.Count) {
                    continue;
                }
                var pos = Data.Rects[i].anchoredPosition;
                pos.y = Data.Positions[position];
                Data.Rects[i].anchoredPosition = pos;
                var size = Data.Rects[i].sizeDelta;
                if (Scroller.Type == 0) {
                    size.y = Data.Heights[position];
                } else {
                    size.x = Data.Widths[position];
                }
                Data.Rects[i].sizeDelta = size;
                Data.Views[i].SetActive(true);
                Data.Views[i].name = position.ToString();
                Scroller.OnFill(position, Data.Views[i]);
            }
            var offset = 0f;
            for (var i = 0; i < index; i++) {
                if (Scroller.Type == 0) {
                    offset += Data.Heights[i] + Scroller.ItemSpacing;
                } else {
                    offset -= Data.Widths[i] + Scroller.ItemSpacing;
                }
            }
            Data.PreviousPosition = index - Data.Views.Length;
            if (Data.PreviousPosition <= 0) {
                Scroller.InitData(Data.Count);
            }
            var top = Data.Content.anchoredPosition;
            if (Scroller.Type == 0) {
                top.y = offset;
            } else {
                top.x = offset;
            }
            Data.Content.anchoredPosition = top;
            Data.Scroll.velocity = Scroller.SCROLL_VELOCITY;
        }
    }
}