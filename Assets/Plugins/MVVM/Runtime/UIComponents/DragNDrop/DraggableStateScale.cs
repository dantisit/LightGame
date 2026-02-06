using System;
using DG.Tweening;
using MVVM;
using Plugins.MVVM.Runtime.UIComponents.Behaviors;
using UnityEngine;
using R3;
using UnityEngine.Serialization;

namespace Plugins.MVVM.Runtime.UIComponents.DragNDrop
{

    public class DraggableStateScale : DragEffect
    {
        [SerializeField] private Transform transformToScale;
        [SerializeField] private float scaleDuration = 0.2f;
        [FormerlySerializedAs("dragStateToScale")] [SerializeField] private DragStateToScale dragState;

        private Vector3 _awakeScale;
        
        private void OnValidate()
        {
           transformToScale ??= transform;
        }
        
        private void Awake()
        {
            _awakeScale = transformToScale.localScale;
        }
        
        public override void SetIsDragging(bool isDragging)
        {
            ScaleToValue(isDragging ? dragState.Dragging : dragState.EndDrag);
        }
        

        private void OnDisable()
        {
            this.DOComplete();
        }

        private void ScaleToValue(float scaleMultiplier)
        {
            if (!enabled) return;

            transformToScale.DOScale(_awakeScale * scaleMultiplier, scaleDuration);
        }
        
        [Serializable]
        public class DragStateToScale
        {
            [SerializeField] private float dragging = 1.1f;
            [SerializeField] private float endDrag = 1f;
            
            public float Dragging => dragging;
            public float EndDrag => endDrag;
        }
    }
}
