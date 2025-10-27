using MVVM;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{
    public static class DragManager
    {
        public static bool IsAnyDragging { get; private set; }
        public static GameObject CurrentDraggedObject { get; private set; }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            if (CurrentDraggedObject == null || CurrentDraggedObject.scene != scene) return;
            
            RegisterDragEnd();
        }
        
        public static void RegisterDragStart(GameObject obj)
        {
            IsAnyDragging = true;
            CurrentDraggedObject = obj;
            EventBus.Emit(new DragBeginEvent(){ Target = CurrentDraggedObject });
        }
    
        public static void RegisterDragEnd()
        {
            EventBus.Emit(new DragEndEvent() { Target = CurrentDraggedObject });
            IsAnyDragging = false;
            CurrentDraggedObject = null;
        }
    }
}