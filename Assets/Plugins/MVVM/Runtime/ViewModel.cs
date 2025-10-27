using System;
using R3;

namespace MVVM
{
    public abstract class ViewModel : IDisposable
    {
        public CompositeDisposable Disposables { get; } = new();


        public bool IsInstanceOf(string type) => GetType().FullName == type;
        
        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}