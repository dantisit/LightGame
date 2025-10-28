using System;
using Core._.UI;
using DG.Tweening;
using UnityEngine;

namespace Light_and_controller.Scripts.UI
{
    public class HealthSectorView : MonoBehaviour
    {
        [SerializeField] private TweenableBase healTween;
        [SerializeField] private TweenableBase damageTween;
        [SerializeField] private TweenableBase pendingHealTween;
        [SerializeField] private TweenableBase pendingDamageTween;

        public bool IsFilled { get; set; } = true;
        private bool _wasPendingToDamage = false;
        private bool _wasPendingToHeal = false;

        private void Awake()
        {
            healTween.Tween = healTween.CreateTween().SetAutoKill(false);
            healTween.Tween.Pause();
            damageTween.Tween = damageTween.CreateTween().SetAutoKill(false);
            damageTween.Tween.Pause();

            pendingHealTween.Tween = pendingHealTween.CreateTween().SetAutoKill(false);
            pendingHealTween.Tween.Pause();
            pendingDamageTween.Tween = pendingDamageTween.CreateTween().SetAutoKill(false);
            pendingDamageTween.Tween.Pause();
        }

        public void OnHeal()
        {
            ResetDuration();
            healTween.Tween.PlayForward();
            if(pendingHealTween.Tween.IsPlaying()) pendingHealTween.Tween.Complete();
            damageTween.Tween.Pause();
            pendingDamageTween.Tween.Rewind(); 
            
            IsFilled = true;
            Debug.Log($"OnHeal {gameObject.name}");
        }

        public void OnDamage()
        {
            ResetDuration();
            damageTween.Tween.PlayForward();
            pendingDamageTween.Tween.Pause();
            healTween.Tween.Rewind();
            pendingHealTween.Tween.Rewind();
            
            IsFilled = false;
            Debug.Log($"OnDamage {gameObject.name}");
        }
        
        public void OnHealPending(float duration)
        {
            pendingHealTween.SetDuration(duration);
            pendingHealTween.Tween.PlayForward();
            damageTween.Tween.Pause();
            pendingDamageTween.Tween.Rewind();
            healTween.Tween.Rewind();
            
            Debug.Log($"OnHealPending {gameObject.name}");
        }

        public void OnHealPendingCancel()
        {
            if(!pendingHealTween.Tween.IsPlaying()) return;
            pendingHealTween.SetDuration(0.5f);
            pendingHealTween.Tween.PlayBackwards();
            damageTween.Tween.Rewind();
            pendingDamageTween.Tween.Rewind();
            healTween.Tween.Rewind();
            
            Debug.Log($"OnHealPendingCancel {gameObject.name}");
        }

        public void OnTakeDamagePending(float duration)
        {
            pendingDamageTween.SetDuration(duration);
            pendingHealTween.Tween.Rewind();
            damageTween.Tween.Rewind();
            pendingDamageTween.Tween.PlayForward();
            healTween.Tween.Rewind();
            
            Debug.Log($"OnTakeDamagePending {gameObject.name}");
        }
        
        public void OnTakeDamagePendingCancel()
        {
            if(!pendingDamageTween.Tween.IsPlaying()) return;
            pendingDamageTween.SetDuration(0.5f);
            pendingHealTween.Tween.Rewind();
            damageTween.Tween.Rewind();
            pendingDamageTween.Tween.PlayBackwards();
            healTween.Tween.Rewind();
            
            Debug.Log($"OnTakeDamagePendingCancel {gameObject.name}");
        }

        private void ResetDuration()
        {
            pendingHealTween.SetDuration(pendingHealTween.Tween.Duration());
            pendingDamageTween.SetDuration(pendingDamageTween.Tween.Duration());
            healTween.SetDuration(healTween.Tween.Duration());
            damageTween.SetDuration(damageTween.Tween.Duration());
        }
    }
}