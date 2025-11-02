using System;
using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "AlphaEvent", menuName = "Easy Text Effects/Custom/Alpha Event", order = 101)]
    public class Effect_AlphaEvent : TextEffectInstance
    {
        [Header("Alpha Threshold")]
        [Tooltip("Fire event when alpha reaches this value (0-1)")]
        [Range(0, 1)]
        public float alphaThreshold = 1f;
        
        [Header("Alpha Range")]
        [Tooltip("Start alpha for timing calculation")]
        [Range(0, 1)]
        public float startAlpha = 0;
        
        [Tooltip("End alpha for timing calculation")]
        [Range(0, 1)]
        public float endAlpha = 1;
        
        private System.Collections.Generic.HashSet<int> firedCharacters = new System.Collections.Generic.HashSet<int>();

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0, int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;

            // Only check once per character (on first vertex)
            if (_startVertex == 0 && !firedCharacters.Contains(_charIndex))
            {
                // Calculate current alpha based on timing (uses same timing as color effects)
                float currentAlpha = Interpolate(startAlpha, endAlpha, _charIndex);
                
                // Fire event when alpha crosses threshold
                if (currentAlpha >= alphaThreshold)
                {
                    firedCharacters.Add(_charIndex);
                    FireCharacterStartEvent(_charIndex);
                }
            }
            
            // Don't actually modify the mesh - this is event-only
        }

        public override void StartEffect(TextEffectEntry entry)
        {
            base.StartEffect(entry);
            firedCharacters.Clear();
        }

        public override void StopEffect()
        {
            base.StopEffect();
            firedCharacters.Clear();
        }
    }
}
