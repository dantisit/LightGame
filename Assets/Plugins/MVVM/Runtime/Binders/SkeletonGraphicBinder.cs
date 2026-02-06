#if SPINE_UNITY
using MVVM;
using MVVM.Binders;
using Spine.Unity;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIBinders
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class SkeletonGraphicBinder  : ObservableBinder<SkeletonGraphic>
    {
        public override void Bind(ViewModel viewModel)
        {
            base.Bind(viewModel);
            Value = GetComponent<SkeletonGraphic>();
        }

        public override void OnPropertyChanged(SkeletonGraphic newValue)
        {
        }
    }
}
#endif