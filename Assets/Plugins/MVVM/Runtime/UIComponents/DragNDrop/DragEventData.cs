using MVVM;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public class DragEventData
    {
        public enum Types
        {
            OnBeginDrag,
            OnDrag,
            OnEndDrag,
        }
        
        public Types Type;

        public DragEventData(Types type)
        {
            Type = type;
        }
    }
}