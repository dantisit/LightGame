using System;
using System.Collections.Generic;
using System.Reflection;
using MVVM;
using MVVM.Binders;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MVVM.Binders
{
    public class OutsideClickMethodBinder : EmptyMethodBinder
    {
        [SerializeField] private List<BinderView> viewsToExclude;
        [SerializeField] private bool debugMode = false;

        private ViewModel _viewModel;
        private MethodInfo _cachedMethod;
        private RectTransform _rectTransform;
        private Canvas _rootCanvas;
        private Camera _uiCamera;

        protected override void BindInternal(ViewModel viewModel)
        {
            _viewModel = viewModel;
            _cachedMethod = viewModel.GetType().GetMethod(MethodName);
            _rectTransform = GetComponent<RectTransform>();
            
            // Find the root canvas
            _rootCanvas = GetComponentInParent<Canvas>();
            while (_rootCanvas != null && !_rootCanvas.isRootCanvas)
            {
                _rootCanvas = _rootCanvas.transform.parent.GetComponent<Canvas>();
            }
            
            // Get the camera (null for overlay canvas)
            _uiCamera = _rootCanvas != null && _rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? 
                _rootCanvas.worldCamera : null;
            
            base.BindInternal(viewModel);
        }

        private void Update()
        {
            // Check for mouse or touch input
            bool isClicked = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            
            if (!isClicked) return;

            // Get the current pointer position
            Vector2 pointerPosition = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                pointerPosition = Input.GetTouch(0).position;
            }

            // Check if the pointer is over any UI element
            if (IsPointerOverUIObject(pointerPosition))
            {
                if (debugMode) Debug.Log("Pointer is over a UI element");
                return;
            }

            // If we got here, the click was outside this and all excluded elements
            if (debugMode) Debug.Log("Invoking outside click method");
            _cachedMethod?.Invoke(_viewModel, null);
        }

        private bool IsPointerOverUIObject(Vector2 position)
        {
            // Check if pointer is over this object
            if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, position, _uiCamera))
            {
                if (debugMode) Debug.Log("Pointer is over this object");
                return true;
            }

            // Check if pointer is over any excluded view
            if (viewsToExclude != null)
            {
                foreach (var view in viewsToExclude)
                {
                    if (view == null) continue;
                    
                    var viewRect = view.GetComponent<RectTransform>();
                    if (viewRect != null && RectTransformUtility.RectangleContainsScreenPoint(viewRect, position, _uiCamera))
                    {
                        if (debugMode) Debug.Log($"Pointer is over excluded view: {view.name}");
                        return true;
                    }
                }
            }

            // Also check if pointer is over any UI element using raycasts
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            // Check if any of the results are this object or excluded views
            foreach (var result in results)
            {
                // Skip non-UI elements
                if (result.gameObject.GetComponent<Graphic>() == null && 
                    result.gameObject.GetComponent<GraphicRaycaster>() == null)
                {
                    continue;
                }
                
                // Check if it's this object
                if (result.gameObject == gameObject)
                {
                    if (debugMode) Debug.Log("Raycast hit this object");
                    return true;
                }
                
                // Check if it's any of the excluded views
                if (viewsToExclude != null)
                {
                    foreach (var view in viewsToExclude)
                    {
                        if (view == null) continue;
                        
                        if (result.gameObject == view.gameObject || 
                            result.gameObject.transform.IsChildOf(view.transform))
                        {
                            if (debugMode) Debug.Log($"Raycast hit excluded view: {view.name}");
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}
