using System;

namespace MVVM.Binders
{
    public class EmptyMethodBinder : MethodBinder
    {
        private event Action _action;

        protected override void BindInternal(ViewModel viewModel)
        {
            _action = (Action)Delegate.CreateDelegate(typeof(Action), viewModel, MethodName);
        }

        public void Perform()
        {
            _action?.Invoke();
        }
    }
}
