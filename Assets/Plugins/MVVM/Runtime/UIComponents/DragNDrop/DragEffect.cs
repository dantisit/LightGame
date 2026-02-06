using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public abstract class DragEffect : MonoBehaviour
    {
        public abstract void SetIsDragging(bool isDragging);
    }
}