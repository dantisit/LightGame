using System;
using UnityEngine;

namespace MVVM.Binders
{
    public class TransformBinder : ObservableBinder<Transform>
    {
        protected override void OnStart()
        {
            base.OnStart();
            Value = transform;
        }

        public void Update()
        {
            Value = transform;
        }

        public override void OnPropertyChanged(Transform newValue)
        {
        }
    }
}
