using MVVM;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events;
using R3;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.Behaviors
{
    public class DragViewModel : ViewModel
    {
        public Subject<Unit> OnDrag { get; } = new();
        public ReactiveProperty<bool> IsDraggable { get; } = new(true);
        public ReactiveProperty<bool> IsDragging { get; } = new();
        public ReactiveProperty<Vector3> RevertPosition { get; } = new();
        
        public Observable<Unit> OnDragStart => IsDragging.Skip(1).Where(x => x).Select(_ => Unit.Default);
        public Observable<Unit> OnDragEnd => IsDragging.Skip(1).Where(x => !x).Select(_ => Unit.Default);

        public virtual void UpdateDrag(DragEventData eventData)
        {
            IsDragging.Value = eventData.Type switch
            {
                DragEventData.Types.OnBeginDrag => true,
                DragEventData.Types.OnEndDrag => false,
                _ => IsDragging.Value
            };
        }
    }
}
