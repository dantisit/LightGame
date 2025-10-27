using UnityEngine;

namespace MVVM.MVVMAnimator
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorExtender : MonoBehaviour
    {
        protected Animator Animator { get; set; }
        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
        }
    }
}