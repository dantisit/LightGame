using MVVM;

namespace Plugins.MVVM.Runtime
{
    public interface IView
    {
        ViewModel ViewModel { get; }
    }
}