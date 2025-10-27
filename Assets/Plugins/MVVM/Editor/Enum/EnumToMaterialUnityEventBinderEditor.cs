using UnityEditor;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToMaterialUnityEventBinder))]
    public class EnumToMaterialUnityEventBinderEditor : EnumMappingBinderEditor<Binders.EnumToMaterialUnityEventBinder>
    {
        protected override string MappingsPropertyName => "_mappings";
    }
}