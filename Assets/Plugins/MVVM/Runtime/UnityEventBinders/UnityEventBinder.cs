using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public abstract class UnityEventBinder<T> : ObservableBinder<T>
    {
        [Header("Queue")]
        [SerializeField] protected bool _asQueue;
        [SerializeField] protected bool _passFirstValue;
        
        [Header("UnityEvent")]
        [SerializeField] protected UnityEvent<T> _event;
        [SerializeField] protected UltEvent<T> _ultEvent;

        private Queue<T> _queue = new();
        private bool _dequeuedFirst = false;

        public UnityEvent<T> Event => _event;

        public bool AsQueue
        {
            get => _asQueue;
            set => _asQueue = value;
        }
        
        public sealed override void OnPropertyChanged(T newValue)
        {
            if(_asQueue) Enqueue(newValue);
            else Invoke(newValue);
        }
        
        public void Enqueue(T value)
        {
            if(!gameObject.activeInHierarchy) return;
            _queue.Enqueue(value);
            if (_dequeuedFirst || !_passFirstValue) return;
            _dequeuedFirst = true;
            Dequeue();
        }
        public void Dequeue() 
        {
            if(!gameObject.activeInHierarchy) return;
            Invoke(_queue.Dequeue());
        }
        
        public void DequeueAll() 
        {
            if(!gameObject.activeInHierarchy) return;
            while (_queue.Count > 0)
            {
                Invoke(_queue.Dequeue());
            }
        }

        private void Invoke(T value)
        {
            _event.Invoke(value);
            _ultEvent.Invoke(value);
        }
    }
}
