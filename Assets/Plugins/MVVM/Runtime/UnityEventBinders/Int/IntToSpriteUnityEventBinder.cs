using System;
using System.Collections.Generic;
using System.Linq;
using MVVM.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntToSpriteUnityEventBinder : ObservableBinder<int>
    {
        [SerializeField] private List<IntToSpriteMapping> _mappings = new();
        [SerializeField] private Sprite _spriteByDefault;

        [SerializeField] private UnityEvent<Sprite> _event;

        public override void OnPropertyChanged(int newValue)
        {
            var mapping = _mappings.FirstOrDefault(x => x.CompareOperation.Compare(newValue, x.Value));
            _event.Invoke(mapping != null ? mapping.Sprite : _spriteByDefault);
        }
    }

    [Serializable]
    public class IntToSpriteMapping
    {
        [SerializeField] private CompareOperation _compare;
        [SerializeField] private int _value;
        [SerializeField] private Sprite _sprite;

        public CompareOperation CompareOperation => _compare;
        public int Value => _value;
        public Sprite Sprite => _sprite;
    }
}
