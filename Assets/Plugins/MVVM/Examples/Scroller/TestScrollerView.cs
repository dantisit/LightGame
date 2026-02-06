using System;
using System.Collections.Generic;
using MVVM;
using NUnit.Framework;
using Plugins.MVVM.Runtime.UIComponents.Scroll;
using UnityEngine;

namespace Core.Plugins.MVVM.Examples.Scroller
{
    public class TestScrollerView : View<TestScrollerViewModel>
    {
        [SerializeField] private UIS.Scroller scroller;
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private GameObject itemPrefab;

        public void Awake()
        {
            var recentlyPlayed = new List<TestScrollerItemModel>();
            var onlineFriends = new List<TestScrollerItemModel>();
            var offlineFriends = new List<TestScrollerItemModel>();

            for (int i = 0; i < 5; i++)
            {
                recentlyPlayed.Add(new TestScrollerItemModel(i));
            }
            
            
            for (int i = 0; i < 10; i++)
            {
                onlineFriends.Add(new TestScrollerItemModel(i));
            }

            for (int i = 0; i < 1000; i++)
            {
                offlineFriends.Add(new TestScrollerItemModel(i));
            }
            
            BindViewModel(new TestScrollerViewModel(recentlyPlayed, onlineFriends, offlineFriends));
        }

        protected override void OnBindViewModel(TestScrollerViewModel viewModel)
        {
            // Multi-section fluent binding (RECOMMENDED - clean, auto-disposal)
            Bind(viewModel.RecentlyPlayedLabel, labelPrefab)
                .BindCollection(viewModel.RecentlyPlayed, itemPrefab)
                .Bind(viewModel.OnlineFriendsLabel, labelPrefab)
                .BindCollection(viewModel.OnlineFriends, itemPrefab)
                .Bind(viewModel.OfflineFriendsLabel, labelPrefab)
                .BindCollection(viewModel.OfflineFriends, itemPrefab)
                .OnChanged(() => Debug.Log("Scroller content changed"))
                .ToScroller(scroller);  // ToScroller automatically calls Build()
            
            // Alternative: Single collection
            // Bind(viewModel.OnlineFriends).ToScroller(scroller, itemPrefab).Build();
        }
    }
}