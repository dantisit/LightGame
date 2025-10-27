using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace MVVM.Binders
{
    [RequireComponent(typeof(LocalizeSpriteEvent))]
    public class LocalizeSpriteBinder : ObservableBinder<LocalizedSprite>
    {
        protected LocalizeSpriteEvent? _localizeSpriteEvent;

        public override void OnPropertyChanged(LocalizedSprite newValue)
        {
            _localizeSpriteEvent ??= GetComponent<LocalizeSpriteEvent>();
            _localizeSpriteEvent.AssetReference = newValue;
        }
    }
}