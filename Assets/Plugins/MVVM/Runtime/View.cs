using System;
using ObservableCollections;
using R3;
using UnityEngine;

namespace MVVM
{
    public abstract class View<TViewModel> : BinderView where TViewModel : ViewModel
    {
        public new TViewModel ViewModel => (TViewModel)base.ViewModel;
        
        protected sealed override void OnBindViewModel(ViewModel viewModel)
        {
            if (viewModel is TViewModel typedVM)
            {
                OnBindViewModel(typedVM);
            }
        }

        protected abstract void OnBindViewModel(TViewModel viewModel);
    
        protected CollectionBindBuilder<T> Bind<T>(ObservableList<T> collection) where T : ViewModel
        {
            return new CollectionBindBuilder<T>(collection, this);
        }
    }
}
