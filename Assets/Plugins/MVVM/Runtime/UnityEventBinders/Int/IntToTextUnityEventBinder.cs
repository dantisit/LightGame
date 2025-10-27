using UnityEngine;
using UnityEngine.Events;

namespace MVVM.Binders
{
    public class IntToTextUnityEventBinder : ConverterUnityEventBinder<int, string>
    {
        protected override string Convert(int value) => value.ToString();
    }
}
