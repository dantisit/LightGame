using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public abstract class ConverterUnityEventBinder<TFrom, TTo> : ObservableBinder<TFrom>
    {
        [Header("Queue")]
        [SerializeField] protected bool _asQueue;
        [SerializeField] protected bool _passFirstValue;
        
        [Header("UnityEvent")]
        [SerializeField] protected UnityEvent<TTo> _event;

        private Queue<TTo> _queue = new();
        private bool _dequeuedFirst = false;
        
        public sealed override void OnPropertyChanged(TFrom newValue)
        {
            if(_asQueue) Enqueue(newValue);
            else _event.Invoke(Convert(newValue));
        }
        
        public void Enqueue(TFrom value)
        {
            if(!gameObject.activeInHierarchy) return;
            _queue.Enqueue(Convert(value));
            if (_dequeuedFirst || !_passFirstValue) return;
            _dequeuedFirst = true;
            Dequeue();
        }
        public void Dequeue() 
        {
            if(!gameObject.activeInHierarchy) return;
            _event.Invoke(_queue.Dequeue());
        }

        protected abstract TTo Convert(TFrom value);
    }
}
