using R3;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MVVM
{
    public static class AsyncOperationExtensions
    {
        public static Observable<T> ObserveResult<T>(this AsyncOperationHandle<T> handle)
        {
            return Observable.Create<T>(observer =>
            {
                if (handle.IsDone)
                {
                    observer.OnNext(handle.Result);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                void OnComplete(AsyncOperationHandle<T> h)
                {
                    observer.OnNext(h.Result);
                    observer.OnCompleted();
                    handle.Completed -= OnComplete;
                }

                handle.Completed += OnComplete;

                return Disposable.Create(() => handle.Completed -= OnComplete);
            });
        }
    }
}