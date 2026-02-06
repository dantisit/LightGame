using UIS;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerLayoutCalculator
    {
        public Scroller Scroller { get; private set; }
        public ScrollerData Data { get; private set; }
        
        public ScrollerLayoutCalculator(Scroller scroller) {
            Scroller = scroller;
            Data = scroller.Data;
        }
        
        /// <summary>
        /// Calc all items sizes and positions
        /// </summary>
        /// <param name="count">Item count</param>
        /// <param name="type">Scroller type: 0 for vertical, 1 for horizontal</param>
        /// <returns>Common content size</returns>
        public float CalcSizesPositions(int count, int type) {
            var isVertical = type == 0;
            var sizes = isVertical ? Data.Heights : Data.Widths;
            sizes.Clear();
            Data.Positions.Clear();
            Data.OffsetData = 0f;
            
            var result = 0f;
            var startPadding = isVertical ? Scroller.TopPadding : Scroller.LeftPadding;
            
            for (var i = 0; i < count; i++) {
                var size = isVertical ? Scroller.OnHeight(i) : Scroller.OnWidth(i);
                sizes[i] = size;
                Data.OffsetData += size;
                Data.Positions[i] = isVertical 
                    ? -(startPadding + i * Scroller.ItemSpacing + result)
                    : startPadding + i * Scroller.ItemSpacing + result;
                result += size;
            }
            
            Data.OffsetData /= count;
            Data.IsComplexList = Data.OffsetData != sizes[0];
            
            var endPadding = isVertical ? Scroller.BottomPadding : Scroller.RightPadding;
            result += startPadding + endPadding + (count == 0 ? 0 : ((count - 1) * Scroller.ItemSpacing));
            
            return result;
        }
    }
}