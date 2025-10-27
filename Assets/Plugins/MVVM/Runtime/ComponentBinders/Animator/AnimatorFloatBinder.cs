namespace MVVM.Binders.Animator
{
    public class AnimatorFloatBinder : AnimatorVariableBinder<float>
    {
        public override void BindPropertyToAnimator(float newValue)
        {
            Animator.SetFloat(_key, newValue);
        }
    }
}