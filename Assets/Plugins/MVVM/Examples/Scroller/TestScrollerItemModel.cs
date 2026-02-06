using MVVM;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerItemModel : Model
    { 
        public int Index { get; private set; }
        
        public TestScrollerItemModel(int index)
        {
            Index = index;
        }
    }
}