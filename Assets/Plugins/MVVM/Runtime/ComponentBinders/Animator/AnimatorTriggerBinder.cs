using R3;
using UnityEngine;

namespace MVVM.Binders.Animator
{
    public class AnimatorTriggerBinder : AnimatorVariableBinder<object>
    {
        [SerializeField] private int skip;
        private int _currentSkip;

        public override void Bind(ViewModel viewModel)
        {
            base.Bind(viewModel);
            _currentSkip = skip;
        }

        public override void BindPropertyToAnimator(object newValue)
        {
            if (_currentSkip > 0)
            {
                _currentSkip--;
                return;
            }
            Debug.Log("[DEBUG ANIMATOR] Trigger: " + _key);
            Animator.SetTrigger(_key);
        }
        
        
    }
}