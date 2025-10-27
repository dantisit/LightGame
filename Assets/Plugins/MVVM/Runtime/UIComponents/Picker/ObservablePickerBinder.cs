using System;
using System.Collections.Generic;
using System.Linq;
using MVVM;
using MVVM.Binders;
using Plugins.MVVM.Runtime.Binders;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Plugins.MVVM.Runtime.UIComponents.Picker
{
    [RequireComponent(typeof(IntUnityEventBinder))]
    public abstract class ObservablePickerBinder<T> : ObservableCollectionBinder<T>
    {
        [SerializeField] private bool isCyclic;
        [SerializeField] private Button previous;
        [SerializeField] private Button next;
        [SerializeField] private IntUnityEventBinder currentIndexBinder;
        [SerializeField] private UltEvent<bool> onPreviousIsAvailable;
        [SerializeField] private UltEvent<bool> onNextIsAvailable;

        protected List<T> Values { get; set; } = new();

        protected int CurrentIndex
        {
            get => currentIndexBinder.Value;
            set => currentIndexBinder.Value = value;
        }

        protected int MaxIndex => Values.Count - 1;

        private void OnEnable()
        {
            next.onClick.AddListener(Next);
            previous.onClick.AddListener(Previous);
            currentIndexBinder.Event.AddListener(UpdateIndexInternal);
            UpdateIndexInternal(CurrentIndex);
        }

        private void OnDisable()
        {
            next.onClick.RemoveListener(Next);
            previous.onClick.RemoveListener(Previous);
            currentIndexBinder.Event.RemoveListener(UpdateIndexInternal);
        }

        private void Next() => CurrentIndex = GetWrappedIndex(CurrentIndex + 1);
        private void Previous() => CurrentIndex = GetWrappedIndex(CurrentIndex - 1);
        
        private void UpdateIndexInternal(int value)
        {
            if (!isCyclic)
            {
                onPreviousIsAvailable.Invoke(CurrentIndex != 0);
                onNextIsAvailable.Invoke(CurrentIndex != MaxIndex);
            }
            OnValueChange(Values.ElementAtOrDefault(value));
        }
        
        protected int GetWrappedIndex(int index)
        {
            if (Values.Count == 0) return 0;

            return isCyclic ? 
                MathEx.ClampCyclic(index, 0, MaxIndex) :
                Mathf.Clamp(index, 0, MaxIndex);
        }
        
        protected int GetWrappedOffset(int offset)
        {
            if (Values.Count == 0) return 0;

            int targetIndex = CurrentIndex + offset;
            int wrappedIndex = GetWrappedIndex(targetIndex);
            
            int rawOffset = wrappedIndex - CurrentIndex;

            if (isCyclic && Values.Count > 0)
            {
                int forwardDistance  = (wrappedIndex - CurrentIndex + Values.Count) % Values.Count;
                int backwardDistance = (CurrentIndex - wrappedIndex + Values.Count) % Values.Count;

                if (forwardDistance <= backwardDistance)
                    return forwardDistance;
                else
                    return -backwardDistance;
            }

            return rawOffset;
        }
        
        protected abstract void OnValueChange(T value);

        public override void OnItemAdded(T value)
        {
            Values.Add(value);
            UpdateIndexInternal(CurrentIndex);
        }

        public override void OnItemRemoved(T value)
        {
            Values.Remove(value);
            UpdateIndexInternal(CurrentIndex);
        }

        public override void OnCollectionSort(IList<T> observableList)
        {
            Values = observableList.ToList();
            UpdateIndexInternal(CurrentIndex);
        }

        public override void OnCollectionClear()
        {
            Values.Clear();
            UpdateIndexInternal(CurrentIndex);
        }
    }
}