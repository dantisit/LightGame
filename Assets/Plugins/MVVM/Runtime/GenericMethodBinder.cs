using System;

namespace MVVM.Binders
{
    public abstract class GenericMethodBinder : MethodBinder
    {
        public abstract Type ParameterType { get; }
    }

    public class GenericMethodBinder<T> : GenericMethodBinder
    {
        public override Type ParameterType => typeof(T);

        private event Action<T> _action;

        protected override void BindInternal(ViewModel viewModel)
        {
            _action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), viewModel, MethodName);
        }

        public void Perform(T value)
        {
            _action?.Invoke(value);
        }
    }
}
