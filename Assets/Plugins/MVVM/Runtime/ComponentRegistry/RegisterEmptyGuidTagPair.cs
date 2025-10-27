using System;
using UnityEngine;

namespace Plugins.MVVM.Runtime
{
    public class RegisterEmptyGuidTagPair : MonoBehaviour
    {
        private void Awake()
        {
            ComponentRegistry.Register(gameObject, (Guid.Empty, tag));
        }
    }
}