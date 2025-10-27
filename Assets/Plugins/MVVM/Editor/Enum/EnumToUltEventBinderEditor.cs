using UnityEditor;

namespace MVVM.Editor
{
    [CustomEditor(typeof(Binders.EnumToUltEventBinder))]
    public class EnumToUltEventBinderEditor : EnumMappingBinderEditor<Binders.EnumToUltEventBinder>
    {
        protected override string MappingsPropertyName => "_mappings";
    }
}