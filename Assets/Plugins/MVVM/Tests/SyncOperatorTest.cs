using System;
using System.Collections.Generic;
using NUnit.Framework;
using ObservableCollections;
using Plugins.MVVM.Runtime.Operators;

namespace Plugins.MVVM.Runtime.Tests
{
    public class SyncOperatorTest
    {
        private class TestDisposable : IDisposable
        {
            public int Value { get; }
            public bool IsDisposed { get; private set; }
            
            public TestDisposable(int value)
            {
                Value = value;
            }
            
            public void Dispose()
            {
                IsDisposed = true;
            }
        }
        
        [Test]
        public void Constructor_AutoSyncsExistingValues()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            
            // Act
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            
            // Assert
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target[0].Value);
            Assert.AreEqual(2, target[1].Value);
            Assert.AreEqual(3, target[2].Value);
        }
        
        [Test]
        public void OnAdd_FiresForExistingItems()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            var addedPairs = new List<(int, TestDisposable)>();
            
            // Act
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnAdd((src, tgt) => addedPairs.Add((src, tgt)));
            
            // Assert
            Assert.AreEqual(3, addedPairs.Count);
            Assert.AreEqual(1, addedPairs[0].Item1);
            Assert.AreEqual(2, addedPairs[1].Item1);
            Assert.AreEqual(3, addedPairs[2].Item1);
        }
        
        [Test]
        public void OnRemove_DoesNotFireForExistingItems()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            var removedPairs = new List<(int, TestDisposable)>();
            
            // Act
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnRemove((src, tgt) => removedPairs.Add((src, tgt)));
            
            // Assert
            Assert.AreEqual(0, removedPairs.Count);
        }
        
        [Test]
        public void Add_CreatesMappedItem()
        {
            // Arrange
            var source = new ObservableList<int>();
            var target = new ObservableList<TestDisposable>();
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            
            // Act
            source.Add(42);
            
            // Assert
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(42, target[0].Value);
        }
        
        [Test]
        public void Add_InsertsAtCorrectIndex()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 3 };
            var target = new ObservableList<TestDisposable>();
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            
            // Act
            source.Insert(1, 2);
            
            // Assert
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target[0].Value);
            Assert.AreEqual(2, target[1].Value);
            Assert.AreEqual(3, target[2].Value);
        }
        
        [Test]
        public void Add_FiresOnAddCallback()
        {
            // Arrange
            var source = new ObservableList<int>();
            var target = new ObservableList<TestDisposable>();
            var addedPairs = new List<(int, TestDisposable)>();
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnAdd((src, tgt) => addedPairs.Add((src, tgt)));
            
            // Act
            source.Add(42);
            
            // Assert
            Assert.AreEqual(1, addedPairs.Count);
            Assert.AreEqual(42, addedPairs[0].Item1);
            Assert.AreEqual(42, addedPairs[0].Item2.Value);
        }
        
        [Test]
        public void Remove_FiresOnRemoveCallback()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            var removedPairs = new List<(int, TestDisposable)>();
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnRemove((src, tgt) => removedPairs.Add((src, tgt)));
            
            // Act
            source.Remove(2);
            
            // Assert
            Assert.AreEqual(1, removedPairs.Count);
            Assert.AreEqual(2, removedPairs[0].Item1);
            Assert.AreEqual(2, removedPairs[0].Item2.Value);
        }
        
        [Test]
        public void Clear_FiresOnRemoveCallbackForEachItem()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            var removedPairs = new List<(int, TestDisposable)>();
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnRemove((src, tgt) => removedPairs.Add((src, tgt)));
            
            // Act
            source.Clear();
            
            // Assert
            Assert.AreEqual(3, removedPairs.Count);
        }
        
        [Test]
        public void Sort_ReordersTargetList()
        {
            // Arrange
            var source = new ObservableList<int> { 3, 1, 2 };
            var target = new ObservableList<TestDisposable>();
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            
            // Act
            source.Sort();
            
            // Assert
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target[0].Value);
            Assert.AreEqual(2, target[1].Value);
            Assert.AreEqual(3, target[2].Value);
        }
        
        [Test]
        public void Sort_DoesNotDisposeItems()
        {
            // Arrange
            var source = new ObservableList<int> { 3, 1, 2 };
            var target = new ObservableList<TestDisposable>();
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            var items = new[] { target[0], target[1], target[2] };
            
            // Act
            source.Sort();
            
            // Assert
            Assert.IsFalse(items[0].IsDisposed);
            Assert.IsFalse(items[1].IsDisposed);
            Assert.IsFalse(items[2].IsDisposed);
        }
        
        [Test]
        public void Sort_DoesNotFireCallbacks()
        {
            // Arrange
            var source = new ObservableList<int> { 3, 1, 2 };
            var target = new ObservableList<TestDisposable>();
            var addCount = 0;
            var removeCount = 0;
            using var op = new SyncOperator<int, TestDisposable>(
                    source, target, x => new TestDisposable(x))
                .OnAdd((_, _) => addCount++)
                .OnRemove((_, _) => removeCount++);
            
            addCount = 0; // Reset after initial sync
            
            // Act
            source.Sort();
            
            // Assert
            Assert.AreEqual(0, addCount);
            Assert.AreEqual(0, removeCount);
        }
        
        [Test]
        public void Dispose_StopsListeningToSource()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
            var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
            
            // Act
            op.Dispose();
            source.Add(4);
            
            // Assert
            Assert.AreEqual(3, target.Count); // Should not have added the 4th item
        }
        
        [Test]
        public void Move_PreservesMappedValuesAndReorders()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
    
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
    
            var mappedValue1 = target[0];
    
            // Act
            source.Move(0, 2);
    
            // Assert - Same instances, new order, not disposed
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(2, target[0].Value);
            Assert.AreEqual(3, target[1].Value);
            Assert.AreEqual(1, target[2].Value);
            Assert.AreSame(mappedValue1, target[2], "Should reuse same mapped instance");
            Assert.IsFalse(mappedValue1.IsDisposed, "Should not dispose during move");
        }

        [Test]
        public void Reverse_PreservesMappedValuesAndReorders()
        {
            // Arrange
            var source = new ObservableList<int> { 1, 2, 3 };
            var target = new ObservableList<TestDisposable>();
    
            using var op = new SyncOperator<int, TestDisposable>(
                source, target, x => new TestDisposable(x));
    
            var mappedValue1 = target[0];
            var mappedValue3 = target[2];
    
            // Act
            source.Reverse();
    
            // Assert
            Assert.AreEqual(3, target.Count);
            Assert.AreSame(mappedValue3, target[0]);
            Assert.AreSame(mappedValue1, target[2]);
            Assert.IsFalse(mappedValue1.IsDisposed, "Should not dispose during reverse");
            Assert.IsFalse(mappedValue3.IsDisposed, "Should not dispose during reverse");
        }
    }
}