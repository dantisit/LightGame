using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace MVVM.Binders.Animator
{
    [RequireComponent(typeof(UnityEngine.Animator))]
    public abstract class AnimatorVariableBinder<T> : ObservableBinder<T>
    {
        [FormerlySerializedAs("key")] [SerializeField] protected string _key;
        [SerializeField] private bool _enableAnimator;

        protected UnityEngine.Animator Animator { get; set; }

        public override void OnPropertyChanged(T newValue)
        {
            Animator ??= GetComponent<UnityEngine.Animator>();
            if(!Animator) return;

            if (_enableAnimator) Animator.enabled = true;
            // TODO: Make more elegant solution for this
            if (!Animator.isInitialized) Observable.NextFrame().Subscribe(_ => OnPropertyChanged(newValue)).AddTo(this);
            else
            {
                BindPropertyToAnimator(newValue);
                // We update animator so any transitions is triggered
                // This will run any attached events, so VM event handlers runs afterwards.a
                Animator.Update(Time.deltaTime);
            }
        }

        public abstract void BindPropertyToAnimator(T newValue);
    }
}