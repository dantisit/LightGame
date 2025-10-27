using Plugins.MVVM.Runtime;
using UnityEngine;
using UnityEngine.Animations;

namespace MVVM.MVVMAnimator
{
    public class OverrideSortingBehaviour: StateMachineBehaviour
    {
        [SortingLayer, SerializeField] private int sortingLayerID;
        [SerializeField] private int sortingOrder;
        
        private Canvas _canvas;
        private bool _defaultOverrideSorting;
        private int _defaultSortingLayerID;
        private int _defaultSortingOrder;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            _canvas ??= animator.GetComponentInChildren<Canvas>();
            _defaultOverrideSorting = _canvas.overrideSorting;
            _defaultSortingLayerID = _canvas.sortingLayerID;
            _defaultSortingOrder = _canvas.sortingOrder;
            _canvas.overrideSorting = true;
            _canvas.sortingLayerID = sortingLayerID;
            _canvas.sortingOrder = sortingOrder;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _canvas.overrideSorting = _defaultOverrideSorting;
            _canvas.sortingLayerID = _defaultSortingLayerID;
            _canvas.sortingOrder = _defaultSortingOrder;
        }
    }
}