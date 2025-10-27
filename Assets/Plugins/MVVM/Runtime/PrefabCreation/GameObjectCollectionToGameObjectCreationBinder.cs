using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MVVM.Binders
{
    public class GameObjectCollectionToGameObjectCreationBinder : ObservableCollectionBinder<GameObject>
    {
        protected readonly List<(GameObject prefab, GameObject instance)> _createdGameObjects = new();
        
        public override void OnItemAdded(GameObject prefab)
        {
            var instance = Instantiate(prefab, transform);
            _createdGameObjects.Add((prefab, instance));
        }

        public override void OnItemRemoved(GameObject prefab)
        {
            var instance = _createdGameObjects.Last(x => x.prefab == prefab).instance;
            Destroy(instance);
        }

        public override void OnCollectionSort(IList<GameObject> observableList)
        {
            foreach (var pair in _createdGameObjects)
            {
                var index = observableList.IndexOf(pair.prefab);
                pair.instance.transform.SetSiblingIndex(index);
            }
        }

        public override void OnCollectionClear()
        {
            foreach (var pair in _createdGameObjects)
            {
                Destroy(pair.instance);
            }
        }
    }
}