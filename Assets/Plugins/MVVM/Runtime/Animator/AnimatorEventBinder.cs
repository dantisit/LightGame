using System;
using Cysharp.Threading.Tasks;
using MVVM.Binders;
using R3;
using UnityEngine;


namespace MVVM.MVVMAnimator
{
    public class AnimatorEventBinder : GenericMethodBinder<AnimatorEventData>
    {
        [SerializeField] private string animationKey;
        private Animator animator;

        protected override void BindInternal(ViewModel viewModel)
        {
            base.BindInternal(viewModel);
            SubscribeInternal();
        }

        private async void SubscribeInternal()
        {
            animator ??= GetComponent<Animator>();
            await UniTask.WaitUntil(() => animator.isInitialized);
            animator.ObserveEvents()
                .Where(x => x.OnStateInfo.StateInfo.IsName(animationKey))
                .Subscribe(Perform)
                .AddTo(ViewModel.Disposables);
        }
    }
}