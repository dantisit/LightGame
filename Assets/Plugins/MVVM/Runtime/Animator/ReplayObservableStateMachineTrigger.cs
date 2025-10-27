using R3;
using UnityEngine;

namespace MVVM.MVVMAnimator
{
    using Animator = UnityEngine.Animator;
    
    public class ReplayObservableStateMachineTrigger : StateMachineBehaviour
    {
        public class OnStateInfo
        {
            public Animator Animator { get; private set; }
            public AnimatorStateInfo StateInfo { get; private set; }
            public int LayerIndex { get; private set; }

            public OnStateInfo(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                Animator = animator;
                StateInfo = stateInfo;
                LayerIndex = layerIndex;
            }
        }

        public class OnStateMachineInfo
        {
            public Animator Animator { get; private set; }
            public int StateMachinePathHash { get; private set; }

            public OnStateMachineInfo(Animator animator, int stateMachinePathHash)
            {
                Animator = animator;
                StateMachinePathHash = stateMachinePathHash;
            }
        }

        // OnStateExit
        ReplaySubject<OnStateInfo> onStateExit = new();

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onStateExit?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateExitAsObservable()
        {
            return onStateExit;
        }

        // OnStateEnter
        ReplaySubject<OnStateInfo> onStateEnter = new();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onStateEnter?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateEnterAsObservable()
        {
            return onStateEnter;
        }
    }
}