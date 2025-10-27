using System;
using System.Collections.Generic;
using Core.Client.Events;
using MVVM;
using Plugins.MVVM.Runtime.UIComponents.DragNDrop;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class FilteredGraphicRaycasterBase : GraphicRaycaster
{
    public virtual bool IsRoot { get; }
    public bool IsFilterActive() => GetFilter() != null;
    public bool TestGameObject(GameObject go) => GetFilter()?.Invoke(go) ?? true;

    public void RaycastUnfiltered(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        base.Raycast(eventData, resultAppendList);
    }
    
    protected abstract Func<GameObject, bool> GetFilter();
}

/// <summary>
/// Base raycaster with per-type static filter.
/// Each derived type (T) gets its own static ShouldAllowInput.
/// </summary>
public abstract class FilteredGraphicRaycaster<T> : FilteredGraphicRaycasterBase
    where T : FilteredGraphicRaycaster<T>
{
    protected static Func<GameObject, bool> ShouldAllowInput { get; set; }
    
    // ✅ Component lifetime disposables (never cleared until OnDestroy)
    protected CompositeDisposable ComponentDisposables { get; } = new();
    
    // ✅ Lock lifetime disposables (cleared on each unlock)
    protected CompositeDisposable LockDisposables { get; private set; } = new();

    [SerializeField] private bool isRoot;

    public override bool IsRoot => isRoot;
    
    protected override void Awake()
    {
        base.Awake();
        if (isRoot) InitializeRootFilter();
    }

    protected override void OnDestroy()
    {
        if (isRoot) CleanupRootFilter();
        base.OnDestroy();
    }

    protected virtual void InitializeRootFilter()
    {
        SubscribeToEvents();
    }

    protected virtual void CleanupRootFilter()
    {
        ComponentDisposables.Dispose();
        LockDisposables.Dispose();
        ShouldAllowInput = null;
    }

    private void SubscribeToEvents()
    {
        EventBus.On<LockInputEvent>()
            .Subscribe(evt =>
            {
                LockDisposables.Dispose();
                LockDisposables = new CompositeDisposable();
                
                HandleLockInput(evt);
            })
            .AddTo(ComponentDisposables);
        
        EventBus.On<UnlockInputEvent>()
            .Subscribe(evt =>
            {
                LockDisposables.Dispose();
                LockDisposables = new CompositeDisposable();
                
                HandleUnlockInput(evt);
            })
            .AddTo(ComponentDisposables);
    }

    protected abstract void HandleLockInput(LockInputEvent evt);
    protected abstract void HandleUnlockInput(UnlockInputEvent evt);

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (ShouldSkipFiltering())
        {
            base.Raycast(eventData, resultAppendList);
            return;
        }

        FilterRaycastResults(eventData, resultAppendList);
    }

    protected virtual bool ShouldSkipFiltering()
    {
        return DragManager.IsAnyDragging || ShouldAllowInput == null;
    }

    private void FilterRaycastResults(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        int originalCount = resultAppendList.Count;
        base.Raycast(eventData, resultAppendList);
        RemoveDisallowedResults(resultAppendList, originalCount);
    }

    private void RemoveDisallowedResults(List<RaycastResult> results, int startIndex)
    {
        if (ShouldAllowInput == null) return;

        for (int i = results.Count - 1; i >= startIndex; i--)
        {
            if (!ShouldAllowInput(results[i].gameObject))
            {
                results.RemoveAt(i);
            }
        }
    }

    protected override Func<GameObject, bool> GetFilter()
    {
        return ShouldAllowInput;
    }
}