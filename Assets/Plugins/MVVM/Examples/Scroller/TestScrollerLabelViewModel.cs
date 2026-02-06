using MVVM;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerLabelViewModel : ViewModel
    {
        public enum LabelType
        {
            RecentlyPlayed,
            OnlineFriends,
            OfflineFriends,
        }
        
        public string Label { get; private set; }
        
        public TestScrollerLabelViewModel(LabelType labelType)
        {
            Label = labelType.ToString();
        }
    }
}