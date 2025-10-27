using R3;
using UnityEngine;

namespace MVVM
{
    public class EventBus : MonoBehaviour
    {
        private static Subject<IEvent> OnMessage { get; set; } = null!;

        public static Observable<T> On<T>() where T : IEvent => OnMessage.OfType<IEvent, T>();

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            OnMessage = new Subject<IEvent>();
        }

        public static void Emit(IEvent @event)
        {
            @event.Execute();
            OnMessage.OnNext(@event);
        }

        public void OnDestroy()
        {
            OnMessage.OnCompleted();
        }
    }
}