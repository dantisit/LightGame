using System;
using System.Linq;
using R3;
using R3.Triggers;
using UnityEngine;

namespace MVVM.MVVMAnimator
{
    public enum AnimatorEventType
    {
        Start,
        End
    }

    public static class AnimatorExtensions
    {
        public static Observable<AnimatorEventData> ObserveEvents(this Animator animator)
        {
            if (animator == null )
                throw new ArgumentNullException();

            var behaviours = animator.GetBehaviours<ReplayObservableStateMachineTrigger>();
            if (behaviours.Length == 0)
                Debug.LogError($"No {nameof(ReplayObservableStateMachineTrigger)} found on Animator {animator.gameObject.name}");

            var begin = behaviours
                .Select(b => b.OnStateEnterAsObservable()).Merge()
                .Select(x => new AnimatorEventData(AnimatorEventData.Types.OnBegin, x));
            var end = behaviours
                .Select(b => b.OnStateExitAsObservable()).Merge()
                .Select(x => new AnimatorEventData(AnimatorEventData.Types.OnEnd, x));
            
            return begin.Merge(end);
        }
    }
}