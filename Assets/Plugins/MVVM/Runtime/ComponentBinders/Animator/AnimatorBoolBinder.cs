using R3;
using UnityEngine;

namespace MVVM.Binders.Animator
{
    public class AnimatorBoolBinder : AnimatorVariableBinder<bool>
    {
        [SerializeField] private bool _inverse;

        public override void BindPropertyToAnimator(bool newValue)
        {
            Animator.SetBool(_key, _inverse ? !newValue : newValue);
        }
    }
}