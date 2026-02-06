using MVVM;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events
{
    public class DragBeginEvent : IEvent
    {
        public DragView Target { get; set; }
    }
}