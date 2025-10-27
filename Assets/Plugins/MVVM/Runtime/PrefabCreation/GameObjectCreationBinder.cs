using UnityEngine;

namespace MVVM.Binders
{
    public class GameObjectCreationBinder : ObservableBinder<GameObject>
    {
        private GameObject _gameObject;
        public override void OnPropertyChanged(GameObject prefab)
        {
            if(_gameObject) Destroy(_gameObject);
            if(prefab == null) return;
            _gameObject = Instantiate(prefab, transform);
        }
    }
}