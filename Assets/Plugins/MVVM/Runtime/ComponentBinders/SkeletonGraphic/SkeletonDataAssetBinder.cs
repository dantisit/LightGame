#if SPINE_UNITY
using Spine.Unity;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders.SkeletonGraphic
{
    [RequireComponent(typeof(Spine.Unity.SkeletonGraphic))]
    public class SkeletonDataAssetBinder : ObservableBinder<SkeletonDataAsset>
    {
        [SerializeField] private UnityEvent _onChanged;
        
        private Spine.Unity.SkeletonGraphic _skeletonGraphic;

        public override void OnPropertyChanged(SkeletonDataAsset newValue)
        {
            if(newValue == null) return;
            
            _skeletonGraphic ??= GetComponent<Spine.Unity.SkeletonGraphic>();
            
            _skeletonGraphic.skeletonDataAsset = newValue;
            _skeletonGraphic.Initialize(true);
            _onChanged?.Invoke();
        }
    }
}
#endif