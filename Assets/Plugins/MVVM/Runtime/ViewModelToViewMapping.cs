using System;

namespace MVVM
{
    [Serializable]
    public class ViewModelToViewMapping
    {
        public string ViewModelTypeFullName;
        public View PrefabView;
    }
}