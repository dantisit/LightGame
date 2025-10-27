using System;
using System.Text.RegularExpressions;
using Core._.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace Light_and_controller.Scripts.UI
{
    public class HintView : WindowView<HintView, HintView.EventData>
    {
        [SerializeField] private TMP_Text tmp;
        [SerializeField] private TweenableBase showTween;
        [SerializeField] private TweenableBase hideTween;

        private bool _isLocked;
        private Sequence _sequence;
        
        protected override async void OnOpen(OpenWindowEvent<HintView, EventData> eventData)
        {
            await UniTask.WaitUntil(() => !_isLocked);
            base.OnOpen(eventData);
            
            // Get localized string
            string localizedText = eventData.Data.LocalizedText?.GetLocalizedString() ?? string.Empty;
            
            // Process input placeholders if enabled
            if (eventData.Data.UseInputPlaceholders && eventData.Data.InputActions != null)
            {
                localizedText = ProcessInputPlaceholders(
                    localizedText,
                    eventData.Data.InputActions,
                    eventData.Data.ActionMapName,
                    eventData.Data.AutoDetectControlScheme,
                    eventData.Data.FallbackControlScheme
                );
            }
            
            tmp.text = localizedText;
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
 

        /// <summary>
        /// Process input action placeholders in the hint text
        /// </summary>
        private string ProcessInputPlaceholders(
            string text,
            InputActionAsset inputActions,
            string actionMapName,
            bool autoDetectControlScheme,
            string fallbackControlScheme)
        {
            return Regex.Replace(text, @"\{([^}]+)\}", match =>
            {
                string actionName = match.Groups[1].Value;
                return GetInputBindingDisplay(
                    actionName,
                    inputActions,
                    actionMapName,
                    autoDetectControlScheme,
                    fallbackControlScheme
                );
            });
        }
        
        private string GetInputBindingDisplay(
            string actionName,
            InputActionAsset inputActions,
            string actionMapName,
            bool autoDetectControlScheme,
            string fallbackControlScheme)
        {
            var actionMap = inputActions.FindActionMap(actionMapName);
            if (actionMap == null)
                return $"[{actionName}]";
            
            var action = actionMap.FindAction(actionName);
            if (action == null || action.bindings.Count == 0)
                return $"[{actionName}]";
            
            string controlScheme = GetCurrentControlScheme(autoDetectControlScheme, fallbackControlScheme);
            InputBinding? bestBinding = FindBindingForControlScheme(action, controlScheme);
            
            if (bestBinding.HasValue)
                return GetBindingDisplayString(bestBinding.Value, action);
            
            // Fallback: first non-composite binding
            foreach (var binding in action.bindings)
            {
                if (!binding.isComposite && !binding.isPartOfComposite)
                    return GetBindingDisplayString(binding, action);
            }
            
            return GetBindingDisplayString(action.bindings[0], action);
        }
        
        private string GetCurrentControlScheme(bool autoDetect, string fallback)
        {
            if (!autoDetect)
                return fallback;
            
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad)
                {
                    if (gamepad.leftStick.IsActuated() ||
                        gamepad.rightStick.IsActuated() ||
                        gamepad.buttonSouth.isPressed ||
                        gamepad.buttonNorth.isPressed ||
                        gamepad.buttonEast.isPressed ||
                        gamepad.buttonWest.isPressed)
                    {
                        return "Gamepad";
                    }
                }
            }
            
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.isPressed)
                return "Keyboard&Mouse";
            
            var mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed))
                return "Keyboard&Mouse";
            
            return fallback;
        }
        
        private InputBinding? FindBindingForControlScheme(InputAction action, string controlScheme)
        {
            foreach (var binding in action.bindings)
            {
                if (binding.isComposite)
                    continue;
                
                if (!string.IsNullOrEmpty(binding.groups))
                {
                    var groups = binding.groups.Split(';');
                    foreach (var group in groups)
                    {
                        if (group.Trim().Equals(controlScheme, StringComparison.OrdinalIgnoreCase))
                            return binding;
                    }
                }
            }
            return null;
        }
        
        private string GetBindingDisplayString(InputBinding binding, InputAction action)
        {
            int bindingIndex = -1;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].id == binding.id)
                {
                    bindingIndex = i;
                    break;
                }
            }
            
            if (bindingIndex == -1)
                return "[?]";
            
            string displayString = action.GetBindingDisplayString(
                bindingIndex,
                InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
            
            return string.IsNullOrEmpty(displayString) ? "[?]" : displayString;
        }

        public class EventData
        {
            public LocalizedString LocalizedText;
            public bool UseInputPlaceholders;
            public InputActionAsset InputActions;
            public string ActionMapName;
            public bool AutoDetectControlScheme;
            public string FallbackControlScheme;
        }
    }
}