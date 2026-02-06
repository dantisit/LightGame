using MVVM;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerItemViewModel : ViewModel
    {
        public string Label => "Item " + Model.Index;
        public TestScrollerItemModel Model { get; }
        
        public TestScrollerItemViewModel(TestScrollerItemModel model)
        {
            Model = model;
        }
    }
}