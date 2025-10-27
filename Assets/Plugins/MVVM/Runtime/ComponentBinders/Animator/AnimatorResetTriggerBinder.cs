using R3;
using UnityEngine;

namespace MVVM.Binders.Animator
{
    public class AnimatorResetTriggerBinder : AnimatorVariableBinder<object>
    {
        public override void BindPropertyToAnimator(object newValue)
        {
            Animator.ResetTrigger(_key);
        }
    }
}