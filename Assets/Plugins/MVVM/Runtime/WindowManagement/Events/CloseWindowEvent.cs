using System;
using MVVM;

namespace MVVM.Events
{
    public class CloseWindowEvent : IEvent
    {
        public ViewModel ViewModel;

        public CloseWindowEvent(){}

        public CloseWindowEvent(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        
        public virtual Type GetWindowType() => null;
    }
    
    public class CloseWindowEvent<T> : CloseWindowEvent
    {
        public override Type GetWindowType() => typeof(T);
    }
}