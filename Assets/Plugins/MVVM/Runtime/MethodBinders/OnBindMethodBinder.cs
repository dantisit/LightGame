using System;
using R3;
using UnityEngine;

namespace MVVM.Binders
{
    public class OnBindMethodBinder : GenericMethodBinder<Transform>
    {
        protected override void BindInternal(ViewModel viewModel)
        {
            base.BindInternal(viewModel);
            Observable.NextFrame().Subscribe(_ => Perform(transform)).AddTo(this);
        }
    }
}