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
    [RequireComponent(typeof(BinderView))]
    public class ObserveWindowEvents : MonoBehaviour
    {
        [SerializeField] private bool setLastSiblingIndexOnOpen = true;
        private BinderView _binderView;
        public void Initialize()
        {
            _binderView ??= GetComponent<BinderView>();
            
            EventBus.On<OpenWindowEvent>()
                .Where(x => x.ViewModel.IsInstanceOf(_binderView.ViewModelTypeFullName))
                .Subscribe(x =>
                {
                    _binderView.BindViewModel(x.ViewModel);
                    if(setLastSiblingIndexOnOpen) transform.SetSiblingIndex(transform.parent.childCount - 1);
                    gameObject.SetActive(true);
                })
                .AddTo(this);
            
            EventBus.On<CloseWindowEvent>()
                .Where(_ => _binderView.ViewModel != null)
                .Where(x => x.ViewModel == _binderView.ViewModel || x.GetWindowType() == _binderView.ViewModel.GetType())
                .Subscribe(_ =>
                {
                    gameObject.SetActive(false);
                    _binderView.ViewModel?.Dispose();
                })
                .AddTo(this);
        }
    }
}