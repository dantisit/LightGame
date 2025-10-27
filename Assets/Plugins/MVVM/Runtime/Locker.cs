using System;
using System.Collections.Generic;
using System.Diagnostics;
using ObservableCollections;
using R3;
using Debug = UnityEngine.Debug;

public class Locker : IDisposable
{
    public ReadOnlyReactiveProperty<bool> IsLocked { get; }
    
    private Dictionary<LockerTypes, IDisposable> TypeToLocker { get; } = new();
    private ObservableList<string> Lockers { get; } = new();
    private CompositeDisposable disposables = new();
    
    public int LockCount => Lockers.Count;

    private string _lockerName;
    
    public Locker(string lockerName)
    {
        _lockerName = lockerName;
        IsLocked = Lockers.ObserveChanged().Select(_ => Lockers.Count > 0).ToReadOnlyReactiveProperty().AddTo(disposables);
    }
    
    public IDisposable Lock()
    {
        var caller = new StackFrame(1, true).GetMethod().Name;
        var callerMethodName = caller;

        Debug.LogWarning($"[DEBUG] {_lockerName} {callerMethodName} locked");

        Lockers.Add(callerMethodName);
        var locker = new Subject<Unit>();
        var disposable = locker.Do(locker, onDispose: _ =>
        {
            Debug.LogWarning($"[DEBUG] {_lockerName} {callerMethodName} unlocked");
            Lockers.Remove(callerMethodName);
        }).Subscribe();
        
        disposable.AddTo(disposables);
        
        return disposable;
    }

    public void Lock(LockerTypes type)
    {
        Unlock(type);
        var locker = Lock();
        TypeToLocker[type] = locker;
    }

    public void Unlock(LockerTypes type)
    {
        if (!TypeToLocker.TryGetValue(type, out var locker)) return;
        
        locker.Dispose();
        TypeToLocker.Remove(type);
    }

    public void Dispose()
    {
        disposables?.Dispose();
        TypeToLocker.Clear();
        Lockers.Clear();
    }
}