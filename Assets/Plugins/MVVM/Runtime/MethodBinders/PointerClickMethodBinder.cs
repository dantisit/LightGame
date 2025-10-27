using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.Binders
{

    public class PointerClickMethodBinder : EmptyMethodBinder, IPointerClickHandler
    {
        private ViewModel _viewModel;
        private MethodInfo _cachedMethod;

        protected override void BindInternal(ViewModel viewModel)
        {
            _viewModel = viewModel;
            _cachedMethod = viewModel.GetType().GetMethod(MethodName);

            base.BindInternal(viewModel);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            _cachedMethod?.Invoke(_viewModel, null);
        }
    }
}
