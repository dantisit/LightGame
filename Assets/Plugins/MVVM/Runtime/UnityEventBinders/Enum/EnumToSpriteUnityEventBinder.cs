using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class EnumToSpriteUnityEventBinder : ObservableBinder<object>
    {
        [SerializeField] private List<EnumToSpriteMapping> _mappings = new();
        [SerializeField] private Sprite _spriteByDefault;
        [SerializeField] private UnityEvent<Sprite> _event;
        [SerializeField] private UnityEvent<bool> _visibilityEvent;

        private Dictionary<int, Sprite>? _spritesMap;

        public override void OnPropertyChanged(object newValue)
        {
            if (_spritesMap == null)
            {
                _spritesMap = new Dictionary<int, Sprite>();

                foreach (var mapping in _mappings)
                {
                    _spritesMap[mapping.Value] = mapping.Sprite;
                }
            }

            bool hasSprite = false;
            Sprite spriteToUse = _spriteByDefault;

            if (newValue != null)
            {
                var intValue = Convert.ToInt32(newValue);
                if (_spritesMap.TryGetValue(intValue, out var sprite))
                {
                    spriteToUse = sprite;
                }
            }

            if(spriteToUse != null)  hasSprite = true;

            _event.Invoke(spriteToUse);
            _visibilityEvent.Invoke(hasSprite);
        }

        [Serializable]
        public class EnumToSpriteMapping
        {
            [SerializeField] private int _value;
            [SerializeField] private Sprite _sprite;

            public int Value => _value;
            public Sprite Sprite => _sprite;
        }
    }
}
