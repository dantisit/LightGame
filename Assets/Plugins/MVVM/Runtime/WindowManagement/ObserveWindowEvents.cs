using System;
using MVVM;
using MVVM.Events;
using MVVM.Utils;
using R3;
using UltEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plugins.MVVM.Runtime
{
    [RequireComponent(typeof(View))]
    public class ObserveWindowEvents : MonoBehaviour
    {
        [SerializeField] private bool setLastSiblingIndexOnOpen = true;
        private View _view;
        public void Initialize()
        {
            _view ??= GetComponent<View>();
            
            EventBus.On<OpenWindowEvent>()
                .Where(x => x.ViewModel.IsInstanceOf(_view.ViewModelTypeFullName))
                .Subscribe(x =>
                {
                    _view.Bind(x.ViewModel);
                    if(setLastSiblingIndexOnOpen) transform.SetSiblingIndex(transform.parent.childCount - 1);
                    gameObject.SetActive(true);
                })
                .AddTo(this);
            
            EventBus.On<CloseWindowEvent>()
                .Where(_ => _view.ViewModel != null)
                .Where(x => x.ViewModel == _view.ViewModel || x.GetWindowType() == _view.ViewModel.GetType())
                .Subscribe(_ =>
                {
                    gameObject.SetActive(false);
                    _view.ViewModel?.Dispose();
                })
                .AddTo(this);
        }
    }
}