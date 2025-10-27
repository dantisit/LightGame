namespace MVVM.Data
{
    public struct DropEventData
    {
        public enum Types
        {
            OnDrop,
            OnDropEnter,
            OnDropExit
        }
        
        public ViewModel ViewModel;
        public Types Type;

        public DropEventData(ViewModel viewModel, Types type)
        {
            ViewModel = viewModel;
            Type = type;
        }
    }
}

