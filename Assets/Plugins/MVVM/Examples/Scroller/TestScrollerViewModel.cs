using System.Collections.Generic;
using System.Linq;
using MVVM;
using ObservableCollections;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerViewModel : ViewModel
    {
        public TestScrollerLabelViewModel RecentlyPlayedLabel { get; private set; } = new(TestScrollerLabelViewModel.LabelType.RecentlyPlayed);
        public TestScrollerLabelViewModel OnlineFriendsLabel { get; private set; } = new(TestScrollerLabelViewModel.LabelType.OnlineFriends);
        public TestScrollerLabelViewModel OfflineFriendsLabel { get; private set; } = new(TestScrollerLabelViewModel.LabelType.OfflineFriends);
        
        public ObservableList<TestScrollerItemViewModel> RecentlyPlayed { get; private set; } = new();
        public ObservableList<TestScrollerItemViewModel> OnlineFriends { get; private set; } = new();
        public ObservableList<TestScrollerItemViewModel> OfflineFriends { get; private set; } = new();
        
        public TestScrollerViewModel(List<TestScrollerItemModel> recentlyPlayed, List<TestScrollerItemModel> onlineFriends, List<TestScrollerItemModel> offlineFriends)
        {
            RecentlyPlayed.AddRange(recentlyPlayed.Select(x => new TestScrollerItemViewModel(x)));
            OnlineFriends.AddRange(onlineFriends.Select(x => new TestScrollerItemViewModel(x)));
            OfflineFriends.AddRange(offlineFriends.Select(x => new TestScrollerItemViewModel(x)));            
        }
    }
}