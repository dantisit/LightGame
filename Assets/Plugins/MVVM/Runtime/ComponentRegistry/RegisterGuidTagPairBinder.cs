using System;
using System.Collections.Generic;
using MVVM.Binders;
using Plugins.MVVM.Runtime;
using UnityEngine;

namespace Core._.UI.Domains.Battle.Binders
{
    public class RegisterGuidTagPairBinder : ObservableBinder<Guid>
    {
        public override void OnPropertyChanged(Guid newValue)
        {
            ComponentRegistry.Register(gameObject, (newValue, tag));
        }
    }
}