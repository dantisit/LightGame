using System.Collections.Generic;
using UnityEngine;

namespace MVVM
{
    public class ViewModelToViewMapper : MonoBehaviour
    {
        [SerializeField] private List<ViewModelToViewMapping> _prefabMappings;
        [SerializeField] private BinderView _prefabByDefault;

        private readonly Dictionary<string, BinderView> _mappings = new();

        private void Awake()
        {
            foreach (var prefabMapping in _prefabMappings)
            {
                _mappings.TryAdd(prefabMapping.ViewModelTypeFullName, prefabMapping.prefabBinderView);
            }
        }

        public BinderView GetPrefab(string viewModelTypeFullName)
        {
            if (_mappings.TryGetValue(viewModelTypeFullName, out var value))
            {
                return value;
            }

            return _prefabByDefault;
        }
    }
}