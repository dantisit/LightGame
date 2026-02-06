using MVVM;
using MVVM.Components;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events
{
    public class DropEvent : IEvent
    {
        public IView DroppedView { get; set; }
        public GroupDropComponent DropZone { get; set; }
    }
}
