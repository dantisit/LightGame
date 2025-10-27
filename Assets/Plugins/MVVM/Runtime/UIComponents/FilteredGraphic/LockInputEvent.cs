using System;
using System.Collections.Generic;
using MVVM;

namespace Core.Client.Events
{
    public class LockInputEvent : IEvent
    {
        public List<(Guid guid, ElementTagType tag)> Interactable;
    }
}