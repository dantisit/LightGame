using System;
using R3;

namespace MVVM
{
    public abstract class Model : IDisposable
    {
        public CompositeDisposable Disposables { get; } = new();

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}