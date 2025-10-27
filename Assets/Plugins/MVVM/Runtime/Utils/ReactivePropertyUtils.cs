using System;
using System.Linq;
using System.Reflection;
using R3;
using UnityEngine;

namespace MVVM.Utils
{
    public static class ReactivePropertyUtils
    {
        public static bool IsReactiveProperty(Type type)
        {
            if (type == null) return false;

            return type.IsGenericType && (
                type.GetGenericTypeDefinition().Name.StartsWith("ReactiveProperty`") ||
                type.GetInterfaces().Any(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition().Name.StartsWith("IReadOnlyReactiveProperty`"))
            );
        }
        
        public static bool IsObservable(Type type)
        {
            if (type == null) return false;

            // Check if it's a generic type
            if (!type.IsGenericType) return false;

            // Check if it directly inherits from Observable<T>
            if (IsObservableType(type)) return true;

            // Check if any of its base types inherit from Observable<T>
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && IsObservableType(baseType))
                    return true;
                baseType = baseType.BaseType;
            }

            // Check if it implements any interfaces that inherit from Observable<T>
            return type.GetInterfaces().Any(i => i.IsGenericType && IsObservableType(i));
        }

        private static bool IsObservableType(Type type)
        {
            if (!type.IsGenericType) return false;
    
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            return genericTypeDefinition.Name.StartsWith("Observable`") ||
                   genericTypeDefinition.FullName?.StartsWith("Observable`") == true;
        }

        /// <summary>
        /// Sets up subscription to a ReactiveProperty and returns its current ViewModel value.
        /// The subscription will be automatically cleaned up when the GameObject is destroyed.
        /// </summary>
        /// <param name="reactiveProperty">The ReactiveProperty instance to handle</param>
        /// <param name="gameObject">GameObject to bind the subscription lifetime to</param>
        /// <param name="onViewModelChanged">Callback to invoke when the ReactiveProperty value changes</param>
        /// <returns>Current ViewModel value of the ReactiveProperty</returns>
        public static ViewModel HandleReactivePropertyValue(object reactiveProperty, GameObject gameObject, Action<ViewModel> onViewModelChanged)
        {
            if (reactiveProperty == null) return null;

            var reactivePropertyType = reactiveProperty.GetType();
            var genericArgType = reactivePropertyType.GetGenericArguments().FirstOrDefault();

            if (genericArgType == null) return null;

            var subscribeMethod = typeof(ReactivePropertyUtils).GetMethod(nameof(SubscribeToReactiveProperty), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericArgType);

            return subscribeMethod.Invoke(null, new object[] { reactiveProperty, gameObject, onViewModelChanged }) as ViewModel;
        }

        private static ViewModel SubscribeToReactiveProperty<T>(ReadOnlyReactiveProperty<T> reactiveProperty, GameObject gameObject, Action<ViewModel> onViewModelChanged)
        {
            var currentViewModel = reactiveProperty.CurrentValue as ViewModel;
            
            reactiveProperty.Subscribe(newValue =>
            {
                var newViewModel = newValue as ViewModel;
                onViewModelChanged?.Invoke(newViewModel);
            }).AddTo(gameObject);

            return currentViewModel;
        }
    }
}
