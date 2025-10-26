using System;
using Core._.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Light_and_controller.Scripts.UI
{
    public class HintView : WindowView<HintView, HintView.EventData>
    {
        [SerializeField] private TMP_Text tmp;
        [SerializeField] private TweenableBase showTween;
        [SerializeField] private TweenableBase hideTween;

        private bool _isLocked;
        
        protected override async void OnOpen(OpenWindowEvent<HintView, EventData> eventData)
        {
            await UniTask.WaitUntil(() => !_isLocked);
            base.OnOpen(eventData);
            tmp.text = eventData.Data.Text;
            _isLocked = true;
            showTween.Play();
            showTween.Tween.OnComplete(() => _isLocked = false);
        }
        
        protected override async void OnClose(CloseWindowEvent<HintView> eventData)
        {
            await UniTask.WaitUntil(() => !_isLocked);
            _isLocked = true;
            hideTween.Play();
            hideTween.Tween.OnComplete(() =>
            {
                _isLocked = false;
                gameObject.SetActive(false);
            });
        }
 

        public class EventData
        {
            public string Text;
        }
    }
}