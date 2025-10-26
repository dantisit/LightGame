
using DG.Tweening;
using UnityEngine;

namespace Core._.UI
{
    /// <summary>
    /// Tween for showing a hint popup from the bottom of the screen using slide animation.
    /// The popup slides up from below the screen with an optional bounce effect.
    /// </summary>
    public class ScaleTween : TweenableBase
    {
        [SerializeField] private Vector3 _startScale;
        [SerializeField] private Vector3 _endScale;
        [SerializeField] private float _duration = 0.6f;
        [SerializeField] private Ease _scaleEase = Ease.OutElastic;

        public override Tween Tween { get; set; }

        public override Tween CreateTween()
        {
            var sequence = DOTween.Sequence();
            var rect = (RectTransform)transform.parent;

            sequence.Append(rect.DOScale(_startScale, 0f));
            sequence.Append(rect.DOScale(_endScale, _duration).SetEase(_scaleEase));
            
            return sequence;
        }
    }
}
