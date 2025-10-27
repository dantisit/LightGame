using UnityEngine;

namespace MVVM.Binders
{
    public class BoolToMaterialUnityEventBinder : ConverterUnityEventBinder<bool, Material>
    {
        [SerializeField] private Material _true;
        [SerializeField] private Material _false;
        
        protected override Material Convert(bool value)
        {
            return value ? _true : _false;
        }
    }
}