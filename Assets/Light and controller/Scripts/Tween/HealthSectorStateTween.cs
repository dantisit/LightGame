using DG.Tweening;
using UnityEngine;

namespace Core._.UI
{
    /// <summary>
    /// Unified tween for health sector animations with different state-based behaviors.
    /// Supports heal, damage, pending heal, and pending damage states with configurable parameters.
    /// </summary>
    public class HealthSectorStateTween : TweenableBase
    {
        public enum AnimationState
        {
            Heal,
            Damage,
            PendingHeal,
            PendingDamage,
            CancelPendingHeal,
            CancelPendingDamage
        }

        [Header("Animation State")]
        [SerializeField] private AnimationState currentState = AnimationState.Heal;
        
        [Header("Tweens")]
        [SerializeField] private TweenableBase healTween;
        [SerializeField] private TweenableBase damageTween;
        [SerializeField] private TweenableBase pendingHealTween;
        [SerializeField] private TweenableBase pendingDamageTween;

        [Header("Settings")]
        [SerializeField] private float defaultCancelDuration = 0.5f;

        private float _customDuration = -1f;
        private bool _isInitialized = false;
        
        public override Tween Tween { get; set; }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            
            // Initialize all child tweens on first use
            if (healTween != null)
            {
                healTween.Tween = healTween.CreateTween().SetAutoKill(false);
                healTween.Tween.Pause();
            }
            
            if (damageTween != null)
            {
                damageTween.Tween = damageTween.CreateTween().SetAutoKill(false);
                damageTween.Tween.Pause();
            }
            
            if (pendingHealTween != null)
            {
                pendingHealTween.Tween = pendingHealTween.CreateTween().SetAutoKill(false);
                pendingHealTween.Tween.Pause();
            }
            
            if (pendingDamageTween != null)
            {
                pendingDamageTween.Tween = pendingDamageTween.CreateTween().SetAutoKill(false);
                pendingDamageTween.Tween.Pause();
            }
            
            _isInitialized = true;
        }

        public override Tween CreateTween()
        {
            // For editor preview, return the appropriate tween based on current state
            // This allows the CustomTweenAnimationEditor to preview the animation
            switch (currentState)
            {
                case AnimationState.Heal:
                    return healTween?.CreateTween() ?? DOTween.Sequence();
                case AnimationState.Damage:
                    return damageTween?.CreateTween() ?? DOTween.Sequence();
                case AnimationState.PendingHeal:
                    return pendingHealTween?.CreateTween() ?? DOTween.Sequence();
                case AnimationState.PendingDamage:
                    return pendingDamageTween?.CreateTween() ?? DOTween.Sequence();
                case AnimationState.CancelPendingHeal:
                case AnimationState.CancelPendingDamage:
                    // For cancel states, return the pending tween in reverse
                    var cancelTween = currentState == AnimationState.CancelPendingHeal 
                        ? pendingHealTween?.CreateTween() 
                        : pendingDamageTween?.CreateTween();
                    if (cancelTween != null)
                    {
                        cancelTween.SetInverted(true);
                    }
                    return cancelTween ?? DOTween.Sequence();
                default:
                    return DOTween.Sequence();
            }
        }
        
        public override void Play()
        {
            // Execute the appropriate animation based on current state
            switch (currentState)
            {
                case AnimationState.Heal:
                    ExecuteHeal();
                    break;
                case AnimationState.Damage:
                    ExecuteDamage();
                    break;
                case AnimationState.PendingHeal:
                    ExecutePendingHeal();
                    break;
                case AnimationState.PendingDamage:
                    ExecutePendingDamage();
                    break;
                case AnimationState.CancelPendingHeal:
                    ExecuteCancelPendingHeal();
                    break;
                case AnimationState.CancelPendingDamage:
                    ExecuteCancelPendingDamage();
                    break;
            }
        }

        /// <summary>
        /// Set the animation state and optional duration, then call Play() to execute.
        /// </summary>
        public void SetState(AnimationState state, float duration = -1f)
        {
            currentState = state;
            _customDuration = duration;
        }

        private void ExecuteHeal()
        {
            EnsureInitialized();
            ResetDurations();
            healTween?.Tween.PlayForward();
            if (pendingHealTween?.Tween.IsPlaying() == true) 
                pendingHealTween.Tween.Complete();
            damageTween?.Tween.Pause();
            pendingDamageTween?.Tween.Rewind();
        }

        private void ExecuteDamage()
        {
            EnsureInitialized();
            ResetDurations();
            damageTween?.Tween.PlayForward();
            pendingDamageTween?.Tween.Pause();
            healTween?.Tween.Rewind();
            pendingHealTween?.Tween.Rewind();
        }

        private void ExecutePendingHeal()
        {
            EnsureInitialized();
            if (pendingHealTween != null && _customDuration > 0)
            {
                pendingHealTween.SetDuration(_customDuration);
            }
            
            pendingHealTween?.Tween.PlayForward();
            damageTween?.Tween.Pause();
            pendingDamageTween?.Tween.Rewind();
            healTween?.Tween.Rewind();
        }

        private void ExecutePendingDamage()
        {
            EnsureInitialized();
            if (pendingDamageTween != null && _customDuration > 0)
            {
                pendingDamageTween.SetDuration(_customDuration);
            }
            
            pendingDamageTween?.Tween.PlayForward();
            pendingHealTween?.Tween.Rewind();
            damageTween?.Tween.Rewind();
            healTween?.Tween.Rewind();
        }

        private void ExecuteCancelPendingHeal()
        {
            EnsureInitialized();
            if (pendingHealTween?.Tween.IsPlaying() != true) return;
            
            pendingHealTween.SetDuration(defaultCancelDuration);
            pendingHealTween.Tween.PlayBackwards();
            damageTween?.Tween.Rewind();
            pendingDamageTween?.Tween.Rewind();
            healTween?.Tween.Rewind();
        }

        private void ExecuteCancelPendingDamage()
        {
            EnsureInitialized();
            if (pendingDamageTween?.Tween.IsPlaying() != true) return;
            
            pendingDamageTween.SetDuration(defaultCancelDuration);
            pendingDamageTween.Tween.PlayBackwards();
            pendingHealTween?.Tween.Rewind();
            damageTween?.Tween.Rewind();
            healTween?.Tween.Rewind();
        }

        private void ResetDurations()
        {
            if (pendingHealTween?.Tween != null)
                pendingHealTween.SetDuration(pendingHealTween.Tween.Duration());
            if (pendingDamageTween?.Tween != null)
                pendingDamageTween.SetDuration(pendingDamageTween.Tween.Duration());
            if (healTween?.Tween != null)
                healTween.SetDuration(healTween.Tween.Duration());
            if (damageTween?.Tween != null)
                damageTween.SetDuration(damageTween.Tween.Duration());
        }
    }
}
