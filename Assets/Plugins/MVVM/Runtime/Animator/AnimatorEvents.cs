using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using R3.Triggers;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MVVM.MVVMAnimator
{
    // TODO: Refactor
    public class AnimatorEvents : AnimatorExtender
    {
        [SerializeField] private List<Event> startEvents = new();
        [SerializeField] private List<Event> endEvents = new();
        
        private RuntimeAnimatorController _lastController;
        private CompositeDisposable _disposables = new();
        
        private void Update()
        {
            if (Animator.runtimeAnimatorController == _lastController) return;
            
            _lastController = Animator.runtimeAnimatorController;
            Initialize();
        }

        private void Initialize()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable().AddTo(this);

            SubscribeToObservables();
            SubscribeToReplayObservables();
        }
        
        // We use Replay cause initialization happens in Update and some events can be already happen at Awake\Start
        // We can`t use Awake\Start for Initialization cause Unity at some point after can reinstantiate states and break subs. 
        private void SubscribeToReplayObservables()
        {
            var behaviours = Animator.GetBehaviours<ReplayObservableStateMachineTrigger>();
            if (behaviours.Length == 0)
                Debug.LogError($"ReplayObservableStateMachineTrigger is not assign to states on {gameObject.name} Animator, but trying to observe events.");
            
            behaviours.Select(x => x.OnStateExitAsObservable()).Merge().Subscribe(x =>
            {
                foreach (var e in endEvents.Where(e => x.StateInfo.IsName(e.AnimationKey)))
                {
                    e.UnityEvent.Invoke();
                    e.UltEvent.Invoke();
                }
            }).AddTo(_disposables);
            
            behaviours.Select(x => x.OnStateEnterAsObservable()).Merge().Subscribe(x =>
            {
                foreach (var e in startEvents.Where(e => x.StateInfo.IsName(e.AnimationKey)))
                {
                    e.UnityEvent.Invoke();
                    e.UltEvent.Invoke();
                }
            }).AddTo(_disposables );
        }

        // Should be eventually replaced by SubscribeToReplayObservables
        [Obsolete]
        private void SubscribeToObservables()
        {
            var behaviours = Animator.GetBehaviours<ObservableStateMachineTrigger>();
            // if (behaviours.Length == 0)
            //     Debug.LogError($"ObservableStateMachineTriggers is not assign to states on {gameObject.name} Animator, but trying to observe events.");
            
            behaviours.Select(x => x.OnStateExitAsObservable()).Merge().Subscribe(x =>
            {
                foreach (var e in endEvents.Where(e => x.StateInfo.IsName(e.AnimationKey)))
                {
                    e.UnityEvent.Invoke();
                    e.UltEvent.Invoke();
                }
            }).AddTo(_disposables);
            
            behaviours.Select(x => x.OnStateEnterAsObservable()).Merge().Subscribe(x =>
            {
                foreach (var e in startEvents.Where(e => x.StateInfo.IsName(e.AnimationKey)))
                {
                    e.UnityEvent.Invoke();
                    e.UltEvent.Invoke();
                }
            }).AddTo(_disposables );
        }
        
        [Serializable]
        public class Event
        {
            public string AnimationKey;
            public UnityEvent UnityEvent;
            public UltEvent UltEvent;
        }
    }
}