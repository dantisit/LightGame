namespace MVVM.Binders.Animator
{
    public class AnimatorIntBinder : AnimatorVariableBinder<int>
    {
        public override void BindPropertyToAnimator(int newValue)
        {
            Animator.SetInteger(_key, newValue);
        }
    }
}