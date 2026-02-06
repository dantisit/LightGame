using MVVM;
using TMPro;
using UnityEngine;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerLabelView : View<TestScrollerLabelViewModel>
    {
        [SerializeField] private TextMeshProUGUI label;
        protected override void OnBindViewModel(TestScrollerLabelViewModel viewModel)
        {
            label.text = viewModel.Label;
        }
    }
}