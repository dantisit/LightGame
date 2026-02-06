using MVVM;
using TMPro;
using UnityEngine;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerItemView : View<TestScrollerItemViewModel>
    {
        [SerializeField] private TextMeshProUGUI label;
        protected override void OnBindViewModel(TestScrollerItemViewModel viewModel)
        {
            label.text = viewModel.Label;
        }
    }
}