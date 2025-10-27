using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using Light_and_controller.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Light_and_controller.Scripts.UI.Triggers
{
    /// <summary>
    /// Combined trigger component that shows hint on enter and hides on exit
    /// Supports input action placeholders in hint text (e.g., "Press {Jump} to jump")
    /// </summary>
    public class HintTrigger : MonoBehaviour
    {
        [Header("Hint Settings")]
        [SerializeField] private LocalizedString hintText;
        [SerializeField] private string playerTag = "Player";
        
        [Header("Input System Integration")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private bool useInputPlaceholders = true;
        [Tooltip("Auto-detect and show bindings for the currently active control scheme (Gamepad/Keyboard)")]
        [SerializeField] private bool autoDetectControlScheme = true;
        [Tooltip("Fallback control scheme if auto-detect is disabled (e.g., 'Keyboard&Mouse', 'Gamepad')")]
        [SerializeField] private string fallbackControlScheme = "Keyboard&Mouse";
        
        [Header("Trigger Behavior")]
        [SerializeField] private bool showOnEnter = true;
        [SerializeField] private bool hideOnExit = true;
        [SerializeField] private bool triggerOnce = false;
        
        private bool _hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerOnce && _hasTriggered)
                return;
            
            if (showOnEnter && other.CompareTag(playerTag))
            {
                ShowHint();
                _hasTriggered = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (hideOnExit && other.CompareTag(playerTag))
            {
                HideHint();
            }
        }

        private void ShowHint()
        {
            var eventData = new OpenWindowEvent<HintView, HintView.EventData>
            {
                Data = new HintView.EventData
                {
                    LocalizedText = hintText,
                    UseInputPlaceholders = useInputPlaceholders,
                    InputActions = inputActions,
                    ActionMapName = actionMapName,
                    AutoDetectControlScheme = autoDetectControlScheme,
                    FallbackControlScheme = fallbackControlScheme
                }
            };
            
            EventBus.Publish(eventData);
        }

        private void HideHint()
        {
            var eventData = new CloseWindowEvent<HintView>();
            EventBus.Publish(eventData);
        }

        /// <summary>
        /// Manually show the hint (useful for testing or programmatic triggering)
        /// </summary>
        public void ManualShowHint()
        {
            ShowHint();
        }

        /// <summary>
        /// Manually hide the hint (useful for testing or programmatic triggering)
        /// </summary>
        public void ManualHideHint()
        {
            HideHint();
        }

        /// <summary>
        /// Reset the trigger so it can be activated again
        /// </summary>
        public void ResetTrigger()
        {
            _hasTriggered = false;
        }

        /// <summary>
        /// Update the hint text at runtime
        /// </summary>
        public void SetHintText(LocalizedString newText)
        {
            hintText = newText;
        }
    }
}
