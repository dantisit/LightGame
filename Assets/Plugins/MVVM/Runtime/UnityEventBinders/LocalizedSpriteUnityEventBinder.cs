using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace MVVM.Binders
{
    [RequireComponent(typeof(LocalizeSpriteEvent))]
    public class LocalizedSpriteUnityEventBinder : UnityEventBinder<LocalizedSprite>
    {
        [SerializeField] private bool _disableIfNull;

        protected override void OnStart()
        {
            base.OnStart();
            _event.AddListener(OnEvent);
        }

        private void OnEvent(LocalizedSprite newValue)
        {
            if (_disableIfNull) gameObject.SetActive(newValue != null);
        }
    }
}
