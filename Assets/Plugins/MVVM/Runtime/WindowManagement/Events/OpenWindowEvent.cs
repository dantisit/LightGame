using System;
using MVVM;

namespace MVVM.Events
{
    public class OpenWindowEvent : IEvent
    {
        public ViewModel ViewModel;
        
        public OpenWindowEvent() {}

        public OpenWindowEvent(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}