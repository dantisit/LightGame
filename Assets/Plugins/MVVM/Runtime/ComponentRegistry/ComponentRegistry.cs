using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;

namespace Plugins.MVVM.Runtime
{
    public static class ComponentRegistry
    {
        private static class TypedStorage<T>
        {
            public static readonly Dictionary<GameObject, T> ByGameObject = new();
            public static readonly Dictionary<T, GameObject> ByData = new();
            
            public static readonly Subject<KeyValuePair<GameObject, T>> OnChanged = new();
        }
        
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_initialized) return;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            _initialized = true;
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            // Cleanup subjects for destroyed GameObjects
        }

        public static void Register<T>(GameObject go, T data)
        {
            if (go == null)
            {
                Debug.LogWarning("ComponentRegistry: Cannot register null GameObject");
                return;
            }

            // Remove old reverse mapping if updating
            if (TypedStorage<T>.ByGameObject.TryGetValue(go, out var oldData))
            {
                TypedStorage<T>.ByData.Remove(oldData);
            }

            TypedStorage<T>.ByGameObject[go] = data;
            TypedStorage<T>.ByData[data] = go;

            TypedStorage<T>.OnChanged.OnNext(new KeyValuePair<GameObject, T>(go, data));
        }
        
        public static void Unregister<T>(GameObject go)
        {
            if (TypedStorage<T>.ByGameObject.TryGetValue(go, out var data))
            {
                TypedStorage<T>.ByData.Remove(data);
                TypedStorage<T>.ByGameObject.Remove(go);
            }
        }

        public static Observable<KeyValuePair<GameObject, T>> ObserveChanges<T>()
        {
            return TypedStorage<T>.OnChanged;
        }
        

        public static GameObject GetGameObject<T>(T data)
        {
            return TypedStorage<T>.ByData.GetValueOrDefault(data);
        }
        
        public static bool Has<T>(GameObject go)
        {
            return TypedStorage<T>.ByGameObject.ContainsKey(go);
        }
        
        public static T Get<T>(GameObject go)
        {
            return TypedStorage<T>.ByGameObject.GetValueOrDefault(go);
        }

        public static bool TryGet<T>(GameObject go, out T data)
        {
            return TypedStorage<T>.ByGameObject.TryGetValue(go, out data);
        }
        
        public static IEnumerable<GameObject> GetAllWith<T>()
        {
            return TypedStorage<T>.ByGameObject.Keys;
        }
        
        public static IEnumerable<KeyValuePair<GameObject, T>> GetAll<T>()
        {
            return TypedStorage<T>.ByGameObject;
        }

        public static void Clear<T>()
        {
            TypedStorage<T>.ByGameObject.Clear();
            TypedStorage<T>.ByData.Clear();
        }
    }
}