using R3;
using UnityEngine;

namespace MVVM.Binders.Animator
{
    public class UnitToAnimatorBoolBinder : AnimatorVariableBinder<Unit>
    {
        [SerializeField] private bool _value;

        public override void BindPropertyToAnimator(Unit newValue)
        {
            Animator.SetBool(_key, _value);
        }
    }
}