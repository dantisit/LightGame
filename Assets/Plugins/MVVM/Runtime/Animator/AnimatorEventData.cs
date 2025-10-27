namespace MVVM.MVVMAnimator
{
    public class AnimatorEventData
    {
        public enum Types
        {
            OnBegin,
            OnEnd,
        }
        
        public Types Type;
        public ReplayObservableStateMachineTrigger.OnStateInfo OnStateInfo;

        public AnimatorEventData(Types type, ReplayObservableStateMachineTrigger.OnStateInfo onStateInfo)
        {
            Type = type;
            OnStateInfo = onStateInfo;
        }
    }
}