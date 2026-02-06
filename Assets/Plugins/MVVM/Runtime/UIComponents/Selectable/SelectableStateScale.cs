using System;
using DG.Tweening;
using UnityEngine;
using R3;
using UnityEngine.Serialization;

namespace Core.Client.UI.Components
{
    [RequireComponent(typeof(ISelectable))]
    public class SelectableStateScale : MonoBehaviour
    {
        [SerializeField] private Transform transformToScale;
        [SerializeField] private float scaleDuration = 0.2f;
        [FormerlySerializedAs("selectionStateToTextColor")] [SerializeField] private SelectionStateToScale selectionState;

        private Vector3 _awakeScale;
        private Tweener _lastTween;
        
        private void OnValidate()
        {
            if (transformToScale == null) transformToScale = transform;
        }
        
        private void Awake()
        {
            var button = GetComponent<ISelectable>();
            _awakeScale = transformToScale.localScale;
            button.SelectionStateTransition.Subscribe(OnSelectionStateTransition).AddTo(this);
        }

        private void OnDisable()
        {
            _lastTween?.Kill();
        }

        public void OnSelectionStateTransition(SelectionState selectionState)
        {
            if(!enabled) return;
            _lastTween?.Kill();
            var scale = this.selectionState.Get(selectionState);
            _lastTween = transformToScale.DOScale(_awakeScale * scale, scaleDuration);
        }
        
        [Serializable]
        public class SelectionStateToScale
        {
            [SerializeField] private SelectionStateValues<float> values = new()
            {
                Normal = 1f,
                Highlighted = 1.1f,
                Pressed = 0.9f,
                Selected = 1f,
                Disabled = 1f,
                // DisabledHighlighted = 1f,
                // DisabledPressed = 1f,
            };
    
            public float Get(SelectionState state) => values.Get(state);
            public void SetNormal(float value) => values.SetNormal(value);
        }
    }
}