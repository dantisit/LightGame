using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using Light_and_controller.Scripts.UI;
using UnityEngine;

namespace Light_and_controller.Scripts.UI.Triggers
{
    /// <summary>
    /// Trigger component that hides the hint when the player enters the trigger area
    /// </summary>
    public class HideHintTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool hideOnce = false;
        
        private bool _hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hideOnce && _hasTriggered)
                return;
            
            if (other.CompareTag(playerTag))
            {
                HideHint();
                _hasTriggered = true;
            }
        }

        private void HideHint()
        {
            var eventData = new CloseWindowEvent<HintView>();
            EventBus.Publish(eventData);
        }

        /// <summary>
        /// Manually trigger the hint hide (useful for testing or programmatic triggering)
        /// </summary>
        public void TriggerHide()
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
    }
}
