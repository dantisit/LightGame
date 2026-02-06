using MVVM;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events
{
    public class DragEndEvent : IEvent
    {
        public DragView Target { get; set; }
    }
}