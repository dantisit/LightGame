using JetBrains.Annotations;
using MVVM;
using MVVM.Binders;
using MVVM.Components;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public static class DragManager
    {
        public static bool IsAnyDragging { get; private set; }
        [CanBeNull] public static DragView CurrentDragView { get; private set; }
        public static GroupDropComponent ActiveDropZone { get; private set; }
        
        public static ReactiveProperty<Vector3?> ActiveDropZonePosition { get; } = new();
        public static ReactiveProperty<DropMethodBinder> ActiveDropTargetProperty { get; } = new();
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            if (CurrentDragView == null || CurrentDragView.gameObject.scene != scene) return;
            
            RegisterDragEnd();
        }
        
        public static void RegisterDragStart(DragView obj)
        {
            IsAnyDragging = true;
            CurrentDragView = obj;
            EventBus.Emit(new DragBeginEvent(){ Target = CurrentDragView });
        }
    
        public static void RegisterDragEnd()    
        {
            EventBus.Emit(new DragEndEvent() { Target = CurrentDragView });
            IsAnyDragging = false;
            CurrentDragView = null;
            ActiveDropZone = null;
        }
        
        public static void RegisterDropZoneEnter(GroupDropComponent dropZone, DropMethodBinder dropTarget, Vector3? position)
        {
            ActiveDropZone = dropZone;
            ActiveDropTargetProperty.Value = dropTarget;
            ActiveDropZonePosition.Value = position;
        }
        
        public static void RegisterDropZoneExit(GroupDropComponent dropZone)
        {
            // For standalone DropMethodBinder (dropZone is null), or matching group
            if (dropZone != null && ActiveDropZone != dropZone) return;
            
            ActiveDropZone = null;
            ActiveDropTargetProperty.Value = null;
            ActiveDropZonePosition.Value = null;
        }
    }
}