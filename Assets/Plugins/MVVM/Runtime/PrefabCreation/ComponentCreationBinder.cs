using UnityEngine;

namespace MVVM.Binders
{
    public class ComponentCreationBinder : ObservableBinder<Component>
    {
        private GameObject _gameObject;
        
        public override void OnPropertyChanged(Component prefab)
        {
            if(_gameObject) Destroy(_gameObject);
            if(prefab == null) return;
            _gameObject = Instantiate(prefab, transform).gameObject;
        }
    }
}