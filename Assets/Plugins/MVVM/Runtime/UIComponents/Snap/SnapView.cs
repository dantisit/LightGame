using System;
using MVVM;
using Plugins.MVVM.Runtime.UIComponents.Behaviors;
using UnityEngine;

namespace Plugins.MVVM.Runtime.UIComponents
{
    public class SnapView : View<SnapViewModel>
    {
        [Header("Snap Thresholds")]
        [SerializeField, Tooltip("Distance threshold to start snapping")]
        private float snapInThreshold = 1.5f;
        
        [SerializeField, Tooltip("Distance threshold to release snap (prevents flickering)")]
        private float snapOutThreshold = 2f;
        
        [Header("Settings")]
        [SerializeField] private bool enableSnapping = true;
        
        private bool _isCurrentlySnapped;
        private Vector3? _currentSnapTarget;
        
        public bool EnableSnapping
        {
            get => enableSnapping;
            set => enableSnapping = value;
        }
        
        protected override void OnBindViewModel(SnapViewModel viewModel)
        {
            DisposeOnDestroy = false;
            viewModel.SnapInThreshold.Value = snapInThreshold;
            viewModel.SnapOutThreshold.Value = snapOutThreshold;
            viewModel.EnableSnapping.Value = enableSnapping;
            
            // Bind ViewModel properties to component
            Bind(viewModel.EnableSnapping).To(x => EnableSnapping = x);
            Bind(viewModel.SnapInThreshold).To(x => snapInThreshold = x);
            Bind(viewModel.SnapOutThreshold).To(x => snapOutThreshold = x);
        }
    }
}
